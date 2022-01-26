using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using WebSocketSharp;


namespace AgribankDigital
{
    public partial class Service1 : ServiceBase
    {
        static Thread listenerThread;
        WebSocket ws;

        static TcpListener listener = null;
        static TcpClient tcpClient = null;
        static Socket socketATM = null;
        static Socket socketHost = null;
  
        public Service1()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);

        }

        protected override void OnStart(string[] args)
        {
            listenerThread = new Thread(new ThreadStart(main));
            listenerThread.Start();
        }

        public static void startAll()
        {
            //Tao ket noi toi Host 
            socketHost = new Host().connect();

            //Tao cong lang nghe ket noi tu ATM
            listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            listener.Start();
            socketATM = new ATM(listener, socketATM).createListener();

            //Gui/nhan data tu ATM - Host
            ThreadPool.QueueUserWorkItem(new ATM(socketATM, socketHost).ReceiveDataFromATM, null);
            ThreadPool.QueueUserWorkItem(new Host(socketATM, socketHost, tcpClient, listener).ReceiveDataFromHost, null);
        }

        public static void stopAll()
        {
            socketATM.Close();
            socketHost.Close();
            listener.Stop();
            //tcpClient.Close();
            listenerThread.Abort();
        }

        public static void restartAll()
        {
            stopAll();
            Thread.Sleep(100);
            listenerThread = new Thread(new ThreadStart(main));
            listenerThread.Start();
        }

        static void main()
        {
            Logger.Log("Service is started");

            //Tao ket noi toi Host 
            socketHost = new Host().connect();

            //Tao cong lang nghe ket noi tu ATM
            listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            listener.Start();
            socketATM = new ATM(listener, socketATM).createListener();

            //Gui/nhan data tu ATM - Host
            ThreadPool.QueueUserWorkItem(new ATM(socketATM, socketHost).ReceiveDataFromATM, null);
            ThreadPool.QueueUserWorkItem(new Host(socketATM, socketHost, tcpClient, listener).ReceiveDataFromHost, null);
        }
        
        protected override void OnStop()
        {
            //SocketConnection.CloseSocket(socketATM);

            //if (tcpClient != null)
            //    tcpClient.Close();

            if (listener != null)
                listener.Stop();

            listenerThread.Abort();
            ws.Close();
        }
    }
}
