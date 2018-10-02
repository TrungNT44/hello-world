using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common.Enums;

namespace Med.ServiceModel.Request
{
    public class FilterRequestModel : RequestModel
    {
        public int filterItemType { get; set; }
        public bool optionAllItems { get; set; }
    }
}
