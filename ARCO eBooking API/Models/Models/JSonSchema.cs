using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCO.EndPoint.Model
{
    public class JSonSchema
    {
        public String VERSION { get; set; }
        public List<SiteLocation> SiteLocation { get; set; }
        public List<Expedite> Expedite { get; set; }
        public List<InboundCollect> InboundCollect { get; set; }
        public List<ContainerType> ContainerType { get; set; }
        public List<Conditions> Conditions { get; set; }
        public List<PartRollLocation> PartRollLocation { get; set; }
        public List<Transfer> Transfer { get; set; }
        public List<NewRFIDChip> NewRFIDChip { get; set; }
        public List<RFIDScanned> RFIDScanned { get; set; }
        public List<TrimType> TrimType { get; set; }
        public List<ColorCode> ColorCode { get; set; }
        public List<IncomingCoverType> IncomingCoverType { get; set; }
        //public List<CoreTypes> CoreTypes { get; set; }
        public List<CoreMaterial> CoreMaterial { get; set; }
        //public List<WorkRequired> WorkRequired { get; set; }
        public List<CrownType> CrownType { get; set; }
        public List<FinishType> FinishType { get; set; }
        public List<GroovePattern> GroovePattern { get; set; }
        public List<GrooveShape> GrooveShape { get; set; }
        public List<CylorFlat> CylorFlat { get; set; }
        public List<DrillnType> DrillnType { get; set; }
        public List<BreatherHole> BreatherHole { get; set; }
        public List<PartsRemoval> PartsRemoval { get; set; }
        public List<PartsRemovalList> PartsRemovalList { get; set; }
        public List<PartsAssembly> PartsAssembly { get; set; }
        public List<PartsAssemblyList> PartsAssemblyList { get; set; }
        public List<BearingBoreStampedEndRepair> BearingBoreStampedEndRepair { get; set; }
        public List<BearingBoreNonStampedEndRepair> BearingBoreNonStampedEndRepair { get; set; }
        public List<CoreBodyShoulderRepairRequired> CoreBodyShoulderRepairRequired { get; set; }
        public List<Recenter> Recenter { get; set; }
        public List<MillKeywayRepair> MillKeywayRepair { get; set; }
        public List<GearRepair> GearRepair { get; set; }
        public List<FlushorDrain> FlushorDrain { get; set; }
        public List<Balancing> Balancing { get; set; }
        public List<NDTTesting> NDTTesting { get; set; }
        public List<PressureTesting> PressureTesting { get; set; }
        public List<ReverseEngineer> ReverseEngineer { get; set; }
        public List<Inspections> Inspections { get; set; }

    }


    public class SiteLocation
    {
        public string Location { get; set; }
        public int Id { get; set; }
    }

    public class Expedite
    {
        public string Description { get; set; }
        public int ID { get; set; }
    }

    public class InboundCollect
    {
        public string Description { get; set; }
        public int ID { get; set; }
    }

    public class ContainerType
    {
        public string Type { get; set; }
        public int ID { get; set; }
    }

    public class Conditions
    {
        public string Type { get; set; }
        public int ID { get; set; }
    }

    public class PartRollLocation
    {
        public string Type { get; set; }
        public int ID { get; set; }
    }

    public class Transfer
    {
        public string Location { get; set; }
        public int Id { get; set; }
    }

    public class NewRFIDChip
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    public class RFIDScanned
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    public class TrimType
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    public class ColorCode
    {
        public string ColorDesc { get; set; }
        public int Id { get; set; }
    }

    public class IncomingCoverType
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    //public class CoreTypes
    //{
    //    public string Type { get; set; }
    //    public int Id { get; set; }
    //}

    public class CoreMaterial
    {
        public string Material { get; set; }
        public int Id { get; set; }
    }

    //public class WorkRequired
    //{
    //    public string Description { get; set; }
    //    public int Id { get; set; }
    //}

    public class CrownType
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    public class FinishType
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }

    public class GroovePattern
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class GrooveShape
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class CylorFlat
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class DrillnType
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class BreatherHole
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PartsRemoval
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PartsRemovalList
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PartsAssembly
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PartsAssemblyList
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class BearingBoreStampedEndRepair
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class BearingBoreNonStampedEndRepair
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class CoreBodyShoulderRepairRequired
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class Recenter
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class MillKeywayRepair
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class GearRepair
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class FlushorDrain
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class Balancing
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class NDTTesting
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class PressureTesting
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class ReverseEngineer
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }

    public class Inspections
    {
        public string Description { get; set; }
        public int Id { get; set; }
    }
}
