using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Common
{
    public class DrugStoreInfo
    {
        public string DrugStoreCode { get; set; }
        public string DrugStoreName { get; set; }
        public bool IsParent { get; set; }
    }
}
