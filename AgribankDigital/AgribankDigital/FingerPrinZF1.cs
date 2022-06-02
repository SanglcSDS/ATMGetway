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

                // fake success message
                string fakeAcc = "1600282002291";
                string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fakeAcc);
                ReplaceDataStr = ReplaceDataStr.Remove(0, 2);
                Byte[] data = Utilities.DCTCP2H_Send(ReplaceDataStr);
                if (socketHost.Connected)
                {
                    if (Utils.Test == true)
                    {
                       
                        string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                        Logger.Log("> " + dataStr);
                        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                        Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));
                        socketHost.Send(data);
                    }
                    else
                    {
                     //   string condition = @"\1c\1c\1c1";
                      //  string coordination = Utilities.getCoordination(dataStr, condition);
                      
                     //   string errData = Utilities.fingerErr(coordination);
                     /*   Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                        Logger.Log("> " + errData);
                        socketATM.Send(Encoding.ASCII.GetBytes(errData));*/
                    }


                }

                //Model fingerData = WeeFinger(ImageToBase64String(e.Image));
                //Logger.LogFingrprint("Finger data:" + ImageToBase64String(e.Image));
                //if (fingerData != null)
                //{
                //if (fingerData.code == 0)
                //{
                //    string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fingerData.customerInfos.listAccount[0].accountNumber);
                //    ReplaceDataStr = ReplaceDataStr.Remove(0, 2);
                //    ReplaceDataStr = Utilities.resizeMess(ReplaceDataStr);

                //    Byte[] data = Encoding.ASCII.GetBytes(ReplaceDataStr);
                //    if (socketHost.Connected)
                //    {
                //        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to Host:");
                //        Logger.LogRaw("> " + ReplaceDataStr);
                //        Logger.LogRaw("> " + System.Text.Encoding.ASCII.GetString(data));

                //        socketHost.Send(data);
                //        string dataStr = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c");
                //        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to Host:");
                //        Logger.Log("> " + dataStr);

                //    }
                //}
                //else
                //{
                //    if (socketATM.Connected)
                //    {
                //        Logger.LogRaw(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " Finger to ATM:");
                //        Logger.LogRaw("> " + dataStr);

                //        //socketATM.Send(Encoding.ASCII.GetBytes(Utilities.FingerReplaceTextErr(dataStr)));

                //        string condition = Utilities.HEX2ASCII(@"1c1c1c") + "1";
                //        string coordination = Utilities.getCoordination(dataStr, condition);
                //        string errData = Utilities.fingerErr(coordination);

                //        socketATM.Send(Encoding.ASCII.GetBytes(errData));

                //        // Logger.Log("FW to ATM:" + Utilities.convertToHex(Utilities.FingerReplaceTextErr(dataStr), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c"));
                //        Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                //        Logger.Log("> " + Utilities.convertToHex(Utilities.FingerReplaceTextErr(dataStr), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c"));
                //    }

                //}
                //    }
                //    else
                //    {
                //        if (socketATM.Connected)
                //        {
                //            //socketATM.Send(Encoding.ASCII.GetBytes(Utilities.FingerReplaceTextErr(dataStr)));
                //            //Logger.Log("FW to ATM:" + Utilities.convertToHex(Utilities.FingerReplaceTextErr(dataStr), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c"));

                //            string condition = Utilities.HEX2ASCII(@"1c1c1c") + "1";
                //            string coordination = Utilities.getCoordination(dataStr, condition);
                //            string errData = Utilities.fingerErr(coordination);

                //            socketATM.Send(Encoding.ASCII.GetBytes(errData));

                //            Logger.Log(Environment.NewLine + DateTime.Now.ToString("HH:mm:ss fff") + " FW to ATM:");
                //            Logger.Log("> " + Utilities.convertToHex(Utilities.FingerReplaceTextErr(dataStr), Utils.asciiDictionary, Utils.SEND_CHARACTER, @"\1c"));
                //        }
                //    }
            }
            catch (Exception ex)
            {
                if (socketATM.Connected)
                {
                    Logger.Log("FW to ATM:" + ex.Message.ToString());
                    //  socketATM.Send(Encoding.ASCII.GetBytes(Utilities.FingerReplaceTextErr(dataStr)));
                }
            }

        }

        public void CloseDevice()
        {
            if (this._capDevice != null)
            {
                if (this._capDevice.IsCapturing)
                {
                    this._capDevice.Stop();
                }
                this.UnbindEvents();
                this._capDevice.Dispose();
                _capDevice = null;
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
