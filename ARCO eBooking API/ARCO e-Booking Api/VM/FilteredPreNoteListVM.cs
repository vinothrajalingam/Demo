using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class FilteredPreNoteListVM
    {
        public int PreNoteId { get; set; }
        public String CustomerNumber { get; set; }
        public String CustomerName { get; set; }
        public String Site { get; set; }
        public String Subject { get; set; }
        public Nullable<DateTime> StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public Nullable<DateTime> LastUpdatedDate { get; set; }
        public bool ?Status { get; set; }
        [Required]
        public int Page { get; set; }
        [Required]
        public int PageSize { get; set; }
        public string Sorting { get; set; }
        public string Direction { get; set; }
        public bool ?Deleted { get; set; }
        public String SaleOrderNumber { get; set; }
        public String CityNState { get; set; }
    }
}