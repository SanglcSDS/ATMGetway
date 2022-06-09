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

            if (mess[2].Equals('4') && mess.Substring(9, 3).Equals("795")) result = true;

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
        public static List<string> DecodeCardNumber(string hexStr)
        {
            List<string> listCardFomat = new List<string>();
            string ascii = Utilities.HEX2ASCII(hexStr);

            string[] arr = ascii.Split(';');

            if (arr.Length > 0)
            {
                Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " List card number:");
                Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " List card number:");


                Utilities.DeleteSubKeyLocalMachine(Utils.REGISTRY);

                listCard = new List<string>();
                foreach (string str in arr)
                {
                    if (str == null || str.Length == 0) continue;
                    Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Card Number:");
                    Logger.Log("> " + str);
                    Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Card Number:");
                    Logger.LogRaw("> " + str);
                    listCard.Add(str);
                }
                listCardFomat = Utilities.addSubKeyLocalMachine(Utils.REGISTRY, listCard);


            }
            return listCardFomat;
        }
    }
}
