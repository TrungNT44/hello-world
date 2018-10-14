using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class PhieuKiemKeChiTietRepository:GenericRepository<PhieuKiemKeChiTiet>
    {
        public PhieuKiemKeChiTietRepository(SecurityContext context) : base(context)
        {
        }
    }
}
