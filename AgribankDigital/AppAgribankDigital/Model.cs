using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppAgribankDigital
{
   public class Model
    {
        public int code { get; set; }
        public string message { get; set; }
        public CustomerInfo customerInfos { get; set; }
        public string signature { get; set; }

    }
}
