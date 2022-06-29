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

            if (mess[2].Equals('4') && mess.Substring(9, 3).Equals("795") && mess.Contains(Utilities.Hex2Ascii(@"\1b") + "(0*")) result = true;

            return result;
        }

        public static string GetListCardNumber(string mess)
        {
            int startIdx = mess.IndexOf("0*");
            int endIdx = mess.IndexOf(@"\1b(1\0f");

            string result = mess.Substring(startIdx + 2, endIdx - startIdx - 2);

            return result;
        }
       
        /*   public static List<string> DecodeCardNumber(string hexStr)
           {
               List<string> listCardFomat = new List<string>();
               string ascii = Utilities.HEX2ASCII(hexStr);

               string[] arr = ascii.Split(';');

               if (arr.Length > 0)
               {
                   Utilities.LogFW(" List card number:");
                   Utilities.DeleteSubKeyLocalMachine(Utils.REGISTRY);

                   listCard = new List<string>();
                   foreach (string str in arr)
                   {
                       if (str == null || str.Length == 0) continue;
                       Utilities.LogFW(" Card Number:");
                       Logger.Log("> " + str);
                       Logger.LogRaw("> " + str);
                       listCard.Add(str);
                   }
                   listCardFomat = Utilities.addSubKeyLocalMachine(Utils.REGISTRY, listCard);


               }
               return listCardFomat;
           }
   */
        public static List<Cards> DecodeCardNumber(string hexStr)
        {
            string[] keyPage1 = { "I", "H", "G", "F", "A", "B", "C", "D" };
            string[] positionPage1 = { "FA", "IA", "LA", "OA", "F2", "I2", "L2", "O2" };

            string[] keyPage2 = { "I", "H", "G", "F", "A", "B", "C", "I", "H", "G", "A", "B", "C", "D" };
            string[] positionPage2 = { "FA", "IA", "LA", "OA", "F2", "I2", "L2", "FA", "IA", "LA", "F2", "I2", "L2", "O2" };
            List<Cards> listCardFomat = new List<Cards>();
            string ascii = Utilities.HEX2ASCII(hexStr);
            try
            {
                string[] arr = ascii.Split(';');
                Utilities.LogFW(" List card number:");

                if (arr.Length > 8)
                {

                    listCardFomat.Add(new Cards()
                    {
                        CardNumberFormat = "NEXT",
                        CardNumberFull = "",
                        index = 1,
                        KeyPosition = "O2",
                        LetterKey = "D",

                    });
                    listCardFomat.Add(new Cards()
                    {
                        CardNumberFormat = "BACK",
                        CardNumberFull = "",
                        index = 2,
                        KeyPosition = "OA",
                        LetterKey = "F",

                    }); ;
                    for (int i = 0; i < (arr.Length > 14 ? 14 : arr.Length); i++)
                    {
                        string[] tr = arr[i].Split('|');
                        string keysceen = tr[0].Substring(0, 6) + Utilities.xLenght(3, "X") + tr[0].Substring(tr[0].Length - 4);
                        string keystepart = ";" + tr[0] + "=" + tr[1] + "999" + tr[2] + "?";

                        Utilities.LogFW(" Card Number:" + arr[i]);
                        Cards itemcard = new Cards();
                        if (i <= 6)
                        {
                            itemcard.CardNumberFormat = keysceen;
                            itemcard.CardNumberFull = keystepart;
                            itemcard.LetterKey = keyPage2[i];
                            itemcard.KeyPosition = positionPage2[i];
                            itemcard.index = 1;
                            listCardFomat.Add(itemcard);
                        }
                        else
                        {
                            itemcard.CardNumberFormat = keysceen;
                            itemcard.CardNumberFull = keystepart;
                            itemcard.LetterKey = keyPage2[i];
                            itemcard.KeyPosition = positionPage2[i];
                            itemcard.index = 2;
                            listCardFomat.Add(itemcard);
                        }


                    }



                }
                else
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string[] tr = arr[i].Split('|');
                        string keysceen = tr[0].Substring(0, 6) + Utilities.xLenght(3, "X") + tr[0].Substring(tr[0].Length - 4);
                        string keystepart = ";" + tr[0] + "=" + tr[1] + "999" + tr[2] + "?";
                        Utilities.LogFW(" Card Number:" + arr[i]);
                        Cards itemcard = new Cards();
                        itemcard.CardNumberFormat = keysceen;
                        itemcard.CardNumberFull = keystepart;
                        itemcard.LetterKey = keyPage1[i];
                        itemcard.KeyPosition = positionPage1[i];
                        itemcard.index = 1;
                        listCardFomat.Add(itemcard);
                    }
                }

            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);

            }
            return listCardFomat;
        }
    }
}
