using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class UserListReturnVM
    {
        public int Pages { get; set; }

        public ICollection<UserListVM> Users { get; set; }
    }
}