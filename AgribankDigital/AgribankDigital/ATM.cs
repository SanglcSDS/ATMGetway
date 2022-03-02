using Dermalog.Imaging.Capturing;
using System;
using System.Net;
using System.Net.Sockets;
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
        FingerPrinZF1 fingerPrinZF1;

        public ATM()
        {
            Logger.Log("Waiting connect from ATM ...");
            listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            listener.Start();


            socketATM = listener.AcceptSocket();

            socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
            LingerOption lingerOption = new LingerOption(true, 5);
            socketATM.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

            listener.Stop();
            if (socketATM.Connected)
            {
                Logger.Log("Connected to ATM : " + socketATM.Connected);
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
        public void initFingerPrintCB100(Socket socketHost, Socket socketATM,string dataStr)
        {
            ws = new WebSocket("ws://192.168.42.129:8887");
            FingerPrintCB100 fingerPrint = new FingerPrintCB100(ws);
            fingerPrint.FingerPrintWorking(socketHost, socketATM, dataStr);
        }
        public void initFingerPrintZF1(Socket socketHost, Socket socketATM)
        {
            fingerPrinZF1 = new FingerPrinZF1();
            fingerPrinZF1._capDevice = DeviceManager.GetDevice(DeviceIdentity.FG_ZF1);
            fingerPrinZF1.socketATM = socketATM;
            fingerPrinZF1.socketHost = socketHost;
            fingerPrinZF1.InitializeDevice();

            fingerPrinZF1._capDevice.Start();

            //Không cho phép nhận vân tay
            this.fingerPrinZF1._capDevice.Freeze(true);

            //Không nháy đèn xanh
            fingerPrinZF1._capDevice.Property[PropertyType.FG_GREEN_LED] = 0;
        }
        public void reset()
        {
            isResetting = true;

            socketATM.Disconnect(true);

            Logger.Log("Waiting connect from ATM ...");
            listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            listener.Start();
            socketATM = listener.AcceptSocket();
            listener.Stop();
            if (socketATM.Connected)
            {
                Logger.Log("Connected to ATM : " + socketATM.Connected);
            }
        }

        public Socket createListener()
        {
            Logger.Log("Waiting connect from ATM ...");

            listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            listener.Start();
            var socketATM = listener.AcceptSocket();
            listener.Stop();
            if (socketATM.Connected)
            {
                Logger.Log("Connected to ATM : " + socketATM.Connected);
            }
            return socketATM;
        }

        public void ReceiveDataFromATM(Host host)
        {
            initFingerPrintZF1(host.socketHost, this.socketATM);
            while (true)
            {
                if (!this.isResetting && !host.isResetting)
                {
                    if (this.IsConnected())
                    {
                        Byte[] data = Utils.ReceiveAll(socketATM);
                        if (data.Length > 0)
                        {
                           
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                            dataStr = Utilities.formatCardNumber(dataStr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");
                         
                            if (dataStr.Contains("HBCI"))
                            {
                                if (Utils.HAS_CONTROLLER==true)
                                {
                                    fingerPrinZF1.dataStr = dataStr;

                                    //Cho phép nhận vân tay
                                    this.fingerPrinZF1._capDevice.Freeze(false);

                                    //Đèn xanh bật
                                    fingerPrinZF1._capDevice.Property[PropertyType.FG_GREEN_LED] = 1;
                                }
                                if(Utils.HAS_CONTROLLER == false)
                                {
                                    initFingerPrintCB100(host.socketHost, socketATM, dataStr);

                                }
                               
                            }
                            else
                            {
                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                                Logger.Log("> " + dataStr);

                                if (host.IsConnected())
                                {
                                    host.socketHost.Send(data);
                                   

                                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                    Logger.Log("> " + dataStr);
                                }
                            }


                        }
                    }
                }
            }
        }

        public void Close()
        {
            if (socketATM.Connected)
                socketATM.Close();
            listener.Stop();
        }

        public void Terminate()
        {
            if (socketATM.Connected)
                socketATM.Close();
            listener.Stop();
        }
    }
}
