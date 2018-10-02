using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Common.DI;
using Med.Service.Common;
using Med.Service.Report;
using Med.Common;
using App.Common.Base;
using Med.Service.System;
using Med.Service.Notifications;
using Med.Entity;
using App.Common.Data;

namespace Med.Service.Base
{
    public class MedBaseService: BaseService
    {
        #region Fields
        protected readonly IDataFilterService _dataFilterService;
        protected readonly ICommonService _commonService;
        #endregion

        #region Constructors
        public MedBaseService()
        {
            _dataFilterService = IoC.Container.Resolve<IDataFilterService>();
            _commonService = IoC.Container.Resolve<ICommonService>();
        }
        #endregion

        #region Interface Implementation
        #endregion

        #region Public Methods
        public void ValidateUpdateRequest(object request, Action<object> validationFunc)
        {
            if (validationFunc != null)
            {
                validationFunc(request);
            }
        }

        public void ValidateDeleteRequest(object request, Action<object> validationFunc)
        {
            if (validationFunc != null)
            {
                validationFunc(request);
            }
        }

        public void ValidateAddRequest(object request, Action<object> validationFunc)
        {
            if (validationFunc != null)
            {
                validationFunc(request);
            }
        }
        public DrugStoreSession GetDrugStoreSession()
        {
            try
            {
                var drugStoreSession = (DrugStoreSession)AppBase.Instance.SessionManager.CommonSessionData;

                return drugStoreSession;
            }
            catch
            {

            }

            return null;
        }
        public bool IsChildDrugStore(string drugStoreCode)
        {
            bool retVal = false;
            IBaseRepository<NhaThuoc> repository = null;
            var drugStoreInfo = _dataFilterService.GetValidDrugStores(out repository)
                .Where(i => i.MaNhaThuoc == drugStoreCode)
                .Select(i => new { DrugStoreCode = i.MaNhaThuoc, ParentDrugStoreCode = i.MaNhaThuocCha }
                ).FirstOrDefault();
            if (drugStoreInfo != null)
            {
                retVal = !string.IsNullOrWhiteSpace(drugStoreInfo.ParentDrugStoreCode) && drugStoreInfo.DrugStoreCode != drugStoreInfo.ParentDrugStoreCode;
            }

            return retVal;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Protected Methods
        protected void GenerateReportData(string drugStoreCode)
        {
            var rpDataService = IoC.Container.Resolve<IReportGenDataService>();
            var isGenSuccess = rpDataService.GenerateReceiptDrugPriceRefs(drugStoreCode);
            if (isGenSuccess)
            {
                var sysService = IoC.Container.Resolve<ISystemService>();
                sysService.GenerateSystemMessages(drugStoreCode);               
            }
            var notifyService = IoC.Container.Resolve<INotificationService>();
            notifyService.NotifyUsersLoadNotifications();
        }
        #endregion        
    }
}
