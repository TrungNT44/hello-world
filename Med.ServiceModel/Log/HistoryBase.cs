using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Log
{
    [Serializable]
    public class HistoryBaseModel
    {
        public int EntityID { get; set; }
        public string DrugStoreCode { get; set; }
        public int? ActorID { get; set; }
    }
}
