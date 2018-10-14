using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class DonViTinhRepository:GenericRepository<DonViTinh>
    {
        public DonViTinhRepository(SecurityContext context) : base(context)
        {
        }
    }
}