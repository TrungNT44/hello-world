using Med.Common;
using Med.ServiceModel.Common;
using System;

namespace Med.ServiceModel.Report
{
    public class ReportByBaseItem: BaseItem
    {        

        /// <summary>
        /// Total amount
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Paid amount
        /// </summary>
        public double PaidAmount { get; set; }

        /// <summary>
        /// Later Paid amount
        /// </summary>
        public double LaterPaidAmount { get; set; }

        /// <summary>
        /// Debt amount
        /// </summary>
        public double DebtAmount { get; set; }

        /// <summary>
        /// Revenue
        /// </summary>
        public double Revenue { get; set; }

        /// <summary>
        /// Revenue
        /// </summary>
        public bool ReturnedItem { get; set; }

        public double ReturnedAmount { get; set; }

        public bool IsDebt { get; set; }
    }
}
