using Dermalog.Imaging.Capturing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BmpToBase64
{
    public class ConsoleFingerPrint
    {
        public Device _capDevice { get; set; }

        /// <summary>
        /// Image convert to base64 string
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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
                this._capDevice.Freeze(true);
                
                Console.WriteLine("Base64String: " + ImageToBase64String(filePath));
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

        public void InitializeDevice()
        {

            this._capDevice.CaptureMode = CaptureMode.ROLLED_FINGER;
            this._capDevice.Property[PropertyType.FG_FAKE_DETECT] = 1;

            this.BindEvents();
        }
    }
}
