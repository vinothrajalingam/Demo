using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class FilteredOrdersListReturnVM
    {
        public int Pages { get; set; }

        public ICollection<FilteredOrderListVM> Orders { get; set; }
    }
}