using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuNhapRepository:GenericRepository<PhieuNhap>
    {
        public PhieuNhapRepository(SecurityContext context) : base(context)
        {
        }
    }
}