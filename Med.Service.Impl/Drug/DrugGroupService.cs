using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Common.Data;
using App.Common.DI;
using Med.DbContext;
using Med.Entity;
using Med.Service.Base;
using Med.Service.Drug;
using Med.ServiceModel.Drug;

namespace Med.Service.Impl.Drug
{
    public class DrugGroupService : MedBaseService, IDrugGroupService
    {
        public int SaveDrugGroup(string maNhaThuoc, int userId, GroupDrugInfo model)
        {
            int retval = -1;
            NhomThuoc newNhomThuoc = new NhomThuoc
            {
                MaNhaThuoc = maNhaThuoc,
                CreatedBy_UserId = userId,
                Created = DateTime.Now,
                TenNhomThuoc = model.TenNhomThuoc,
                KyHieuNhomThuoc = model.KyHieuNhomThuoc
            };

            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>();
            nhomThuocRepo.Insert(newNhomThuoc);
            nhomThuocRepo.Commit();
            retval = newNhomThuoc.MaNhomThuoc;

            return retval;
        }

        public GroupDrugInfo GetGroupDrugInfo(string maNhaThuoc, int? maNhomThuoc)
        {
            var nhomThuocRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, NhomThuoc>>().GetAll();
            var query = from nt in nhomThuocRepo
                        where (nt.MaNhaThuoc == maNhaThuoc && nt.MaNhomThuoc == maNhomThuoc)
                        select new GroupDrugInfo
                        {
                            MaNhaThuoc = nt.MaNhaThuoc,
                            MaNhomThuoc = nt.MaNhomThuoc,
                            KyHieuNhomThuoc = nt.KyHieuNhomThuoc,
                            TenNhomThuoc = nt.TenNhomThuoc
                        };

            var groupDrugInfo = query.FirstOrDefault();

            return groupDrugInfo;
        }
    }
}
