using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;


namespace AppAgribankDigital
{
    class Logger
    {
        private static string FILE_LOG = ConfigurationManager.AppSettings["trans_log"];
        private static string FIGNPRINT_LOG = ConfigurationManager.AppSettings["figrprint_log"];
        public static object _locked = new object();
        public static void Log(string message)
        {
            try
            {
                lock (_locked)
                {
                    string fileLog = PathLocation(FILE_LOG) + DateTime.Now.ToString("yyyyMMdd") + ".log";

                    string _message = string.Format("{0}{1}", message, Environment.NewLine);
                    File.AppendAllText(fileLog, _message);

                }


            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));
            }
        }
        public static void LogCardNumber(string message)
        {
            try
            {
                lock (_locked)
                {
                    string fileLog = PathLocation(FILE_LOG) + DateTime.Now.ToString("cardNumber") + ".txt";

                    string _message = string.Format("{0}{1}", message, Environment.NewLine);
                    File.AppendAllText(fileLog, _message);

                }


            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));
            }
        }
        public static void LogFingrprint(string message)
        {
            try
            {
                lock (_locked)
                {
                    string fileLog = PathLocation(FIGNPRINT_LOG) + DateTime.Now.ToString("yyyyMMdd") + ".log";

                    string _message = string.Format("{0}{1}", message, Environment.NewLine);
                    File.AppendAllText(fileLog, _message);

                }


            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));
            }
        }
        public static void LogRaw(string message)
        {
            try
            {
                lock (_locked)
                {
                    string fileLog = PathLocation(FILE_LOG) + DateTime.Now.ToString("yyyyMMdd") + "-LogRaw" + ".log";

                    string _message = string.Format("{0}{1}", message, Environment.NewLine);
                    File.AppendAllText(fileLog, _message);

                }


            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));
            }
        }

        public static void LogGeneral(string message)
        {
            try
            {
                lock (_locked)
                {
                    string fileLog = PathLocation(FILE_LOG) + DateTime.Now.ToString("yyyyMMdd") + "-LogGeneral" + ".log";

                    string _message = string.Format("{0}{1}", message, Environment.NewLine);
                    File.AppendAllText(fileLog, _message);

                }


            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));
            }
        }

        public static string PathLocation(string value)
        {
            try
            {
                if (Directory.Exists(value))
                {
                    return value;
                }
                DirectoryInfo di = Directory.CreateDirectory(value);
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("The process failed: {0}", ex.ToString()));

            }
            return value;



        }
    }
}
