using System.Linq;
using sThuoc.DAL;

namespace sThuoc.Repositories
{
    public class MembershipRepository
    {
        private SecurityContext _context;

        public MembershipRepository(SecurityContext context)
        {
            _context = context;
        }

        public string GetConfirmationToken(int userId)
        {
            string cmd = "select ConfirmationToken from webpages_Membership where UserId = " + userId.ToString();
            return _context.Database.SqlQuery<string>(cmd).FirstOrDefault();
        }

        public string GetRoleName(int userId)
        {
            string cmd = "select RoleName from webpages_UsersInRoles inner join webpages_Roles on webpages_UsersInRoles.RoleId =  webpages_Roles.RoleId where UserId = " + userId.ToString();
            return _context.Database.SqlQuery<string>(cmd).FirstOrDefault();
        }

        public int DeleteRoleInUsers(int userId)
        {
            string cmd = "delete webpages_UsersInRoles where UserId = " + userId;
            return _context.Database.ExecuteSqlCommand(cmd);
        }
    }
}
