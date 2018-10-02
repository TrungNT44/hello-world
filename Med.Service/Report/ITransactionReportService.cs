using Med.ServiceModel.Report;
using Med.ServiceModel.Common;
using Med.Common.Enums;

namespace Med.Service.Report
{
    public interface ITransactionReportService
    {
        DrugTransHistoryResponse GetDrugTransHistoryData(string drugStoreCode, FilterObject filter, int noteTypeId);
    }
}
