using Med.ServiceModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class CreateReserveResponse: ResponseModel<CreateReserveItem>
    {
        public double Total { get; set; }
    }
}
