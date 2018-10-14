
using System;
using System.Collections.Generic;
using System.Linq;
using App.Common;
using App.Common.Base;
using App.Common.DI;
using App.Common.Helpers;
using App.Configuration;
using App.Constants.Enums;
using Med.Common;
using Med.Service.Drug;
using Med.Service.Registration;
using Med.Service.Report;
using Med.Service.System;
using Med.ServiceModel.CacheObjects;
using Med.Web.Data.Session;
using System.Collections.Concurrent;
using WebGrease.Css.Extensions;
using Med.Service.Common;
using Med.ServiceModel.Admin;
using Med.Service.Caching;
using Med.Web.Helpers;

namespace MedMan
{
    public class MainApp : AppBase
    {
        #region Fields
        private bool _shallNotifyUsersUpdateNewestFeatures = true;
        private readonly MedCacheManager _medCacheManager = null;
        #endregion

        #region Constants
        #endregion

        #region Constructors
        protected MainApp(WebSessionManager sessionManager = null):base(sessionManager)
        {
            RunningMode = AppRunningMode.Production;
            _medCacheManager = MedCacheManager.Instance;
        }

        #endregion

        #region Properties
        public new static MainApp Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MainApp(new WebSessionManager());
                        }
                    }
                }

                return (MainApp)_instance;
            }
        }

        public new WebSessionManager SessionManager
        {
            get { return (WebSessionManager)Instance._sessionManager; }
        }
        #endregion

        #region Private Methods
        #endregion

        #region Overridden Methods
        public override void Initialize(ApplicationType appType = ApplicationType.Console, object realAppInstance = null, string appName = "")
        {
            base.Initialize(appType, realAppInstance, appName);
            LogHelper.InitLogHelper();
        }

        public override void StartCaching()
        {
            base.StartCaching();
            _medCacheManager.FlushCache();
        }

        #endregion

        #region Public Methods
        public UserAccount GetCachedUserAccount(int userId, bool forceReload = false)
        {
            return _medCacheManager.GetCachedUserAccount(userId, forceReload);
        }

        public UserAccount CacheUserAccount(int userId)
        {
            return _medCacheManager.CacheUserAccount(userId);
        }

        public UserAccount GetCachedUserAccountV2(int userId, bool forceReload = false)
        {
            return _medCacheManager.GetCachedUserAccountV2(userId, forceReload);
        }

        public UserAccount CacheUserAccountV2(int userId)
        {
            return _medCacheManager.CacheUserAccountV2(userId);
        }

        public List<CacheDrug> GetCacheDrugs(string drugStoreCode, params int[] drugIds)
        {
            return _medCacheManager.GetCacheDrugs(drugStoreCode, drugIds);
        }

        public void ReloadCacheDrugs(string drugStoreCode)
        {
            _medCacheManager.ReloadCacheDrugs(drugStoreCode);
        }

        public List<CacheDrug> GetCacheDrugs(string drugStoreCode, string searchingText = "", int maxItems = 50)
        {
            return _medCacheManager.GetCacheDrugs(drugStoreCode, searchingText, maxItems);
        }
        public void NotifyUsersUpdateNewestFeatures()
        {
            if (!_shallNotifyUsersUpdateNewestFeatures) return;
            var service = IoC.Container.Resolve<INotificationBaseService>();
            service.NotifyUsersUpdateNewestFeatures();
        }
        #endregion
    }
}
