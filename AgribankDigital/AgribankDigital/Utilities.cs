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
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //Copy all the files & Replaces any files with the same name
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

        public static string convertToHex(string str, Dictionary<int, string> asciiDictionary, string[] character, string cindex)
        {
            char[] charValues = str.ToCharArray();
            string textOutput = "";

            foreach (char _eachChar in charValues)
            {
                int value = Convert.ToInt32(_eachChar);
                if (asciiDictionary.ContainsKey(value))
                {
                    textOutput += asciiDictionary[value];
                }
                else
                {
                    textOutput += _eachChar;
                }
            }

            for (var i = 0; i < character.Length; i++)
            {
                if (textOutput.Length > 1)
                {
                    int characterIndex = textOutput.Substring(0, textOutput.IndexOf(cindex) + 3).IndexOf(character[i]);
                    if (textOutput.Substring(0, textOutput.IndexOf(cindex) + 3).Equals(cindex))
                    {
                        textOutput = textOutput.Substring(cindex.Length, textOutput.Length - cindex.Length);
                        break;
                    }
                    if (characterIndex >= 0)
                    {
                        textOutput = textOutput.Substring(characterIndex, textOutput.Length - characterIndex);
                        break;

                    }
                }

            }
            if (textOutput.Remove(0, 2).Equals("11"))
            {
                textOutput = formatCardNumber(textOutput, @"\1c;", "=", @"?\1c", @"11\1c", @"A\1c000000000000\1c");
            }

            {
                return textOutput;
            }
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


        static string xLenght(int lenght, string character)
        {
            string result = "";
            for (int i = 0; i < lenght; i++)
            {
                result += character;
            }
            return result;
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



        public static string resizeMess(string mess)
        {
            int rawLen = mess.Length;

            string hexStr = rawLen.ToString("X");

            while (hexStr.Length < 4)
            {
                hexStr = hexStr.Insert(0, "0");
            }

            //   string replaceLen = HEX2ASCII(hexStr);

            return mess.Insert(0, HEX2ASCII(hexStr));
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
        public static void getSerialNumber(string mess)
        {
            string str = @"4\1c000\1c\1c795\1c00000000\1c";
            RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            int index = mess.IndexOf(str) + str.Length;

            versie1.SetValue("SerialNumber", mess.Substring(index, 4));
            versie1.Close();
        }
        public static string getCoordination(string mess, string condition)
        {
            RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            int index = mess.IndexOf(condition) + condition.Length;
            versie1.SetValue("condition", mess.Substring(index, 1));
            versie1.Close();
            return mess.Substring(index, 1);
        }
        public static string getcondition(string mess)
        {
            string iscondition = @"\1c\1c\1c1";
            int index = mess.IndexOf(iscondition) + iscondition.Length;
            return mess.Substring(index, 1);
        }
        public static string getconditionHEX2(string mess)
        {
            string iscondition = Hex2Ascii(@"\1c\1c\1c1");
            int index = mess.IndexOf(iscondition) + iscondition.Length;
            return mess.Substring(index, 1);
        }
        /*------------------------registry Editor-----------------------------*/
        public static List<string> addSubKeyLocalMachine(string stepart, List<string> listkey)
        {
            RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            versie1.SetValue("CountCard", listkey.Count.ToString());
            versie1.Close();
            List<string> listcard = new List<string>();
            for (int i = 0; i < listkey.Count; i++)
            {
                if (listkey[i].IndexOf("|") > 0)
                {
                    string[] tr = listkey[i].Split('|');
                    if (tr.Length >= 1)
                    {
                        RegistryKey Registrystepart = Registry.LocalMachine.CreateSubKey(stepart + "\\" + (i + 1));
                        string keysceen = tr[0].Substring(0, 6) + xLenght(3, "X") + tr[0].Substring(tr[0].Length - 4);
                        listcard.Add(keysceen);
                        string keystepart = ";" + tr[0] + "=" + tr[1] + "999" + tr[2] + "?";
                        Registrystepart.SetValue("CONTENTSFORMAT", keysceen);
                        Registrystepart.SetValue("CONTENTS", keystepart);
                        Registrystepart.Close();
                    }
                }

            }

            return listcard;
        }
        public static string getValueRegittry(string name)
        {
            RegistryKey versie2 = Registry.LocalMachine.CreateSubKey(Utils.REGISTRY);
            string key = versie2.GetValue(name).ToString();
            versie2.Close();
            return key;
        }
        public static void DeleteSubKeyLocalMachine(string stepart)
        {
            RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(stepart);
            versie1.Close();


            for (int i = 1; i <= 14; i++)
            {
                RegistryKey Registrystepart = Registry.LocalMachine.CreateSubKey(stepart + "\\" + i.ToString());
                Registrystepart.SetValue("CONTENTS", "");
                Registrystepart.SetValue("CONTENTSFORMAT", "");
                Registrystepart.Close();


            }
        }
        /* ------------------------registry Editor-----------------------------*/
        public static byte[] lengthMess(string mess)
        {
            byte[] result = new byte[0];
            mess = Hex2Ascii(mess);
            int rawLen = mess.Length;

            string hexStr = rawLen.ToString("X");

            while (hexStr.Length < 4)
            {
                hexStr = hexStr.Insert(0, "0");
            }

            mess.Insert(0, HEX2ASCII(hexStr));

            result = Encoding.ASCII.GetBytes(mess);
            return result;

        }
        public static string formartMessCard1(string mess, List<string> listCard)
        {
            string card = "";
            foreach (string item in listCard)
            {
                if (item != null)
                {
                    string[] itemCard = item.Split('|');

                    card = card + "-" + itemCard[0];
                }

            }
            card.Substring(0, 3);
            int startIdx = mess.IndexOf("0*");

            int endIdx = mess.IndexOf(HEX2ASCII("1b") + "(1" + HEX2ASCII("0f"));
            string textCard = mess.Substring(2, startIdx + 2) + card.Remove(0, 1) + mess.Substring(endIdx, mess.Length - endIdx);
            return resizeMess(textCard);

        }
        public static string formartMessCard(List<string> listCard, int iscancel)
        {
            List<string> stt = new List<string> { "FA", "IA", "LA", "OA", "F2", "I2", "L2", "O2" };
            //   string strCart = @"3\1c000\1c\1c210" + fomatStrCard(listCard.Count) + @"\1c036\1c" + fomatStrCardNumber(listCard, stt)+ @"\0c\1bPEC:\5cVBA_ncrpict_2007\5cVietnamese\5cv800.pcx\1b\5c";
            string strCart = @"3\1c000\1c\1c210" + fomatStrCard(listCard.Count, iscancel) + @"\1c027\1c\0e914" + fomatStrCardNumber(listCard, stt, iscancel) + @"\1c0fL2\0fHA\0fJA\0fL@\0fO";

            return strCart;
        }
        public static string fomatStrCardNumber(List<string> listCard, List<string> stt, int iscancel)
        {
            string strCart = "";
            if (iscancel == 1)
            {
                for (int i = 0; i < listCard.Count; i++)
                {
                    strCart = strCart + @"\0f" + stt[i] + listCard[i];
                }
                strCart = strCart + @"\0fO2NEXT";
                Logger.LogRaw("string  listCard.Count: > " + strCart);
            }
            else
            {

                for (int i = 0; i < listCard.Count; i++)
                {
                    strCart = strCart + @"\0f" + stt[i] + listCard[i];
                }
                Logger.LogRaw("string  listCard.Count: > " + strCart);

            }

            return strCart;

        }
        public static string fomatStrCard(int lenghtCard, int iscancel)
        {
            string strCard = "";

            if (iscancel == 1)
            {

                strCard = xLenght(9, "1");


            }
            else
            {
                if (lenghtCard <= 4)
                {
                    strCard = xLenght(4, "0") + "1" + xLenght(4 - lenghtCard, "0") + xLenght(lenghtCard, "1");

                }
                else
                {
                    strCard = xLenght(lenghtCard - 4, "1") + xLenght(4 - (lenghtCard - 4), "0") + "1" + xLenght(4, "1");
                }

            }


            return strCard;
        }
        public static List<string> listCard2(string part)
        {
            List<string> listCard2 = new List<string>();
            for (int i = 8; i <= 14; i++)
            {
                RegistryKey versie1 = Registry.LocalMachine.CreateSubKey(part + "\\" + i.ToString());
                string keycard = versie1.GetValue("CONTENTSFORMAT").ToString();

                if (!keycard.Equals(""))
                {
                    listCard2.Add(keycard);
                }
                versie1.Close();

            }
            return listCard2;
        }
    }
}
