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
        static Thread checkConnectionThread;
        static Thread fingerPrintThread;
        static ATM atm = null;
        static Host host = null;

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
            fingerPrintThread = new Thread(new ThreadStart(initFingerPrint));
            fingerPrintThread.Start();

            Logger.Log("Service is started");
            atm = new ATM();
            host = new Host();

            listenerThread = new Thread(new ThreadStart(main));
            listenerThread.Start();

            checkConnectionThread = new Thread(new ThreadStart(checkConnection));
            checkConnectionThread.Start();
        }

        public void initFingerPrint()
        {
            ws = new WebSocket("ws://192.168.42.129:8887");
            FingerPrint fingerPrint = new FingerPrint(ws);
            fingerPrint.FingerPrintWorking(null);
        }

        public static void checkConnection()
        {
            while (true)
            {
                if (atm == null || host == null) continue;
                else
                {
                    Console.WriteLine("Checking");
                    if (!atm.IsConnected() || !host.IsConnected())
                    {
                        Console.WriteLine("false");
                        if (listenerThread.IsAlive)
                        {
                            listenerThread.Abort();
                            Console.WriteLine("ListenerThread Abort");
                        }

                        if (!host.IsConnected())
                        {
                            host.reset();
                        }
                        if (!atm.IsConnected())
                        {
                            atm.reset();
                        }

                        Thread.Sleep(100);
                        listenerThread = new Thread(new ThreadStart(main));
                        listenerThread.Start();
                    }
                }

                Thread.Sleep(100);
            }
        }

        static void main()
        {
            //Tao cong lang nghe ket noi tu ATM
            //listener = new TcpListener(IPAddress.Any, Utils.PORT_FORWARD);
            //listener.Start();
            //socketATM = atm.createListener();

            //Tao ket noi toi Host 
            //socketHost = new Host().connect();

            //Gui/nhan data tu ATM - Host
            ThreadPool.QueueUserWorkItem(atm.ReceiveDataFromATM, host);
            ThreadPool.QueueUserWorkItem(host.ReceiveDataFromHost, atm);

            //Thread listenATMThread = new Thread(new ATM(socketATM, socketHost).ReceiveDataFromATM);
            //listenATMThread.Start();
            //Thread callHostThread = new Thread(new Host(socketATM, socketHost, tcpClient, listener).ReceiveDataFromHost);
            //callHostThread.Start();
        }
        
        protected override void OnStop()
        {
            socketHost.Disconnect(false);
            socketATM.Disconnect(true);

            if (tcpClient != null)
                tcpClient.Close();

            if (listener != null)
                listener.Stop();

            listenerThread.Abort();
            checkConnectionThread.Abort();

            ws.Close();
            fingerPrintThread.Abort();
        }
    }
}
