using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace AgribankDigital
{
    class ATM
    {
        Socket socketATM;
        Socket socketHost;
        TcpClient tcpClient;
        TcpListener listener;

        public ATM()
        {

        }

        public ATM(TcpListener listener, Socket socketATM)
        {
            this.listener = listener;
            this.socketATM = socketATM;
        }

        public ATM(Socket socketATM, Socket socketHost)
        {
            this.socketATM = socketATM;
            this.socketHost = socketHost;
            
        } 

        public Socket createListener()
        {
            Logger.Log("Waiting connect from ATM ...");
            var socketATM = listener.AcceptSocket();
            listener.Stop();
            if (socketATM.Connected)
            {
                Logger.Log("Connected to ATM : " + socketATM.Connected);
            }
            return socketATM;
        }

        public void ReceiveDataFromATM(object state)
        {
            while (true)
            {
                if (socketATM.Connected && socketHost.Connected)
                {
                    if (socketATM.Poll(100, SelectMode.SelectRead) && socketATM.Available == 0)
                    {
                        Logger.Log("Cannot connect to ATM, trying to connect");

                        socketHost = new Host().connect();

                        if (!socketHost.Connected && socketATM.Connected)
                            socketATM.Disconnect(true);

                        if (socketHost.Connected && !socketATM.Connected)
                        {
                            listener.Start();
                            socketATM = new ATM().createListener();
                        }
                    }
                    else
                    {
                        Byte[] data = Utils.ReceiveAll(socketATM);
                        if (data.Length > 0)
                        {
                            //  Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                            dataStr = Utilities.formatCardNumber(dataStr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");

                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                            Logger.Log("> " + dataStr);

                            try
                            {
                                socketHost.Send(data);

                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                                Logger.Log("> " + dataStr);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Exception while connecting to Host: " + ex.Message);
                                Logger.Log("Cannot connect to Host, trying to reconnect ...");

                                socketATM.Disconnect(true);
                                Thread.Sleep(100);

                                socketHost = new Host().connect();
                                if (socketHost.Connected && !socketATM.Connected)
                                {
                                    listener.Start();
                                    socketATM = createListener();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
