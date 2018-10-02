using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using Med.Service.Base;
using System;
using Med.DbContext;
using App.Common.DI;
using Med.Common.Enums.Notification;
using Med.Entity.Notifications;
using Med.Service.Notifications;
using Med.Entity;
using Med.Service.Impl.Common;

namespace Med.Service.Impl.Notifications
{
    public class NotificationService : NotificationBaseService, INotificationService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public bool NotifyUsersLoadNotifications()
        {
            return NotifyToUsers("loadNotification", null, null);
        }
        public bool CreateNotification(Notification notify)
        {
            try
            {
                notify.Status = (int)NotificationStatusTypeId.UnReleased;
                notify.CreateDate = DateTime.Now;
                var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
                notificationRepo.Add(notify);
                notificationRepo.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<NotificationType> GetListNotificationType()
        {
            var notifiRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationType>>();
            return notifiRepo.GetAll().ToList();
        }
        public object SearchNotification(string drugStoreID, int? notificationTypeID, string title, int pageIndex, int pageSize)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var notificationTypeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.Notifications.NotificationType>>();
            var notificationStatusTypeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.Notifications.NotificationStatusType>>();
            var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            IQueryable<Notification> qNotification = notificationRepo.GetAll();
            if (!string.IsNullOrEmpty(drugStoreID))
                qNotification = qNotification.Where(e => e.DrugStoreID == drugStoreID);
            if (notificationTypeID.HasValue)
                qNotification = qNotification.Where(e => e.NotificationTypeID == notificationTypeID.Value);
            if (!string.IsNullOrEmpty(title))
                qNotification = qNotification.Where(e => e.Title.ToLower().Contains(title.ToLower()));
            qNotification = qNotification.OrderByDescending(e => e.CreateDate);
            int totalSize = qNotification.Count();
            qNotification = qNotification.Skip(pageIndex * pageSize).Take(pageSize);
            var qResult = from notifi in qNotification
                          join notiType in notificationTypeRepo.GetAll()
                          on notifi.NotificationTypeID equals notiType.ID
                          join status in notificationStatusTypeRepo.GetAll()
                          on notifi.Status equals status.ID
                          join dstore in drugStoreRepo.GetAll()
                          on notifi.DrugStoreID equals dstore.MaNhaThuoc into nt
                          from dstore in nt.DefaultIfEmpty()
                          select new
                          {
                              notifi.ID,
                              notifi.NotificationTypeID,
                              NotificationTypeName = notiType.Name,
                              notifi.Title,
                              notifi.Link,
                              DrugStoreID = dstore != null ? dstore.MaNhaThuoc : string.Empty,
                              DrugStoreName = dstore != null ? dstore.TenNhaThuoc : "Tất cả nhà thuốc",
                              CreateDate = notifi.CreateDate,
                              notifi.Status,
                              StatusName = status.Name
                          };
            var results = qResult.ToList().Select((e, index) => new
            {
                e.ID,
                e.DrugStoreID,
                e.DrugStoreName,
                e.Link,
                e.NotificationTypeID,
                e.NotificationTypeName,
                e.Title,
                Order = index + (pageIndex * pageSize) + 1,
                CreateDate = e.CreateDate.ToString("dd/MM/yyyy HH:mm:ss"),
                e.Status,
                e.StatusName
            });
            return new
            {
                results = results,
                totalSize = totalSize
            };
        }
        public object GetNotificationHistory(string drugStoreID, int? notificationTypeID, string title, int pageIndex, int pageSize)
        {
            try
            {
                var targetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
                var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
                var notificationTypeRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.Notifications.NotificationType>>();
                var qTarget = targetRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID);
                //lọc theo loại thông báo + title
                qTarget = from target in qTarget
                          join notifi in notificationRepo.GetAll()
                          on target.NotificationID equals notifi.ID
                          where (!notificationTypeID.HasValue || notifi.NotificationTypeID == notificationTypeID.Value)
                          && (string.IsNullOrEmpty(title) || notifi.Title.ToLower().Contains(title.ToLower()))
                          select target;
                var totalSize = qTarget.Count();
                qTarget = qTarget.OrderByDescending(e => e.NotificationDate).Skip(pageIndex * pageSize).Take(pageSize);
                var notifiAvai = qTarget.Select(e => e.NotificationID).ToList();
                var qNotifi = from notifi in notificationRepo.GetAll().Where(e => notifiAvai.Contains(e.ID))
                              join notifiType in notificationTypeRepo.GetAll()
                              on notifi.NotificationTypeID equals notifiType.ID
                              select new
                              {
                                  notifi.ID,
                                  notifi.DrugStoreID,
                                  notifi.CreateDate,
                                  notifi.Link,
                                  notifi.Title,
                                  notifi.Status,
                                  notifi.NotificationTypeID,
                                  NotificationTypeName = notifiType.Name
                              };
                var qQuery = from target in qTarget
                             join notifi in qNotifi
                             on target.NotificationID equals notifi.ID
                             select new
                             {
                                 target.ID,
                                 target.NotificationReadTypeID,
                                 target.NotificationDate,
                                 notifi.Title,
                                 notifi.Link,
                                 notifi.NotificationTypeID,
                                 notifi.NotificationTypeName
                             };
                var results = qQuery.ToList().Select((e, index) => new
                {
                    Order = index + (pageIndex * pageSize) + 1,
                    e.ID,
                    e.NotificationReadTypeID,
                    NotificationDate = e.NotificationDate.ToString("dd/MM/yyyy"),
                    e.Title,
                    e.Link,
                    e.NotificationTypeID,
                    e.NotificationTypeName
                });
                return new
                {
                    results = results,
                    totalSize = totalSize
                };

            }
            catch
            {
                return null;
            }
        }
        public Notification GetNotificationInfo(int id)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            return notificationRepo.GetAll().Where(e => e.ID == id).FirstOrDefault();
        }
        public bool DeleteNotification(int id)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var notifi = notificationRepo.GetAll().Where(e => e.ID == id && e.Status == (int)Med.Common.Enums.Notification.NotificationStatusTypeId.UnReleased).FirstOrDefault();
            if (notifi == null)
                return false;
            else
            {
                try
                {
                    notificationRepo.Delete(notifi);
                    notificationRepo.Commit();
                    return true;
                }
                catch
                {

                    return false;
                }
            }
        }
        public bool UpdateNotification(Notification notify)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            try
            {
                var notifiExits = notificationRepo.GetAll().Where(e => e.ID == notify.ID).FirstOrDefault();
                if (notifiExits == null)
                    return false;
                notifiExits.DrugStoreID = notify.DrugStoreID;
                notifiExits.Title = notify.Title;
                notifiExits.Link = notify.Link;
                notifiExits.NotificationTypeID = notify.NotificationTypeID;
                notificationRepo.Update(notifiExits);
                notificationRepo.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ReleaseNotification(int id, bool doSendToClient = true)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            try
            {
                var notifi = notificationRepo.GetAll().Where(e => e.ID == id && e.Status == (int)Med.Common.Enums.Notification.NotificationStatusTypeId.UnReleased).FirstOrDefault();
                if (notifi == null)
                    return false;
                var targetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
                if (string.IsNullOrEmpty(notifi.DrugStoreID))
                {
                    var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
                    var lstNhaThuoc = drugStoreRepo.GetAll().Where(e => e.HoatDong).Select(e => e.MaNhaThuoc).ToList();
                    NotificationTarget taget = null;
                    foreach (string manhaThuoc in lstNhaThuoc)
                    {
                        taget = new NotificationTarget()
                        {
                            NotificationID = notifi.ID,
                            DrugStoreID = manhaThuoc,
                            NotificationReadTypeID = (int)Med.Common.Enums.Notification.NotificationReadTypeId.UnRead,
                            NotificationDate = DateTime.Now
                        };
                        targetRepo.Add(taget);
                    }
                }
                else
                {
                    var taget = new NotificationTarget()
                    {
                        NotificationID = notifi.ID,
                        DrugStoreID = notifi.DrugStoreID,
                        NotificationReadTypeID = (int)Med.Common.Enums.Notification.NotificationReadTypeId.UnRead,
                        NotificationDate = DateTime.Now
                    };
                    targetRepo.Add(taget);
                }
                notifi.Status = (int)Med.Common.Enums.Notification.NotificationStatusTypeId.Released;
                notificationRepo.Update(notifi);
                targetRepo.Commit();
                notificationRepo.Commit();
                if (doSendToClient)
                {
                    NotifyUsersLoadNotifications();                    
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public object GetNumberNotification(string drugStoreID, bool isSupperAdmin)
        {
            var targetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var qTarget = from target in targetRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID && e.NotificationReadTypeID == (int)Med.Common.Enums.Notification.NotificationReadTypeId.UnRead)
                          join notifi in notificationRepo.GetAll()
                          on target.NotificationID equals notifi.ID
                          select new
                          {
                              target.ID,
                              target.NotificationDate,
                              notifi.NotificationTypeID
                          };
            var resultQuery = qTarget.OrderByDescending(e => e.ID).ToList();
            return new
            {
                total = resultQuery.Count,
                hasNotificationHot = resultQuery.FindIndex(e => e.NotificationTypeID == (int)Med.Common.Enums.Notification.NotificationTypeId.HotNews || e.NotificationTypeID == (int)Med.Common.Enums.Notification.NotificationTypeId.Warning) >= 0,
                isSupperAdmin = isSupperAdmin
            };
        }
        public object GetListNotificationForView(string drugStoreID)
        {
            var targetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var qTarget = from target in targetRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID).OrderByDescending(e => e.NotificationDate).Take(10)
                          join notifi in notificationRepo.GetAll()
                          on target.NotificationID equals notifi.ID
                          select new
                          {
                              target.ID,
                              notifi.Title,
                              notifi.Link,
                              target.NotificationDate,
                              notifi.NotificationTypeID,
                              target.NotificationReadTypeID
                          };
            var lstResult = qTarget.ToList().Select(e => new
            {
                e.ID,
                e.Title,
                e.Link,
                NotificationDate = e.NotificationDate.ToString("dd/MM/yyyy HH:mm:ss"),
                e.NotificationTypeID,
                e.NotificationReadTypeID
            }).ToList();
            if (lstResult.Count > 0)
            {
                var lstNotifiUnRead = targetRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID && e.NotificationReadTypeID == (int)Med.Common.Enums.Notification.NotificationReadTypeId.UnRead).ToList();
                foreach (var target in lstNotifiUnRead)
                {
                    target.NotificationReadTypeID = (int)Med.Common.Enums.Notification.NotificationReadTypeId.Read;
                    target.ReadDate = DateTime.Now;
                    targetRepo.Update(target);
                }
                targetRepo.Commit();
            }
            return lstResult;
        }
        public bool EvictNotification(int id)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var notificationTargetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
            var notifi = notificationRepo.GetAll().Where(e => e.ID == id && e.Status == (int)Med.Common.Enums.Notification.NotificationStatusTypeId.Released).FirstOrDefault();
            if (notifi == null)
                return false;
            else
            {
                try
                {
                    notificationTargetRepo.DeleteWhere(x => x.NotificationID == notifi.ID);
                    notificationTargetRepo.Commit();
                    notificationRepo.Delete(notifi);
                    notificationRepo.Commit();
                    return true;
                }
                catch
                {

                    return false;
                }
            }
        }
        //Hàm dùng cho các cảnh báo warning
        public void SendNotificationWarning(string drugStoreID, string title, string link, int resourceId = 0)
        {
            try
            {
                var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
                var notificationTargetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
                var notifiWarningOfDrugStore = notificationRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID
                    && e.NotificationTypeID == (int)Med.Common.Enums.Notification.NotificationTypeId.Warning && e.ResourceID == resourceId).Select(e => e.ID).ToList();
                if (notifiWarningOfDrugStore.Count > 0)
                {
                    //xóa tất cả các thông báo cảnh báo của nhà thuốc
                    notificationTargetRepo.DeleteWhere(e => notifiWarningOfDrugStore.Contains(e.NotificationID));
                    notificationRepo.DeleteWhere(e => notifiWarningOfDrugStore.Contains(e.ID));
                    notificationTargetRepo.Commit();
                    notificationRepo.Commit();
                }
                var notifiAdd = new Notification()
                {
                    DrugStoreID = drugStoreID,
                    Title = title,
                    Link = link,
                    NotificationTypeID = (int)Med.Common.Enums.Notification.NotificationTypeId.Warning,
                    Status = (int)Med.Common.Enums.Notification.NotificationStatusTypeId.UnReleased,
                    ResourceID = resourceId
                };
                CreateNotification(notifiAdd);
                ReleaseNotification(notifiAdd.ID, false);
            }
            catch
            {

            }
        }
        public void DeleteNotificationByResourceId(string drugStoreID, int resourceId = 0)
        {
            var notificationRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Notification>>();
            var notificationTargetRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NotificationTarget>>();
            var notifiWarningOfDrugStore = notificationRepo.GetAll().Where(e => e.DrugStoreID == drugStoreID
                && e.NotificationTypeID == (int)Med.Common.Enums.Notification.NotificationTypeId.Warning && e.ResourceID == resourceId).Select(e => e.ID).ToList();
            if (notifiWarningOfDrugStore.Count > 0)
            {
                //xóa tất cả các thông báo cảnh báo của nhà thuốc
                notificationTargetRepo.DeleteWhere(e => notifiWarningOfDrugStore.Contains(e.NotificationID));
                notificationRepo.DeleteWhere(e => notifiWarningOfDrugStore.Contains(e.ID));
                notificationTargetRepo.Commit();
                notificationRepo.Commit();
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
