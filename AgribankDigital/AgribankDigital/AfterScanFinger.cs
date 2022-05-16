using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgribankDigital
{
    class AfterScanFinger
    {
        public static bool IsCorrectNews(string mess)
        {
            if (mess == null || mess.Length < 1) return false;

            bool result = false;

            if (mess[2].Equals('4') && mess.Contains("795")) result = true;

            return result;
        }

        public static string GetListCardNumber(string mess)
        {
            int startIdx = mess.IndexOf("0*");
            int endIdx = mess.IndexOf(@"\1b(1\0f");

            string result = mess.Substring(startIdx + 2, endIdx - startIdx - 2);

            return result;
        }
       public static List<string> listCard;
        public static void DecodeCardNumber(string hexStr)
        {
            string ascii = Utilities.HEX2ASCII(hexStr);

            string[] arr = ascii.Split(';');

            if (arr.Length > 0)
            {
                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " List card number:");
                RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\AgribankDigital");
                versie4.Close();
                RegistryKey versie5 = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AgribankDigital");
                versie5.Close();
                Utilities.DeleteSubKeyLocalMachine("SOFTWARE", "AgribankDigital");
              //  Utilities.DeleteSubKeyCurrentUser("SOFTWARE", "AgribankDigital");
                listCard = new List<string>();
                foreach (string str in arr)
                {
                    if (str == null || str.Length == 0) continue;
                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Card Number:");
                    Logger.Log("> " + str);
                    listCard.Add(str);
                    Logger.LogCardNumber(str);
                }
                Utilities.addSubKeyLocalMachine(@"SOFTWARE\AgribankDigital", listCard);
             //   Utilities.addSubKeyCurrentUser(@"SOFTWARE\AgribankDigital", listCard);

            }
        }
    }
}
