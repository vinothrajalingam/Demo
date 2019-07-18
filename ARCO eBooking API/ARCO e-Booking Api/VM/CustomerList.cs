using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class CustomerList
    {
        public String CustomerNumber { get; set; }

        public String CompanyName { get; set; }

        public String Company
        {
            get { return $"{CustomerNumber}-{CompanyName}"; }
        }
    }
}