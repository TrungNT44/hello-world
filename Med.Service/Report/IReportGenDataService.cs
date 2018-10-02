using System.Collections.Generic;
using Med.Entity;

namespace Med.Service.Report
{
    public interface IReportGenDataService
    {
        bool GenerateReceiptDrugPriceRefs(string drugStoreCode, bool isReGenData = false);
    }
}
