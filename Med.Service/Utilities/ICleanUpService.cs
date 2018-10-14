using Med.Common;
using Med.Common.Enums;
using Med.ServiceModel.Common;
using Med.ServiceModel.Response;
using Med.ServiceModel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Med.Service.Utilities
{
    public interface ICleanUpService
    {
        void CleanUp(string drugStoreCode);       
    }
}
