using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Drug;

namespace Med.Service.Drug
{
    public interface IDrugGroupService
    {
        // lưu thông tin nhóm thuốc
        int SaveDrugGroup(string maNhaThuoc, int userId, GroupDrugInfo model);

        GroupDrugInfo GetGroupDrugInfo(string maNhaThuoc, int? maNhomThuoc);
    }
}
