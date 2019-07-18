using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCO.EndPoint.Model
{
    public class Specification
    {
        public string SpecificationHeader { get; set; }
        public string ItemQuantity { get; set; }
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
        //public string Comments { get; set; }


        public string NewRFIDChip { get; set; }
        public string RFIDScanned { get; set; }
        public string ActualCoreDia { get; set; }
        public string CoreBodyLength { get; set; }
        public string DiameterReceived { get; set; }
        public string CoverLength { get; set; }
        public string WorkingFace { get; set; }
        public string OAL { get; set; }
        public string TrimType { get; set; }
        public string TrimDimensions { get; set; }
        public string ColorCode { get; set; }
        public string CrownOnOD { get; set; }
        public string CrownFL { get; set; }
        public string FlatLength { get; set; }
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
        public string FinODOrWidth { get; set; }
        public string CoverLNOrLength { get; set; }
        public string OALOrHeight { get; set; }
        public string CYLSQINOrFLATSQIN { get; set; }
    }
}
