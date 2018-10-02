using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Delivery
{
    public class DrugDeliveryInfo
    {
        public int DrugId { get; set; }
        public double QuantityTotal { get; set; }
        public double ValueTotal { get; set; }
    }
}
