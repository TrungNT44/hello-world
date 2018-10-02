using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;
using App.Common.Utility;

namespace Med.ServiceModel.Common
{
    public class GroupFilterData
    {
        public List<GroupFilterItem> GroupItems { get; set; }
        public List<GroupFilterItem> NameItems { get; set; }
    }
}
