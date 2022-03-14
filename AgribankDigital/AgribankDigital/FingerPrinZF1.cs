using Dermalog.Imaging.Capturing;
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

        /// <summary>
        /// Chuyển từ image sang base64String
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string ImageToBase64String(Image image)
        {
            Bitmap bitmap = new Bitmap(image);

            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] arr = new byte[ms.Length];

            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();

            string strBase64 = Convert.ToBase64String(arr);

            return strBase64;
        }

        /// <summary>
        /// Bắt sự kiện chạm tay vào thiết bị lấy mẫu vân tay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void _capDevice_OnDetect(object sender, DetectEventArgs e)
        {
            try
            {
                //Lấy vân tay thành công, không cho phép nhận vân tay thêm
                this._capDevice.Freeze(true);

                //Đèn xanh tắt
                this._capDevice.Property[PropertyType.FG_GREEN_LED] = 0;

                Model fingerData = WeeFinger(ImageToBase64String(e.Image));
                Logger.LogFingrprint("Finger data:" + ImageToBase64String(e.Image));
                if (fingerData.code == 0)
                {
                    string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fingerData.customerInfos.customerNumber);
                    Byte[] data = Encoding.ASCII.GetBytes(ReplaceDataStr);
                    if (socketHost.Connected)
                    {
                        socketHost.Send(data);
                        Logger.Log("Finger to Host: " + ReplaceDataStr);
                    }
                }
                else
                {
                    if (socketATM.Connected)
                    {
                        socketATM.Send(Encoding.ASCII.GetBytes("Fp does not exist"));
                        Logger.Log("Finger to ATM: Fp does not exist");
                    }

                }
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
            //Model modelFinger = Http.GetModelFinger("http://192.168.1.149:8080/fake-api", new ModelFinger
            Model modelFinger = Http.GetModelFinger("http://10.0.7.23:8081/external/finger/identify", new ModelFinger
            {
                dpi = 508,
                fingerData = fingerData,

            });
            return modelFinger;
        }
    }
}
