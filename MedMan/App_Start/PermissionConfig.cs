using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sThuoc.Models.ViewModels;

namespace MedMan.App_Start
{
    public class PermissionConfig
    {
        public static List<Permission> GetBlankPermissions()
        {
            var perms = new List<Permission>();
            // Nhom KHach Hang
            var perm = new Permission("Nhóm khách hàng", "nhomkhachhangs");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller
                ,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = false
            });

            perms.Add(perm);
            // KHach Hang
            perm = new Permission("Khách hàng", "khachhangs");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xuất ra Excel",
                Checked = false,
                action = "ExportToExcel",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Nhom Nha Cung Cap
            perm = new Permission("Nhóm nhà cung cấp", "nhomnhacungcaps");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = false
            });

            perms.Add(perm);

            // Nha Cung Cap
            perm = new Permission("Nhà cung cấp", "nhacungcaps");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xuất ra Excel",
                Checked = false,
                action = "ExportToExcel",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Bac sy
            perm = new Permission("Bác sỹ", "bacsys");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xuất ra Excel",
                Checked = false,
                action = "ExportToExcel",
                controller = perm.Controller,
                Visible = false
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Nhom thuoc
            perm = new Permission("Nhóm thuốc", "nhomthuocs");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Thuoc
            perm = new Permission("Quản lý thuốc", "thuocs");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem Giá Nhập",
                Checked = false,
                action = "ViewPriceInput",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Thêm mới",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xuất ra Excel",
                Checked = false,
                action = "ExportToExcel",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Lập dự trù",
                Checked = false,
                action = "lapdutru",
                controller = "drugmanagement",
                Visible = true
            });

            perms.Add(perm);
            // Kiem ke 

            perm = new Permission("Phiếu kiểm kê", "phieukiemkes");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Kiểm kê kho",
                Checked = false,
                action = "Create",
                controller = "phieukiemkes",
                Visible = true
            });

            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Index",
                controller = "phieukiemkes",
                Visible = true
            });

            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = "phieukiemkes",
                Visible = true
            });

            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = "phieukiemkes",
                Visible = true
            });

            perms.Add(perm);

            // Ban hang
            perm = new Permission("Xuất hàng", "phieuxuats");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xuất",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "In",
                Checked = false,
                action = "In",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = true
            });

            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Khôi phục",
                Checked = false,
                action = "Restore",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
           {
               Permission = "Nhập từ Excel",
               Checked = false,
               action = "Upload2",
               controller = perm.Controller,
               Visible = true
           });

            perms.Add(perm);

            // nhap hang
            perm = new Permission("Nhập hàng", "phieunhaps");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "DS Nhập/Xuất",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "In",
                Checked = false,
                action = "In",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = true
            });

            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Khôi phục",
                Checked = false,
                action = "Restore",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhập từ Excel",
                Checked = false,
                action = "Upload2",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Thu chi
            perm = new Permission("Phiếu thu/chi", "phieuthuchis");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "DS phiếu",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Tạo phiếu",
                Checked = false,
                action = "Create",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "In",
                Checked = false,
                action = "In",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Chi tiết",
                Checked = false,
                action = "Details",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Sửa",
                Checked = false,
                action = "Edit",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xóa",
                Checked = false,
                action = "Delete",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            // Bao cao
            perm = new Permission("Báo cáo", "baocao");
            perm.Permissions = new List<PermissionItem>();
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Tổng hợp",
                Checked = false,
                action = "Index",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhân viên",
                Checked = false,
                action = "TheoNhanVien",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Nhà cung cấp",
                Checked = false,
                action = "TheoNhaCungCap",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Khách hàng",
                Checked = false,
                action = "TheoKhachHang",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Bác sỹ",
                Checked = false,
                action = "Theobacsy",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Mặt hàng bán",
                Checked = false,
                action = "TheoMatHang",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Kho hàng",
                Checked = false,
                action = "TheoKhoHang",
                controller = perm.Controller,
                Visible = true
            });
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Theo ngày",
                Checked = false,
                action = "TheoNgay",
                controller = perm.Controller,
                Visible = true
            });           
            perm.Permissions.Add(new PermissionItem()
            {
                Permission = "Xem lợi nhuận",
                Checked = false,
                action = "ViewProfit",
                controller = perm.Controller,
                Visible = true
            });

            perms.Add(perm);

            return perms;
        }
    }
}