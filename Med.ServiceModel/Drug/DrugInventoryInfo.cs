using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class DrugInventoryInfo
    {
        public int DrugId { get; set; }
        public int DrugUnitId { get; set; }
        public double FirstInventoryQuantity { get; set; }
        public double LastInventoryQuantity { get; set; }
        public double InPeriodInventoryQuantity { get { return LastInventoryQuantity - FirstInventoryQuantity; } }
        public double InPrice { get; set; }
        public double OutPrice { get; set; }
    }
}
