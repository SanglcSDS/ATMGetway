using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AppAgribankDigital
{
    class Host
    {
        //Socket socketATM;
        public Socket socketHost;
        TcpClient tcpClient;
        TcpListener listener;
        public bool isResetting = false;
        public bool isClosed = false;

        public Host()
        {
            while (true)
            {
                try
                {

                 /*   IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(Utils.IP_HOST), Utils.PORT_HOST);
                    listener = new TcpListener(localEP);
                    listener.Start();*/
                    tcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                //    tcpClient = listener.AcceptTcpClient();
                    socketHost = tcpClient.Client;
                 //   listener.Stop();
                    if (socketHost.Connected)
                    {
                        Utilities.LogFW("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(Utils.RESET_ERR_DELAY);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogFW("Exception while connecting to Host: " + ex.Message);
                    Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
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
                    tcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = tcpClient.Client;
                    if (socketHost.Connected)
                    {
                        Utilities.LogFW("Connected to Host : " + socketHost.Connected);
                        return;
                    }
                    else
                    {
                        Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
                        Thread.Sleep(Utils.RESET_ERR_DELAY);
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogFW("Exception while connecting to Host: " + ex.Message);
                    Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
                }
            }
        }

        public Socket connect()
        {
            while (true)
            {
                try
                {
                    tcpClient = new TcpClient(Utils.IP_HOST, Utils.PORT_HOST);
                    socketHost = tcpClient.Client;

                    if (socketHost.Connected)
                    {
                        Utilities.LogFW("Connected to Host : " + socketHost.Connected);
                        return socketHost;
                    }
                    else
                    {
                        Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
                        socketHost.Close();
                        tcpClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogFW("Exception while connecting to Host: " + ex.Message);
                    Utilities.LogFW("Cannot connect to Host, trying to reconnect ...");
                }
            }
        }

        public static Socket Connect(Socket socketHost, TcpClient tcpClient, string ipHost, int portHost)
        {
            Utilities.LogFW("Connecting to Host ...");
            if (tcpClient != null && !tcpClient.Connected)
            {
                tcpClient.Close();
            }
            while (true)
            {
                tcpClient = new TcpClient(ipHost, portHost);
                socketHost = tcpClient.Client;

                if (socketHost.Connected)
                {
                    Utilities.LogFW("Connected to Host : " + socketHost.Connected);
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

                return check;
            }
            catch (SocketException e)
            {
                Utilities.LogFW(e.Message.ToString());
                return false;
            }
            catch (ObjectDisposedException e)
            {
                Utilities.LogFW(e.Message.ToString());

                return false;
            }
        }

        public bool CheckNetwork()
        {
            Ping ping = new Ping();
            PingReply pingresult = ping.Send(Utils.IP_HOST, Utils.CHECK_CONNECTION_TIMEOUT);
            if (pingresult.Status.ToString() == "Success")
            {
                return true;
            }
            else
            {
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
                            string dataStr = Encoding.ASCII.GetString(data);
                            string dataStrFormat = Utilities.convertToHex(dataStr, Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                            Utilities.LogHostToFW(dataStrFormat, dataStr);

                            if (atm.IsConnected())
                            {
                                if (AfterScanFinger.IsCorrectNews(dataStr))
                                {
                                    Utilities.getSerialNumber(dataStrFormat);
                                    atm.isKeyD = true;
                                    atm.isCheckFinger = true;
                                    Utilities.LogHostToFW("Ban tin 4 - code 795", "Ban tin 4 - code 795");
                                    List<string> listcard = AfterScanFinger.DecodeCardNumber(AfterScanFinger.GetListCardNumber(dataStrFormat));

                                    if (listcard.Count > 8)
                                    {
                                        string cardMess = Utilities.formartMessCard(listcard.GetRange(0, 7), 1);
                                        byte[] isdata = Utilities.DCTCP2H_Send(cardMess);
                                        Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(isdata));
                                        atm.socketATM.Send(isdata);
                                    }
                                    else
                                    {
                                        string cardMess = Utilities.formartMessCard(listcard, 0);
                                        byte[] isdata = Utilities.DCTCP2H_Send(cardMess);
                                        Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(isdata));
                                        atm.socketATM.Send(isdata);
                                    }

                                }
                                else
                                {
                                    //  atm.isCheckFinger = false;
                                    atm.socketATM.Send(data);
                                    Utilities.LogFWToATM(dataStrFormat, dataStr);
                                }
                            }

                        }
                    }
                }
            }
        }

    /*    public void Close()
        {
            try
            {
                if (socketHost != null)
                {
                    if (socketHost.Connected)
                        socketHost.Disconnect(true);
                    if (tcpClient.Connected)
                        tcpClient.Close();

                    this.isClosed = true;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }


        }*/

        public void Terminate()
        {
            try
            {
                if (socketHost != null)
                {
                    if (socketHost.Connected)
                        socketHost.Close();
                   
                        tcpClient.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }
    }
}
