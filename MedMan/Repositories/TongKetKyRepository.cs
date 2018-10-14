using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class TongKetKyRepository : GenericRepository<TongKetKy>
    {
        public TongKetKyRepository(SecurityContext context)
            : base(context)
        {
        }
    }
}