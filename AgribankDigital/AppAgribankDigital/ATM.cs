using Dermalog.Imaging.Capturing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace AppAgribankDigital
{

    class ATM
    {
        WebSocket ws = null;
        public Socket socketATM;
        TcpListener listener;
        public bool isResetting = false;
        public static string CardNumber = "";
        public bool isCheckFinger = false;
        public bool isWithdrawMoney = false;

        public bool isKeyD = true;
        public static int itemnCard = 1;
        public static Thread ThreadTimeoutFinger = null;
        static Thread Threadcheckdcctrl = null;
        FingerPrinZF1 fingerPrinZF1;

        public ATM()
        {
            try
            {
                Utilities.LogFW("Waiting connect from ATM ...");
                listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
                listener.Start();

                Utilities.LogFW("Start listener ");

                socketATM = listener.AcceptSocket();
                Utilities.LogFW("socketATM Accept ");
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Utils.SEND_DATA_TIMEOUT);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                LingerOption lingerOption = new LingerOption(false, 3);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                listener.Stop();
                if (socketATM.Connected)
                {
                    Utilities.LogFW("Connected to ATM : " + socketATM.Connected);
                    return;
                }
                else
                {

                    Utilities.LogFW("Cannot connect to ATM");
                    socketATM.Close();
                    listener.Stop();

                }
            }
            catch (Exception ex)
            {
                Utilities.LogFW("Exception while connecting to ATM: " + ex.Message);

            }

        }

        public bool IsConnected()
        {
            try
            {
                bool check = !(socketATM.Poll(Utils.CHECK_CONNECTION_TIMEOUT, SelectMode.SelectRead) && socketATM.Available == 0);
                //if (!check)
                //    Logger.Log("ATM not responding");
                return check;
            }
            catch (SocketException)
            {
                //Logger.Log("ATM not responding");
                return false;
            }
            catch (ObjectDisposedException)
            {
                //Logger.Log("ATM not responding");
                return false;
            }
        }
        public void initFingerPrintCB100(Socket socketHost, Socket socketATM, string dataStr)
        {
            try
            {
                ws = new WebSocket("ws://192.168.42.129:8887");

                FingerPrintCB100 fingerPrint = new FingerPrintCB100(ws);
                fingerPrint.FingerPrintWorking(socketHost, socketATM, dataStr);
            }
            catch (Exception e)
            {

                Logger.Log("Err: " + e.ToString());
                Logger.Log("CB100 start Success!");
            }
        }
        public void initFingerPrintZF1(Socket socketHost, Socket socketATM)
        {
            try
            {
                if (fingerPrinZF1 != null)
                {
                    fingerPrinZF1.CloseDevice();
                }
                fingerPrinZF1 = new FingerPrinZF1();
                fingerPrinZF1._capDevice = DeviceManager.GetDevice(DeviceIdentity.FG_ZF1);
                fingerPrinZF1.socketATM = socketATM;
                fingerPrinZF1.socketHost = socketHost;
                fingerPrinZF1.InitializeDevice();
               
                Utilities.LogFW("ZF1 start Success!");
                fingerPrinZF1._capDevice.Start();

                //Không cho phép nhận vân tay
                this.fingerPrinZF1._capDevice.Freeze(true);
            }
            catch (Exception e)
            {
                Utilities.LogFW("Err: " + e.ToString());
                Utilities.LogFW("ZF1 start failed!");
            }
        }

        public void closeFingerPrintZF1()
        {
            try
            {
                if (fingerPrinZF1 != null)
                {
                    fingerPrinZF1.CloseDevice();
                }
            }
            catch (Exception ex)
            {

                Utilities.LogFW(ex.Message);
            }


        }
        public void TimeoutFinger(Socket socketATM, string dataStr)
        {

            Thread.Sleep(15000);
            fingerPrinZF1._capDevice.Freeze(true);
            fingerPrinZF1._capDevice.Property[PropertyType.FG_GREEN_LED] = 0;
            if (socketATM.Connected)
            {
                string coordination = Utilities.getconditionHEX2(dataStr);
                byte[] errData = Utilities.fingerErr(coordination);
                Utilities.LogFWToHost(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                socketATM.Send(errData);
                ThreadTimeoutFinger.Abort();
            }




        }

        public void checkconnetedZF1(Socket socketHost, Socket socketATM, string dataStr)
        {
            try
            {
                if (fingerPrinZF1 != null)
                {
                    if (fingerPrinZF1._capDevice != null)
                    {
                        fingerPrinZF1.dataStr = dataStr;
                        //Cho phép nhận vân tay
                        this.fingerPrinZF1._capDevice.Freeze(false);
                        fingerPrinZF1._capDevice.Property[PropertyType.FG_GREEN_LED] = 1;

                        ThreadTimeoutFinger = new Thread(new ThreadStart(() => TimeoutFinger(socketATM, dataStr)));
                        ThreadTimeoutFinger.Start();
                    }
                    else
                    {
                        Utilities.LogFW("Err: The scanner is disconnected from the ATM");
                        if (this.IsConnected())
                        {
                            string coordination = Utilities.getconditionHEX2(dataStr);
                            byte[] errData = Utilities.fingerErr(coordination);
                            Utilities.LogFW("ZF1 start failed!");
                            Utilities.LogFWToHost(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                            socketATM.Send(errData);
                            initFingerPrintZF1(socketHost, socketATM);
                        }
                    }
                }
                else
                {
                    Utilities.LogFW("Err: The scanner is disconnected from the host");
                    if (this.IsConnected())
                    {
                        string coordination = Utilities.getconditionHEX2(dataStr);
                        byte[] errData = Utilities.fingerErr(coordination);
                        Utilities.LogFWToHost(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                        socketATM.Send(errData);
                        initFingerPrintZF1(socketHost, socketATM);
                    }
                }


            }
            catch (Exception ex)
            {

                Utilities.LogFW("Err: " + ex.Message.ToString());
                Utilities.LogFW("ZF1 start failed!");
                if (this.IsConnected())
                {
                    string coordination = Utilities.getconditionHEX2(dataStr);
                    byte[] errData = Utilities.fingerErr(coordination);
                    Utilities.LogFWToHost(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                    Logger.LogRaw("> " + Encoding.ASCII.GetString(errData));
                    socketATM.Send(errData);
                    initFingerPrintZF1(socketHost, socketATM);
                }
            }


        }
        public void checkdcctrl()
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "/C dcctrl.exe checkmode";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "cmd.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            while (true)
            {
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                string str = process.StandardOutput.ReadToEnd();
                Logger.LogFingrprint(str);

                if (str.Contains("Exit: 49"))
                {
                    RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
                    RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Wincor Nixdorf\AgribankDigital\cardTrack2");
                    versie3.SetValue("CardNumber", "");
                    versie4.SetValue("CardNumber", "");
                    versie4.Close();
                    versie3.Close();
                    Logger.LogFingrprint(str);
                    this.isWithdrawMoney = false;
                    process.Close();
                    break;
                }
                process.Close();

            }



            Threadcheckdcctrl.Abort();



        }

        public void ReceiveDataFromATM(Host host)
        {

            while (true)
            {
                if (!this.isResetting && !host.isResetting)
                {
                    if (this.IsConnected())
                    {

                        Byte[] data = Utils.ReceiveAll(socketATM);
                        if (data.Length > 0)
                        {
                            string dataStr = Encoding.ASCII.GetString(data);
                            string dataStrFormart = Utilities.convertToHex(dataStr, Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");

                            if (dataStr.Contains(Utilities.Hex2Ascii(@"?\1c\1c") + "HBCI"))
                            {
                                try
                                {
                                    itemnCard = 1;
                                    Utilities.LogATMToFW(dataStrFormart, dataStr);

                                    if (Utils.HAS_CONTROLLER)
                                    {
                                        checkconnetedZF1(host.socketHost, socketATM, dataStr);
                                    }
                                    else
                                    {
                                        initFingerPrintCB100(host.socketHost, socketATM, dataStr);
                                        if (ws.ReadyState == WebSocketState.Closed)
                                        {
                                            ws.Close();

                                            Logger.LogFingrprint(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " The scanner is disconnected from the atm");

                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utilities.LogFW(e.Message);
                                }

                            }
                            else if (this.isCheckFinger == true)
                            {
                                try
                                {
                                    Utilities.LogFW("activate to Finger");
                                    if (dataStrFormart.Substring(0, 2).Trim().Equals("11"))
                                    {
                                        int countCard = Int32.Parse(Utilities.getValueRegittry("CountCard"));
                                        string[] abc = { "I", "H", "G", "F", "A", "B", "C", "D" };

                                        if (dataStrFormart.Contains(@"\1c\1c\1c\1c\1c\1cD\1c") && countCard > 8 && isKeyD == true)
                                        {
                                            itemnCard = 8;
                                            List<string> listcard2 = Utilities.listCard2(Utils.REGISTRY);
                                            string cardMess = Utilities.formartMessCard(listcard2, 0);
                                            byte[] cardMessByte = Utilities.DCTCP2H_Send(cardMess);
                                            Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(cardMessByte));
                                            socketATM.Send(cardMessByte);
                                            isKeyD = false;
                                        }
                                        else if (dataStrFormart.Contains(@"\1c\1c\1c\1c\1c\1cE\1c"))
                                        {
                                            this.isCheckFinger = false;
                                            Utilities.LogFW("removet to Finger");
                                            Utilities.LogATMToFW(dataStrFormart, dataStr);
                                            if (host.IsConnected())
                                            {
                                                host.socketHost.Send(data);
                                                Utilities.LogFWToHost(dataStrFormart, dataStr);
                                            }
                                        }
                                        else
                                        {

                                            for (int i = 0; i < abc.Length; i++)
                                            {
                                                string str = @"\1c\1c\1c\1c\1c\1c" + abc[i] + @"\1c";

                                                if (dataStrFormart.Contains(str))
                                                {
                                                    this.isCheckFinger = false;
                                                    this.isWithdrawMoney = true;

                                                    Threadcheckdcctrl = new Thread(new ThreadStart(() => checkdcctrl()));
                                                    Threadcheckdcctrl.Start();
                                                    RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + "\\" + (i + itemnCard));
                                                    RegistryKey versie2 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
                                                    RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
                                                    RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Wincor Nixdorf\AgribankDigital\cardTrack2");
                                                    string condition = Utilities.getcondition(dataStrFormart);
                                                    string serialNumber = versie2.GetValue("SerialNumber").ToString();
                                                    CardNumber = versie1.GetValue("CONTENTS").ToString();
                                                    versie3.SetValue("CardNumber", CardNumber);
                                                    versie4.SetValue("CardNumber", CardNumber);
                                                    string keyformat = @"4\1c000\1c\1c" + "002" + @"\1c00000000\1c" + serialNumber + @"5000\1c" + condition + "00";
                                                    byte[] keyformatByte = Utilities.DCTCP2H_Send(keyformat);
                                                    Utilities.LogFWToATM(keyformat, Encoding.ASCII.GetString(keyformatByte));
                                                    socketATM.Send(keyformatByte);
                                                    Utilities.LogFW("removet to Finger");
                                                    versie1.Close();
                                                    versie2.Close();
                                                    versie3.Close();
                                                    versie4.Close();
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        this.isCheckFinger = false;
                                        Utilities.LogFW("removet to Finger");
                                        Utilities.LogATMToFW(dataStrFormart, dataStr);
                                        if (host.IsConnected())
                                        {
                                            host.socketHost.Send(data);
                                            Utilities.LogFWToHost(dataStrFormart, dataStr);
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utilities.LogFW(ex.Message);
                                }

                            }
                            else if (this.isWithdrawMoney == true)
                            {
                                try
                                {
                                    if (dataStrFormart.Substring(0, 2).Trim().Equals("11") && dataStr.Contains(Utilities.Hex2Ascii(@"?\1c\1c") + "ADC"))
                                    {
                                        string datacardStr = "";
                                        string card = CardNumber.Substring(CardNumber.Length - 9, 8);
                                        int indexstr4 = dataStr.IndexOf(Utilities.Hex2Ascii(@"\1c\1c\1c\1c"));
                                        int indexstr3 = dataStr.LastIndexOf(Utilities.Hex2Ascii(@"\1c\1c\1c"));
                                        if (indexstr4 > 0)
                                        {
                                            datacardStr = dataStr.Substring(0, indexstr4 + 2) + card + dataStr.Substring(indexstr4 + 2);
                                            datacardStr = datacardStr.Remove(0, 2);
                                        }
                                        else
                                        {
                                            datacardStr = dataStr.Substring(0, indexstr3 + 1) + card + dataStr.Substring(indexstr3 + 1);
                                            datacardStr = datacardStr.Remove(0, 2);
                                        }
                                        byte[] dataWithdrawMoney = Utilities.DCTCP2H_Send(datacardStr);
                                        if (host.IsConnected())
                                        {
                                            host.socketHost.Send(dataWithdrawMoney);
                                            string dataWithdrawMoneyStr = Encoding.ASCII.GetString(dataWithdrawMoney);
                                            string dataStrWithdrawMoney = Utilities.convertToHex(dataWithdrawMoneyStr, Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                                            Utilities.LogFWToHost(dataStrWithdrawMoney, dataWithdrawMoneyStr);
                                        }

                                    }
                                    else
                                    {
                                        Utilities.LogATMToFW(dataStrFormart, dataStr);
                                        if (host.IsConnected())
                                        {
                                            host.socketHost.Send(data);
                                            Utilities.LogFWToHost(dataStrFormart, dataStr);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utilities.LogFW(ex.Message);
                                }

                            }

                            else
                            {
                                this.isCheckFinger = false;
                                Utilities.LogATMToFW(dataStrFormart, dataStr);
                                if (host.IsConnected())
                                {
                                    host.socketHost.Send(data);
                                    Utilities.LogFWToHost(dataStrFormart, dataStr);
                                }
                            }


                        }
                    }
                }
            }
        }

        public void Close()
        {
            try
            {
                if (socketATM != null)
                {
                    if (socketATM.Connected)
                        socketATM.Close();
                        listener.Stop();
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

        }

    }
}
