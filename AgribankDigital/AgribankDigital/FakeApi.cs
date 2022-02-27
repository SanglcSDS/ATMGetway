using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace AgribankDigital
{
    class FakeApi
    {
        public static void fakeApi ()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8080/task/staff/fake-api");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"dpi\":\"test\"," +
                              "\"fingerData\":\"abc\"}";

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine("Result =-====> " + result);
            }
        }
    }
}
