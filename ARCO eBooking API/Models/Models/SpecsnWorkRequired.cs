using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCO.EndPoint.Model
{
    public class SpecsnWorkRequired
    {
        public int ItemQuantity { get; set; }
        public string DrawingOrPrintNo { get; set; }
        public string CustStampedNo { get; set; }
        public string ARCoStampedNo { get; set; }
        public string Transfer { get; set; }
        public string IncomingCoverType { get; set; }
        public string Hardness { get; set; }
        public string MaterialColor { get; set; }
        public string CoreTypes { get; set; }
        public string CoreMaterial { get; set; }
        public string WorkRequired { get; set; }
        public string Comments { get; set; }


        public string NewRFIDChip { get; set; }
        public string RFIDScanned { get; set; }
        public decimal ActualCoreDia { get; set; }
        public decimal CoreBodyLength { get; set; }
        public decimal DiameterReceived { get; set; }
        public decimal CoverLength { get; set; }
        public decimal WorkingFace { get; set; }
        public decimal OAL { get; set; }
        public string TrimType { get; set; }
        public string TrimDimensions { get; set; }
        public string ColorCode { get; set; }
        public decimal CrownOnOD { get; set; }
        public decimal CrownFL { get; set; }
        public decimal FlatLength { get; set; }
        public string CrownNote { get; set; }
        public string CrownType { get; set; }
        public string FinishType { get; set; }
        public string FinishSpecification { get; set; }
        public string GroovePattern { get; set; }
        public string GrooveShape { get; set; }
        public string GrooveWidth { get; set; }
        public string GrooveDepth { get; set; }
        public string GrooveAngle { get; set; }
        public string GrooveDirection { get; set; }
        public string GrooveLandWidth { get; set; }
        public string GrooveNoStarts { get; set; }



        public string Coating { get; set; }
        public string CylorFlat { get; set; }
        public string PartNoSizenDescCoatings { get; set; }
        public int FinODOrWidth { get; set; }
        public int CoverLNOrLength { get; set; }
        public int OALOrHeight { get; set; }
        public string CYLSQINOrFLATSQIN { get; set; }

        public RepairsnOtherservices RepairsnotherServices { get; set; }
    }
}
