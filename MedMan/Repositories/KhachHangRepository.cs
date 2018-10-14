using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class KhachHangRepository:GenericRepository<KhachHang>
    {
        public KhachHangRepository(SecurityContext context) : base(context)
        {
        }
    }
}