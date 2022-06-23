using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AgribankDigital
{
    public class Utilities
    {
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
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            PathLocation(sourcePath);
            PathLocation(targetPath);
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
            }
            catch (Exception e)
            {
                LogFW(e.Message);
            }

        }

        public static void LogATMToFW(string log, string Raw)
        {
            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " ATM to FW:");
            Logger.Log("> " + log);
            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + "ATM to FW:");
            Logger.LogRaw("> " + Raw);

        }
        public static void LogFWToATM(string log, string Raw)
        {
            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
            Logger.Log("> " + log);
            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + "FW to ATM:");
            Logger.LogRaw("> " + Raw);

        }
        public static void LogFWToHost(string log, string Raw)
        {
            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
            Logger.Log("> " + log);
            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + "FW to Host:");
            Logger.LogRaw("> " + Raw);

        }
        public static void LogHostToFW(string log, string Raw)
        {
            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Host to FW:");
            Logger.Log("> " + log);
            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + "Host to FW:");
            Logger.LogRaw("> " + Raw);

        }
        public static void LogFW(string log)
        {
            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff"));
            Logger.Log("> " + log);
            Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff"));
            Logger.LogRaw("> " + log);

        }

        public static string convertToHex(string str)
        {
            string indexstr = Ascii2Hex(str);
            try
            {
                int index1c = indexstr.IndexOf(@"\1c");

                string index = indexstr.Substring(index1c - 2, 2);

                string strNeedCheck = "23,22,12,11";
                string[] arrListStr = strNeedCheck.Split(',');


                if (arrListStr.Contains(index))
                {
                    indexstr = indexstr.Substring(index1c - 2);
                }
                else
                {
                    indexstr = indexstr.Substring(index1c - 1);

                }

                if (indexstr.Remove(0, 2).Equals("11"))
                {
                    indexstr = formatCardNumber(indexstr, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");
                }
            }
            catch (Exception ex)
            {
                LogFW(ex.Message);
            }



            return indexstr;

        }
        public static string formatCardNumber(string data, string prefix, string middle, string surfix, string condition, string textnumber)
        {
            if ((data.Length >= condition.Length))
            {
                if (data.Substring(0, condition.Length).Equals(condition))
                {
                    int prefixIndex = data.IndexOf(prefix);
                    int middleIndex = data.IndexOf(middle);
                    int surfixIndex = data.IndexOf(surfix);
                    if (prefixIndex > 0 && surfixIndex > 0)
                    {
                        string cardnumber1 = data.Substring(prefixIndex + prefix.Length, middleIndex - prefixIndex - prefix.Length);
                        string cardnumber2 = data.Substring(middleIndex + middle.Length, surfixIndex - middleIndex - middle.Length);
                        data = data.Replace(cardnumber1 + "=" + cardnumber2, xLenght(5, "*") + xLenght(cardnumber1.Length - 10, "X") + xLenght(5, "*") + "=" + xLenght(7, "*") + xLenght(cardnumber2.Length - 7, "X"));
                        if (data.IndexOf(cardnumber1) > 0 && data.IndexOf(cardnumber2) > 0)
                        {
                            data = data.Replace(data.Substring(data.LastIndexOf(cardnumber1), cardnumber2.Length + cardnumber1.Length + 1), xLenght(data.Substring(data.LastIndexOf(cardnumber1), cardnumber2.Length + cardnumber1.Length + 1).Length, "*"));
                        }
                        if (data.IndexOf(textnumber) > 0)
                        {
                            string intnumber = data.Substring(data.IndexOf(textnumber) + textnumber.Length);
                            data = data.Replace(intnumber.Substring(0, intnumber.IndexOf(@"\1c")), xLenght(intnumber.Substring(0, intnumber.IndexOf(@"\1c")).Length, "*"));

                        }
                    }
                    return data;
                }
            }

            return data;
        }

        public static string FingerReplaceText(string str, string character)
        {
            string strdata = "";
            try
            {


                int prefixIndex = str.IndexOf("");

                if (prefixIndex > 0)
                {
                    strdata = str.Substring(0, prefixIndex + 2) + character + str.Substring(prefixIndex + 2, str.Length - (prefixIndex + 2));
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message.ToString());
            }


            return strdata;
        }

        public static string formarHEX2ASCII(string str, string[] character)
        {
            foreach (string eachChar in character)
            {
                str = str.Replace(eachChar, HEX2ASCII(eachChar));

            }
            return str;

        }
        public static byte[] DCTCP2H_Send(string Msg)
        {
            byte[] array = new byte[0];
            try
            {
                byte[] array2 = new byte[2];
                Msg = Hex2Ascii(Msg);
                byte[] bytes = Encoding.ASCII.GetBytes(Msg);
                int length = Msg.Length;
                if (length <= 255)
                {
                    array2[0] = 0;
                    array2[1] = Convert.ToByte(length);
                }
                else
                {
                    int num = length / 255;
                    array2[1] = Convert.ToByte(length - num * 255 - 1);
                    array2[0] = Convert.ToByte(num);
                }
                array = new byte[array2.Length + bytes.Length];
                Buffer.BlockCopy(array2, 0, array, 0, array2.Length);
                Buffer.BlockCopy(bytes, 0, array, array2.Length, bytes.Length);
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);
            }
            return array;
        }

        public static string HEX2ASCII(string hex)
        {
            string res = String.Empty;

            for (int a = 0; a < hex.Length; a = a + 2)

            {
                string Char2Convert = hex.Substring(a, 2);

                int n = Convert.ToInt32(Char2Convert, 16);

                char c = (char)n;

                res += c.ToString();

            }

            return res;

        }


        public static string xLenght(int lenght, string character)
        {
            string result = "";
            for (int i = 0; i < lenght; i++)
            {
                result += character;
            }
            return result;
        }
        public static string Ascii2Hex(string Ascii)
        {
            try
            {
                Ascii = Ascii.Replace(" ", "\\00");
                Ascii = Ascii.Replace("\u0001", "\\01");
                Ascii = Ascii.Replace("\u0002", "\\02");
                Ascii = Ascii.Replace("\u0003", "\\03");
                Ascii = Ascii.Replace("\u0004", "\\04");
                Ascii = Ascii.Replace("\u0005", "\\05");
                Ascii = Ascii.Replace("\u0006", "\\06");
                Ascii = Ascii.Replace("\a", "\\07");
                Ascii = Ascii.Replace("\b", "\\08");
                Ascii = Ascii.Replace("\t", "\\09");
                Ascii = Ascii.Replace("\v", "\\0b");
                Ascii = Ascii.Replace("\f", "\\0c");
                Ascii = Ascii.Replace("\u000e", "\\0e");
                Ascii = Ascii.Replace("\u000f", "\\0f");
                Ascii = Ascii.Replace("\u0010", "\\10");
                Ascii = Ascii.Replace("\u0011", "\\11");
                Ascii = Ascii.Replace("\u0012", "\\12");
                Ascii = Ascii.Replace("\u0013", "\\13");
                Ascii = Ascii.Replace("\u0014", "\\14");
                Ascii = Ascii.Replace("\u0015", "\\15");
                Ascii = Ascii.Replace("\u0016", "\\16");
                Ascii = Ascii.Replace("\u0017", "\\17");
                Ascii = Ascii.Replace("\u0018", "\\18");
                Ascii = Ascii.Replace("\u0019", "\\19");
                Ascii = Ascii.Replace("\u001a", "\\1a");
                Ascii = Ascii.Replace("\u001b", "\\1b");
                Ascii = Ascii.Replace("\u001c", "\\1c");
                Ascii = Ascii.Replace("\u001d", "\\1d");
                Ascii = Ascii.Replace("\u001e", "\\1e");
                Ascii = Ascii.Replace("\u001f", "\\1f");
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);
            }
            return Ascii;
        }

        public static string Hex2Ascii(string Hex)
        {
            try
            {
                Hex = Hex.Replace("\\00", " ");
                Hex = Hex.Replace("\\01", "\u0001");
                Hex = Hex.Replace("\\02", "\u0002");
                Hex = Hex.Replace("\\03", "\u0003");
                Hex = Hex.Replace("\\04", "\u0004");
                Hex = Hex.Replace("\\05", "\u0005");
                Hex = Hex.Replace("\\06", "\u0006");
                Hex = Hex.Replace("\\07", "\a");
                Hex = Hex.Replace("\\08", "\b");
                Hex = Hex.Replace("\\09", "\t");
                Hex = Hex.Replace("\\0a", "\n");
                Hex = Hex.Replace("\\0b", "\v");
                Hex = Hex.Replace("\\0c", "\f");
                Hex = Hex.Replace("\\0e", "\u000e");
                Hex = Hex.Replace("\\0f", "\u000f");
                Hex = Hex.Replace("\\10", "\u0010");
                Hex = Hex.Replace("\\11", "\u0011");
                Hex = Hex.Replace("\\12", "\u0012");
                Hex = Hex.Replace("\\13", "\u0013");
                Hex = Hex.Replace("\\14", "\u0014");
                Hex = Hex.Replace("\\15", "\u0015");
                Hex = Hex.Replace("\\16", "\u0016");
                Hex = Hex.Replace("\\17", "\u0017");
                Hex = Hex.Replace("\\18", "\u0018");
                Hex = Hex.Replace("\\19", "\u0019");
                Hex = Hex.Replace("\\1a", "\u001a");
                Hex = Hex.Replace("\\1b", "\u001b");
                Hex = Hex.Replace("\\1c", "\u001c");
                Hex = Hex.Replace("\\1d", "\u001d");
                Hex = Hex.Replace("\\1e", "\u001e");
                Hex = Hex.Replace("\\1f", "\u001f");
                Hex = Hex.Replace(@"\5c", HEX2ASCII("5c"));
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);
            }
            return Hex;
        }





        public static byte[] fingerErr(string condination)
        {
            string baseMess = @"4\1c000\1c\1c158\1c00000000\1c00005000158\0fKHONG XAC DINH DUOC VAN TAY\1c$00\1c";
            string ascii = baseMess.Replace("$", condination);
            return (DCTCP2H_Send(ascii));
        }
        public static string fingerErrstring(string condination)
        {
            string baseMess = @"4\1c000\1c\1c158\1c00000000\1c00005000158\0fKHONG XAC DINH DUOC VAN TAY\1c$00\1c";
            string ascii = baseMess.Replace("$", condination);
            return ascii;
        }
        public static void setSerialNumber(string mess)
        {
            string str = @"4\1c000\1c\1c795\1c00000000\1c";
            RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            int index = mess.IndexOf(str) + str.Length;

            versie1.SetValue("SerialNumber", mess.Substring(index, 4));
            versie1.Close();
        }
        public static void setCardTrack2(string CardNumber)
        {
            RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
            RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Wincor Nixdorf\AgribankDigital\cardTrack2");
            versie3.SetValue("CardNumber", CardNumber);
            versie4.SetValue("CardNumber", CardNumber);
            versie3.Close();
            versie4.Close();
        }
        public static string getCardTrack2()
        {
            string CardNumber;
            RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
            CardNumber = versie3.GetValue("CardNumber").ToString();
            versie3.Close();
            return CardNumber;


        }
        public static void CleanCardTrack2()
        {
            RegistryKey versie3 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY + @"\cardTrack2");
            RegistryKey versie4 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Wincor Nixdorf\AgribankDigital\cardTrack2");
            versie3.SetValue("CardNumber", "");
            versie4.SetValue("CardNumber", "");
            versie4.Close();
            versie3.Close();
        }
        public static string getSerialNumber()
        {
            string SerialNumber ;
            RegistryKey versie2 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            SerialNumber = versie2.GetValue("SerialNumber").ToString();
            return SerialNumber;
        }
        public static string getcondition(string dataStrFormart)
        {
            string iscondition = @"\1c\1c\1c1";
            int index = dataStrFormart.IndexOf(iscondition) + iscondition.Length;
            return dataStrFormart.Substring(index, 1);
        }
        public static string getconditionHEX2(string dataStr)
        {
            string iscondition = Hex2Ascii(@"\1c\1c\1c1");
            int index = dataStr.IndexOf(iscondition) + iscondition.Length;
            return dataStr.Substring(index, 1);
        }

        public static string formartMessCard(List<Cards> listCard, int page)
        {

            string strCart = @"3\1c000\1c\1c210" + fomatStrCard(listCard, page) + @"\1c027\1c\0e914" + fomatStrCardNumber(listCard, page) + @"\1c0fL2\0fHA\0fJA\0fL@\0fO";

            return strCart;
        }
        public static string fomatStrCardNumber(List<Cards> listCard, int page)

        {
            string strCart = "";
            try
            {
                foreach (Cards item in listCard)
                {
                    if (item.index == page)
                    {
                        strCart = strCart + @"\0f" + item.KeyPosition + item.CardNumberFormat;
                    }
                }
                Utilities.LogFW("string  listCard: > " + strCart);
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);

            }


            return strCart;

        }
        public static string fomatStrCard(List<Cards> listCard, int page)
        {
            string strCard = "";
            try
            {
                int page1 = listCard.Where(n => n.index == 1).Count();
                int page2 = listCard.Where(n => n.index == 2).Count();

                if (page == 1)
                {

                    if (page1 <= 4)
                    {
                        strCard = xLenght(4, "0") + "1" + xLenght(4 - page1, "0") + xLenght(page1, "1");

                    }
                    else
                    {
                        strCard = xLenght(page1 - 4, "1") + xLenght(4 - (page1 - 4), "0") + "1" + xLenght(4, "1");
                    }


                }
                else
                {
                    if (page2 <= 4)
                    {
                        strCard = xLenght(4, "0") + "11" + xLenght(4 - page2, "0") + xLenght(page2 - 1, "1");

                    }
                    else
                    {
                        strCard = xLenght(page2 - 4, "1") + xLenght(4 - (page2 - 4), "0") + "1" + xLenght(4, "1");
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);

            }

            return strCard;
        }

    }
}
