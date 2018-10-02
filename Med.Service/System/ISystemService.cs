using System;
using System.Collections.Generic;
using App.Common.Data;
using Med.Entity.Registration;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.System;
using Med.Common;

namespace Med.Service.System
{
    public interface ISystemService
    {
        void GenerateSystemMessages(string drugStoreCode);
        SystemMessageResponse GetSystemMessages(string drugStoreCode);
        int GetSystemMessagesCount(string drugStoreCode);
        void GenerateNegativeProfitWarning(string drugStoreCode, int numberOfDaysFromNow, bool notify = false);
        void GenerateDrugExpiredDateWarning(string drugStoreCode, DrugStoreSetting setting, bool notify = false);
        void GenerateOutOfStockWarning(string drugStoreCode, bool notify = false);
    }
}
