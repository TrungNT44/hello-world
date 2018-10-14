using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class FunctionRepository: GenericRepository<Function>
    {
        public FunctionRepository(SecurityContext context) : base(context) { }
    }
}
