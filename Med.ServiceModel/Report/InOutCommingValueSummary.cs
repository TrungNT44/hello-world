using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Report
{
    public class InOutCommingValueSummary
    {
        #region In/out comming values - Tổng thu/chi
        /// <summary>
        /// II.1.1. Bán hàng hóa/dịch vụ
        /// </summary>
        public double DeliveryIncomingTotal { get; set; }
        /// <summary>
        /// II.1.2. Các nguồn thu khác
        /// </summary>
        public double OtherIncommingTotal { get; set; }

        /// <summary>
        /// II.2.1.  Mua hàng hóa/ dịch vụ
        /// </summary>
        public double ReceiptOutcomingTotal { get; set; } 
        /// <summary>
        /// II.2.2. Chi phí kinh doanh
        /// </summary>
        public double BusinessCostsTotal { get; set; }
        /// <summary>
        /// II.2.3. Các khoản chi khác
        /// </summary>
        public double OtherOutcomingTotal { get; set; }
        #endregion

        #region In/out due values - Tổng nợ nhập/bán
        /// <summary>
        /// I.4.1. Nợ bán hàng
        /// </summary>
        public double DeliveryDueTotal { get; set; }
        /// <summary>
        /// I.4.1. Các khoản nợ khác
        /// </summary>
        public double DeliveryOtherDueTotal { get; set; }

        /// <summary>
        /// I.5.1. Nợ mua hàng
        /// </summary>
        public double ReceiptDueTotal { get; set; }
        /// <summary>
        /// I.5.2. Các khoản nợ khác
        /// </summary>
        public double ReceiptOtherDueTotal { get; set; }
        #endregion

    }
}
