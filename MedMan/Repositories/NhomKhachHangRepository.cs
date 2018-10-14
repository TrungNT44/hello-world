using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class NhomKhachHangRepository:GenericRepository<NhomKhachHang>
    {
        private SecurityContext _context;
        public NhomKhachHangRepository(SecurityContext context) : base(context){}

        public bool CheckCustomerExist(int groupCustomerId)
        {
            if (_context == null) _context = new SecurityContext();
            if( _context.PhieuNhaps.Where(c => c.KhachHang.MaNhomKhachHang == groupCustomerId).FirstOrDefault() !=null)
            {
                return true;                
            }

            if (_context.PhieuThuChis.Where(c => c.KhachHang.MaNhomKhachHang == groupCustomerId).FirstOrDefault() != null)
            {
                return true;
            }

            if (_context.PhieuXuats.Where(c => c.KhachHang.MaNhomKhachHang == groupCustomerId).FirstOrDefault() != null)
            {
                return true;
            }
          
            return false;
        }
    }

    public class KhanhHangData
    {      
        public int? MaPhieuNhap;
        public int? MaPhieuXuat;
        public int? MaPhieu;
    }
}