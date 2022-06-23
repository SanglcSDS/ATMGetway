using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp;

namespace AppAgribankDigital
{
    public partial class Form1 : Form
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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            setupRoute();
            mainThread = new Thread(new ThreadStart(main));
            mainThread.Start();
        }
        public void setupRoute()
        {

            string strCmdText;
            //  strCmdText = @"/C ""route add 172.18.26.0 mask 255.255.255.0 172.18.5.5 metric 1 -p & route add 172.18.26.0 mask 255.255.255.0 172.18.5.6 metric 1 -p""";
            strCmdText = "/C \" route add 10.0.0.0 mask 255.0.0.0 " + Utils.IP_ATM + " metric 1  & route add 192.168.42.129 mask 255.255.255.0 " + Utils.IP_ATM + " metric 1 \"";
            Process p = new Process();
            //  p.Responding
            p.StartInfo.FileName = "CMD.exe";
            p.StartInfo.Arguments = strCmdText;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
        }
        public void main()
        {


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


                    // start ZF1
                    try
                    {
                        if (Utils.HAS_CONTROLLER)
                        {
                            Logger.Log("ZF1 is starting...");
                            atm.initFingerPrintZF1(host.socketHost, atm.socketATM);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log("err: " + e.ToString());
                    }

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

                        atm.closeFingerPrintZF1();

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
                        //if (host.CheckNetwork())
                        //{
                        host.Close();
                        // reconnect Host
                        host.isResetting = true;
                        host = new Host();
                        //}

                        //while (!host.CheckNetwork() && host.IsConnected())
                        //{
                        //    Logger.Log("Host network disconnected");
                        //    Thread.Sleep(1000);
                        //}

                        atmThread = new Thread(new ThreadStart(initATM));
                        atmThread.Start();
                        host.isResetting = false;
                        atm.isResetting = false;
                        // restart ZF1
                        try
                        {
                            if (Utils.HAS_CONTROLLER)
                            {
                                Logger.Log("ZF1 is starting...");
                                atm.initFingerPrintZF1(host.socketHost, atm.socketATM);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log("err: " + e.ToString());
                        }

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

        private void bt_Start_Click(object sender, EventArgs e)
        {

            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (atm != null)
                atm.closeFingerPrintZF1();
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
            if (mainThread != null)
                mainThread.Abort();

        }
    }
}
