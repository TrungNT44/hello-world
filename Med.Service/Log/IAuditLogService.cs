namespace Med.Service.Log
{
    using Med.Entity.Common;
    using global::System.Collections.Generic;
    using Med.Entity.Log;
    using Med.ServiceModel.Log;

    public interface IAuditLogService
    {
        void Add(IList<Audit> audits);
        void Add(Audit audit);
        void Add(HistoryBaseModel history, int hisEntityTypeId);
        void LogReceiptNote(string drugStoreCode, int noteId, int? actorId);
        void LogDeliveryNote(string drugStoreCode, int noteId, int? actorId);
    }
}
