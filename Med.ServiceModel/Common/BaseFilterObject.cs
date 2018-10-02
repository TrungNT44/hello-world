using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.Common;

namespace Med.ServiceModel.Common
{
    public class BaseFilterObject
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }
}
