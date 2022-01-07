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


namespace AgribankDigital
{
    public partial class Service1 : ServiceBase
    {

        private static string HOST_CLIENT = ConfigurationManager.AppSettings["ip_host"];
        private static int PORT_FORWARD = Int32.Parse(ConfigurationManager.AppSettings["port_listen"]);
        private static int PORT_CLIENT = Int32.Parse(ConfigurationManager.AppSettings["port_host"]);

        Thread listenerThread;

        TcpListener listener = null;
        Socket socketATM = null;
        Socket socketHost = null;
        Dictionary<int, string> asciiDictionary = new Dictionary<int, string>()
        {
            {1, "\\1"},// SOH
            {4, "\\4"},// EOT
            {8, "\\8"},// BS
            {12, "\\c"},// FF
            {14, "\\e"},// SO
            {15, "\\f"},// SI
            {19, "\\13"},// DC3
            {21, "\\15"},// NAK
            {27, "\\1b"},// ESC
            {28, "\\1c"}, // FS
            {29, "\\1b"},// GS
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


        protected void ListenerMethod()
        {
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
                        Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                        string dataStr = convertToHex(System.Text.Encoding.ASCII.GetString(data), asciiDictionary);
                               dataStr = formatCardNumber(dataStr, "\\1c;", "=", "?\\1c", "t11\\1c");
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
                        Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                        string dataStr = convertToHex(System.Text.Encoding.ASCII.GetString(data), asciiDictionary);
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

         string convertToHex(String str, Dictionary<int, string> asciiDictionary)
        {
            char[] charValues = str.ToCharArray();
            string hexOutput = "";

            foreach (char _eachChar in charValues)
            {
                int value = Convert.ToInt32(_eachChar);
                if (asciiDictionary.ContainsKey(value))
                {
                    hexOutput += asciiDictionary[value];
                }
                else
                {
                    hexOutput += _eachChar;
                }
            }
            return hexOutput;
        }

        string formatCardNumber(string data, string prefix, string middle, string surfix, string condition)
        {
            if (data.Substring(0, condition.Length).Equals(condition))
            {
                int phayIndex = data.IndexOf(prefix);
                int bangIndex = data.IndexOf(middle);
                int hoiIndex = data.IndexOf(surfix);

                data = data.Replace(data.Substring(phayIndex + prefix.Length, bangIndex - phayIndex - prefix.Length), xLenght(5, "*") + xLenght(bangIndex - 10 - phayIndex - prefix.Length, "X") + xLenght(5, "*"));
                data = data.Replace(data.Substring(bangIndex + middle.Length, hoiIndex - bangIndex - middle.Length), xLenght(7, "*") + xLenght(hoiIndex - 7 - bangIndex - middle.Length, "X"));

                return data;
            }
            return data;
        }

        string xLenght(int lenght, string character)
        {
            string result = "";
            for (int i = 0; i < lenght; i++)
            {
                result += character;
            }
            return result;
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
        }
    }
}
