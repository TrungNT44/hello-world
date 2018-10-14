using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhomThuocRepository:GenericRepository<NhomThuoc>
    {
        public NhomThuocRepository(SecurityContext context) : base(context)
        {
        }
    }
}