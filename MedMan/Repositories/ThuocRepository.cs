using System.Collections.Generic;
using System.Linq;
using MvcJqGrid;
using Med.Web.Controllers;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class ThuocRepository:GenericRepository<Thuoc>
    {
        public ThuocRepository(SecurityContext context) : base(context)
        {
        }

       

        public IList<Thuoc> GetThuocs(string maNhaThuoc,GridSettings gridSettings)
        {
            var orderedThuocs = OrderThuocs(GetMany(e=> e.NhaThuoc.MaNhaThuoc==maNhaThuoc), gridSettings.SortColumn, gridSettings.SortOrder);

            if (gridSettings.IsSearch)
            {
                orderedThuocs = gridSettings.Where.rules.Aggregate(orderedThuocs, FilterThuocs);
            }

            return orderedThuocs.Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize).ToList();
        }

        public int CountThuocs(string maNhaThuoc,GridSettings gridSettings)
        {
            return gridSettings.IsSearch ? gridSettings.Where.rules.Aggregate(GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc), FilterThuocs).Count() : GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).Count();
        }

        private static IQueryable<Thuoc> FilterThuocs(IQueryable<Thuoc> thuocs, Rule rule)
        {
            switch (rule.field)
            {
                case "MaThuoc":
                    return thuocs.Where(c => c.MaThuoc.ToLower().Contains(rule.data.ToLower()));

                case "TenDayDu":
                    return thuocs.Where(c => (c.TenThuoc + c.ThongTin).ToLower().Contains(rule.data.ToLower()));

                case "TenNhomThuoc":
                    return thuocs.Where(c => c.NhomThuoc.TenNhomThuoc.ToLower().Contains(rule.data.ToLower()));

                case "DonViThuNguyen":
                    return thuocs.Where(c => c.DonViXuatLe.TenDonViTinh.ToLower().Contains(rule.data.ToLower()));
                default:
                    return thuocs;
            }
        }

        private static IQueryable<Thuoc> OrderThuocs(IQueryable<Thuoc> thuocs, string sortColumn, string sortOrder)
        {
            switch (sortColumn)
            {
                case "MaThuoc":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.MaThuoc) : thuocs.OrderBy(c => c.MaThuoc);

                case "TenDayDu":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.TenThuoc + c.ThongTin) : thuocs.OrderBy(c => c.TenThuoc + c.ThongTin);

                case "TenNhomThuoc":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.NhomThuoc.TenNhomThuoc) : thuocs.OrderBy(c => c.NhomThuoc.TenNhomThuoc);
                case "TenDonViTinh":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.DonViXuatLe.TenDonViTinh) : thuocs.OrderBy(c => c.DonViXuatLe.TenDonViTinh);
                case "GiaBanLe":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.GiaBanLe) : thuocs.OrderBy(c => c.GiaBanLe);
                case "GiaNhap":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.GiaNhap) : thuocs.OrderBy(c => c.GiaNhap);
                case "GioiHan":
                    return (sortOrder == "desc") ? thuocs.OrderByDescending(c => c.GioiHan) : thuocs.OrderBy(c => c.GioiHan);
                default:
                    return thuocs.OrderBy(c => c.TenThuoc + c.ThongTin);
            }
        }        
    }
}
    