﻿using System;
using System.Collections.Generic;
using Med.Entity.Registration;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Report;

namespace Med.Service.Report
{
    public interface IDrugWarehouseReportService
    {
        DrugWarehouseResponse GetDrugWarehouses(string drugStoreCode, FilterObject filter);
    }
}
