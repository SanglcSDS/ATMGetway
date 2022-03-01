using System;
using System.Net.Sockets;
using System.Threading;

namespace AgribankDigital
{
    class Host
    {
        //Socket socketATM;
        public Socket socketHost;
        TcpClient tcpClient;
        //TcpListener listener;
        public bool isResetting = false;

        public Host()
        {
            while (true)
            {
                try
                {
                    TcpClient newTcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = newTcpClient.Client;

                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Utils.SEND_DATA_TIMEOUT);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                    LingerOption lingerOption = new LingerOption(false, 3);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

                    if (socketHost.Connected)
                    {
                        Logger.Log("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Logger.Log("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(Utils.RESET_ERR_DELAY);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                }
            }
        }

        public void reset()
        {
            isResetting = true;

            while (true)
            {
                try
                {
                    TcpClient newTcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = newTcpClient.Client;

                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Utils.SEND_DATA_TIMEOUT);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                    LingerOption lingerOption = new LingerOption(false, 3);
                    socketHost.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

                    if (socketHost.Connected)
                    {
                        Logger.Log("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Logger.Log("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(Utils.RESET_ERR_DELAY);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
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
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception while connecting to Host: " + ex.Message);
                    Logger.Log("Cannot connect to Host, trying to reconnect ...");
                }
            }
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
                bool check = !(socketHost.Poll(Utils.CHECK_CONNECTION_TIMEOUT, SelectMode.SelectRead) && socketHost.Available == 0);
                //if (!check)
                //    Logger.Log("Host not responding");
                return check;
            }
            catch (SocketException) {
                //Logger.Log("Host not responding");
                return false;
            }
            catch (ObjectDisposedException) {
                //Logger.Log("Host not responding");
                return false;
            }
        }

        public void ReceiveDataFromHost(ATM atm)
        {
            while (true)
            {
                if (!this.isResetting && !atm.isResetting)
                {
                    if (this.IsConnected())
                    {
                        Byte[] data = Utils.ReceiveAll(socketHost);

                        if (data.Length > 0)
                        {
                            //Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.RECEIVE_CHARACTER, @"\1c");
                            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Host to FW:");
                            Logger.Log("< " + dataStr);

                            if (atm.IsConnected())
                            {
                                atm.socketATM.Send(data);

                                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                                Logger.Log("< " + dataStr);
                            }
                        }
                    }
                }
            }
        }

        public void Close()
        {
            if (socketHost.Connected)
                socketHost.Disconnect(true);
            //if (tcpClient.Connected)
            //    tcpClient.Close();
        }

        public void Terminate()
        {
            if (socketHost.Connected)
                socketHost.Disconnect(true);
            if (tcpClient.Connected)
                tcpClient.Close();
        }
    }
}
