using System.Collections.Generic;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;

namespace Med.Service.Report
{
    public interface ICustomizeReportService
    {
        CustomizeReportItemResponse GetCustomizeReportItems(string drugStoreCode, FilterObject filter);
    }
}
