/*using Dermalog.Imaging.Capturing;
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
        string PubKeyFile = @"-----BEGIN RSA PRIVATE KEY-----
MIIEogIBAAKCAQEA1LvVBQ9eIfiIQuHA/HAbSHTUlZG0B1FTyvVSaY7JbUDiwyDv
MYMDq5jPe2Snbmvlxeav18yLNhNzNusaApvvzDE7gwgxALRod3WqymL1rYqqF2Bo
tjch1HdSBrJoikCqQ4ycInsdtfTUUe43M8BuN8GDS+vWlgUqYaew6F5IrGBmvC+U
KP7qJbGHJYAkrMz/w3Bgdb1xnzhF3Ec0vIRJOChfLflgmTqUPLrIkxwKgADlSH7k
5UPV0hBikwJNqcdzs/STW/ytlS3HBT31UlUjdLTA/GYVgY5kT/B2OK3TNchGdPrY
A1zum009Z81I8+tr17kLlEZeMPJD3Gw0soZ/1QIDAQABAoIBAFS3tJ5+PzuCESmp
Y8RkFMlnFV23F52uapMx3S8CWP70TxnsHLV3+lc01LPMIs1blLaaJr7myy2u0zw0
pjgRx45msM9+zJz+O3gPWQOeIz6IMTJ8B1dBX2yQVA8sr2sXU3bxISCuLOfQZ1l1
Z3BpmrsDinkKo3s89WLMeCwhmAlk5I6GRT/JtmyebXcTjTAk9WUFk5+GKisOmXxc
AadO4I0Z1v4ocYwiYioOz6JpelJv6msir/f2B46bd4Vs+NNRl7INRKXzrNyRX4QT
iFwkQzmk1zu7kd9Pht/D32MRDttOH218d9y7CO6fUY0xfHIt7l6OzDn1igJ4WIl5
6JCC4OECgYEA3mGY75EeGNgV9J65ZlIPTSuPJVfV5Px+FXDpx2Ka5mZbZKLzoqZ6
PYxguTOQhL+tKQtZSog+OYKfvMnP1pV6kW3wtWxvI54ZQ8Bw6nxG1KCuWK+YHVGk
TPgVxx3SttWiOCRid1leo5wB+GJQWugpoozvm97K1v+Cru+ztn1IGFkCgYEA9OTd
Z+fXBrz/eWazjhGV42Hpe84Nl0aqBNba1SUYic9LrZfPD4/H+gzZXJsGRb/prJLd
cEb4xAoe9+qFydpbCbBSwxjBqizFbvDEuF3uYG0XPZO5Ynn/FIj1qB8jUvK9z4DJ
cpezb3rDyef2kAWh8QhZpcSDDT/9F+JAFGf1890CgYBVgJT614MI7lxSt4x1SOvV
MgBRzVnSNzqLJ+Ta4pDIMWbGZNjkGro5W/X8f1T7lW8Qrupf+85g8lZUkgu1Z7e+
ntTEDLMWsLzqDd3cangZXMZsFueXrkJLzb8h1deksLM0ftjVJql6oosnYzWeHlGT
zDh8Z8b5rzgh5svkSHsl0QKBgGXg0DrpuAqVlbJrh0cTCcgOc5ONpRnJy/E3cNI7
HHo4QsN05C8VIZxkvAhKddGOhSfF8VlELTlg/IEmj4Hp1PWy5LtFEUw6U+hTQYNx
kDmNKJ31VqohFnz0fT3ztj5LvasVaLjDY2J9L/ZPCcPgk/4x+xl7JwncFO5asU3L
8gYBAoGAE41DUuRkwOPdbZlSqOnHVkCc0yBz0OAJbwcihqONrik6hafR7woL4tNG
k5oBEatimddD9xh7Rz+5NtZkodX2BNM+MRxVSOGsOmRiSM588CqIPxpYpYsqFIC7
IjPXul0tAoF40+TgfRc9geNEIubJP/rEp2Y7Yazu7TNuXnP1NDA=
-----END RSA PRIVATE KEY-----
";

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
            ATM.ThreadTimeoutFinger.Abort();
            try
            {
                //Lấy vân tay thành công, không cho phép nhận vân tay thêm
                this._capDevice.Freeze(true);

                //Đèn xanh tắt
                this._capDevice.Property[PropertyType.FG_GREEN_LED] = 0;
                string datafingerprint = ImageToBase64String(e.Image);
                Logger.LogFingrprint("Finger data:" + datafingerprint);
                string signature = RSASignature.signature(PubKeyFile, datafingerprint);
                Logger.LogFingrprint("signature data:" + signature);
                Model fingerData = WeeFinger(signature, datafingerprint);

               //  Model fingerData = WeeFingers(PubKeyFile, datafingerprint);

                if (fingerData.code == 0)
                {
                    string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fingerData.customerInfos.listAccount[0].accountNumber);
                    ReplaceDataStr = ReplaceDataStr.Remove(0, 2);
                    
                    Byte[] data = Utilities.DCTCP2H_Send(ReplaceDataStr);

                    if (socketHost.Connected)
                    {
                        string dataStr =  Encoding.ASCII.GetString(data);
                        string dataStrFormat = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data));
                        Utilities.LogFWToHost(dataStrFormat, dataStr);
                        socketHost.Send(data);

                    }
                }
                else
                {
                    if (socketATM.Connected)
                    {
                        string coordination = Utilities.getconditionHEX2(dataStr);
                        byte[] errData = Utilities.fingerErr(coordination);
                        Utilities.LogFWToATM(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                        socketATM.Send(errData);



                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message.ToString());
                string coordination = Utilities.getconditionHEX2(dataStr);
                byte[] errData = Utilities.fingerErr(coordination);
                Utilities.LogFWToATM(Utilities.fingerErrstring(coordination), Encoding.ASCII.GetString(errData));
                socketATM.Send(errData);
            }

        }

        public void CloseDevice()
        {
            try
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
            catch (Exception e)
            {
                Logger.Log(e.Message);
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
        public Model WeeFinger(string signature, string fingerData)
        {

            Model modelFinger = Http.GetModelFinger("http://10.0.7.23:8081/external/finger/identify", new ModelFinger
            {
                dpi = 508,
                signature = signature,
                fingerData = fingerData,

            });
            return modelFinger;
        }
        public Model WeeFingers(string signature, string fingerData)
        {
            ListAccount itemAccount = new ListAccount
            {
                branchCode = "1600",
                cif = "109184157",
                dpProductCode = "282",
                dpProductName = "",
                currencyCode = "VND",
                accountSequence = "006806",
                accountNumber = "1600282006677",
                accountStatus = "001",
                openDate = ""
            };
            Model modelFinger = new Model
            {
                code = 0,
                message = "",
                signature = signature,
                customerInfos = new CustomerInfo
                {
                    customerID = "629eac13eeb87d806100eff9",
                    customerNumber = "109184157",
                    customerName = "TÔ VIỆT PHƯƠNG",
                    customerStatus = "01",
                    customerMobile = "0984619940",
                    smsMobileNumber = "",
                    listAccount = new List<ListAccount> { itemAccount, }
                }

            };
            return modelFinger;
        }
    }
}
*/