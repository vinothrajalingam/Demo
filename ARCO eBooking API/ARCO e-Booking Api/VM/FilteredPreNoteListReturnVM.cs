using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.eCard.VM
{
    public class FilteredPreNoteListReturnVM
    {
        public int Pages { get; set; }

        public ICollection<FilteredPreNoteListVM> PreNotes { get; set; }
    }
}