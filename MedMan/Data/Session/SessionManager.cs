using System.Collections.Generic;
using System.Web;
using System;
using System.Linq;
using App.Common.Session;
using Med.Common.Enums;
using Med.ServiceModel.Admin;
using MedMan;
using Med.Common;

namespace Med.Web.Data.Session
{
    public class WebSessionManager: SessionManagerBase
    {
        private static WebSessionManager _instance = null;
        protected static readonly object Padlock = new object();
        public static readonly string[] SupportedDrugStoreCodesForNewReports = new[] { "0055", "0065", "0020", "0022" };

        public static WebSessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = MainApp.Instance.SessionManager;
                        }
                    }
                }

                return (WebSessionManager)_instance;
            }
        }
        public int CurrentUserId
        {
            get
            {
                var value = (int?)GetSessionObject(SessionObjectEnum.CurrentUserId.ToString());
                return value.GetValueOrDefault();
            }
            set
            {
                SetSessionObject(SessionObjectEnum.CurrentUserId.ToString(), value);
            }
        }

        public string CurrentDrugStoreCode
        {
            get
            {
                var value = (string)GetSessionObject(SessionObjectEnum.CurrentDrugStoreCode.ToString());
                return value;
            }
            set
            {
                SetSessionObject(SessionObjectEnum.CurrentDrugStoreCode.ToString(), value);
            }
        }       
        public int NumberOfDrugStores
        {
            get
            {
                var value = (int?)GetSessionObject(SessionObjectEnum.NumberOfDrugStores.ToString());
                return value.GetValueOrDefault();
            }
            set
            {
                SetSessionObject(SessionObjectEnum.NumberOfDrugStores.ToString(), value);
            }
        }

        public override bool HasPermission(int action)
        {
            return true;
            //return HasPermission((HttpActionEnum)action);
        }

        public bool HasPermission(HttpActionEnum resourceId)
        {
            var currentUser = CurrentUser;
            return currentUser != null && currentUser.HasWritePermission(resourceId);
        }

        public bool HasReadPermission(HttpActionEnum resourceId)
        {
            var currentUser = CurrentUser;
            return currentUser != null && currentUser.HasReadPermission(resourceId);
        }

        public UserAccount CurrentUser
        {
            get
            {
                return CurrentUserId > 0 ? MainApp.Instance.GetCachedUserAccountV2(CurrentUserId) : null;
            }
        }

        public bool HasPermisionToAccessNewReports()
        {
            return SupportedDrugStoreCodesForNewReports.Contains(CurrentDrugStoreCode);
        }

        public bool HasPermisionToAccessDrugMappings()
        {
            return (CurrentDrugStoreCode == "0012");
        }
        public bool HasNotPermisionToAccessOldReports()
        {
            var supportedDrugStoreCodes = new[] { "0065", "0020", "0022" };
            return supportedDrugStoreCodes.Contains(CurrentDrugStoreCode);
        }
        public bool HasPermisionToAccessNewInventoryAdjustment()
        {
            var supportedDrugStoreCodes = new[] { "0055"};
            return supportedDrugStoreCodes.Contains(CurrentDrugStoreCode);
        }
    }
    public class MedSessionManager
    {
        public static bool IsSystemAdmin()
        {
            return WebSessionManager.Instance.CurrentUser.IsSystemAdmin();
        }
        public static bool HasPermission(HttpActionEnum resourceId)
        {
            return WebSessionManager.Instance.HasPermission(resourceId);
        }
        public static int CurrentUserId
        {
            get
            {
                return WebSessionManager.Instance.CurrentUserId;
            }
            set
            {
                WebSessionManager.Instance.CurrentUserId = value;               
            }
        }              
        public static UserAccount CurrentUser
        {
            get
            {
                return WebSessionManager.Instance.CurrentUser;
            }
        }
        public static DrugStoreSession DSSession
        {
            get
            {
                var sess = (DrugStoreSession)WebSessionManager.Instance.CommonSessionData;

                return sess;
            }
        }
        public static int? CurrentDrugStoreId
        {
            get
            {
                int? drugStoreId = null;
                var sess = (DrugStoreSession)WebSessionManager.Instance.CommonSessionData;
                if (sess != null)
                {
                    drugStoreId = sess.DrugStoreID;
                }

                return drugStoreId;
            }
        }
        public static string CurrentDrugStoreCode
        {
            get
            {
                string drugStoreCode = string.Empty;
                var sess = (DrugStoreSession)WebSessionManager.Instance.CommonSessionData;
                if (sess != null)
                {
                    drugStoreCode = sess.DrugStoreCode;
                }

                return drugStoreCode;
            }
        }
    }    
}