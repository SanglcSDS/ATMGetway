using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgribankDigital
{
    public class CustomerInfo
    {
        public string customerID { get; set; }
        public List<ListAccount> listAccount { get; set; }
        public string customerNumber { get; set; }
        public string customerName { get; set; }
        public string customerStatus { get; set; }
        public string customerMobile { get; set; }
        public string smsMobileNumber { get; set; }

    }
}
