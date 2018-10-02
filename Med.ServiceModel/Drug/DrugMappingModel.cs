using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class DrugMappingModel
    {
        public int MasterDrugID { get; set; }
        public decimal? MinInPrice { get; set; }
        public DateTime? InLastUpdateDate { get; set; }
        public decimal? MinOutPrice { get; set; }
        public DateTime? OutLastUpdateDate { get; set; }
    }
}
