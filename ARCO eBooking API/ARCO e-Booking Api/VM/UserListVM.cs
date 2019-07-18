using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class UserListVM
    {
        public String UserName { get; set; }
        public String Email { get; set; }
        public int SiteId { get; set; }
        public int Id { get; set; }
        [Required]
        public int Page { get; set; }
        [Required]
        public int PageSize { get; set; }
        public string Sorting { get; set; }
        public string Direction { get; set; }
        public String SiteName { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<DateTime> LastUpdatedDate { get; set; }
    }
}