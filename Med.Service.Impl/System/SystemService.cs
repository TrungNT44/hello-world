using System;
using System.Dynamic;
using System.Linq;
using App.Common;
using App.Common.Base;
using App.Common.Extensions;
using App.Common.FaultHandling;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using App.Configuration;
using App.Constants.Enums;
using Med.Common.Enums;
using Med.Common.Enums.SystemMessage;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Delivery;
using Med.Service.Receipt;
using Med.Service.Report;
using Med.ServiceModel.Common;
using Med.ServiceModel.Registration;
using Med.Service.Registration;
using Med.Entity;
using Med.Repository.Registration;
using Med.ServiceModel.Report;
using Med.ServiceModel.Response;
using Med.Common;
using Med.Service.System;
using System.Text;
using Med.ServiceModel.System;
using Med.Service.Notifications;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Med.Service.Utilities;
using System.Data.Entity.Core.Objects;
using Med.Entity.Report;

namespace Med.Service.Impl.System
{
    public class SystemService : MedBaseService, ISystemService
    {
        #region Fields
        #endregion

        #region Interface Implementation

        public void GenerateSystemMessages(string drugStoreCode)
        {
            try
            {
                var service = IoC.Container.Resolve<IUtilitiesService>();
                var settings = service.GetDrugStoreSetting(drugStoreCode);
                LogHelper.Debug("Generate system messages for drug store: {0}", drugStoreCode);
                GenerateNegativeProfitWarning(drugStoreCode, MedConstants.LastNumberOfDaysToCheckNagativeProfit);
                GenerateDrugExpiredDateWarning(drugStoreCode, settings);
                GenerateOutOfStockWarning(drugStoreCode);
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, string.Format("GenerateSystemMessages - {0}", drugStoreCode));
            }
        }

        public SystemMessageResponse GetSystemMessages(string drugStoreCode)
        {
            var result = new SystemMessageResponse();

            var messages = _dataFilterService.GetValidSystemMessages(drugStoreCode).ToList();
            result.PagingResultModel = new PagingResultModel<SystemMessage>(messages, messages.Count);

            return result;
        }

        public int GetSystemMessagesCount(string drugStoreCode)
        {
            var result = _dataFilterService.GetValidSystemMessages(drugStoreCode).Count();

            return result;
        }

        #endregion

        #region Private Methods
        private static string GetWarningMessageLink(string relativeUrl)
        {
            var productionHost = AppBase.Instance.ProductionHost;
            var url = string.Format("{0}/{1}", productionHost, relativeUrl);
            var sb = new StringBuilder();
            sb.AppendFormat("<a href='{0}' target='_blank'>Chi tiết</a><br/>", url);

            return sb.ToString();
        }

