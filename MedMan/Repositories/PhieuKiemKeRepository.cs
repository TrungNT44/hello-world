using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuKiemKeRepository:GenericRepository<PhieuKiemKe>
    {
        public PhieuKiemKeRepository(SecurityContext context) : base(context)
        {
        }
    }
}