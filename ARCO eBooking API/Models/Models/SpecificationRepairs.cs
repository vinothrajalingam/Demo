using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCO.EndPoint.Model
{
    public class SpecificationRepairs
    {    
        public SpecificationRepairs()
        {
            ARCOSpecification = new Model.Specification();
            ARCORepairs = new Repairs();
        }
        
        public string Comments { get; set; }
        public String Header { get; set; }
        public String SpecificationHeader { get; set; }
        public String RepairsHeader { get; set; }
        public Specification ARCOSpecification { get; set; }
        public Repairs ARCORepairs { get; set; }
    }
}
