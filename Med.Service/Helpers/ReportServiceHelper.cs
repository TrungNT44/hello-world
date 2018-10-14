using App.Common.DI;
using Hangfire;
using Med.Service.Background;
using Med.Service.Caching;
using System;
using System.Linq;
using System.Collections.Generic;
using Med.Entity;
using Med.Common.Enums;
using Med.Service.Report;

namespace Med.Service.Helpers
{
    public static class ReportServiceHelper
    {
        public static void CalAffectedNoteItemsByDeliveryNotes(string drugStoreID, params int[] noteIds)
        {
            IoC.Container.Resolve<IReportHelperService>().MakeAffectedChangesByDeliveryNotes(drugStoreID, noteIds);
        }
        public static void CalAffectedNoteItemsByReceiptNotes(string drugStoreID, params int[] noteIds)
        {
            IoC.Container.Resolve<IReportHelperService>().MakeAffectedChangesByReceiptNotes(drugStoreID, noteIds);
        }
    }
}