using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARCO.EndPoint.Model
{
    public class InboundnPackaging
    {
        public string ReceivedVia { get; set; }
        public string ProNumberIn { get; set; }
        public string InboundCollect{ get; set; }
        public string CollectCharges { get; set; }
        public string InboundWeight { get; set; }
        public string QauntityContainerReceived { get; set; }
        public string ContainerIdentifier { get; set; }
        public string ContainerType { get; set; }
        public string Condition { get; set; }
        public string RepairHours { get; set; }
        public string RepairMaterials { get; set; }
        public string AdditionalComments { get; set; }
        public string PartOrRollLocation { get; set; }
        

  }
}