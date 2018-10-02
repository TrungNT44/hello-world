using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Response;

namespace Med.ServiceModel.Report
{
    public class ReportByResponse : ResponseModel<ReportByBaseItem>
    {
        public double TotalAmount { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalPaidAmount { get; set; }
        public double TotalLaterPaidAmount { get; set; }
        public double TotalDebtAmount { get; set; }
    }
}
