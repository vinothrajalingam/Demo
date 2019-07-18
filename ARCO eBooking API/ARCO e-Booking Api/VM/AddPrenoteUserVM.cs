using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class AddPrenoteUserVM
    {
        public PrenotesUser user { get; set; }
        public int defaultsite { get; set; }
        public bool Admin { get; set; }
        public bool Manager { get; set; }

        public List<SiteLocationVM> othersites { get; set; }


    }
}