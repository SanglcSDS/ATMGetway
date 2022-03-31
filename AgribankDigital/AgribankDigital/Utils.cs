using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;

namespace AgribankDigital
{
    public static class Utils
    {
        public static string IP_WEBSOCKET = ConfigurationManager.AppSettings["ip_webSocket"];
        public static string IP_HOST= ConfigurationManager.AppSettings["ip_host"];
        public static int PORT_FORWARD = Int32.Parse(ConfigurationManager.AppSettings["port_listen"]);
        public static int PORT_HOST = Int32.Parse(ConfigurationManager.AppSettings["port_host"]);
        public static string[] SEND_CHARACTER = ConfigurationManager.AppSettings["send_character"].Split(new char[] { ',' });
        public static string[] RECEIVE_CHARACTER = ConfigurationManager.AppSettings["receive_character"].Split(new char[] { ',' });

        public static bool HAS_CONTROLLER = Boolean.Parse(ConfigurationManager.AppSettings["hasController"]);
        public static bool Test = Boolean.Parse(ConfigurationManager.AppSettings["Test"]);
        public static int CHECK_CONNECTION_TIMEOUT = Int32.Parse(ConfigurationManager.AppSettings["check_connection_timeout"]);
        public static int CHECK_CONNECTION_DELAY = Int32.Parse(ConfigurationManager.AppSettings["check_connection_delay"]);
        public static int RESET_ERR_DELAY = Int32.Parse(ConfigurationManager.AppSettings["reset_err_delay"]);
        public static int FINGER_PRINT_DELAY = Int32.Parse(ConfigurationManager.AppSettings["finger_print_delay"]);
        public static int SEND_DATA_TIMEOUT = Int32.Parse(ConfigurationManager.AppSettings["send_data_timeout"]);
        public static int TIMEOUT_API = Int32.Parse(ConfigurationManager.AppSettings["timeout_api"]);
        public static Dictionary<int, string> asciiDictionary = new Dictionary<int, string>()
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

        public static byte[] ReceiveAll(Socket socket)
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
    }
}
