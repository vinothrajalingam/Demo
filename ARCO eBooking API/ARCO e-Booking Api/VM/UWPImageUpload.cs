using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class UWPImageUpload
    {

        
        public String base64StringImage { get; set; }

       
        public String SalesOrderNumber
        {
            get;
            set;
        }
        
        public String CompanyNameandNumber
        {
            get;
            set;
        }

       
        public String FileName
        {
            get;
            set;
        }

    }
}