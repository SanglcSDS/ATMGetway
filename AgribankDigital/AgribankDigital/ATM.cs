using Dermalog.Imaging.Capturing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace AgribankDigital
{

    class ATM
    {
        WebSocket ws = null;
        public Socket socketATM;
        //Socket socketHost;
        //TcpClient tcpClient;
        TcpListener listener;
        public bool isResetting = false;
        public static string CardNumber = "";
        public bool isCheckFinger = false;
        public bool isKeyD = true;
        public static int itemnCard = 1;
        public static  Thread ThreadTimeoutFinger = null;
        FingerPrinZF1 fingerPrinZF1;

        public ATM()
        {


            try
            {

                Logger.Log("Waiting connect from ATM ...");
                Logger.LogRaw("Waiting connect from ATM ...");
                listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
                listener.Start();

                Logger.Log("Start listener ");
                Logger.LogRaw("Start listener ");
                socketATM = listener.AcceptSocket();
                Logger.Log("socketATM Accept ");
                Logger.LogRaw("socketATM Accept ");
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Utils.SEND_DATA_TIMEOUT);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                LingerOption lingerOption = new LingerOption(false, 3);
                socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                listener.Stop();
                if (socketATM.Connected)
                {
                    Logger.Log("Connected to ATM : " + socketATM.Connected);
                    Logger.LogRaw("Connected to ATM : " + socketATM.Connected);
                    return;
                }
                else
                {
                    Logger.Log("Trying to reconnect connect from ATM ...");
                    Logger.LogRaw("Trying to reconnect connect from ATM ...");
                    socketATM.Close();
                    listener.Stop();

                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception while connecting to Host: " + ex.Message);
                Logger.Log("Cannot connect to Host, trying to reconnect ..."); 
                Logger.LogRaw("Exception while connecting to Host: " + ex.Message);
                Logger.LogRaw("Cannot connect to Host, trying to reconnect ...");
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
                Logger.Log("ZF1 start Success!");
                Logger.LogRaw("ZF1 start Success!");
                fingerPrinZF1._capDevice.Start();

                //Không cho phép nhận vân tay
                this.fingerPrinZF1._capDevice.Freeze(true);
            }
            catch (Exception e)
            {
                Logger.Log("Err: " + e.ToString());
                Logger.Log("ZF1 start failed!");
                Logger.LogRaw("Err: " + e.ToString());
                Logger.LogRaw("ZF1 start failed!");
                /*  if (this.IsConnected())
                  {
                      Logger.LogRaw("ZF1 start failed!");
                      Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                      Logger.LogRaw("> " + Encoding.ASCII.GetString(Utilities.fingerErr()));
                      socketATM.Send(Utilities.fingerErr());
                  }*/

            }
        }

        public void closeFingerPrintZF1()
        {
            if (fingerPrinZF1 != null)
            {
                fingerPrinZF1.CloseDevice();
            }
        }
        public  void TimeoutFinger( Socket socketATM , string dataStr)
        {
            Thread.Sleep(15000);
            fingerPrinZF1._capDevice.Freeze(true);
            fingerPrinZF1._capDevice.Property[PropertyType.FG_GREEN_LED] = 0;
            if (socketATM.Connected)
            {
                string coordination = Utilities.getconditionHEX2(dataStr);
                byte[] errData = Utilities.fingerErr(coordination);
                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                Logger.Log("> " + Utilities.fingerErrstring(coordination));
                Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                Logger.LogRaw("> " + Encoding.ASCII.GetString(errData));
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
                        ThreadTimeoutFinger = new Thread(new ThreadStart(() => TimeoutFinger( socketATM, dataStr)));
                        ThreadTimeoutFinger.Start();
                    }
                    else
                    {
                        Logger.Log("Err: The scanner is disconnected from the host");
                        Logger.Log("ZF1 start failed!");
                        Logger.LogRaw("Err: The scanner is disconnected from the host");
                        Logger.LogRaw("ZF1 start failed!");
                        if (this.IsConnected())
                        {
                            string coordination = Utilities.getconditionHEX2(dataStr);
                            byte[] errData = Utilities.fingerErr(coordination);
                            Logger.LogRaw("ZF1 start failed!");
                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                            Logger.Log("> " + Utilities.fingerErrstring(coordination));
                            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                            Logger.LogRaw("> " + Encoding.ASCII.GetString(errData));
                            socketATM.Send(errData);
                            initFingerPrintZF1(socketHost, socketATM);
                        }
                    }
                }
                else
                {
                    Logger.Log("Err: The scanner is disconnected from the host");
                    Logger.Log("ZF1 start failed!");
                    Logger.LogRaw("Err: The scanner is disconnected from the host");
                    Logger.LogRaw("ZF1 start failed!");
                    if (this.IsConnected())
                    {
                        string coordination = Utilities.getconditionHEX2(dataStr);
                        byte[] errData = Utilities.fingerErr(coordination);
                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                        Logger.Log("> " + Utilities.fingerErrstring(coordination));
                        Logger.LogRaw("ZF1 start failed!");
                        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                        Logger.LogRaw("> " + Encoding.ASCII.GetString(errData));
                        socketATM.Send(errData);
                        initFingerPrintZF1(socketHost, socketATM);
                    }
                }


            }
            catch (Exception ex)
            {

                Logger.Log("Err: " + ex.Message.ToString());
                Logger.Log("ZF1 start failed!"); 
                Logger.LogRaw("Err: " + ex.Message.ToString());
                Logger.LogRaw("ZF1 start failed!");
                if (this.IsConnected())
                {
                    string coordination = Utilities.getconditionHEX2(dataStr);
                    byte[] errData = Utilities.fingerErr(coordination);
                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                    Logger.Log("> " + Utilities.fingerErrstring(coordination));
                    Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                    Logger.LogRaw("> " + Encoding.ASCII.GetString(errData));
                    socketATM.Send(errData);
                    initFingerPrintZF1(socketHost, socketATM);
                }
            }


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
                            /* Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Raw ATM to FW:");
                             Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));*/
                            string dataFinger = System.Text.Encoding.ASCII.GetString(data);
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                            dataStr = Utilities.formatCardNumber(dataStr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");
                        

                            if (dataFinger.Contains("HBCI"))
                            {
                                itemnCard = 1;
                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                Logger.Log("> " + dataStr);
                                Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));

                                if (Utils.HAS_CONTROLLER)
                                {
                                    checkconnetedZF1(host.socketHost, socketATM, dataFinger);
                                }
                                else
                                {
                                    initFingerPrintCB100(host.socketHost, socketATM, dataFinger);
                                    if (ws.ReadyState == WebSocketState.Closed)
                                    {
                                        ws.Close();

                                        Logger.LogFingrprint(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " The scanner is disconnected from the atm");

                                    }
                                }
                            }
                            else if (this.isCheckFinger == true)
                            {
                                Logger.Log("> " + "activate to Finger");
                                if (dataStr.Substring(0, 2).Trim().Equals("11"))
                                {
                                    int countCard = Int32.Parse(Utilities.getValueRegittry("CountCard"));
                                    string[] abc = { "I", "H", "G", "F", "A", "B", "C", "D" };

                                    if (dataStr.Contains(@"\1c\1c\1c\1c\1c\1cD\1c") && countCard > 8 && isKeyD == true)
                                    {
                                        itemnCard = 8;
                                        List<string> listcard2 = Utilities.listCard2(Utils.REGISTRY);
                                        string cardMess = Utilities.formartMessCard(listcard2, 0);
                                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                                        Logger.Log("> " + cardMess);
                                        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                        Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                                        socketATM.Send(Utilities.DCTCP2H_Send(cardMess));
                                        isKeyD = false;

                                    }
                                    else if (dataStr.Contains(@"\1c\1c\1c\1c\1c\1cE\1c"))
                                    {
                                        Logger.Log("> " + "removet to Finger");
                                        this.isCheckFinger = false;
                                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                        Logger.Log("> " + dataStr);
                                        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                        Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));

                                        if (host.IsConnected())
                                        {
                                            host.socketHost.Send(data);
                                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                            Logger.Log("> " + dataStr);
                                            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                            Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                                        }
                                    }
                                    else
                                    {

                                        for (int i = 0; i < abc.Length; i++)
                                        {
                                            string str = @"\1c\1c\1c\1c\1c\1c" + abc[i] + @"\1c";

                                            if (dataStr.Contains(str))
                                            {
                                                this.isCheckFinger = false;
                                                RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + "\\" + (i + itemnCard));
                                                RegistryKey versie2 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
                                                RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
                                                RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Wincor Nixdorf\AgribankDigital\cardTrack2");
                                                string condition = Utilities.getcondition(dataStr);
                                                string serialNumber = versie2.GetValue("SerialNumber").ToString();
                                                CardNumber = versie1.GetValue("CONTENTS").ToString();
                                              //  Logger.LogRaw("item i:" + i + " item itemCard: " + itemnCard + "(i+itemcard):" + (i + itemnCard) + "; CardNumber:" + CardNumber);
                                                versie3.SetValue("CardNumber", CardNumber);
                                                versie4.SetValue("CardNumber", CardNumber);
                                                string keyformat = @"4\1c000\1c\1c" + "002" + @"\1c00000000\1c" + serialNumber + @"5000\1c" + condition + "00";
                                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                                                Logger.Log("> " + keyformat);
                                                Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + "FW to ATM:");
                                                Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(Utilities.DCTCP2H_Send(keyformat)));
                                                socketATM.Send(Utilities.DCTCP2H_Send(keyformat));
                                                Logger.Log("> " + "removet to Finger");
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
                                    Logger.Log("> " + "removet to Finger");
                                    this.isCheckFinger = false;
                                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                    Logger.LogRaw("> " + dataStr);
                                    Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                    Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));

                                    if (host.IsConnected())
                                    {
                                        host.socketHost.Send(data);
                                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                        Logger.Log("> " + dataStr);
                                        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                        Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                                    }

                                }
                            }
                            else
                            {
                                this.isCheckFinger = false;
                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                Logger.Log("> " + dataStr);
                                Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                                if (host.IsConnected())
                                {
                                    host.socketHost.Send(data);
                                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                    Logger.Log("> " + dataStr);
                                    Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                    Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                                }
                            }


                        }
                    }
                }
            }
        }

        public void Close()
        {
            if (socketATM != null)
            {
                if (socketATM.Connected)
                    socketATM.Close();
                listener.Stop();
            }

        }

    }
}
