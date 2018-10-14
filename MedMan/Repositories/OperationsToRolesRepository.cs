using System.Linq;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class OperationsToRolesRepository : GenericRepository<OperationsToRoles>
    {
        public OperationsToRolesRepository(SecurityContext context) : base(context) { }

        public OperationsToRoles GetOperationToRolesByUser(int userId, int functionId, string role, string maNhaThuoc)
        {
            return Context.OperationsToRoles.FirstOrDefault(x => x.UserId == userId && x.FunctionId == functionId && x.RoleName == role && x.MaNhaThuoc == maNhaThuoc);
        }
    }
}
