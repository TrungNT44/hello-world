using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class UserPermissionsRespository:GenericRepository<UserPermission>
    {
        public UserPermissionsRespository(SecurityContext context) : base(context)
        {
        }
    }
}