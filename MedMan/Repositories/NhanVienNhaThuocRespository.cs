using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhanVienNhaThuocRespository : GenericRepository<NhanVienNhaThuoc>
    {
        private SecurityContext _context;
        public NhanVienNhaThuocRespository(SecurityContext contenxt)
            : base(contenxt)
        {
        }

        public bool CheckStaffExist(int userId)
        {
            if (_context == null) _context = new SecurityContext();
            var user = _context.UserProfiles.Find(userId);

            if (user != null)
            {                
                if (_context.PhieuKiemKes.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.PhieuNhaps.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.PhieuThuChis.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.PhieuXuats.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.Thuocs.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.NhomThuocs.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.NhomKhachHangs.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.KhachHangs.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }

                if (_context.BacSys.Where(c => c.CreatedBy.UserId == userId).FirstOrDefault() != null)
                {
                    return true;
                }                
            }

            return false;
        }
    }
}