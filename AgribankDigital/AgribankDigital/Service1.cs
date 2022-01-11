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
            {2, "\\2"},// STX
            {3, "\\3"},// ETX
            {4, "\\4"},// EOT
            {5, "\\5"},// ENQ
            {6, "\\6"},// ACK
            {7, "\\7"},// BEL
            {8, "\\8"},// BS
            {9, "\\9"},// TAB
            {10, "\\a"},// LF
            {11, "\\b"},// VT
            {12, "\\c"},// FF
            {13, "\\d"},// CR
            {14, "\\e"},// SO
            {15, "\\f"},// SI
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
        Dictionary<int, string> characters = new Dictionary<int, string>()
        { {36, "\\24"},// $
          {37, "\\25"},// %
          {39, "\\27"},// '
          {43, "\\2b"},// +
          {61, "\\3d"},// =
          {63, "\\3f"},// ?
          {95, "\\5f"},// _
          {122,"\\7a"},// z

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
                      //  Logger.Log("Raw > " + System.Text.Encoding.ASCII.GetString(data));
                        string dataStr = convertToHex(System.Text.Encoding.ASCII.GetString(data), asciiDictionary);
                               dataStr = formatCardNumber(dataStr, "\\1c;", "=", "?\\1c", "11\\1c");
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

            for (var i = 0; i < charValues.Length; i++)
            {
                int value = Convert.ToInt32(charValues[i]);
                if (i == 0)
                {
                    if (asciiDictionary.ContainsKey(value)|| characters.ContainsKey(value))
                    {
                        hexOutput += "";
                    }
                }
                 else if (i == 1){
                    if (characters.ContainsKey(value))
                    {
                        hexOutput += "";
                    }
                    else
                    {
                        if (asciiDictionary.ContainsKey(value))
                        {
                            hexOutput += asciiDictionary[value];
                        }
                        else
                        {
                            hexOutput += charValues[i];
                        }

                    }
                }
                else
                {
                    if (asciiDictionary.ContainsKey(value))
                    {
                        hexOutput += asciiDictionary[value];
                    }
                    else
                    {
                        hexOutput += charValues[i];
                    }

                }

           
            }
         
            return hexOutput;
        }


        string formatCardNumber(string data, string prefix, string middle, string surfix, string condition)
        {
           // dataStr = formatCardNumber(dataStr, "\\1c;", "=", "?\\1c", "11\\1c");
            if (data.Substring(0, condition.Length).Equals(condition))
            {
                int phayIndex = data.IndexOf(prefix);
                int bangIndex = data.IndexOf(middle);
                int hoiIndex = data.IndexOf(surfix);
                string cardnumber1 = data.Substring(phayIndex + prefix.Length, bangIndex - phayIndex - prefix.Length+1);
                string cardnumber2 = data.Substring(bangIndex + middle.Length-1, hoiIndex - bangIndex - middle.Length);

            
              
                Console.WriteLine(cardnumber1);
                Console.WriteLine(cardnumber2);

                data = data.Replace(data.Substring(phayIndex + prefix.Length, bangIndex - phayIndex - prefix.Length+1), xLenght(5, "*") + xLenght(bangIndex - 10 - phayIndex - prefix.Length, "X") + xLenght(5, "*")+"=");
                data = data.Replace(data.Substring(bangIndex + middle.Length-1, hoiIndex - bangIndex - middle.Length), "="+xLenght(7, "*") + xLenght(hoiIndex - 6 - bangIndex - middle.Length, "X"));
                int bangbangIndex = data.IndexOf("==\\1c");

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
