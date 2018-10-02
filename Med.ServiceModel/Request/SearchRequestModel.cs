using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common.Enums;

namespace Med.ServiceModel.Request
{
    public class SearchRequestModel : RequestModel
    {
        public int searchType { get; set; }
        public string searchText { get; set; }
    }
}
