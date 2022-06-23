using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgribankDigital
{
    class Host
    {
        public Socket socketHost;
        TcpClient tcpClient;
        public static List<Cards> listcard ;
     
        //   public bool isClosed = false;

        public Host()
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

        public void ReceiveDataFromHost(ATM atm)
        {
            while (true)
            {

                if (this.IsConnected())
                {
                    Byte[] data = Utils.ReceiveAll(socketHost);

                    if (data.Length > 0)
                    {
                        string dataStr = Encoding.ASCII.GetString(data);
                        string dataStrFormat = Utilities.convertToHex(dataStr);
                        Utilities.LogHostToFW(dataStrFormat, dataStr);

                        if (atm.IsConnected())
                        {
                            if (AfterScanFinger.IsCorrectNews(dataStr))
                            {
                                Utilities.setSerialNumber(dataStrFormat);
                                ATM.page = 1;
                                atm.isCheckFinger = true;
                                Utilities.LogHostToFW("Ban tin 4 - code 795", "Ban tin 4 - code 795");
                               listcard = AfterScanFinger.DecodeCardNumber(AfterScanFinger.GetListCardNumber(dataStrFormat));
                            
                                string cardMess = Utilities.formartMessCard(listcard, 1);

                                byte[] isdata = Utilities.DCTCP2H_Send(cardMess);
                                Utilities.LogFWToATM(cardMess, Encoding.ASCII.GetString(isdata));
                                atm.socketATM.Send(isdata);


                            }
                            else if (atm.isCheckFinger == true)
                            {
                                atm.isCheckFinger = false;
                                Utilities.LogFW("removet to Finger");
                                if (this.IsConnected())
                                {
                                    byte[] errData = Utilities.fingerErr(ATM.coordination21);
                                    Utilities.LogFWToATM(Utilities.fingerErrstring(ATM.coordination21), Encoding.ASCII.GetString(errData));
                                    atm.socketATM.Send(errData);
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

        public void Terminate()
        {
            try
            {
                if (socketHost != null)
                {
                    if (socketHost.Connected)
                        socketHost.Close();
                    if (tcpClient.Connected)
                        tcpClient.Close();

                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

        }
        public void Close()
        {
            try
            {
                if (socketHost != null)
                {
                    if (socketHost.Connected)
                        socketHost.Disconnect(true);
                    if (tcpClient.Connected)
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
