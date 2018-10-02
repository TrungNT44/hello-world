using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class DrugInfo
    {
        public int DrugId { get; set; }
        public int? UnitId { get; set; }
        public int? RetailUnitId { get; set; }
        public int Factors { get; set; }

        public decimal InPrice { get; set; }

        public decimal OutPrice { get; set; }

        public decimal OutPriceB { get; set; }

        public int UnitCode { get; set; }

        public string UnitName { get; set; }
    }
}
