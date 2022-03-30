using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgribankDigital
{
    public class Utilities
    {
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
            return textOutput;
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
            int prefixIndex = str.IndexOf("");

            if (prefixIndex > 0)
            {
                return str.Substring(0, prefixIndex + 2) + character + str.Substring(prefixIndex + 2, str.Length - (prefixIndex + 2));
            }

            return str;
        }
        public static string formarHEX2ASCII(string str, string[] character)
        {
            foreach (string eachChar in character)
            {
                str = str.Replace(eachChar, HEX2ASCII(eachChar));

            }
            return str;

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

        public static string INT2HEX(int intValue)
        {
            return intValue.ToString("X");
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

        public static string fingerErr(string coordination)
        {
            string baseMess = @"4\1c000\1c\1c158\1c00000000\1c00005000158\0fKHONG XAC DINH DUOC VAN TAY\1c$00\1c";

            string ascii = baseMess.Replace("$", coordination);
            ascii = ascii.Replace(@"\1c", HEX2ASCII("1c"));
            ascii = ascii.Replace(@"\0f", HEX2ASCII("0f"));

            return resizeMess(ascii);
        }

        public static string getCoordination(string mess, string condition)
        {
            int index = mess.IndexOf(condition) + condition.Length;
            
            return mess.Substring(index, 1);
        }
    }
}
