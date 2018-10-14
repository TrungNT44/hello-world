using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class TongKetKyChiTietRepository : GenericRepository<TongKetKyChiTiet>
    {
        public TongKetKyChiTietRepository(SecurityContext context)
            : base(context)
        {
        }
    }
}