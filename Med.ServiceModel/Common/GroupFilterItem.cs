using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;
using App.Common.Utility;

namespace Med.ServiceModel.Common
{
    public class GroupFilterItem
    {
        public object ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
    }
}
