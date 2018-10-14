using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class LoaiXuatNhapRepository:GenericRepository<LoaiXuatNhap>
    {
        public LoaiXuatNhapRepository(SecurityContext context) : base(context)
        {
        }
    }
}