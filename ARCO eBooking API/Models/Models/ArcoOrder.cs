using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.Model
{
    public class ArcoOrder
    {
        public OrderFramework OrderFramework { get; set; }
        public InboundnPackaging InboundnPackaging { get; set; }
        public List<SpecificationRepairs> SpecificationRepairs { get; set; }
        public VoiceNotesnPics VoiceNotesnPics { get; set; }        
    }
}