using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuXuatRepository:GenericRepository<PhieuXuat>
    {
        public PhieuXuatRepository(SecurityContext context) : base(context)
        {
        }
    }
}