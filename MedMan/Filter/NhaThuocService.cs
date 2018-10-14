using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.Models;
using sThuoc.Repositories;

namespace sThuoc.Filter
{
    public static class NhaThuocService
    {
        public static NhaThuoc NhaThuoc { get; set; }

       
        public static string GetTen()
        {
            return NhaThuoc != null ? NhaThuoc.TenNhaThuoc : "";
        }

        public static string GetDiaChi()
        {
            return NhaThuoc != null ? NhaThuoc.DiaChi : "";
        }
    }
}