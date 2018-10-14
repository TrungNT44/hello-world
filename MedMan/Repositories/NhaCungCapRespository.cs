using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhaCungCapRespository:GenericRepository<NhaCungCap>
    {
        public NhaCungCapRespository(SecurityContext context) : base(context)
        {
        }
    }
}