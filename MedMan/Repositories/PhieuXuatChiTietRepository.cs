using System.Linq;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuXuatChiTietRepository : GenericRepository<PhieuXuatChiTiet>
    {
        private readonly UnitOfWork unitOfWork;
        public PhieuXuatChiTietRepository(SecurityContext context)
            : base(context)
        {
            unitOfWork = new UnitOfWork(context);
        }
    }
}