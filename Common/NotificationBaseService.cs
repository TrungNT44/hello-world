using System.Collections.Generic;
using System.Linq;
using App.Common.Data;
using Med.Entity.Registration;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Common;
using Med.ServiceModel.Drug;
using Med.ServiceModel.Common;
using System;
using Med.ServiceModel.Report;
using System.Threading.Tasks;
using App.Common.FaultHandling;
using App.Configuration;
using App.Common.Helpers;
using App.Common.Base;
using Med.Common;

namespace Med.Service.Impl.Common
{
    public class NotificationBaseService : MedBaseService, INotificationBaseService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public bool NotifyToUsers(string callBackFunctionName, object attachmentParams,  List<int> userIds = null)
        {
            try
            {
                LogHelper.Debug("Start notify 4 clients. Function: {0}. Params: {1}", callBackFunctionName, attachmentParams);
                var destinationIds = new List<int>();
                if (userIds != null && userIds.Count > 0) // Notify to specific users
                {
                    destinationIds.AddRange(userIds);
                }
                else // Notify to all users
                {
                    destinationIds.Add(MedConstants.WebAppId);
                }

                Task.Run(() =>
                {
                    string webSocketUrl = MachineConfig.Instance.Config.WebSocketUrl.Value;
                    WebSocketHelper.Send(webSocketUrl, MedConstants.WebAppName, destinationIds, callBackFunctionName, attachmentParams);
                });
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this);
                return false;
            }

            return true;
        }        
        public bool NotifyUsersUpdateNewestFeatures()
        {
            return NotifyToUsers("reloadWebApp", "Reload web app", null);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
