﻿using Dermalog.Imaging.Capturing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AgribankDigital
{
    public class FingerPrinZF1
    {
        public Device _capDevice { get; set; }
        public Socket socketHost { get; set; }
        public Socket socketATM { get; set; }
        public string dataStr { get; set; }
        public static string ImageToBase64String(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] arr = new byte[ms.Length];

            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();

            string strBase64 = Convert.ToBase64String(arr);

            return strBase64;
        }

        public void _capDevice_OnDetect(object sender, DetectEventArgs e)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);

                string _pathFolder = Path.Combine(directoryInfo.FullName, "FingerPrintImage");

                if (!Directory.Exists(_pathFolder))
                {
                    Directory.CreateDirectory(_pathFolder);
                }

                string filePath = Path.Combine(_pathFolder, "FingerPrint" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".bmp");
                e.Image.Save(filePath);
                this._capDevice.Stop();

                FpData str = JsonConvert.DeserializeObject<TestModel>(ImageToBase64String(filePath)).FpData;
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
              //  Console.WriteLine("Base64String: " + ImageToBase64String(filePath));


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void BindEvents()
        {
            this._capDevice.OnDetect += new OnDetect(_capDevice_OnDetect);
        }

        private void UnbindEvents()
        {
            this._capDevice.OnDetect -= new OnDetect(_capDevice_OnDetect);

        }

        public void InitializeDevice()
        {

            this._capDevice.CaptureMode = CaptureMode.ROLLED_FINGER;
            this._capDevice.Property[PropertyType.FG_FAKE_DETECT] = 1;

            this.BindEvents();
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