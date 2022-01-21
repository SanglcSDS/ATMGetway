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


        static string xLenght(int lenght, string character)
        {
            string result = "";
            for (int i = 0; i < lenght; i++)
            {
                result += character;
            }
            return result;
        }

    }
}
