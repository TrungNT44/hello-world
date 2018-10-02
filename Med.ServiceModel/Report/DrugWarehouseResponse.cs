using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Response;

namespace Med.ServiceModel.Report
{
    public class DrugWarehouseResponse : ResponseModel<DrugWarehouseItem>
    {
        public double FirsInventoryValueTotal { get; set; }
        public double ReceiptValueTotal { get; set; }
        public double DeliveryValueTotal { get; set; }
        public double LastInventoryValueTotal { get; set; }
    }
}
