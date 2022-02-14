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
        static Thread mainThread = null;
        static Thread atmThread = null;
        static Thread hostThread = null;
        static Thread checkConnectionThread = null;
        static Thread fingerPrintThread = null;
        static ATM atm = null;
        static Host host = null;

        WebSocket ws = null;
  
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
            mainThread = new Thread(new ThreadStart(main));
            mainThread.Start();
        }

        public void main()
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
                    if (!atm.IsConnected() || !host.IsConnected())
                    {
                        if (atmThread.IsAlive)
                        {
                            atmThread.Abort();
                        }

                        if (hostThread.IsAlive)
                        {
                            hostThread.Abort();
                        }

                        if (!host.IsConnected())
                        {
                            atm.Close();
                            host.reset();

                            // Reconnect ATM 
                            atm.isResetting = true;
                            atm = new ATM();
                            
                            host.isResetting = false;
                            atm.isResetting = false;
                        }
                        if (!atm.IsConnected())
                        {
                            host.Close();
                            atm.reset();

                            // Reconnect Host
                            host.isResetting = true;
                            host = new Host();

                            atm.isResetting = false;
                            host.isResetting = false;
                        }

                        if (atm.IsConnected() && host.IsConnected())
                        {
                            atmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                            atmThread.Start();
                            hostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                            hostThread.Start();
                        }
                        Thread.Sleep(Utils.CHECK_CONNECTION_DELAY);
                    }
                }
            }
        }
        
        protected override void OnStop()
        {
            if (checkConnectionThread != null)
                checkConnectionThread.Abort();
            if (atmThread != null)
                atmThread.Abort();
            if (hostThread != null)
                hostThread.Abort();

            if (atm != null)
                atm.Terminate();
            if (host != null)
                host.Terminate();

            if (ws != null)
                ws.Close();
            if (fingerPrintThread != null)
                fingerPrintThread.Abort();
            if (mainThread != null)
                mainThread.Abort();
        }
    }
}
