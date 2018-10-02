using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Response;
using Med.Common;

namespace Med.ServiceModel.Report
{
    public class RevenueDrugSynthesisResponse: ResponseModel<RevenueDrugItem>
    {
        public double Total { get; set; }
        public double Revenue { get; set; }
        public double DeliveryTotal { get; set; }
        public double DebtTotal { get; set; }

        public bool HasDebtValue{ get { return DebtTotal > MedConstants.EspQuantity; } }
    }
}
