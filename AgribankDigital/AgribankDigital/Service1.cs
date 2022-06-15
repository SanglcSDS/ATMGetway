using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using WebSocketSharp;
using System.Text;
using Dermalog.Imaging.Capturing;

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
            /* if (Utils.HAS_CONTROLLER == false)
             {
                 setupRoute();
             }*/

            mainThread = new Thread(new ThreadStart(main));
            mainThread.Start();
        }

        public void setupRoute()
        {

            string strCmdText;
            //  strCmdText = @"/C ""route add 172.18.26.0 mask 255.255.255.0 172.18.5.5 metric 1 -p & route add 172.18.26.0 mask 255.255.255.0 172.18.5.6 metric 1 -p""";
            strCmdText = "/C \" route add 10.0.0.0 mask 255.0.0.0 " + Utils.IP_ATM + " metric 1  & route add 192.168.42.129 mask 255.255.255.0 " + Utils.IP_ATM + " metric 1 \"";
            Process p = new Process();
            p.StartInfo.FileName = "CMD.exe";
            p.StartInfo.Arguments = strCmdText;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
        }
        public void main()
        {
            Utilities.LogFW("Service is started");
            atm = new ATM();
            // wait ATM connected
            while (true)
            {
                if (atm.IsConnected())
                {
                    host = new Host();
                }

                if (atm != null && host != null && atm.IsConnected() && host.IsConnected())
                {
                    try
                    {
                        Utilities.LogFW("Another Thread starting....");
                        receiveDataAtmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                        receiveDataAtmThread.Start();
                        receiveDataHostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                        receiveDataHostThread.Start();
                        checkConnectionThread = new Thread(new ThreadStart(checkConnection));
                        checkConnectionThread.Start();
                        break;
                    }
                    catch (Exception e)
                    {
                        Utilities.LogFW(e.Message);
                    }

                }
                else continue;
            }
            // start ZF1
            try
            {
                if (Utils.HAS_CONTROLLER)
                {
                    Utilities.LogFW("ZF1 is starting...");
                    atm.initFingerPrintZF1(host.socketHost, atm.socketATM);
                }
            }
            catch (Exception e)
            {
                Utilities.LogFW("err: " + e.ToString());
                Utilities.LogFW("ZF1 start failed!");
            }

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
                        Utilities.LogFW("Check connection failed: ATM is connected " + atm.IsConnected() + " / Host is connected " + host.IsConnected());
                        if (receiveDataAtmThread.IsAlive)
                        {
                            Utilities.LogFW("Receive data ATM aborting...");
                            receiveDataAtmThread.Abort();
                        }

                        if (receiveDataHostThread.IsAlive)
                        {
                            Utilities.LogFW("Receive data Host aborting...");
                            receiveDataHostThread.Abort();
                        }
                        if (Utils.HAS_CONTROLLER)
                        {
                            atm.closeFingerPrintZF1();
                        }

                        // Close ATM

                        atm.Close();
                        atm.isResetting = false;
                        atm = new ATM();
                        atm.isResetting = true;
                        host.Close();
                        host.isResetting = false;
                        // wait ATM connected
                        while (true)
                        {
                            // reconnect Host
                            if (atm.IsConnected())
                            {
                                host = new Host();
                                host.isResetting = true;
                            }

                            if (atm != null && host != null && atm.IsConnected() && host.IsConnected())
                            {
                                Utilities.LogFW("Reconnect = true");
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
            try
            {
                if (atm != null)
                {
                    if (Utils.HAS_CONTROLLER)
                    {
                        atm.closeFingerPrintZF1();
                    }
                    atm.Close();
                }
                if (checkConnectionThread != null)
                    checkConnectionThread.Abort();
                if (receiveDataAtmThread != null)
                    receiveDataAtmThread.Abort();
                if (receiveDataHostThread != null)
                    receiveDataHostThread.Abort();
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
            catch (Exception e)
            {
                Utilities.LogFW(e.Message);
            }

        }
    }
}
