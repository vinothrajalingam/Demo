using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class FilteredOrderListVM
    {
        public int OrderId { get; set; }
        public String Customer{ get; set; }
        public String CustomerName { get; set; }
        public String Site { get; set; }
        public Nullable<DateTime> StartDate { get; set; }
        public Nullable<DateTime> LastUpdatedDate { get; set; }
        public Nullable<DateTime> LastStartDate { get; set; }
        public Nullable<DateTime> LastEndUpdate { get; set; }
        public String Status { get; set; }
        [Required]
        public int Page { get; set; }
        [Required]
        public int PageSize { get; set; }
        public string Sorting { get; set; }
        public string Direction { get; set; }
        public String SalesOrderNumber { get; set; }
        public String CityNState { get; set; }
        public String UserName { get; set; }
        public int OrderStatus { get; set; }
        public bool StatusForDate { get; set; }
        public int StatusForProgress { get; set; }
        public String Assignusername { get; set; }
        public int DateForSearch { get; set; }
    }
}