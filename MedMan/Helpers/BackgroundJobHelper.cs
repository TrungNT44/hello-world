using App.Common.DI;
using Hangfire;
using Med.Service.Background;
using Med.Service.Drug;
using Med.Service.Log;
using Med.Web.Data.Session;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Med.Web.Helpers
{
    public static class BackgroundJobHelper
    {
        public static void EnqueueMakeAffectedChangesRelatedDeliveryNotes(params int[] noteIds)
        {
            BackgroundServiceJobHelper.EnqueueMakeAffectedChangesRelatedDeliveryNotes(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, noteIds);
        }
        public static void EnqueueMakeAffectedChangesRelatedReceiptNotes(params int[] noteIds)
        {
            BackgroundServiceJobHelper.EnqueueMakeAffectedChangesRelatedReceiptNotes(MedSessionManager.CurrentDrugStoreCode, MedSessionManager.CurrentUserId, noteIds);
        }
        public static void EnqueueUpdateNewestInventories(params int[] drugIds)
        {
            BackgroundServiceJobHelper.EnqueueUpdateNewestInventories(MedSessionManager.CurrentDrugStoreCode, drugIds);
        }
        public static void EnqueueMakeAffectedChangesByUpdatedDrugs(params int[] drugIds)
        {
            BackgroundServiceJobHelper.EnqueueMakeAffectedChangesByUpdatedDrugs(MedSessionManager.CurrentDrugStoreCode, drugIds);
        }
        public static void EnqueueDeleteForeverDrugs(params int[] drugIds)
        {
            BackgroundServiceJobHelper.DeleteForeverDrugs(MedSessionManager.CurrentDrugStoreCode, drugIds);
        }
    }
}