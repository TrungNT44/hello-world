
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuNhapChiTietRespository:GenericRepository<PhieuNhapChiTiet>
    {
        public PhieuNhapChiTietRespository(SecurityContext context) : base(context)
        {
        }
    }
}