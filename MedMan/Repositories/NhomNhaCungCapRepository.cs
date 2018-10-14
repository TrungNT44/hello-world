using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhomNhaCungCapRepository:GenericRepository<NhomNhaCungCap>
    {
        public NhomNhaCungCapRepository(SecurityContext context) : base(context)
        {
        }
    }
}