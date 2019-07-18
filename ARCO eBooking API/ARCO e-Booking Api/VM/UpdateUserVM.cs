using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class UpdateUserVM
    {
        public String user { get; set; }
        public int defaultsite { get; set; }
        public bool admin { get; set; }
        public bool manager { get; set; }

        public List<SiteLocationVM> othersites { get; set; }
    }
}