using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AgribankDigital
{
    class ATM
    {
        public Socket socketATM;
        Socket socketHost;
        TcpClient tcpClient;
        TcpListener listener;
        public bool isResetting = false;

        public ATM()
        {
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

        public bool IsConnected()
        {
            try
            {
                return !(socketATM.Poll(Utils.CHECK_CONNECTION_TIMEOUT, SelectMode.SelectRead) && socketATM.Available == 0);
            }
            catch (SocketException) {
                Logger.Log("ATM not responding");
                return false;
            }
            catch (ObjectDisposedException) {
                Logger.Log("ATM not responding");
                return false;
            }
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
            while (true)
            {
                if (!this.isResetting && !host.isResetting)
                {
                    if (this.IsConnected())
                    {
                        Byte[] data = Utils.ReceiveAll(socketATM);
                        if (data.Length > 0)
                        {
                            Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                            string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                            dataStr = Utilities.formatCardNumber(dataStr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");

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

        public void Close()
        {
            if (socketATM.Connected)
                socketATM.Disconnect(true);
        }

        public void Terminate()
        {
            if (socketATM.Connected)
                socketATM.Disconnect(true);
            listener.Stop();
        }
    }
}
