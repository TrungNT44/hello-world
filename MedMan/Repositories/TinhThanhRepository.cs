using sThuoc.DAL;
using sThuoc.Models;


namespace sThuoc.Repositories
{
    public class TinhThanhRepository : GenericRepository<TinhThanhs>
    {
        public TinhThanhRepository(SecurityContext context) : base(context)
        {
        }
    }
}