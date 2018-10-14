using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class BacSyRespository:GenericRepository<BacSy>
    {
        public BacSyRespository(SecurityContext context) : base(context)
        {
        }
    }
}