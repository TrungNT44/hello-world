using System.Linq;
using Med.Common.Enums;
using System.Collections.Generic;
using Med.Service.Base;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.ServiceModel.Response;
using App.Common.Extensions;
using App.Common.Helpers;
using Med.Common;

namespace Med.Service.Impl.Report
{
    public class TransactionReportService : MedBaseService, ITransactionReportService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public DrugTransHistoryResponse GetDrugTransHistoryData(string drugStoreCode, FilterObject filter, int noteTypeId)
        {
            var drugTransHisItems = new List<DrugTransHistoryItem>();
            var result = new DrugTransHistoryResponse();           
            var totalCount = 0;
            using (var trans = TransactionScopeHelper.CreateReadUncommitted())
            {
                if (noteTypeId == (int)NoteInOutType.Receipt)
                {
                    var drugTransHisQable = _dataFilterService.GetValidReceiptNoteItems(drugStoreCode, filter).Where(i => i.NoteNumber >= 0 && i.NoteDate > MedConstants.MinProductionDataDate);
                    totalCount = drugTransHisQable.Count();
                    var candidates = drugTransHisQable.OrderByDescending(i => i.NoteDate).ToPagedQueryable(filter.PageIndex, filter.PageSize, totalCount);
                    drugTransHisItems = candidates
                        .Select(i => new DrugTransHistoryItem()
                        {
                            ItemId = i.NoteId,
                            DrugId = i.DrugId,
                            DrugName = i.DrugName,
                            DrugCode = i.DrugCode,
                            UnitId = i.UnitId,
                            UnitName = i.UnitName,
                            Price = i.Price,
                            Quantity = i.Quantity,
                            Discount = i.Discount,
                            VAT = i.VAT,
                            Amount = i.Quantity * i.Price * (1 - i.Discount/100) * (1 + i.VAT/100),
                            ItemDate = i.NoteDate.Value,
                            ItemNumber = (int)i.NoteNumber
                        }).ToList();
                }
                else if (noteTypeId == (int)NoteInOutType.Delivery)
                {
                    var drugTransHisQable = _dataFilterService.GetValidDeliveryNoteItems(drugStoreCode, filter).Where(i => i.NoteNumber >= 0 && i.NoteDate > MedConstants.MinProductionDataDate);
                    totalCount = drugTransHisQable.Count();
                    var candidates = drugTransHisQable.OrderByDescending(i => i.NoteDate).ToPagedQueryable(filter.PageIndex, filter.PageSize, totalCount);
                    drugTransHisItems = candidates
                        .Select(i => new DrugTransHistoryItem()
                        {
                            ItemId = i.NoteId,
                            DrugId = i.DrugId,
                            DrugName = i.DrugName,
                            DrugCode = i.DrugCode,
                            UnitId = i.UnitId,
                            UnitName = i.UnitName,
                            Price = i.Price,
                            Quantity = i.Quantity,
                            Discount = i.Discount,
                            VAT = i.VAT,
                            Amount = i.Quantity * i.Price * (1 - i.Discount / 100) * (1 + i.VAT / 100),
                            ItemDate = i.NoteDate.Value,
                            ItemNumber = (int)i.NoteNumber
                        }).ToList();
                }
                trans.Complete();
            }            

            var order = filter.PageIndex * filter.PageSize;
            drugTransHisItems.ForEach(i =>
            {
                order++;
                i.Order = order;
            });
            result.PagingResultModel = new PagingResultModel<DrugTransHistoryItem>(drugTransHisItems, totalCount);

            return result;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
