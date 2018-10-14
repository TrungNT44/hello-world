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
        public static void EnqueueMakeAffectedChangesRelatedDeliveryNotes(string drugStoreCode, int? actorId, params int[] noteIds)
        {
            BackgroundJob.Enqueue(() => MakeAffectedChangesRelatedDeliveryNotes(drugStoreCode, actorId, noteIds));
        }
        public static void MakeAffectedChangesRelatedDeliveryNotes(string drugStoreCode, int? actorId, params int[] noteIds)
        {
            IoC.Container.Resolve<IBackgroundService>().MakeAffectedChangesRelatedDeliveryNotes(drugStoreCode, noteIds);
            if (noteIds != null && noteIds.Any())
            {
                foreach (var noteId in noteIds)
                {
                    IoC.Container.Resolve<IAuditLogService>().LogDeliveryNote(drugStoreCode, noteId, actorId);
                }
            }

        }
        public static void EnqueueMakeAffectedChangesRelatedReceiptNotes(string drugStoreCode, int? actorId, params int[] noteIds)
        {
            BackgroundJob.Enqueue(() => MakeAffectedChangesRelatedReceiptNotes(drugStoreCode, actorId, noteIds));
        }
        public static void MakeAffectedChangesRelatedReceiptNotes(string drugStoreCode, int? actorId, params int[] noteIds)
        {
            IoC.Container.Resolve<IBackgroundService>().MakeAffectedChangesRelatedReceiptNotes(drugStoreCode, noteIds);
            if (noteIds != null && noteIds.Any())
            {
                foreach (var noteId in noteIds)
                {
                    IoC.Container.Resolve<IAuditLogService>().LogReceiptNote(drugStoreCode, noteId, actorId);
                }
            }
        }

        public static void EnqueueUpdateNewestInventories(string drugStoreID, params int[] drugIds)
        {
            BackgroundJob.Enqueue(() => UpdateNewestInventories(drugStoreID, drugIds));
        }
        public static void UpdateNewestInventories(string drugStoreID, params int[] drugIds)
        {
            IoC.Container.Resolve<IInventoryService>().GenerateInventory4Drugs(drugStoreID, drugIds == null, false, true, drugIds);
        }

        public static void EnqueueMakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds)
        {
            BackgroundJob.Enqueue(() => MakeAffectedChangesByUpdatedDrugs(drugStoreID, drugIds));
        }
        public static void MakeAffectedChangesByUpdatedDrugs(string drugStoreID, params int[] drugIds)
        {
            IoC.Container.Resolve<IBackgroundService>().MakeAffectedChangesByUpdatedDrugs(drugStoreID, drugIds);
        }

        public static void EnqueueDeleteForeverDrugs(string drugStoreID, params int[] drugIds)
        {
            BackgroundJob.Enqueue(() => DeleteForeverDrugs(drugStoreID, drugIds));
        }
        public static void DeleteForeverDrugs(string drugStoreID, params int[] drugIds)
        {
            IoC.Container.Resolve<IBackgroundService>().DeleteForeverDrugs(drugStoreID, drugIds);
        }
    }
}