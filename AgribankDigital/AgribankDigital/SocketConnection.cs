using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace AgribankDigital
{
    class SocketConnection
    {
        public static Socket ConnectATM (Socket socketATM, TcpListener listener, int portListener)
        {
            if (listener != null)
            {
                listener.Stop();
            }
            Logger.Log("Listening connect from ATM ...");
            while (true)
            {
                listener.Start();
                if (socketATM.Connected)
                {
                    Logger.Log("ATM connected: " + socketATM.Connected);
                    return socketATM;
                }else
                {
                    listener.Stop();
                }
            }
        }

        public static Socket ConnectHost(Socket socketHost, TcpClient tcpClient, string ipHost, int portHost)
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

        public static void CloseSocket (Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

        public static void DisconnectSocket (Socket socket)
        {
            if (socket != null)
            {
                socket.Disconnect(true);
            }
        }
    }
}
