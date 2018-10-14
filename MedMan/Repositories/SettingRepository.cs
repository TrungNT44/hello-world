using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class SettingRepository:GenericRepository<Setting>
    {
        public SettingRepository(SecurityContext context)
            : base(context)
        {
        }
    }
}