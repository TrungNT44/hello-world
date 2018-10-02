using Med.Entity.Notifications;
using System.Collections.Generic;

namespace Med.Service.Notifications
{
    public interface INotificationService
    {
        bool NotifyUsersLoadNotifications();
        bool NotifyUsersUpdateNewestFeatures();
        bool CreateNotification(Notification notify);
        List<NotificationType> GetListNotificationType();
        object SearchNotification(string drugStoreID, int? nNotificationTypeID, string title, int pageIndex, int pageSize);
        Notification GetNotificationInfo(int id);
        bool UpdateNotification(Notification notify);
        bool DeleteNotification(int id);
        bool ReleaseNotification(int id, bool doSendToClient = true);
        object GetNotificationHistory(string drugStoreID, int? notificationTypeID, string title, int pageIndex, int pageSize);
        object GetNumberNotification(string drugStoreID, bool isSupperAdmin);
        object GetListNotificationForView(string drugStoreID);
        bool EvictNotification(int id);
        void SendNotificationWarning(string drugStoreID, string title, string link, int resourceId = 0);
        void DeleteNotificationByResourceId(string drugStoreID, int resourceId = 0);
    }
}