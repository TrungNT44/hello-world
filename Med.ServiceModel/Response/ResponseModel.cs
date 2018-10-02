using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Response
{
    public class ResponseModel<TResult> where TResult : class
    {
        public PagingResultModel<TResult> PagingResultModel { get; set; }
    }
}
