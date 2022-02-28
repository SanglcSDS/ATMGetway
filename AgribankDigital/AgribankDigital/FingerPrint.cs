﻿using Newtonsoft.Json;
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
    
        public void FingerPrintWorking(Socket socketHost, Socket socketATM, string dataStr)
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
                    Model fingerData = WeeFinger(str.Finger1);
                    if (fingerData.code == 0)
                    {
                        string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fingerData.customerInfos.customerNumber);
                        Byte[] data = Encoding.ASCII.GetBytes(ReplaceDataStr);
                        if (socketHost.Connected)
                        {
                            socketHost.Send(data);
                        }
                    }
                    else
                    {
                        if (socketATM.Connected)
                        {
                            socketATM.Send(Encoding.ASCII.GetBytes("Fp does not exist"));
                        }

                    }
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
        public Model WeeFinger(string fingerData)
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
