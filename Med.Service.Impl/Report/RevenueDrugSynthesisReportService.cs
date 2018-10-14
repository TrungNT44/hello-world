using System;
using System.Dynamic;
using System.Linq;
using App.Common;
using App.Common.Extensions;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using Med.Common;
using Med.Common.Enums;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Receipt;
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
    public class RevenueDrugSynthesisReportService : MedBaseService, IRevenueDrugSynthesisReportService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public RevenueDrugSynthesisResponse GetRevenueDrugSynthesis(string drugStoreCode, FilterObject filter)
        {
            var revenueDrugItems = new List<RevenueDrugItem>();
            var result = new RevenueDrugSynthesisResponse()
            {
                Total = 0.0,
                Revenue = 0.0,
                DeliveryTotal = 0.0,
                DebtTotal = 0.0
            };
            
            var rpDataService = IoC.Container.Resolve<IReportGenDataService>();
            rpDataService.GenerateReceiptDrugPriceRefs(drugStoreCode);
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                var deliveryService = IoC.Container.Resolve<IDeliveryNoteService>();
                var deliveryStatuses = new int[] { (int)NoteInOutType.Delivery };
                var deliveryItems = deliveryService.GetDeliveryNoteItems(drugStoreCode, filter, deliveryStatuses);

                if (deliveryItems.Count <= 0)
                {
                    trans.Complete();
                    result.PagingResultModel = new PagingResultModel<RevenueDrugItem>(revenueDrugItems, revenueDrugItems.Count);
                    return result;
                }

                var candidates = deliveryItems.OrderBy(i => i.NoteDate).GroupBy(i => i.NoteId);
                var order = 0;
                foreach (var cand in candidates)
                {
                    order++;
                    var idx = 0;
                    var subItems = cand.ToList();
                    foreach (var di in subItems)
                    {
                        var revenueDrugItem = new RevenueDrugItem()
                        {
                            Order = idx == 0 ? order.ToString() : string.Empty,
                            DeliveryNoteId = di.NoteId,
                            CustomerName = idx == 0 ? di.CustomerName : string.Empty,
                            DrugCode = di.DrugCode,
                            DrugName = di.DrugName,
                            UnitName = di.UnitName,
                            //Quantity = di.FinalRealQuantity,
                            Quantity = di.Quantity,
                            Price = di.Price,
                            Discount = di.Discount,
                            Amount = di.FinalAmount, // Effected by discount & VAT
                            Revenue = di.Revenue,
                            NoteNumber = idx == 0 ? di.NoteNumber.ToString() : string.Empty,
                            VAT = idx == 0 ? (double?)di.VAT : null,
                            DebtAmount = idx == 0 ? di.DebtAmount : 0
                        };
                        revenueDrugItems.Add(revenueDrugItem);
                        result.DeliveryTotal += revenueDrugItem.Amount;
                        result.Revenue += revenueDrugItem.Revenue;
                        result.DebtTotal += revenueDrugItem.DebtAmount;

                        idx++;
                    }
                }

                var receiptNoteService = IoC.Container.Resolve<IReceiptNoteService>();
                var noteItemsReturnFromCustomers = receiptNoteService.GetReceiptNoteItems(drugStoreCode, filter,
                    new int[] { (int)NoteInOutType.ReturnFromCustomer });
                var returnedCandidates = noteItemsReturnFromCustomers.OrderBy(i => i.NoteDate).GroupBy(i => i.NoteId);
                trans.Complete();

                foreach (var cand in returnedCandidates)
                {
                    order++;
                    var idx = 0;
                    var subItems = cand.ToList();
                    foreach (var di in subItems)
                    {
                        var revenueDrugItem = new RevenueDrugItem()
                        {
                            Order = idx == 0 ? order.ToString() : string.Empty,
                            DeliveryNoteId = di.NoteId,
                            CustomerName = idx == 0 ? String.Format("{0} (trả hàng)", di.CustomerName) : string.Empty,
                            DrugCode = di.DrugCode,
                            DrugName = di.DrugName,
                            UnitName = di.UnitName,
                            Quantity = di.Quantity,
                            Price = di.Price,
                            Discount = di.Discount,
                            Amount = -di.Amount,
                            Revenue = 0,
                            NoteNumber = idx == 0 ? di.NoteNumber.ToString() : string.Empty,
                            VAT = idx == 0 ? (double?)di.VAT : null,
                            IsReturnFromCustomer = true
                        };
                        revenueDrugItems.Add(revenueDrugItem);
                        result.DeliveryTotal += revenueDrugItem.Amount;
                        //result.Revenue += -di.RetailAmount;
                        idx++;
                    }
                }
            }
            
            result.Total = result.DeliveryTotal - result.DebtTotal;
            result.PagingResultModel = new PagingResultModel<RevenueDrugItem>(revenueDrugItems, revenueDrugItems.Count);

            return result;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
