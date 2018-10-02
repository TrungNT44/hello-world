using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common.Enums;
using Med.Common;
using Med.ServiceModel.Common;
using App.Common.Extensions;

namespace Med.ServiceModel.Request
{
    public class ReportRequestModel: RequestModel
    {
        public DateTime reportFromDate { get; set; }
        public DateTime reportToDate { get; set; }
        public int[] itemIds { get; set; }
        public int[] secondItemIds { get; set; }
        public int groupFilterType { get; set; } // GroupFilterType
        public int filterItemType { get; set; } // ItemFilterType
        public int secondFilterItemType { get; set; } // Second ItemFilterType
        public int reportByTypeId { get; set; } // ReportByType   
        public double minValue { get; set; }
        public double maxValue { get; set; }
        public DateTime MedReportFromDate
        {
            get
            {
                var retVal = MedConstants.MinProductionDataDate;
                if(reportFromDate > MedConstants.MinProductionDataDate)
                {
                    retVal = reportFromDate.AbsoluteStart();
                }

                return retVal;
            }
        }
        public DateTime MedReportToDate
        {
            get
            {
                var retVal = MedConstants.MaxProductionDataDate;
                if (reportToDate > MedConstants.MinProductionDataDate && reportToDate < MedConstants.MaxProductionDataDate)
                {
                    retVal = reportToDate.AbsoluteEnd();
                }

                return retVal;
            }
        }

        public FilterObject ToFilterObject()
        {
            var filter = new FilterObject()
            {
                FromDate = MedReportFromDate,
                ToDate = MedReportToDate,
                PageIndex = pageIndex,
                PageSize = pageSize,
                GroupFilterTypeId = (GroupFilterType)groupFilterType,
                ReportByTypeId = (ReportByType)reportByTypeId,
                MinValue = minValue,
                MaxValue = maxValue
            };
            if (itemIds != null && itemIds.Any() && !itemIds.Contains(MedConstants.FilterByAllValue))
            {
                var filterItemTypeId = (ItemFilterType)filterItemType;
                switch (filterItemTypeId)
                {
                    case ItemFilterType.DrugGoup:
                        filter.DrugGroupIds = itemIds;
                        break;
                    case ItemFilterType.Drug:
                        filter.DrugIds = itemIds;
                        break;
                    case ItemFilterType.CustomerGroup:
                        filter.CustomerGroupIds = itemIds;
                        break;
                    case ItemFilterType.Customer:
                        filter.CustomerIds = itemIds;
                        break;
                    case ItemFilterType.Staff:
                        filter.StaffIds = itemIds;
                        break;
                    case ItemFilterType.Supplyer:
                        filter.SupplyerIds = itemIds;
                        break;
                    case ItemFilterType.Doctor:
                        filter.DoctorIds = itemIds;
                        break;
                }
            }

            if (secondItemIds != null && secondItemIds.Any() && !secondItemIds.Contains(MedConstants.FilterByAllValue))
            {
                var filterItemTypeId = (ItemFilterType)secondFilterItemType;
                switch (filterItemTypeId)
                {
                    case ItemFilterType.DrugGoup:
                        filter.DrugGroupIds = secondItemIds;
                        break;
                    case ItemFilterType.Drug:
                        filter.DrugIds = secondItemIds;
                        break;
                    case ItemFilterType.CustomerGroup:
                        filter.CustomerGroupIds = itemIds;
                        break;
                    case ItemFilterType.Customer:
                        filter.CustomerIds = secondItemIds;
                        break;
                    case ItemFilterType.Staff:
                        filter.StaffIds = secondItemIds;
                        break;
                    case ItemFilterType.Supplyer:
                        filter.SupplyerIds = secondItemIds;
                        break;
                    case ItemFilterType.Doctor:
                        filter.DoctorIds = itemIds;
                        break;
                }
            }

            return filter;
        }
    }
}
