using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AgribankDigital
{
    public abstract class FpStdP4M1
    {

        /************************************************************************************************************
        Function  : This function opens the device of Fingerprint Recognition Module
        Arguments : 
        Return    : 1   - successed
                others  - failed
        **************************************************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern int FpStdP41M1_OpenDevice();


        /************************************************************************************************************
        Function  : This function closes the device of Fingerprint Recognition Module
        Arguments : [in] Device serial number (starting from 0)
        Return    : none
        *************************************************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern void FpStdP41M1_CloseDevice(int device);

        /*************************************************************************************************************
        Function  : This function captures fingerprint image from device and outputs it.
        Arguments : device - [in] Device serial number (starting from 0)
                    image  - [out]  fingerprint image buffer
        Return    : 1      - successed
                    others - failed
        ***************************************************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern int FpStdP41M1_GetImage(int device, Byte[] image);

        /**************************************************************************
            Function:  convert the original image data to BMP format data
            Parameter: pBmp - Output, BMP format data (size: image data size + 1078)
                       pRaw - Output ，original image data
                       X    - Input，Image Width
                       Y    - Input，Image Height 
            Return:    1    - Successed
                      others- Failed
        ***************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern int zzRaw2Bmp(Byte[] pBmp, Byte[] pRaw, int X, int Y);

        /**************************************************************************
            Function:  Save original image data to bmp
            Parameter: strFileName - Input, Save Image path
                       pImage        - Output ，original image data
                       Width       - Input，Image Width
                       Height      - Input，Image Height 
            Return:    1           - Successed
                      others       - Failed
        ***************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern int SaveBMP(string strFileName, Byte[] pImage, int Width, int Height);


        /**************************************************************************
            Function:  Detect if the sensor has fingers
            Parameter: device - [in] Device serial number (starting from 0)
                       pImage  - Intput ,original image data    
            Return:    Image Area score,>45 -Has finger,else - No finger
        ***************************************************************************/
        [DllImport("FpStdP41M1.dll")]
        public static extern int FpStdP41M1_IsFinger(int device, Byte[] pImage);
    }
}
