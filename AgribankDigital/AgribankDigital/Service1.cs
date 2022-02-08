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
        static Thread atmThread;
        static Thread hostThread;
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

            atmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
            atmThread.Start();

            hostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
            hostThread.Start();

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
                        if (atmThread.IsAlive)
                        {
                            atmThread.Abort();
                            Console.WriteLine("ListenerThread Abort");
                        }

                        if (hostThread.IsAlive)
                        {
                            hostThread.Abort();
                            Console.WriteLine("ListenerThread Abort");
                        }

                        if (!host.IsConnected())
                        {
                            //atm.Close();
                            host.reset();
                            //atm = new ATM();
                            host.isResetting = false;
                        }
                        if (!atm.IsConnected())
                        {
                            atm.reset();
                            atm.isResetting = false;
                        }

                        atmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                        atmThread.Start();

                        hostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                        hostThread.Start();
                    }
                }
            }
        }
        
        protected override void OnStop()
        {
            socketHost.Disconnect(false);
            socketATM.Disconnect(true);

            if (tcpClient != null)
                tcpClient.Close();

            if (listener != null)
                listener.Stop();

            atmThread.Abort();
            hostThread.Abort();
            checkConnectionThread.Abort();

            ws.Close();
            fingerPrintThread.Abort();
        }
    }
}
