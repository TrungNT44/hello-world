using Med.Common.Enums;
using Med.ServiceModel.Common;
using Med.ServiceModel.InOutComming;
using System.Collections.Generic;

namespace Med.Service.Common
{
    public interface IInOutCommingNoteService
    {
        long GetInOutCommingNoteNumber(string drugStoreCode, int noteTypeId);
        ReceiverDebtInfo GetReceiverDebtInfo(string drugStoreCode, int receiverId, int noteTypeId, int? inOutComingNoteId = null);
        InOutcommingNoteModel GetInOutcommingNoteModel(string drugStoreCode, int currentUserId, 
            int? noteId, int? noteTypeId, int? taskMode);
        int SaveInOutCommingNote(string drugStoreCode, int currentUserId, InOutcommingNoteModel model);
        int TransitWarehouse(string drugStoreCode, string targetDrugStoreCode, int deliveryNoteId, int userId);
        bool DeleteInOutCommingNote(string drugStoreCode, int noteId);
    }
}
