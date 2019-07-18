using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.Model
{
    public class OrderFramework
    {
        //public DateTime DateReceived { get; set ; }
        DateTimeOffset _DateReceived = DateTime.Today;
        public DateTimeOffset DateReceived
        {
            get { return _DateReceived; }
            set { _DateReceived = value.Date; }
        }

        //public DateTime DateReceived { get; set ; }
        
        public String DateReceivedForExcel
        {
            get { return String.Format("{0:MM/dd/yyyy}", _DateReceived); }
            
        }
        public String SalesOrderNumber { get; set; }
        public string SiteLocation { get; set; }
        public string BilltoCompany { get; set; }
        public string CustomerNumberBillTo { get; set; }
        public string BillingAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ShiptoCompany { get; set; }
        public int CustomerNumberShipTo { get; set; }
        public string ShiptoAddress { get; set; }
        public string ShiptoCity { get; set; }
        public String ShiptoState { get; set; }
        public String ShiptoCountry { get; set; }
        public String PONumber { get; set; }
        public String RMANumber { get; set; }
        public string Expedite { get; set; }
        public int OrderQuantity { get; set; }
        public string CustomerServiceRepresentative { get; set; }
        public int PrenoteId { get; set; }
    }
}
