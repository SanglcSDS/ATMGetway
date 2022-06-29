using Dermalog.Imaging.Capturing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace AgribankDigital
{

    class ATM
    {
       
        public Socket socketATM;
        TcpListener listener;

        public bool isCheckFinger = false;
        public bool isWithdrawMoney = false;
   
        public static Thread ThreadTimeoutFinger = null;
        static Thread Threadcheckdcctrl = null;
        public static string coordination21 = "";
        public static int page = 1;

        public ATM()
        {
            while (true)
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

                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogFW("Exception while connecting to ATM: " + ex.Message);

                }

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
                    this.isCheckFinger = false;
                    this.isWithdrawMoney = false;
                    page = 1;
                    Host.listcard = null;
                    Utilities.CopyFilesRecursively(Utils.IMAGE_CARD, Utils.IMAGE_NCRPICT);
                    Utilities.CleanCardTrack2();
                    Logger.LogFingrprint(str);
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

                if (this.IsConnected())
                {

                    Byte[] data = Utils.ReceiveAll(socketATM);
                    if (data.Length > 0)
                    {
                        string dataStr = Encoding.ASCII.GetString(data);

                        string dataStrFormart = Utilities.convertToHex(dataStr);
                        if (dataStr.Contains(Utilities.Hex2Ascii(@"?\1c\1c") + "HBCI"))
                        {
                            try
                            {
                                coordination21 = Utilities.getconditionHEX2(dataStr);
                                Utilities.CopyFilesRecursively(Utils.IMAGE_FINGER, Utils.IMAGE_NCRPICT);

                                Utilities.LogATMToFW(dataStrFormart, dataStr);

                                FingerPrintCB100.sendFingerCB100(host.socketHost, socketATM, dataStr);

                               /* if (Utils.HAS_CONTROLLER)
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
                                }*/
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
                              
                                Utilities.LogATMToFW(dataStrFormart, dataStr);
                                if (dataStrFormart.Substring(0, 2).Trim().Equals("11"))
                                {

                                    string condition = Utilities.getcondition(dataStrFormart);


                                    string[] abc = { "I", "H", "G", "F", "A", "B", "C", "D" };

                                    if (dataStrFormart.Contains(@"\1c\1c\1c\1c\1c\1cD\1c") && page == 1)
                                    {
                                        int checkpage = Host.listcard.Where(n => n.index == 2).Count();
                                        if (checkpage > 0)
                                        {
                                            page = 2;
                                            coordination21 = Utilities.getcondition(dataStrFormart);
                                            string cardMess = Utilities.formartMessCard(Host.listcard, 2);
                                            byte[] cardMessByte = Utilities.DCTCP2H_Send(cardMess);
                                            Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(cardMessByte));
                                            socketATM.Send(cardMessByte);
                                        }
                                        else
                                        {
                                            Utilities.LogFW("removet to Finger");
                                            this.isCheckFinger = false;
                                            this.isWithdrawMoney = true;
                                            Threadcheckdcctrl = new Thread(new ThreadStart(() => checkdcctrl()));
                                            Threadcheckdcctrl.Start();
                                            string CardNumber = Host.listcard.Where(f => f.index == 1).Where(f => f.LetterKey == "D").First().CardNumberFull;
                                            Utilities.setCardTrack2(CardNumber);
                                            string keyformat = @"4\1c000\1c\1c" + "002" + @"\1c00000000\1c" + Utilities.getSerialNumber() + @"5000\1c" + condition + "00";
                                            byte[] keyformatByte = Utilities.DCTCP2H_Send(keyformat);
                                            Utilities.LogFWToATM(keyformat, Encoding.ASCII.GetString(keyformatByte));
                                            socketATM.Send(keyformatByte);


                                        }
                                    }
                                    else if (dataStrFormart.Contains(@"\1c\1c\1c\1c\1c\1cF\1c") && page == 2)
                                    {
                                        page = 1;
                                        coordination21= Utilities.getcondition(dataStrFormart);
                                        string cardMess = Utilities.formartMessCard(Host.listcard, 1);
                                        byte[] cardMessByte = Utilities.DCTCP2H_Send(cardMess);
                                        Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(cardMessByte));
                                        socketATM.Send(cardMessByte);
                                    }
                                    else if (dataStrFormart.Contains(@"\1c\1c\1c\1c\1c\1cE\1c"))
                                    {
                                        Utilities.LogFW("removet to Finger");
                                        this.isCheckFinger = false;
                                        this.isWithdrawMoney = false;
                                        page = 1;
                                        Utilities.LogATMToFW(dataStrFormart, dataStr);
                                        if (this.IsConnected())
                                        {
                                            string coordination = Utilities.getconditionHEX2(dataStr);
                                            byte[] errData = Utilities.fingerErr(coordination);
                                            Utilities.LogFWToATM(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                                            socketATM.Send(errData);
                                        }
                                    }
                                    else
                                    {

                                        for (int i = 0; i < abc.Length; i++)
                                        {
                                            string str = @"\1c\1c\1c\1c\1c\1c" + abc[i] + @"\1c";

                                            if (dataStrFormart.Contains(str))
                                            {
                                                Utilities.LogFW("removet to Finger");
                                                this.isCheckFinger = false;
                                                this.isWithdrawMoney = true;
                                                Threadcheckdcctrl = new Thread(new ThreadStart(() => checkdcctrl()));
                                                Threadcheckdcctrl.Start();
                                                string CardNumberFull = Host.listcard.Where(f => f.index == page).Where(f => f.LetterKey == abc[i]).First().CardNumberFull;
                                                Utilities.setCardTrack2(CardNumberFull);
                                                string keyformat = @"4\1c000\1c\1c" + "002" + @"\1c00000000\1c" + Utilities.getSerialNumber() + @"5000\1c" + condition + "00";
                                                byte[] keyformatByte = Utilities.DCTCP2H_Send(keyformat);
                                                Utilities.LogFWToATM(keyformat, Encoding.ASCII.GetString(keyformatByte));
                                                socketATM.Send(keyformatByte);



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
                                    string CardNumber = Utilities.getCardTrack2();
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
                                        string dataStrWithdrawMoney = Utilities.convertToHex(dataWithdrawMoneyStr);
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
