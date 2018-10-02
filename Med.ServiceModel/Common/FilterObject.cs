using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;
using App.Common.Utility;
using Med.Common.Enums;

namespace Med.ServiceModel.Common
{
    public class FilterObject: BaseFilterObject
    {
        public int[] DrugIds { get; set; }       
        public string[] DrugNames { get; set; }
        public bool HasDrugIds { get { return DrugIds != null && DrugIds.Any(); } }
        public bool HasDrugNames { get { return DrugNames != null && DrugNames.Any(); } }

        public int[] DrugGroupIds { get; set; }
        public string[] DrugGroupNames { get; set; }
        public bool HasDrugGroupIds { get { return DrugGroupIds != null && DrugGroupIds.Any(); } }
        public bool HasDrugGroupNames { get { return DrugGroupNames != null && DrugGroupNames.Any(); } }

        public int[] CustomerGroupIds { get; set; }
        public bool HasCustomerGroupIds { get { return CustomerGroupIds != null && CustomerGroupIds.Any(); } }
        public int[] CustomerIds { get; set; }
        public string[] CustomerNames { get; set; }
        public bool HasCustomerIds { get { return CustomerIds != null && CustomerIds.Any(); } }
        public bool HasCustomerNames { get { return CustomerNames != null && CustomerNames.Any(); } }

        public int[] SupplyerIds { get; set; }
        public string[] SupplyerNames { get; set; }
        public bool HasSupplyerIds { get { return SupplyerIds != null && SupplyerIds.Any(); } }
        public bool HasSupplyerNames { get { return SupplyerNames != null && SupplyerNames.Any(); } }

        public int[] StaffIds { get; set; }
        public string[] StaffNames { get; set; }
        public bool HasStaffIds { get { return StaffIds != null && StaffIds.Any(); } }
        public bool HasStaffNames { get { return StaffNames != null && StaffNames.Any(); } }

        public int[] DoctorIds { get; set; }
        public string[] DoctorNames { get; set; }
        public bool HasDoctorIds { get { return DoctorIds != null && DoctorIds.Any(); } }
        public bool HasDoctorNames { get { return DoctorNames != null && DoctorNames.Any(); } }

        public GroupFilterType GroupFilterTypeId { get; set; }
        public ReportByType ReportByTypeId { get; set; }        
        public FilterObject DeepCopy()
        {
            return this.DeepCopyByExpressionTree();
        }

        public int[] DeliveryNoteIds { get; set; }
        public bool HasDeliveryNoteIds { get { return DeliveryNoteIds != null && DeliveryNoteIds.Any(); } }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
}
