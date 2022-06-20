using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
      
        static Thread receiveDataAtmThread = null;
        static Thread receiveDataHostThread = null;
        static ATM atm = null;
        static Host host = null;
        WebSocket ws = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void bt_start_atm_Click(object sender, EventArgs e)
        {
            atm = new ATM();
    


            if (atm.IsConnected())
            {
                lb_atm.Text = "true";
            }
            else
            {
                lb_atm.Text = "false";
            }
        }

        private void bt_close_atm_Click(object sender, EventArgs e)
        {

            if (atm != null)
            {
                if (Utils.HAS_CONTROLLER)
                {
                    atm.closeFingerPrintZF1();
                }
                atm.Close();
            }
            if (receiveDataHostThread != null)
                receiveDataHostThread.Abort();

        }

        private void bt_start_host_Click(object sender, EventArgs e)
        {
            host = new Host();
            if (host.IsConnected())
            {
                lb_host.Text = "true";
            }
            else
            {
                lb_host.Text = "false";
            }

         
        }

        private void bt_close_host_Click(object sender, EventArgs e)
        {
            if (host != null)
            {
                host.Terminate();
            }
               
            if (receiveDataAtmThread != null)
                receiveDataAtmThread.Abort();

        }
   
        private void Form1_Load(object sender, EventArgs e)
        {
           
           

        }

        private void bt_start_zf1_Click(object sender, EventArgs e)
        {
             
            try
            {
                if (Utils.HAS_CONTROLLER)
                {
                    Utilities.LogFW("ZF1 is starting...");
                    atm.initFingerPrintZF1(host.socketHost, atm.socketATM);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogFW("err: " + ex.ToString());
                Utilities.LogFW("ZF1 start failed!");
            }

        }

        private void bt_Connec_Click(object sender, EventArgs e)
        {
            if (atm != null && host != null && atm.IsConnected() && host.IsConnected())
            {
                try
                {
                    Utilities.LogFW("Another Thread starting....");
                    receiveDataAtmThread = new Thread(new ThreadStart(() => atm.ReceiveDataFromATM(host)));
                    receiveDataAtmThread.Start();
                    receiveDataHostThread = new Thread(new ThreadStart(() => host.ReceiveDataFromHost(atm)));
                    receiveDataHostThread.Start();

                }
                catch (Exception ex)
                {
                    Utilities.LogFW(ex.Message);
                }

            }

        }

        private void btn_close_Thread_Click(object sender, EventArgs e)
        {
            if (receiveDataAtmThread != null)
                receiveDataAtmThread.Abort();
            if (receiveDataHostThread != null)
                receiveDataHostThread.Abort();

        }
    }
}
