using App.Common.DI;
using Hangfire;
using Med.Service.Background;
using Med.Service.Drug;
using Med.Service.Log;
using System;
using System.Linq;

namespace Med.Service.Background
{
    public static class BackgroundServiceJobHelper
    {
        public static void EnqueueUpdateDrugExpiredDate4InitInventory(string drugStoreCode, int drugId)
        {
            BackgroundJob.Enqueue(() => UpdateDrugExpiredDate4InitInventory(drugStoreCode, drugId));
            
        }
        public static void UpdateDrugExpiredDate4InitInventory(string drugStoreCode, int drugId)
        {
            IoC.Container.Resolve<IBackgroundService>().UpdateDrugExpiredDate4InitInventory(drugStoreCode, drugId);
        }

        public static void EnqueueUpdateLastInventoryQuantity4CacheDrugs(string drugStoreCode, params int[] drugIds)
        {
            BackgroundJob.Enqueue(() => UpdateLastInventoryQuantity4CacheDrugs(drugStoreCode, drugIds));
        }
        public static void UpdateLastInventoryQuantity4CacheDrugs(string drugStoreCode, params int[] drugIds)
        {
            IoC.Container.Resolve<IBackgroundService>().UpdateLastInventoryQuantity4CacheDrugs(drugStoreCode, drugIds);
        }
        public static void EnqueueUpdateExtraInfo4DeliveryNotes(string drugStoreCode, int actorId, params int[] noteIds)
        {
            BackgroundJob.Enqueue(() => UpdateExtraInfo4DeliveryNotes(drugStoreCode, actorId, noteIds));
        }
        public static void UpdateExtraInfo4DeliveryNotes(string drugStoreCode, int actorId, params int[] noteIds)
        {
            IoC.Container.Resolve<IBackgroundService>().UpdateExtraInfo4DeliveryNotes(drugStoreCode, noteIds);
            if (noteIds != null && noteIds.Any())
            {
                foreach (var noteId in noteIds)
                {
                    IoC.Container.Resolve<IAuditLogService>().LogDeliveryNote(drugStoreCode, noteId, actorId);
                }
            }
            
        }
        public static void EnqueueUpdateExtraInfo4ReceiptNotes(string drugStoreCode, int actorId, params int[] noteIds)
        {
            BackgroundJob.Enqueue(() => UpdateExtraInfo4ReceiptNotes(drugStoreCode, actorId, noteIds));
        }
        public static void UpdateExtraInfo4ReceiptNotes(string drugStoreCode, int actorId, params int[] noteIds)
        {
            IoC.Container.Resolve<IBackgroundService>().UpdateExtraInfo4ReceiptNotes(drugStoreCode, noteIds);
            if (noteIds != null && noteIds.Any())
            {
                foreach (var noteId in noteIds)
                {
                    IoC.Container.Resolve<IAuditLogService>().LogReceiptNote(drugStoreCode, noteId, actorId);
                }
            }
        }
    }
}