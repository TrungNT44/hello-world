using Med.ServiceModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Utilities
{
    public class CanhBaoHetHanResponse : ResponseModel<CanhBaoHetHanItem>
    {
        public double Total { get; set; }
    }
}
