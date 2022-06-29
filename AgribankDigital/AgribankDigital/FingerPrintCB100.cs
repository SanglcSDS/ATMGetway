using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgribankDigital
{
    public class FingerPrintCB100
    {
        private static int DeviceHandle;
        private static byte[] FingerBuf = new byte[256 * 360]; //Image Buffer
        private static string PubKeyFile = @"-----BEGIN RSA PRIVATE KEY-----
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

        public static string GetStringBase64()
        {
            string base64string = "";
            byte[] bmpFingerBuf = new byte[256 * 360 + 1078];
            try
            {

                DateTime startTime = DateTime.Now;
                while (true)
                {
                  int lRV = FpStdP4M1.FpStdP41M1_GetImage(0, FingerBuf);

                    if (DateTime.Now.Subtract(startTime).Seconds >= 15)
                    {
                        CloseCB100();
                        return base64string;
                    }
                    if (lRV != 1)
                    {

                        return base64string;
                    }
                    else
                    {
                        int AreaScore = FpStdP4M1.FpStdP41M1_IsFinger(0, FingerBuf);
                        if (AreaScore < 45)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                FpStdP4M1.zzRaw2Bmp(bmpFingerBuf, FingerBuf, 256, 360);
                MemoryStream ms = new MemoryStream(bmpFingerBuf);
                Image b = Image.FromStream(ms, true);
                Image cloneImage = new Bitmap(b);
                b.Dispose();
                CloseCB100();
                base64string = ImageToBase64String(cloneImage);
            }
            catch (Exception e)
            {
                Utilities.LogFW(e.Message);
            }
            return base64string;
        }
        public  static void CloseCB100()
        {
            try
            {
                FpStdP4M1.FpStdP41M1_CloseDevice(DeviceHandle);
            }
            catch (Exception ex)
            {
                Utilities.LogFW(ex.Message);
            }


        }
        public static void sendFingerCB100(Socket socketHost, Socket socketATM, string dataStr)
        {
            try
            {
                DeviceHandle = FpStdP4M1.FpStdP41M1_OpenDevice();
                if (DeviceHandle > 0)
                {
                    Utilities.LogFW("Open Device CB100 Successed");
                    string datafingerprint = GetStringBase64();

                    if (!datafingerprint.Equals(""))
                    {
                        Logger.LogFingrprint("Finger data:" + datafingerprint);
                        string signature = RSASignature.signature(PubKeyFile, datafingerprint);
                        Logger.LogFingrprint("signature data:" + signature);
                        Model fingerData = WeeFinger(signature, datafingerprint);
                        if (fingerData.code == 0)
                        {
                            string ReplaceDataStr = Utilities.FingerReplaceText(dataStr, fingerData.customerInfos.listAccount[0].accountNumber);
                            ReplaceDataStr = ReplaceDataStr.Remove(0, 2);

                            Byte[] data = Utilities.DCTCP2H_Send(ReplaceDataStr);

                            if (socketHost.Connected)
                            {
                                string dataStrs = Encoding.ASCII.GetString(data);
                                string dataStrFormat = Utilities.convertToHex(System.Text.Encoding.ASCII.GetString(data));
                                Utilities.LogFWToHost(dataStrFormat, dataStrs);
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
                else
                {
                    Utilities.LogFW("Open Device CB100 Failed");
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
                Utilities.LogFW(ex.Message);
            }
        }


        public static Model WeeFinger(string signature, string fingerData)
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
