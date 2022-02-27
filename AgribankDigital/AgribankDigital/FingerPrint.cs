using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgribankDigital
{
    public class FingerPrint
    {
        FingerPrint()
        {

        }

        WebSocketSharp.WebSocket ws;
        public FingerPrint(WebSocketSharp.WebSocket ws)
        {
            this.ws = ws;
        }
        /*  public void FingerPrintWorking(Object socketHost)
          {
              while (!ws.IsAlive)
              {
                 *//* string str = File.ReadAllText("D:\\test.txt");

                  Model fingerData = WeeCheckFinger(str);*//*
                  ws.Connect();
                  ws.Send("FINGERPRINT");
              }

              ws.OnMessage += (sender, e) =>
              {

                  Logger.LogFingrprint(e.Data);
                  if (e.Data.Contains("\"Status\":\"DATA\""))
                  {

                      FpData str = JsonConvert.DeserializeObject<TestModel>(e.Data).FpData;
                      // string str = File.ReadAllText("D:\\test.txt");
                      Model fingerData = WeeCheckFinger(str.Finger1);
                      if (fingerData.code == 0)
                      {
                          Byte[] data = System.Text.Encoding.ASCII.GetBytes(fingerData.customerInfos.customerName);



                      }
                      *//* else if (fingerData.code == 4)
                       {
                           socketHost.Send("");
                       }*//*
                      else
                      {

                      }

                      //   Logger.LogFingrprint(e.Data);
                  }
                  if (e.Data.Contains("\"Status\":\"STOP\""))
                  {
                      Thread.Sleep(Utils.FINGER_PRINT_DELAY);
                      ws.Send("FINGERPRINT");
                  }
              };

              ws.OnError += (sender, e) =>
              {
                  Logger.LogFingrprint("err:" + e.Message);
              };

              ws.OnClose += (sender, e) =>
              {
                  Logger.LogFingrprint("Disconnected");
                  while (!ws.IsAlive)
                  {
                      ws.Connect();
                      ws.Send("FINGERPRINT");
                  }
              };

          }*/

        public void FingerPrintWorking(Socket socketHost)
        {
            while (!ws.IsAlive)
            {
               
                ws.Connect();
                ws.Send("FINGERPRINT");
            }

            ws.OnMessage += (sender, e) =>
            {

                Logger.LogFingrprint(e.Data);
                if (e.Data.Contains("\"Status\":\"DATA\""))
                {

                    FpData str = JsonConvert.DeserializeObject<TestModel>(e.Data).FpData;
                     // string str = File.ReadAllText("D:\\test.txt");
                     Model fingerData = WeeCheckFinger(str.Finger1);
                    if (fingerData.code == 0)
                    {
                        Byte[] data = System.Text.Encoding.ASCII.GetBytes(fingerData.customerInfos.customerName);
                        socketHost.Send(data);


                    }
                    else if (fingerData.code == 4)
                    {
                        Console.WriteLine("lo xac thuc van tay");
                    }
                    else
                    {
                        socketHost.Send(System.Text.Encoding.ASCII.GetBytes("Fp does not exist"));
                    }

                     //   Logger.LogFingrprint(e.Data);
                 }
                if (e.Data.Contains("\"Status\":\"STOP\""))
                {
                    Thread.Sleep(Utils.FINGER_PRINT_DELAY);
                    ws.Send("FINGERPRINT");
                }
            };

            ws.OnError += (sender, e) =>
            {
                Logger.LogFingrprint("err:" + e.Message);
            };

            ws.OnClose += (sender, e) =>
            {
                Logger.LogFingrprint("Disconnected");
                while (!ws.IsAlive)
                {
                    ws.Connect();
                    ws.Send("FINGERPRINT");
                }
            };

        }
        public Model WeeCheckFinger(string fingerData)
        {
            Model modelFinger = Http.GetModelFinger("http://10.0.7.23:8081/external/finger/identify", new ModelFinger
            {
                dpi = 508,
                fingerData = fingerData,

            });
            return modelFinger;
        }


    }
}
