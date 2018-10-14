using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class DangBaoCheRepository:GenericRepository<DangBaoChe>
    {
        public DangBaoCheRepository(SecurityContext context) : base(context)
        {
        }
    }
}