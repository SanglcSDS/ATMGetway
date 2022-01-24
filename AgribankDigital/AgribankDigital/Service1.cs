using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using WebSocketSharp;


namespace AgribankDigital
{
    public partial class Service1 : ServiceBase
    {
        private static string IP_WEBSOCKET = ConfigurationManager.AppSettings["ip_webSocket"];
        private static string HOST_CLIENT = ConfigurationManager.AppSettings["ip_host"];
        private static int PORT_FORWARD = Int32.Parse(ConfigurationManager.AppSettings["port_listen"]);
        private static int PORT_CLIENT = Int32.Parse(ConfigurationManager.AppSettings["port_host"]);
        private static string[] SEND_CHARACTER = ConfigurationManager.AppSettings["send_character"].Split(new char[] { ',' });
        private static string[] RECEIVE_CHARACTER = ConfigurationManager.AppSettings["receive_character"].Split(new char[] { ',' });
 
     
        Thread listenerThread;
        WebSocket ws;

        TcpListener listener = null;
        Socket socketATM = null;
        Socket socketHost = null;
        Dictionary<int, string> asciiDictionary = new Dictionary<int, string>()
        {
            {1, "\\1"},// SOH
            {2, "\\2"},// STX
            {3, "\\3"},// ETX
            {4, "\\4"},// EOT
            {5, "\\5"},// ENQ
            {6, "\\6"},// ACK
            {7, "\\7"},// BEL
            {8, "\\8"},// BS
            {9, "\\9"},// TAB
            {10, "\\0a"},// LF
            {11, "\\0b"},// VT
            {12, "\\0c"},// FF
            {13, "\\0d"},// CR
            {14, "\\0e"},// SO
            {15, "\\0f"},// SI
            {16, "\\10"},// DLE
            {17, "\\11"},// DC1
            {18, "\\12"},// DC2
            {19, "\\13"},// DC3
            {20, "\\14"},// DC4
            {21, "\\15"},// NAK
            {22, "\\16"},// SYN
            {23, "\\17"},// ETB
            {24, "\\18"},// CAM
            {25, "\\19"},// EM
            {26, "\\1a"},// SUB
            {27, "\\1b"},// ESC
            {28, "\\1c"},// FS
            {29, "\\1d"},// GS
            {30, "\\1e"},// RS
            {31, "\\1f"},//UE 
        };
  
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
            listenerThread = new Thread(new ThreadStart(ListenerMethod));
            listenerThread.Start();
        }

        void FingerPrintWorking(object state)
        {
            ws.OnMessage += (sender, e) =>
            {
                Logger.LogFingrprint(e.Data);
                if (e.Data.Contains("\"Status\":\"STOP\""))
                {
                    Thread.Sleep(1000);
                    ws.Send("FINGERPRINT");
                }
            };

            ws.OnError += (sender, e) =>
            {
                Logger.LogFingrprint("err:" + e.Message);
            };

            ws.Connect();
            ws.Send("FINGERPRINT");
        }

        protected void ListenerMethod()
        {
            ws = new WebSocket("ws://192.168.42.129:8887");

            ThreadPool.QueueUserWorkItem(FingerPrintWorking, null);
            try
            {
                Logger.Log("Service is started");

                listener = new TcpListener(IPAddress.Any, PORT_FORWARD);

                listener.Start();

                Logger.Log("Listening connect from ATM ...");

                socketATM = listener.AcceptSocket();

                Logger.Log("ATM connected: " + socketATM.Connected);

                //Tao ket noi toi Host
                Logger.Log("Connecting to Host ...");

                TcpClient tcpClient = new TcpClient(HOST_CLIENT, PORT_CLIENT);
                socketHost = tcpClient.Client;

                Logger.Log("Connected to Host : " + socketHost.Connected);

                if (socketATM.Connected && socketHost.Connected)
                {
                    //Gui/nhan data tu ATM - Host
                    ThreadPool.QueueUserWorkItem(ReceiveDataFromATM, null);
                    ThreadPool.QueueUserWorkItem(ReceiveDataFromHost, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message);
            }
        }

        byte[] ReceiveAll(Socket socket)
        {
            var buffer = new List<byte>();

            while (socket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1))
                {
                    buffer.Add(currByte[0]);
                }
            }

            return buffer.ToArray();
        }

        void ReceiveDataFromATM(object state)
        {
            try
            {
                while (true)
                {
                    Byte[] data = ReceiveAll(socketATM);
                    if (data.Length > 0)
                    {
                      //  Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                        string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), asciiDictionary, SEND_CHARACTER, @"\1c");
                               dataStr = Utilities.formatCardNumber(dataStr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");

                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
                        Logger.Log("> " + dataStr);

                        socketHost.Send(data);

                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                        Logger.Log("> " + dataStr);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message);
            }
        }

        void ReceiveDataFromHost(object state)
        {
            try
            {
                while (true)
                {
                    Byte[] data = ReceiveAll(socketHost);

                    if (data.Length > 0)
                    {
                    //    Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                        string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), asciiDictionary, RECEIVE_CHARACTER, @"\1c");
                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Host to FW:");
                        Logger.Log("< " + dataStr);

                        socketATM.Send(data);

                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                        Logger.Log("< " + dataStr);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message);
            }
        }
      
     
    
        protected override void OnStop()
        {
            if (socketATM != null)
                socketATM.Close();

            if (socketHost != null)
                socketHost.Close();

            if (listener != null)
                listener.Stop();

            listenerThread.Abort();
            ws.Close();
        }
    }
}
