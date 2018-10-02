using Med.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.Service.Recruitment
{
    public interface IRecruitService
    {
        object GetListProvinces();
        object GetListDrugStores();
        TuyenDungs GetRecruitInfo(int id);
        bool CreateRecruit(TuyenDungs inputTuyenDung);
        bool UpdateRecruit(TuyenDungs inputTuyenDung);
        bool DeleteRecruit(int IdRecruit);
        bool ActiveRecruit(int IdRecruit, string sDrugStoreCode);
        object GetListRecruitsOfDrugStore(string sDrugStoreCode, string TieuDe);
        List<TuyenDungs> GetListRecruitActive(string TieuDe, int? IdTinhThanh, string page, string pageSize);
    }
}
