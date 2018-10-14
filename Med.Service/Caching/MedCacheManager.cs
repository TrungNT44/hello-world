using System;
using System.Collections.Generic;
using System.Linq;
using App.Common;
using App.Common.DI;
using App.Common.Helpers;
using Med.Common;
using Med.Service.Drug;
using Med.Service.Registration;
using Med.Service.System;
using Med.ServiceModel.CacheObjects;
using System.Collections.Concurrent;
using Med.Service.Common;
using Med.ServiceModel.Admin;
using Castle.Core.Internal;
using App.Common.Base;
using Med.Service.Report;

namespace Med.Service.Caching
{
    public class MedCacheManager
    {
        #region Fields
        private static ConcurrentDictionary<string, List<CacheDrug>> _drugDictionaryCache = new ConcurrentDictionary<string, List<CacheDrug>>();
        private static ConcurrentDictionary<int, UserAccount> _userDictionaryCache = new ConcurrentDictionary<int, UserAccount>();
        protected static readonly object Padlock = new object();
        private static MedCacheManager _instance = null;

        #endregion

        #region Constants
        #endregion

        #region Constructors
        protected MedCacheManager()
        {            
        }

        #endregion

        #region Properties
        public static MedCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MedCacheManager();
                        }
                    }
                }

                return (MedCacheManager)_instance;
            }
        }
        #endregion

        #region Private Methods

        private void StartCacheDrugs()
        {
            var service = IoC.Container.Resolve<IDrugManagementService>();
            var cacheDrugs =  service.GetAllCacheDrugs();
            if (cacheDrugs.Any())
            {
                cacheDrugs.ForEach(i =>
                {
                    _drugDictionaryCache.TryAdd(i.Key, i.Value);
                });
            }
        }

        private List<CacheDrug> GetCacheDrugsV1(string drugStoreCode, string searchingText = "", int maxItems = 10)
        {
            var results = new List<CacheDrug>();
            if (_drugDictionaryCache.ContainsKey(drugStoreCode))
            {
                results = _drugDictionaryCache[drugStoreCode];
            }
            if (!string.IsNullOrEmpty(searchingText))
            {
                var lowerSearchingText = searchingText.ToUpper();
                results = results.Where(i =>
                    i.FullInfo.ToLower().Contains(lowerSearchingText)).ToList();
            }
            results = results.Take(maxItems).ToList();

            return results;
        }
        #endregion

        #region Overridden Methods
        #endregion

        #region Public Methods
        public UserAccount GetCachedUserAccount(int userId, bool forceReload = false)
        {
            UserAccount result = null;
            var key = GetCacheKeyForUserAccount(userId);
            var cacheManager = AppBase.Instance.RedisCacheManager;
            if (forceReload || !cacheManager.IsSet(key))
            {
                result = CacheUserAccount(userId);
                return result;
            }

            result = cacheManager.Get<UserAccount>(key);

            return result;
        }

        public UserAccount CacheUserAccount(int userId)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var key = GetCacheKeyForUserAccount(userId);
            var service = IoC.Container.Resolve<IUserService>();
            var result = service.GetUserAccount(userId);
            cacheManager.Set(key, result, MedConstants.DrugCacheTimeInMinutes);

            return result;
        }

        public UserAccount GetCachedUserAccountV2(int userId, bool forceReload = false)
        {
            UserAccount result = null;
            if (forceReload || !_userDictionaryCache.ContainsKey(userId))
            {
                result = CacheUserAccountV2(userId);
                return result;
            }

            result = _userDictionaryCache[userId];

            return result;
        }

        public UserAccount CacheUserAccountV2(int userId)
        {
            UserAccount result = null;
            if (_userDictionaryCache.ContainsKey(userId))
            {                
                _userDictionaryCache.TryRemove(userId, out result);
            }
            var service = IoC.Container.Resolve<IUserService>();
            result = service.GetUserAccount(userId);
            if (result != null)
            {
                _userDictionaryCache.TryAdd(userId, result);
            }

            return result;
        }
        public List<CacheDrug> GetCacheDrugs(string drugStoreCode, params int[] drugIds)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var results = new List<CacheDrug>();
            var key = GetCacheKeyForDrugStore(drugStoreCode);
            if (cacheManager.IsSet(key))
            {
                results = cacheManager.Get<List<CacheDrug>>(key);
            }
            else
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                results = service.GetCacheDrugs(drugStoreCode);
                cacheManager.Set(key, results, MedConstants.DrugCacheTimeInMinutes);
            }

            if (drugIds != null && drugIds.Length >0)
            {
                results = results.Where(i => drugIds.Contains(i.DrugId)).ToList();
            }

            return results;
        }
        public void UpdateCacheDrugs(string drugStoreCode, List<CacheDrug> drugs)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;           
            var key = GetCacheKeyForDrugStore(drugStoreCode);
            var alllDrugs = GetAllCacheDrugs(drugStoreCode);
            var updateDrugIds = drugs.Select(i => i.DrugId).ToList();
            var updateDrugs = alllDrugs.Where(i => updateDrugIds.Contains(i.DrugId)).ToList();
            var existingCacheDrugIds = alllDrugs.Select(i => i.DrugId).Distinct().ToList();
            var newDrugs = drugs.Where(i => !existingCacheDrugIds.Contains(i.DrugId)).ToList();
            updateDrugs.ForEach(i =>
            {
                var drug = drugs.FirstOrDefault(d => d.DrugId == i.DrugId);
                if (drug != null)
                {
                    alllDrugs.Remove(i);
                    alllDrugs.Add(drug);
                }
            });

            newDrugs.ForEach(i => alllDrugs.Add(i));

            cacheManager.Set(key, alllDrugs, MedConstants.DrugCacheTimeInMinutes);
        }
        public void ReloadCacheDrugs(string drugStoreCode)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var key = GetCacheKeyForDrugStore(drugStoreCode);
            var service = IoC.Container.Resolve<IDrugManagementService>();
            var drugs = service.GetCacheDrugs(drugStoreCode);
            cacheManager.Set(key, drugs, MedConstants.DrugCacheTimeInMinutes);
        }
        public List<CacheDrug> GetAllCacheDrugs(string drugStoreCode)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var results = new List<CacheDrug>();
            var key = GetCacheKeyForDrugStore(drugStoreCode);
            if (cacheManager.IsSet(key))
            {
                results = cacheManager.Get<List<CacheDrug>>(key);
            }
            else
            {
                var service = IoC.Container.Resolve<IDrugManagementService>();
                results = service.GetCacheDrugs(drugStoreCode);
                cacheManager.Set(key, results, MedConstants.DrugCacheTimeInMinutes);
            }           

            return results;
        }
        public List<CacheDrug> GetCacheDrugs(string drugStoreCode, string searchingText = "", int maxItems = 50)
        {
            var results = GetAllCacheDrugs(drugStoreCode);           

            if (!string.IsNullOrEmpty(searchingText))
            {
                var lowerSearchingText = searchingText.ToLower();
                results = results.Where(i => i.GetFullSearchString().Contains(lowerSearchingText)).ToList();
            }
            results = results.Take(maxItems).ToList();
             
            return results;
        }
        public void UpdateCacheDrug(string drugStoreCode, int drugId, string drugCode, string drugName, string extraInfo, double retailOutPrice)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var key = GetCacheKeyForDrugStore(drugStoreCode);
            if (cacheManager.IsSet(key))
            {
                var cacheDrugs = cacheManager.Get<List<CacheDrug>>(key);
                var drug = cacheDrugs.Where(i => i.DrugId == drugId).FirstOrDefault();
                if (drug != null)
                {
                    drug.DrugCode = drugCode;
                    drug.DrugName = drugName;
                    drug.ExtraInfo = extraInfo;
                    drug.RetailOutPrice = retailOutPrice;
                }
                else
                {
                    cacheDrugs.Add(new CacheDrug()
                    {
                        DrugId = drugId,
                        DrugCode = drugCode,
                        DrugName = drugName,
                        ExtraInfo = extraInfo,
                        RetailOutPrice = retailOutPrice
                    });
                }
                var invService = IoC.Container.Resolve<IInventoryService>();
                invService.UpdateInventoryDrugPrices(drugCode, drugId, retailOutPrice);

                cacheManager.Set(key, cacheDrugs, MedConstants.DrugCacheTimeInMinutes);
            }
        }        
        public int GetMessagesCount(string drugStoreCode)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var messagesCount = 0;
            var key = AppBase.Instance.CacheKeyPrefix + string.Format("CacheKeyForMessagesCount-{0}-{1}", drugStoreCode, DateTime.Now.ToShortDateString());
            if (cacheManager.IsSet(key))
            {
                messagesCount = cacheManager.Get<int>(key);
            }
            else
            {
                var service = IoC.Container.Resolve<ISystemService>();
                messagesCount = service.GetSystemMessagesCount(drugStoreCode);
                cacheManager.Set(key, messagesCount, MedConstants.SystemMessagesCountCacheTimeInMinutes);
            }

            return messagesCount;
        }

        public bool HasRemainQuantityCaching(string drugStoreCode)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;
            bool retVal = false;
            var key = GetCacheKeyForCheckingRemainQuantity(drugStoreCode);
            if (cacheManager.IsSet(key))
            {
                retVal = cacheManager.Get<bool>(key);
            }

            return retVal;
        }
        public void UpdateRemainQuantityCacheFlag(string drugStoreCode, bool val)
        {
            var cacheManager = AppBase.Instance.RedisCacheManager;

            var key = GetCacheKeyForCheckingRemainQuantity(drugStoreCode);
            cacheManager.Set(key, val, MedConstants.RemainQuantityCacheTimeInMinutes);
        }

        public void FlushCache()
        {
            var appService = IoC.Container.Resolve<IAppSettingService>();
            var isClearCache = appService.GetSettingIntValue(AppSettingKey.ResetCacheKey) > 0;
            if (isClearCache)
            {
                var cacheManager = AppBase.Instance.RedisCacheManager;
                cacheManager.ClearCurrentDB();
                appService.UpdateAppSettings(new[]{ new  Entity.ApplicationSetting()
                {
                    SettingKey = AppSettingKey.ResetCacheKey,
                    SettingValue = "0"
                }});
            }
        }

        public int GetInitialInventoryReceiptNoteID(string drugStoreID)
        {
            var retVal = 0;
            var cacheManager = AppBase.Instance.RedisCacheManager;
            var key = GetCacheKeyForInitialReceiptNoteID(drugStoreID);
            if (cacheManager.IsSet(key))
            {
                retVal = cacheManager.Get<int>(key);
            }
            if (retVal <= 0)
            {
                var service = IoC.Container.Resolve<IReportHelperService>();
                retVal = service.CreateInitialInventoryReceiptNote(drugStoreID);
            }
            if (retVal > 0)
            {
                cacheManager.Set(key, retVal, null);
            }

            return retVal;
        }
        #endregion

        #region Private Method
        private string GetCacheKeyForDrugStore(string drugStoreCode)
        {
            var key =AppBase.Instance.CacheKeyPrefix + string.Format("CacheKeyForDrugStore-{0}", drugStoreCode);

            return key;
        }
        private string GetCacheKeyForCheckingRemainQuantity(string drugStoreCode)
        {
            var key = AppBase.Instance.CacheKeyPrefix + string.Format("CacheKeyForCheckingRemainQuantity-{0}", drugStoreCode);

            return key;
        }
        private string GetCacheKeyForInitialReceiptNoteID(string drugStoreCode)
        {
            var key = AppBase.Instance.CacheKeyPrefix + string.Format("CacheKeyForGetCacheKeyForInitialReceiptNoteID-{0}", drugStoreCode);

            return key;
        }
        private string GetCacheKeyForUserAccount(int userId)
        {
            var key = AppBase.Instance.CacheKeyPrefix + string.Format("CacheKeyForUserAccount-{0}", userId);

            return key;
        }
        #endregion
    }
}
