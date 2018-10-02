using Med.Entity.Notifications;
using System.Collections.Generic;

namespace Med.Service.Common
{
    public interface INotificationBaseService
    {
        bool NotifyToUsers(string callBackFunctionName, object attachedParams, List<int> userIds = null);
        bool NotifyUsersUpdateNewestFeatures();
    }
}
