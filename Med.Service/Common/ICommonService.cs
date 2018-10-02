using System;
using System.Collections.Generic;
using System.Linq;
using Med.Entity.Registration;
using Med.Entity;
using App.Common.Data;
using Med.ServiceModel.Common;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Report;

namespace Med.Service.Common
{
    public interface ICommonService
    {
        double GetDrugRetailFactors(int drugId, int? drugUnitIdOnNote);

        double GetDrugRetailFactors(DrugInfo drug, int? drugUnitIdOnNote);

        DrugInfo GetDrugInfo(int? drugId);
        int CreateUniqueInternalSupplier(string drugStoreCode, string supplierName, int userId);
    }
}
