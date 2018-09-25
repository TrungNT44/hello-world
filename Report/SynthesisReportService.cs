using System;
using System.Dynamic;
using System.Linq;
using App.Common;
using App.Common.Extensions;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Service.Registration;
using Med.Entity;
using Med.Repository.Registration;
using Med.ServiceModel.Report;
using Med.ServiceModel.Response;

namespace Med.Service.Impl.Report
{
    public class SynthesisReportService : MedBaseService, ISynthesisReportService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public SynthesisReportResponse GetSynthesisReportData(string drugStoreCode, FilterObject filter)
        {
            GenerateReportData(drugStoreCode);
            var result = new SynthesisReportResponse();
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
                var rpService = IoC.Container.Resolve<IReportService>();
                var warehouseSysnthesis = rpService.GetDrugWarehouseSyntheises(drugStoreCode, filter);
                var inOutCommingSummary = rpService.GetInOutCommingValueSumary(drugStoreCode, filter);
                // I.1. Tổng giá trị nhập kho
                result.ReceiptValueTotal = warehouseSysnthesis.Values.Sum(i => i.ReceiptInventoryValueInPeriod);

                // I.2. Tổng giá trị xuất kho
                result.DeliveryValueTotal = warehouseSysnthesis.Values.Sum(i => i.DeliveryInventoryValueInPeriod);

                // I.3. Tổng giá trị kho hàng
                result.WarehousingValueTotal = warehouseSysnthesis.Values.Sum(i => i.LastInventoryValue);

                // I.4. Tổng nợ khách hàng
                // I.4.1 Nợ bán hàng
                result.DeliveryDueTotal = inOutCommingSummary.DeliveryDueTotal;
                // I.4.2 Các khoản nợ khác
                result.DeliveryOtherDueTotal = inOutCommingSummary.DeliveryOtherDueTotal;

                // I.5. Tổng nợ nhà cung cấp
                // I.5.1  Nợ mua hàng
                result.ReceiptDueTotal = inOutCommingSummary.ReceiptDueTotal;
                // I.5.2 Các khoản nợ khác
                result.ReceiptOtherDueTotal = inOutCommingSummary.ReceiptOtherDueTotal;



                // II.1. Tổng thu
                // II.1.1. Bán hàng hóa/dịch vụ
                result.DeliveryIncomingTotal = inOutCommingSummary.DeliveryIncomingTotal;
                // II.1.2. Các nguồn thu khác
                result.OtherIncommingTotal = inOutCommingSummary.OtherIncommingTotal;

                // II.2. Tổng chi
                // II.2.1.  Mua hàng hóa/ dịch vụ
                result.ReceiptOutcomingTotal = inOutCommingSummary.ReceiptOutcomingTotal; // TODO:
                                                                                          // II.2.2.  Chi phí kinh doanh
                result.BusinessCostsTotal = inOutCommingSummary.BusinessCostsTotal;
                // II.2.3.  Các khoản chi khác
                result.OtherOutcomingTotal = inOutCommingSummary.OtherOutcomingTotal;

                // II.3. Lãi gộp == II.3.1 + II.1.2
                // II.3.1. Lợi nhuận bán thuốc
                result.DeliveryRevenueTotal = deliveryService.GetDeliveryRevenueTotal(drugStoreCode, filter);
            }            

            return result;
        }        
        #endregion

        #region Private Methods

        private double GetTotalDeliveryValue(string drugStoreCode, DateTime? fromDate, DateTime? toDate, IUnitOfWork uow)
        {
            return 0;
        }
        #endregion
    }
}
