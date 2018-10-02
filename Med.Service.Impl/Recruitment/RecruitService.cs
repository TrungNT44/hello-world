using Med.Service.Recruitment;
using System.Collections.Generic;
using System.Linq;
using Med.Entity;
using Med.Service.Common;
using App.Common.Data;
using Med.DbContext;
using Med.Service.Impl.Common;
using System;
using Med.Repository.Factory;
using App.Common;
using System.Data.Entity.SqlServer;
using System.Globalization;
using Med.ServiceModel.Recruitment;
using Med.ServiceModel.Response;
using App.Common.DI;
using Med.Service.Base;

namespace Med.Service.Impl.Recruitment
{
    public class RecruitService : MedBaseService, IRecruitService
    {
        public object GetListDrugStores()
        {
            IDataFilterService serviceImp = new DataFilterService();
            IBaseRepository<NhaThuoc> drugStoreRepo = null;
            var validItems = serviceImp.GetValidDrugStores(out drugStoreRepo);
            var items = validItems.Select(x => new {
                x.MaNhaThuoc,
                x.TenNhaThuoc,
                x.DiaChi,
                x.NguoiDaiDien,
                x.Email,
                x.DienThoai,
                x.TinhThanhId
            }).ToList();
            return items;
        }

        public object GetListProvinces()
        {
            IDataFilterService serviceImp = new DataFilterService();
            var validItems = serviceImp.GetListProvinces();
            var items = validItems.Select(x => new {
                x.IdTinhThanh,
                x.TenTinhThanh
            }).ToList();
            return items;
        }
        public bool CreateRecruit(TuyenDungs inputTuyenDung)
        {
            try
            {
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
                DateTime expireDate = new DateTime();
                DateTime postDate = new DateTime();
                DateTime.TryParseExact(inputTuyenDung.NgayDang_View.Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out postDate);
                if (DateTime.TryParseExact(inputTuyenDung.NgayHetHan_View.Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out expireDate))
                {
                    var itemActive = GetRecruitActive(inputTuyenDung.MaNhaThuoc);
                    inputTuyenDung.NgayHetHan = expireDate;
                    inputTuyenDung.NgayDang = postDate;
                    inputTuyenDung.HoatDong = true;
                    if (inputTuyenDung.HoatDong)
                    {
                        inputTuyenDung.NgaySetUuTien = DateTime.Now;
                    }
                    drugRepo.Add(inputTuyenDung);
                    drugRepo.Commit();
                  
                    if (itemActive != null && inputTuyenDung.HoatDong && inputTuyenDung.MaNhaThuoc != "0011")
                    {
                        itemActive.HoatDong = false;
                        drugRepo.Update(itemActive);
                        drugRepo.Commit();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public bool UpdateRecruit(TuyenDungs inputTuyenDung)
        {
            try
            {
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
                var td = GetRecruitInfo(inputTuyenDung.IdTinTuyenDung);
                if (td == null)
                    return false;
                DateTime expireDate = new DateTime();
                DateTime postDate = new DateTime();
                DateTime.TryParseExact(inputTuyenDung.NgayDang_View.Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out postDate);
                if (DateTime.TryParseExact(inputTuyenDung.NgayHetHan_View.Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out expireDate))
                {
                    var itemActive = GetRecruitActive(inputTuyenDung.MaNhaThuoc);
                    inputTuyenDung.NgayHetHan = expireDate;
                    inputTuyenDung.NgayDang = postDate;
                    inputTuyenDung.NgaySetUuTien = td.NgaySetUuTien;
                    drugRepo.Update(inputTuyenDung);
                    drugRepo.Commit();
                    if (itemActive != null && itemActive.IdTinTuyenDung != inputTuyenDung.IdTinTuyenDung && inputTuyenDung.HoatDong && inputTuyenDung.MaNhaThuoc != "0011")
                    {
                        itemActive.HoatDong = false;
                        drugRepo.Update(itemActive);
                        drugRepo.Commit();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool DeleteRecruit(int IdRecruit)
        {
            try
            {
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
                var td = GetRecruitInfo(IdRecruit);
                if (td == null)
                    return false;
                drugRepo.Delete(td);
                drugRepo.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool ActiveRecruit(int IdRecruit, string sDrugStoreCode)
        {
            try
            {
                var drugRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
                var td = GetRecruitInfo(IdRecruit);
                if (td == null)
                    return false;
                if (td.MaNhaThuoc != "0011")
                {
                    if (td.NgaySetUuTien.HasValue && td.NgaySetUuTien.Value.Date == DateTime.Now.Date)
                    {
                        return false;
                    }
                }
                td.NgaySetUuTien = DateTime.Now;
                drugRepo.Update(td);
                drugRepo.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public TuyenDungs GetRecruitInfo(int id)
        {
            var tdRes = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
            var tdTinhThanh = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TinhThanhs>>();
            var item = (from td in tdRes.GetAll()
                        where td.IdTinTuyenDung == id
                        select td
                    ).FirstOrDefault();
            if (item != null && item.NgayHetHan != null)
            {
                item.NgayHetHan_View = item.NgayHetHan.ToString("dd/MM/yyyy");
                item.NgayDang_View = item.NgayDang.ToString("dd/MM/yyyy");
            }
            item.TenTinhThanh = tdTinhThanh.GetAll().Where(x => x.IdTinhThanh == item.IdTinhThanh).FirstOrDefault().TenTinhThanh;
            return item;
        }
        public TuyenDungs GetRecruitActive(string DrugStoreCode)
        {
            var tdRes = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
            var item = (from td in tdRes.GetAll()
                        where td.MaNhaThuoc == DrugStoreCode && td.HoatDong == true
                        select td
                    ).FirstOrDefault();
            return item;
        }
        public object GetListRecruitsOfDrugStore(string sDrugStoreCode, string TieuDe)
        {
            ListRecruitsResponse res = new ListRecruitsResponse();
            var tdRes = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
            var itemsRef = (from td in tdRes.GetAll()
                            where td.MaNhaThuoc == sDrugStoreCode
                            select td
                    );
            if (!string.IsNullOrEmpty(TieuDe))
            {
                itemsRef = itemsRef.Where(x => x.TieuDe.Contains(TieuDe));
            }
            itemsRef = itemsRef.OrderByDescending(x => x.HoatDong);
            var items = itemsRef.ToList();
            int count = 1;
            foreach (var item in items)
            {
                item.STT = count++;
                if (item.NgayHetHan != null)
                {
                    item.NgayHetHan_View = item.NgayHetHan.ToString("dd/MM/yyyy");
                    item.NgayDang_View = item.NgayDang.ToString("dd/MM/yyyy");
                }
            }
            res.Total = items.Count;
            res.PagingResultModel = new PagingResultModel<TuyenDungs>(items, items.Count);
            return res;
        }
        public List<TuyenDungs> GetListRecruitActive(string TieuDe, int? IdTinhThanh, string page, string pageSize)
        {
            var tdRes = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TuyenDungs>>();
            var tdTinhThanh = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, TinhThanhs>>();
            var itemsRef = tdRes.GetAll().Where(x => x.HoatDong && x.NgayHetHan >= DateTime.Now);
            if (!string.IsNullOrEmpty(TieuDe))
            {
                itemsRef = itemsRef.Where(x => x.TieuDe.Contains(TieuDe) || x.DiaChiNhaTuyenDung.Contains(TieuDe));
            }
            if (IdTinhThanh.HasValue)
            {
                itemsRef = itemsRef.Where(x => x.IdTinhThanh == IdTinhThanh.Value);
            }
            itemsRef = itemsRef.OrderByDescending(x => x.NgaySetUuTien);
            if (!string.IsNullOrEmpty(page) && !string.IsNullOrEmpty(pageSize))
            {
                int ipage = int.Parse(page.Trim());
                int ipage_size = int.Parse(pageSize.Trim());
                itemsRef = itemsRef.Skip((ipage - 1) * ipage_size).Take(ipage_size);
            }
            var items = itemsRef.ToList();
            int count = 1;
            foreach (var item in items)
            {
                item.STT = count++;
                if (item.NgayHetHan != null)
                {
                    item.NgayHetHan_View = item.NgayHetHan.ToString("dd/MM/yyyy");
                    item.NgayDang_View = item.NgayDang.ToString("dd/MM/yyyy");
                }
                item.TenTinhThanh = tdTinhThanh.GetAll().Where(x => x.IdTinhThanh == item.IdTinhThanh).FirstOrDefault().TenTinhThanh;
            }
            return items;
        }
    }
}
