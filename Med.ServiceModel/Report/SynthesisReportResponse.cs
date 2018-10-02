using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Response;

namespace Med.ServiceModel.Report
{
    public class SynthesisReportResponse : ResponseModel<Object>
    {
        // I.

        /// <summary>
        ///  I.1. Tổng giá trị nhập kho
        /// </summary>
        public double ReceiptValueTotal { get; set; }

        /// <summary>
        /// I.2. Tổng giá trị xuất kho
        /// </summary>
        public double DeliveryValueTotal { get; set; }

        /// <summary>
        /// I.3. Tổng giá trị kho hàng
        /// </summary>
        public double WarehousingValueTotal { get; set; }


        /// <summary>
        /// I.4. Tổng nợ khách hàng = I.4.1 + I.4.2
        /// </summary>
        public double DeliveryDueFromCustomersTotal 
        {
            get { return DeliveryDueTotal + DeliveryOtherDueTotal; }
        }
        /// <summary>
        /// I.4.1. Nợ bán hàng
        /// </summary>
        public double DeliveryDueTotal { get; set; }
        /// <summary>
        /// I.4.1. Các khoản nợ khác
        /// </summary>
        public double DeliveryOtherDueTotal { get; set; }
        
        /// <summary>
        ///  I.5. Tổng nợ nhà cung cấp = I.5.1 + I.5.2
        /// </summary>
        public double ReceiptDueFromSupplyersTotal
        {
            get { return ReceiptDueTotal + ReceiptOtherDueTotal; }
        } 
        /// <summary>
        /// I.5.1. Nợ mua hàng
        /// </summary>
        public double ReceiptDueTotal { get; set; }
        /// <summary>
        /// I.5.2. Các khoản nợ khác
        /// </summary>
        public double ReceiptOtherDueTotal { get; set; }

        /// <summary>
        ///  I.6. TÀI SẢN HIỆN CÓ
        /// </summary>
        public double ExistingAssetsTotal
        {
            get {return WarehousingValueTotal + DeliveryDueFromCustomersTotal - ReceiptDueFromSupplyersTotal; }
        } 
       


        // II.

        /// <summary>
        /// II.1. Tổng thu = II.1.1 + II.1.2
        /// </summary>
        public double RevenueTotal
        {
            get { return DeliveryIncomingTotal + OtherIncommingTotal; }
        }
        /// <summary>
        /// II.1.1. Bán hàng hóa/dịch vụ
        /// </summary>
        public double DeliveryIncomingTotal { get; set; } 
        /// <summary>
        /// II.1.2. Các nguồn thu khác
        /// </summary>
        public double OtherIncommingTotal { get; set; }

        /// <summary>
        /// II.2. Tổng chi = II.2.1 + II.2.2 + II.2.3
        /// </summary>
        public double ExpenditureTotal // Tổng chi
        {
            get { return ReceiptOutcomingTotal + OtherOutcomingTotal + BusinessCostsTotal; }
        }
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

        /// <summary>
        /// II.3. Lãi gộp = II.3.1 + II.1.2
        /// </summary>
        public double GrossProfitTotal
        {
            get { return DeliveryRevenueTotal + OtherIncommingTotal; }
        }
        /// <summary>
        /// II.3.1. Lợi nhuận bán thuốc
        /// </summary>
        public double DeliveryRevenueTotal { get; set; }

        /// <summary>
        /// II.4. Lãi ròng = II.3 - II.2.2 - II.2.3
        /// </summary>
        public double NetInterestTotal 
        {
            get { return GrossProfitTotal - BusinessCostsTotal - OtherOutcomingTotal; } 
        } 

        /// <summary>
        /// II.5 = II.1 - II.2
        /// </summary>
        public double CashFundTotal // Quỹ tiền mặt
        {
            get {return RevenueTotal - ExpenditureTotal; }
        } 
    }
}
