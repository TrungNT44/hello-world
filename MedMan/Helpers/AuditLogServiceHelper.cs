using App.Common.DI;
using Hangfire;
using Med.Service.Log;
using Med.Web.Data.Session;

namespace Med.Web.Helpers
{
    public static class AuditLogServiceHelper
    {
        //public static void EnqueueLogReceiptNote(int noteId)
        //{            
        //    BackgroundJob.Enqueue(() => LogReceiptNote(MedSessionManager.CurrentDrugStoreCode, noteId, MedSessionManager.CurrentUserId));
        //}
        //public static void EnqueueLogDeliveryNote(int noteId)
        //{
        //    BackgroundJob.Enqueue(() => LogDeliveryNote(MedSessionManager.CurrentDrugStoreCode, noteId, MedSessionManager.CurrentUserId));
        //}
        public static void LogReceiptNote(string drugStoreCode, int noteId, int? actorId)
        {            
            IoC.Container.Resolve<IAuditLogService>().LogReceiptNote(drugStoreCode, noteId, actorId);
        }
        public static void LogDeliveryNote(string drugStoreCode, int noteId, int? actorId)
        {              
            IoC.Container.Resolve<IAuditLogService>().LogDeliveryNote(drugStoreCode, noteId, actorId);
        }
    }
}