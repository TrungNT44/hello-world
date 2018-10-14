using sThuoc.Models;
using sThuoc.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.DAL;

namespace sThuoc.Repositories
{
    public class DrugMappingRepository : GenericRepository<DrugMapping>
    {
        public DrugMappingRepository(SecurityContext context) : base(context)
        {
        }
    }
}