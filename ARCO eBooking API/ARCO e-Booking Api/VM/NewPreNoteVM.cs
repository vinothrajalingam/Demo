using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class NewPreNoteVM
    {
        public string CustomerNumber { get; set; }
        public Nullable<int> SiteLocationId { get; set; }
        public Nullable<int> SalesOrderId { get; set; }
        public string Notify { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public String ShipToAddress { get; set; }
        public bool Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }
}