using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhaThuocRepository : GenericRepository<NhaThuoc>
    {
        public NhaThuocRepository(SecurityContext context) : base(context) { }
        /// <summary>
        /// Generate next available ID for Nhathuoc entity
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string GenereateNextId(string prefix)
        {
            Int32 baseNumber = DbSet.Count();
            string str = baseNumber.ToString();
            Int32 len = baseNumber.ToString().Length;  
            if (baseNumber < 20)
            {
                baseNumber = 20;       
                str = "0020";
            }
            else
            {
                baseNumber = 40;
                str = "0040";
            }
            
            while (GetById(str)!=null)
            {
                baseNumber++;
                str = baseNumber.ToString();
                len = baseNumber.ToString().Length;
                for (var i = 0; i < 4 - len; i++)
                {
                    str = '0' + str;
                }
            }                           
            return str;
        }
    }
}