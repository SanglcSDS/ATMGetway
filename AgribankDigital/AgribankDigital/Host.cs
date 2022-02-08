using System;
using System.Net.Sockets;
using System.Threading;

namespace AgribankDigital
{
    class Host
    {
        Socket socketATM;
        public Socket socketHost;
        TcpClient tcpClient;
        TcpListener listener;
        public bool isResetting = false;

        public Host()
        {
            while (true)
            {
                try
                {
                    TcpClient newTcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = newTcpClient.Client;

                    if (socketHost.Connected)
                    {
                        Logger.Log("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Logger.Log("Cannot connect to Host, trying to reconnect ...");
                        //Thread.Sleep(1000);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                    //Thread.Sleep(1000);
                }
            }
        }

        public void reset()
        {
            isResetting = true;

            //socketHost.Disconnect(true);

            //socketHost.Connect(Utils.IP_HOST, Utils.PORT_HOST);

            while (true)
            {
                try
                {
                    TcpClient newTcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = newTcpClient.Client;

                    if (socketHost.Connected)
                    {
                        Logger.Log("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Logger.Log("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(100);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                    //Thread.Sleep(1000);
                }
            }
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
                        //Thread.Sleep(1000);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                    //Thread.Sleep(1000);
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

        public bool IsConnected()
        {
            try
            {
                return !(socketHost.Poll(100, SelectMode.SelectRead) && socketHost.Available == 0);
            }
            catch (SocketException) { return false; }
            catch (ObjectDisposedException) { return false; }
        }

        public void ReceiveDataFromHost(object state)
        {
            ATM atm = (ATM)state;

            while (true)
            {
                if (!this.isResetting && !atm.isResetting)
                {
                    if (this.IsConnected())
                    {
                        Byte[] data = Utils.ReceiveAll(socketHost);

                        if (data.Length > 0)
                        {
                            //    Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.RECEIVE_CHARACTER, @"\1c");
                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Host to FW:");
                            Logger.Log("< " + dataStr);

                            if (atm.IsConnected())
                            {
                                atm.socketATM.Send(data);

                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                                Logger.Log("< " + dataStr);
                            }
                            //else
                            //{
                            //    atm.reset();
                            //    this.reset();

                            //    atm.isResetting = false;
                            //    this.isResetting = false;
                            //}
                        }
                    }
                    //else
                    //{
                    //    atm.reset();
                    //    this.reset();

                    //    atm.isResetting = false;
                    //    this.isResetting = false;
                    //}
                }
            }
        }

        public void Close()
        {
            Console.WriteLine("Close socket Host");
            socketHost.Disconnect(true);
            //tcpClient.Close();
        }
    }
}
