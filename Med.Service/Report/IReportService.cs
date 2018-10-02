using System;
using System.Collections.Generic;
using System.Dynamic;
using App.Common.Data;
using Med.Entity.Registration;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Report;
using Med.ServiceModel.Common;
using Med.Common.Enums;

namespace Med.Service.Report
{    
    public interface IReportService
    {
        InOutCommingValueSummary GetInOutCommingValueSumary(string drugStoreCode,  FilterObject filter = null);
        Dictionary<int, DrugWarehouseSynthesis> GetDrugWarehouseSyntheises(string drugStoreCode, FilterObject filter = null);
        GroupFilterData GetFilterItems(string drugStoreCode, ItemFilterType filterType, int? currentItemId = null, bool optionAllItems = false);
        ReportByResponse GetReportByData(string drugStoreCode, FilterObject filter);
    }
}
