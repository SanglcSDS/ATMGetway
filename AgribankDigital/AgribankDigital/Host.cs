using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgribankDigital
{
    class Host
    {
        Socket socketATM;
        Socket socketHost;
        TcpClient tcpClient;
        TcpListener listener;

        public Host()
        {
            ;
        }

        public Socket connect()
        {
            while (true)
            {
                try
                {
                    TcpClient newTcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    var socketHost = newTcpClient.Client;

                    if (socketHost.Connected)
                    {
                        Logger.Log("Connected to Host : " + socketHost.Connected);
                        return socketHost;
                    }
                    else
                    {
                        Logger.Log("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(1000);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                    Thread.Sleep(1000);
                }
            }
        }

        public Host(Socket socketATM, Socket socketHost, TcpClient tcpClient, TcpListener listener)
        {
            this.socketATM = socketATM;
            this.socketHost = socketHost;
            this.tcpClient = tcpClient;
            this.listener = listener;
        }

        public static Socket Connect(Socket socketHost, TcpClient tcpClient, string ipHost, int portHost)
        {
            Logger.Log("Connecting to Host ...");
            if (tcpClient != null && !tcpClient.Connected)
            {
                tcpClient.Close();
            }
            while (true)
            {
                TcpClient newTcpClient = new TcpClient(ipHost, portHost);
                socketHost = newTcpClient.Client;

                if (socketHost.Connected)
                {
                    Logger.Log("Connected to Host : " + socketHost.Connected);
                    return socketHost;
                }
            }
        }

        public void ReceiveDataFromHost(object state)
        {
            while (true)
            {
                if (socketATM.Connected && socketHost.Connected)
                {
                    if (socketHost.Poll(100, SelectMode.SelectRead) && socketHost.Available == 0)
                    {
                        Logger.Log("Cannot connect to Host, trying to connect");

                        if (!socketHost.Connected && socketATM.Connected)
                            socketATM.Disconnect(true);

                        socketHost = new Host().connect();

                        if (socketHost.Connected && !socketATM.Connected)
                        {
                            listener.Start();
                            socketATM = new ATM().createListener();
                        }
                    }
                    else
                    {
                        Byte[] data = Utils.ReceiveAll(socketHost);

                        if (data.Length > 0)
                        {
                            //    Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.RECEIVE_CHARACTER, @"\1c");
                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Host to FW:");
                            Logger.Log("< " + dataStr);
                            try
                            {
                                socketATM.Send(data);

                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                                Logger.Log("< " + dataStr);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Exception while connecting to ATM: " + ex.Message);
                                Logger.Log("Cannot connect to ATM, re-open new listener");

                                if (!socketHost.Connected && socketATM.Connected)
                                    socketATM.Disconnect(true);

                                Thread.Sleep(100);
                                //socketATM = new ATM().createListener();
                                socketHost = new Host().connect();

                                if (socketHost.Connected && !socketATM.Connected)
                                {
                                    listener.Start();
                                    socketATM = new ATM(listener, socketATM).createListener();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
