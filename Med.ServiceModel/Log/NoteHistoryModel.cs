using App.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Log
{
    [Serializable]
    public class NoteHistoryModel: HistoryBaseModel
    {
        public BaseEntity Note { get; set; }
        public IEnumerable<BaseEntity> NoteItems { get; set; }
    }
}
