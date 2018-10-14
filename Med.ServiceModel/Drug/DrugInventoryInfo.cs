using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class DrugInventoryInfo
    {
        public int DrugID { get; set; }
        public int DrugUnitID{ get; set; }
        public double FirstInventoryQuantity { get; set; }
        public double LastInventoryQuantity { get; set; }
        public double InPeriodInventoryQuantity { get { return LastInventoryQuantity - FirstInventoryQuantity; } }
        public double InPrice { get; set; }
        public double OutPrice { get; set; }
        public DateTime NoteDate { get; set; }
        public double RetailPrice { get; set; }
        public double RetailOutPrice { get; set; }
        public double RetailQuantity { get; set; }
    }
}
