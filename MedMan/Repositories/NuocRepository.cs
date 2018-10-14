using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NuocRepository:GenericRepository<Nuoc>
    {
        public NuocRepository(SecurityContext context) : base(context)
        {
        }
    }
}