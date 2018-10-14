using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuThuChiRepository:GenericRepository<PhieuThuChi>
    {
        public PhieuThuChiRepository(SecurityContext context) : base(context)
        {
        }
    }
}