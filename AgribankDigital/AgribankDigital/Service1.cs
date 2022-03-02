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
        static Thread receiveDataAtmThread = null;
        static Thread receiveDataHostThread = null;
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
     /*   public void initFingerPrint()
        {
            ws = new WebSocket("ws://192.168.42.129:8887");
            FingerPrint fingerPrint = new FingerPrint(ws);
            fingerPrint.FingerPrintWorking(null);
        }*/
        public void main()
        {
           /* fingerPrintThread = new Thread(new ThreadStart(initFingerPrint)); ;
            fingerPrintThread.Start();*/

            Logger.Log("Service is started");

            host = new Host();
            // Create new Thread for ATM
            atmThread = new Thread(new ThreadStart(initATM));
            atmThread.Start();

            // wait ATM connected
            while (true)
            {
                if (atm != null && host != null && atm.IsConnected() && host.IsConnected())
                {
                    Logger.Log("Another Thread starting....");
                    receiveDataAtmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                    receiveDataAtmThread.Start();

                    receiveDataHostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                    receiveDataHostThread.Start();
                   
                    checkConnectionThread = new Thread(new ThreadStart(checkConnection));
                    checkConnectionThread.Start();

                    break;
                }
                else continue; 
            }
            
        }

        public static void initATM()
        {
            Console.WriteLine("Thread ATM starting...");
            atm = new ATM();
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
                        // Close Thread receive data
                        Logger.Log("Check connection failed: ATM is connected " + atm.IsConnected() + " / Host is connected " + host.IsConnected());
                        if (receiveDataAtmThread.IsAlive)
                        {
                            Logger.Log("Receive data ATM aborting...");
                            receiveDataAtmThread.Abort();
                        }

                        if (receiveDataHostThread.IsAlive)
                        {
                            Logger.Log("Receive data Host aborting...");
                            receiveDataHostThread.Abort();
                        }

                        // Close ATM
                        atm.Close();
                        if (atmThread != null)
                        {
                            Logger.Log("Thread ATM aborting...");
                            atmThread.Abort();
                            atmThread = null;
                        }
                        atm.isResetting = true;

                        // Check Host 
                        if (host.CheckNetwork())
                        {
                            host.Close();

                            // reconnect Host
                            host.isResetting = true;
                            host = new Host();
                        }
                        else
                        {
                            while (!host.CheckNetwork())
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        atmThread = new Thread(new ThreadStart(initATM));
                        atmThread.Start();

                        host.isResetting = false;
                        atm.isResetting = false;

                        // wait ATM connected
                        while (true)
                        {
                            if (atm != null && host != null && atm.IsConnected() && host.IsConnected())
                            {
                                Logger.Log("Reconnect = true");
                                receiveDataAtmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                                receiveDataAtmThread.Start();
                                receiveDataHostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                                receiveDataHostThread.Start();

                                break;
                            }
                            else continue;
                        }
                    }
                }
                Thread.Sleep(Utils.CHECK_CONNECTION_DELAY);
            }
        }
        
        protected override void OnStop()
        {
            if (checkConnectionThread != null)
                checkConnectionThread.Abort();
            if (receiveDataAtmThread != null)
                receiveDataAtmThread.Abort();
            if (receiveDataHostThread != null)
                receiveDataHostThread.Abort();

            if (atm != null)
                atm.Terminate();
            if (host != null)
                host.Terminate();

            if (atmThread != null)
                atmThread.Abort();

            if (ws != null)
                ws.Close();
            if (fingerPrintThread != null)
                fingerPrintThread.Abort();

            mainThread.Abort();
        }
    }
}
