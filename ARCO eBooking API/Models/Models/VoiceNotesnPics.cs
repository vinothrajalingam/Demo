using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;


namespace ARCO.EndPoint.Model
{
    public class VoiceNotesnPics
    {
        public VoiceNotesnPics()
        {
            ImageAttachments = new List<string>();
        }
        public List<String> ImageAttachments { get; set; }
        public String VoiceAndNotesComments { get; set; }
        public String PrenoteBody { get; set; }
        
        // public DateTime BookingCompletionDate { get; set; }
        DateTimeOffset _BookingCompletionDate = DateTime.Today;
        public DateTimeOffset BookingCompletionDate
        {
            get { return _BookingCompletionDate; }
            set { _BookingCompletionDate = value.Date; }
        }

        public String BookingCompletionDateForExcel
        {
            get { return String.Format("{0:MM/dd/yyyy}", _BookingCompletionDate); }

        }
    }
}