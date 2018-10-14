using System.Collections.Generic;
using System.Linq;
using App.Common;
using Med.DbContext;
using Med.Entity;
using Med.Repository.Factory;
using Med.Service.Base;
using Med.Service.Drug;
using App.Common.Validation;
using App.Common.Data;
using App.Common.Helpers;
using System;
using App.Common.FaultHandling;
using App.Common.DI;
using Med.ServiceModel.Common;
using App.Common.Extensions;

namespace Med.Service.Impl.Drug
{
    public class DrugStoreService : MedBaseService, IDrugStoreService
    {
        #region Fields
        #endregion

        #region Interface Implementation
        public List<string> GetValidDrugStoreCodes()
        {
            var drugstoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var drugstores = drugstoreRepo.GetAll().Where(s => s.HoatDong).OrderBy(i => i.Created).Select(s => s.MaNhaThuoc).ToList();

            return drugstores;
        }
        public List<string> GetOwnerDrugStoreCodes(string drugStoreCode)
        {
            var drugstoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var ownerDrugStoreCodes = drugstoreRepo.GetAll()
                .Where(i => i.MaNhaThuoc == drugStoreCode || i.MaNhaThuocCha == drugStoreCode)
                .Select(i => i.MaNhaThuoc).Distinct().ToList();

            return ownerDrugStoreCodes;
        }
        public bool DeleteDrugStore(string drugStoreCode)
        {
            var retVal = false;
            try
            {
                using (var tran = TransactionScopeHelper.CreateLockAllForWrite())                
                {
                    var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
                    drugStoreRepo.UpdateMany(i => i.MaNhaThuoc == drugStoreCode || i.MaNhaThuocCha == drugStoreCode,
                        i => new NhaThuoc()
                        {
                            HoatDong = false
                        });

                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, "DeleteDrugStore");
            }
            return retVal;
        }

        public bool RollbackDrugStore(string drugStoreCode)
        {
            var retVal = false;
            try
            {
                using (var tran = TransactionScopeHelper.CreateLockAllForWrite())               
                {
                    var ownerDrugStoreCodes = GetOwnerDrugStoreCodes(drugStoreCode);
                    var drugStoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
                    if (ownerDrugStoreCodes.Any())
                    {
                        drugStoreRepo.UpdateMany(i => ownerDrugStoreCodes.Contains(i.MaNhaThuoc),
                            i => new NhaThuoc()
                            {
                                HoatDong = true
                            });

                        var userRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>();
                        userRepo.UpdateMany(i => ownerDrugStoreCodes.Contains(i.MaNhaThuoc),
                            i => new UserProfile()
                            {
                                Enable_NT = true
                            });
                    }

                    tran.Complete();
                }
            }
            catch (Exception ex)
            {
                FaultHandler.Instance.Handle(ex, this, "DeleteDrugStore");
            }
            return retVal;
        }

        public List<DrugStoreInfo> GetRelatedDrugStores(string drugStoreCode, bool excludeCurrentDrugStore = false)
        {
            var drugstoreRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhaThuoc>>();
            var parentDrugStoreCode = drugstoreRepo.GetAll()
                .Where(i => i.MaNhaThuoc == drugStoreCode).Select(i => i.MaNhaThuocCha).FirstOrDefault();
            var results = drugstoreRepo.GetAll().Where(i => i.MaNhaThuocCha == parentDrugStoreCode)
                .Select(i => new DrugStoreInfo()
                {
                    DrugStoreCode = i.MaNhaThuoc,
                    DrugStoreName = i.TenNhaThuoc,
                    IsParent = i.MaNhaThuoc == i.MaNhaThuocCha
                }).ToList().DistinctBy(i => i.DrugStoreCode).ToList();
            if (excludeCurrentDrugStore)
            {
                results = results.Where(i => i.DrugStoreCode != drugStoreCode).ToList();
            }

            return results;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
