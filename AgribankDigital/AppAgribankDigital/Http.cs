using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AppAgribankDigital
{
    public class Http
    {
        public static Model GetModelFinger(string url, ModelFinger modelFinger)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Timeout = Utils.TIMEOUT_API;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(modelFinger));
                    streamWriter.Close();
                }
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string value = streamReader.ReadToEnd();

                    Logger.Log("API > " + value);
                    return JsonConvert.DeserializeObject<Model>(value);

                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex.Message.ToString());


            }
            return null;
           



        }


    }

}
