using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dermalog.Imaging.Capturing;

namespace BmpToBase64
{
    class Program
    {
        

        static void Main(string[] args)
        {
            try
            {
                ConsoleFingerPrint consoleFingerPrint = new ConsoleFingerPrint();
                consoleFingerPrint._capDevice = DeviceManager.GetDevice(DeviceIdentity.FG_ZF1);
                consoleFingerPrint.InitializeDevice();

                bool showMenu = true;
                while (showMenu)
                {
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1) Start");
                    Console.WriteLine("2) Continue");
                    Console.WriteLine("3) Exit");
                    Console.Write("\r\nSelect an option: ");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            consoleFingerPrint._capDevice.Start();
                            break;
                        case "2":
                            consoleFingerPrint._capDevice.Freeze(false);
                            break;
                        case "3":
                            consoleFingerPrint._capDevice.Stop();
                            showMenu = false;
                            break;
                        default:
                            break;
                    }
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
