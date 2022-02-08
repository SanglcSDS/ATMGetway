using System;
using System.Collections.Generic;
using System.Linq;
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
        public FingerPrint(WebSocketSharp.WebSocket ws) {
            this.ws = ws;
        }

        public void FingerPrintWorking(object state)
        {
            while (!ws.IsAlive)
            {
                ws.Connect();
                ws.Send("FINGERPRINT");
            }

            ws.OnMessage += (sender, e) =>
            {
                Logger.LogFingrprint(e.Data);
                if (e.Data.Contains("\"Status\":\"STOP\""))
                {
                    Thread.Sleep(1000);
                    ws.Send("FINGERPRINT");
                }
            };

            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("err: " + e.Message);
                Logger.LogFingrprint("err:" + e.Message);
            };

            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine("Disconnected");
                while (!ws.IsAlive)
                {
                    ws.Connect();
                    ws.Send("FINGERPRINT");
                }
            };
        }
    }
}
