using Med.Service.Base;
using Med.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Med.Entity;
using Med.ServiceModel.Response;
using Med.ServiceModel.Utilities;
using App.Common.Data;
using Med.DbContext;
using Med.Repository.Factory;
using Med.ServiceModel.Common;
using Med.ServiceModel.Report;
using Med.Common.Enums;
using App.Common.Extensions;
using Med.Service.Delivery;
using App.Common.DI;
using Castle.Core.Internal;
using Med.Common;
using Med.Service.Drug;
using App.Common.Helpers;
using Med.Service.System;
using Med.Entity.Report;
using App.Common.FaultHandling;
using App.Constants.Enums;

namespace Med.Service.Impl.Utilities
{    
    public class CleanUpService : MedBaseService, ICleanUpService
    {
        private const int DefaultDaysToKeepMessyReportData = 5;
        private const int NoCleanUpItems = 500;
        public void CleanUp(string drugStoreCode)
        {
            var appService = IoC.Container.Resolve<IAppSettingService>();
            var daysToKeep = appService.GetSettingIntValue(AppSettingKey.DaysToKeepMessyReportDataKey, DefaultDaysToKeepMessyReportData);
            CleanUpPriceRefData(drugStoreCode, daysToKeep);
            CleanUpDeliveryNoteItemSnapshotInfos(drugStoreCode, daysToKeep);
            CleanReduceItems(drugStoreCode);
        }
        private void CleanUpPriceRefData(string drugStoreCode, int daysToKeep)
        {
            try
            {
                var cleanUpRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReceiptDrugPriceRef>>();
                var cleanUpDateTime = DateTime.Now.AddDays(-daysToKeep).AbsoluteStart();
                var cleanUpIds = cleanUpRepo.TableAsNoTracking.Where(i => i.DrugStoreCode == drugStoreCode && i.IsDeleted
                    && i.CreatedDateTime <= cleanUpDateTime)
                    .Select(i => i.Id).ToArray();
                if (!cleanUpIds.Any())
                {
                    LogHelper.Debug("Drug store: {0}. There is no price refs to clean.", drugStoreCode);
                    return;
                }
                LogHelper.Debug("Drug store: {0}. Number of price refs to clean: {1}", drugStoreCode, cleanUpIds.Length);

                for (int i = 0; i < cleanUpIds.Length; i += NoCleanUpItems)
                {
                    var subCleanIds = ArrayHelper.SubArray<int>(cleanUpIds, i, NoCleanUpItems);
                    cleanUpRepo.Delete(c => subCleanIds.Contains(c.Id));
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
            }            
        }
        private void CleanUpDeliveryNoteItemSnapshotInfos(string drugStoreCode, int daysToKeep)
        {
            try
            {               
                var cleanUpRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();
                var cleanUpDateTime = DateTime.Now.AddDays(-daysToKeep).AbsoluteStart();
                var cleanUpIds = cleanUpRepo.TableAsNoTracking.Where(i => i.DrugStoreCode == drugStoreCode && i.IsDeleted
                    && i.CreatedDateTime <= cleanUpDateTime)
                    .Select(i => i.Id).ToArray();
                if (!cleanUpIds.Any())
                {
                    LogHelper.Debug("Drug store: {0}. There is no delivery item snapshot to clean.", drugStoreCode);
                    return;
                }
                LogHelper.Debug("Drug store: {0}. Number of delivery item snapshots to clean: {1}", drugStoreCode, cleanUpIds.Length);

                for (int i = 0; i < cleanUpIds.Length; i += NoCleanUpItems)
                {
                    var subCleanIds = ArrayHelper.SubArray<int>(cleanUpIds, i, NoCleanUpItems);
                    cleanUpRepo.Delete(c => subCleanIds.Contains(c.Id));
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
            }
        }
        private void CleanReduceItems(string drugStoreCode)
        {
            try
            {
                var cleanUpRepo = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, ReduceNoteItem>>();
                var cleanUpIds = cleanUpRepo.TableAsNoTracking.Where(i => i.DrugStoreCode == drugStoreCode && i.RecordStatusId != (byte)RecordStatus.Activated)
                    .Select(i => i.ReduceId).ToArray();
                if (!cleanUpIds.Any())
                {
                    LogHelper.Debug("Drug store: {0}. There is no reduce item to clean.", drugStoreCode);
                    return;
                }
                LogHelper.Debug("Drug store: {0}. Number of reduce items to clean: {1}", drugStoreCode, cleanUpIds.Length);

                for (int i = 0; i < cleanUpIds.Length; i += NoCleanUpItems)
                {
                    var subCleanIds = ArrayHelper.SubArray<int>(cleanUpIds, i, NoCleanUpItems);
                    cleanUpRepo.Delete(c => subCleanIds.Contains(c.ReduceId));
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
            }
        }
    }  
}
