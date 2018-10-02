using Med.Entity;
using Med.ServiceModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Recruitment
{
    public class ListRecruitsResponse : ResponseModel<TuyenDungs>
    {
        public double Total { get; set; }
    }
}