        public void GenerateNegativeProfitWarning(string drugStoreCode, int numberOfDaysFromNow, bool notify = false)
        {
            var notifiService = IoC.Container.Resolve<INotificationService>();
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedReportContext, DeliveryNoteItemSnapshotInfo>>();
            var validItems = repository.GetAll().Where(i =>
                i.DrugStoreCode == drugStoreCode && !i.IsDeleted);
            var minDate = DateTime.Now.AddDays(-numberOfDaysFromNow);
            var nagativeCount = validItems.Where(i => i.NegativeRevenue == true && i.CreatedDateTime >= minDate).GroupBy(i => i.DrugId).Count();
         
            IBaseRepository<SystemMessage> systemMessageRepo = null;
            var systemMessages = _dataFilterService.GetValidSystemMessages(drugStoreCode, null, out systemMessageRepo);
            if (nagativeCount > 0)
            {
                var messageLink = GetWarningMessageLink("/Utilities/NegativeRevenueWarning");
                var message = string.Format("Tổng số mặt hàng bán âm trong thời gian {0} ngày gần nhất: {1}",
                    numberOfDaysFromNow, nagativeCount);
                if (systemMessages.Any(i => i.SystemMessageTypeId == (int)SystemMessageType.NegativeProfitWarning))
                {
                    systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                        i.SystemMessageTypeId == (int) SystemMessageType.NegativeProfitWarning,
                        i => new SystemMessage()
                        {
                            MessageContent = message,
                            IsSolved = false,
                            MessageLink = messageLink
                        });
                }
                else
                {
                    systemMessageRepo.Add(new SystemMessage()
                    {
                        CreatedDate = DateTime.Now,
                        DrugStoreCode = drugStoreCode,
                        RecordStatusId = (int)RecordStatus.Activated,
                        SystemMessageLevelId = (int)SystemMessageLevel.ByDrugStore,
                        SystemMessageTypeId = (int)SystemMessageType.NegativeProfitWarning,
                        MessageContent = message,
                        MessageLink = messageLink
                    });
                }
                notifiService.SendNotificationWarning(drugStoreCode, message, "/Utilities/NegativeRevenueWarning", (int)HttpActionEnum.NegativeRevenueWarningScreen);
            }
            else
            {
                systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                    i.SystemMessageTypeId == (int) SystemMessageType.NegativeProfitWarning,
                    i => new SystemMessage()
                    {
                        RecordStatusId = (int)RecordStatus.Archived
                    });
                notifiService.DeleteNotificationByResourceId(drugStoreCode,(int)HttpActionEnum.NegativeRevenueWarningScreen);
            }
            if (notify)
            {
                var serviceNotifi = IoC.Container.Resolve<Service.Common.INotificationBaseService>();
                serviceNotifi.NotifyToUsers("loadNotification", null, null);
            }
        }

        public void GenerateDrugExpiredDateWarning(string drugStoreCode, DrugStoreSetting setting, bool notify = false)
        {
            var notifiService = IoC.Container.Resolve<INotificationService>();
            var ultilService = IoC.Container.Resolve<IUtilitiesService>();
            var candiates = ultilService.GetRemainQuantityReceiptDrugItems(drugStoreCode, setting);
           
            var expiredDrugsCount = candiates.Count(i => i.IsExpired);
            var litleDrugsCount = candiates.Count(i => i.IsLittleTrans);

            IBaseRepository<SystemMessage> systemMessageRepo = null;
            var systemMessages = _dataFilterService.GetValidSystemMessages(drugStoreCode, null, out systemMessageRepo);
            if (litleDrugsCount > 0 || expiredDrugsCount > 0)
            {
                var messageLink = GetWarningMessageLink("/Utilities/NearExpiredDrugWarning");
                string message = string.Empty;
                if (litleDrugsCount > 0 && expiredDrugsCount > 0)
                {
                    message = string.Format("Tổng số mặt hàng ít giao dịch: {0}. Tổng số mặt hàng hết hạn: {1}.",
                        litleDrugsCount, expiredDrugsCount);
                }
                else
                {
                    if (litleDrugsCount > 0)
                    {
                        message = string.Format("Tổng số mặt hàng ít giao dịch: {0}.",
                            litleDrugsCount);
                    }
                    else
                    {
                        message = string.Format("Tổng số mặt hàng hết hạn: {0}.",
                           expiredDrugsCount);
                    }
                }
               
                if (systemMessages.Any(i => i.SystemMessageTypeId == (int)SystemMessageType.DrugExpiredDateWarning))
                {
                    systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                        i.SystemMessageTypeId == (int)SystemMessageType.DrugExpiredDateWarning,
                        i => new SystemMessage()
                        {
                            MessageContent = message,
                            IsSolved = false,
                            MessageLink = messageLink
                        });
                }
                else
                {
                    systemMessageRepo.Add(new SystemMessage()
                    {
                        CreatedDate = DateTime.Now,
                        DrugStoreCode = drugStoreCode,
                        RecordStatusId = (int)RecordStatus.Activated,
                        SystemMessageLevelId = (int)SystemMessageLevel.ByDrugStore,
                        SystemMessageTypeId = (int)SystemMessageType.DrugExpiredDateWarning,
                        MessageContent = message,
                        MessageLink = messageLink
                    });
                }
                
                notifiService.SendNotificationWarning(drugStoreCode, message, "/Utilities/NearExpiredDrugWarning", (int) HttpActionEnum.ExpiredDateWarningScreen);
            }
            else
            {
                systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                    i.SystemMessageTypeId == (int)SystemMessageType.DrugExpiredDateWarning,
                    i => new SystemMessage()
                    {
                        RecordStatusId = (int)RecordStatus.Archived
                    });
                notifiService.DeleteNotificationByResourceId(drugStoreCode, (int)HttpActionEnum.ExpiredDateWarningScreen);
            }
            if (notify)
            {
                var serviceNotifi = IoC.Container.Resolve<Service.Common.INotificationBaseService>();
                serviceNotifi.NotifyToUsers("loadNotification", null, null);
            }
        }

        public void GenerateOutOfStockWarning(string drugStoreCode, bool notify = false)
        {
            var notifiService = IoC.Container.Resolve<INotificationService>();
            var rpService = IoC.Container.Resolve<IReportService>();
            var drugWarehouses = rpService.GetDrugWarehouseSyntheises(drugStoreCode);
            var outOfStockDrugsCount = drugWarehouses.Count(i => i.Value.IsOutOfStock);
            var nagativeStockDrugsCount = drugWarehouses.Count(i => i.Value.IsNagativeStock);

            IBaseRepository<SystemMessage> systemMessageRepo = null;
            var systemMessages = _dataFilterService.GetValidSystemMessages(drugStoreCode, null, out systemMessageRepo);
            if (outOfStockDrugsCount > 0 || nagativeStockDrugsCount > 0)
            {
                var messageLink = GetWarningMessageLink("/DrugManagement/InventoryWarning");
                string message = string.Empty;
                if (outOfStockDrugsCount > 0 && nagativeStockDrugsCount > 0)
                {
                    message = string.Format("Tổng số mặt hàng hết: {0}. Tổng số mặt hàng âm kho: {1}.",
                        outOfStockDrugsCount, nagativeStockDrugsCount);
                }
                else
                {
                    if (outOfStockDrugsCount > 0)
                    {
                        message = string.Format("Tổng số mặt hàng hết: {0}.",
                            outOfStockDrugsCount);
                    }
                    else
                    {
                        message = string.Format("Tổng số mặt hàng âm kho: {0}.",
                           nagativeStockDrugsCount);
                    }
                }

                if (systemMessages.Any(i => i.SystemMessageTypeId == (int)SystemMessageType.OutOfStockWarning))
                {
                    systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                        i.SystemMessageTypeId == (int)SystemMessageType.OutOfStockWarning,
                        i => new SystemMessage()
                        {
                            MessageContent = message,
                            IsSolved = false,
                            MessageLink = messageLink
                        });
                }
                else
                {
                    systemMessageRepo.Add(new SystemMessage()
                    {
                        CreatedDate = DateTime.Now,
                        DrugStoreCode = drugStoreCode,
                        RecordStatusId = (int)RecordStatus.Activated,
                        SystemMessageLevelId = (int)SystemMessageLevel.ByDrugStore,
                        SystemMessageTypeId = (int)SystemMessageType.OutOfStockWarning,
                        MessageContent = message,
                        MessageLink = messageLink
                    });
                }
                
                notifiService.SendNotificationWarning(drugStoreCode, message, "/DrugManagement/InventoryWarning", (int)HttpActionEnum.InventoryWarningScreen);
            }
            else
            {
                systemMessageRepo.UpdateMany(i => i.DrugStoreCode == drugStoreCode &&
                    i.SystemMessageTypeId == (int)SystemMessageType.OutOfStockWarning,
                    i => new SystemMessage()
                    {
                        RecordStatusId = (int)RecordStatus.Archived
                    });
                notifiService.DeleteNotificationByResourceId(drugStoreCode, (int)HttpActionEnum.InventoryWarningScreen);
            }
            if (notify)
            {
                var serviceNotifi = IoC.Container.Resolve<Service.Common.INotificationBaseService>();
                serviceNotifi.NotifyToUsers("loadNotification", null, null);
            }
        }
        #endregion
    }
}
