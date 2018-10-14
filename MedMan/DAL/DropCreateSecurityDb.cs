using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Security;
using sThuoc.Filter;
using sThuoc.Models;
using WebMatrix.WebData;
using WebSecurity = WebMatrix.WebData.WebSecurity;
using MedMan.App_Start;

namespace sThuoc.DAL
{
    public class DropCreateSecurityDb : CreateDatabaseIfNotExists<SecurityContext>
    {
        protected override void Seed(SecurityContext context)
        {
            //Security Import
            WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("SimpleSecurityConnection",
               "UserProfile", "UserId", "UserName", autoCreateTables: true);
            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;
            //Security
            if (!roles.RoleExists("Admin"))
            {
                roles.CreateRole("Admin");
            }
            if (!roles.RoleExists("User"))
            {
                roles.CreateRole("User");
            }
            if (!roles.RoleExists("SuperUser"))
            {
                roles.CreateRole("SuperUser");
            }

            if (!Filter.WebSecurity.FoundUser("webnt1"))
            {
                Filter.WebSecurity.CreateUserAndAccount("webnt1", "Web@1234", "Web nha thuoc 1", "webnt1@gmail.com", "01692999997", "0000000000");
            }
            if (!roles.GetRolesForUser("webnt1").Contains("SuperUser"))
            {
                roles.AddUsersToRoles(new[] { "webnt1" }, new[] { "SuperUser" });
            }

            if (!Filter.WebSecurity.FoundUser("webnt2"))
            {
                Filter.WebSecurity.CreateUserAndAccount("webnt2", "Web@1234", "Web nha thuoc 2", "webnt2@gmail.com", "01692999998", "");
            }
            if (!roles.GetRolesForUser("webnt2").Contains("SuperUser"))
            {
                roles.AddUsersToRoles(new[] { "webnt2" }, new[] { "SuperUser" });
            }                      
                     
            //Function
            const Operations basicOperations = Operations.Create | Operations.Read | Operations.Modify | Operations.Import | Operations.Export;
            const Operations advOperations = Operations.Create | Operations.Read | Operations.Modify | Operations.Import | Operations.Export | Operations.Execute | Operations.History | Operations.Delete;

            var func1 = new Function()
            {
                MaFunction = 1,
                Name = "Danh Mục Nhóm Thuốc",
                Operations = basicOperations
            };
            var func2 = new Function()
            {
                MaFunction = 2,
                Name = "Danh Mục Thuốc",
                Operations = basicOperations
            };
            var func3 = new Function()
            {
                MaFunction = 3,
                Name = "Danh Mục Đơn Vị Tính",
                Operations = basicOperations
            };
            var func4 = new Function()
            {
                MaFunction = 4,
                Name = "Danh Mục Nhóm Khách Hàng",
                Operations = basicOperations
            };
            var func5 = new Function()
            {
                MaFunction = 5,
                Name = "Danh Mục Nhóm Nhà Cung Cấp",
                Operations = basicOperations
            };
            var func6 = new Function()
            {
                MaFunction = 6,
                Name = "Danh Mục Nhà Cung Cấp",
                Operations = basicOperations
            };
            var func7 = new Function()
            {
                MaFunction = 7,
                Name = "Danh Mục Bác Sỹ",
                Operations = basicOperations
            };
            var func8 = new Function()
            {
                MaFunction = 8,
                Name = "Danh Mục Nhân Viên",
                Operations = basicOperations
            };
            var func9 = new Function()
            {
                MaFunction = 9,
                Name = "Bán Hàng",
                Operations = advOperations
            };
            var func10 = new Function()
            {
                MaFunction = 10,
                Name = "Nhập Hàng",
                Operations = advOperations
            };
            var func11 = new Function()
            {
                MaFunction = 11,
                Name = "Khách Trả Lại Hàng",
                Operations = advOperations
            };
            var func12 = new Function()
            {
                MaFunction = 12,
                Name = "Hàng Trả Lại Cho Nhà Cung Cấp",
                Operations = advOperations
            };
            var func13 = new Function()
            {
                MaFunction = 13,
                Name = "Viết Phiếu Thu Cho Khách",
                Operations = advOperations
            };
            var func14 = new Function()
            {
                MaFunction = 14,
                Name = "Viết Phiếu Chi Cho Nhà Cung Cấp",
                Operations = advOperations
            };
            var func15 = new Function()
            {
                MaFunction = 15,
                Name = "Viết Phiếu Thu/Chi Khác",
                Operations = advOperations
            };
            var func16 = new Function()
            {
                MaFunction = 16,
                Name = "Báo Cáo",
                Operations = basicOperations
            };
            var func17 = new Function()
            {
                MaFunction = 17,
                Name = "Dạng Bào Chế",
                Operations = basicOperations
            };
            var func18 = new Function()
            {
                MaFunction = 18,
                Name = "Nước",
                Operations = basicOperations
            };
            var func19 = new Function()
            {
                MaFunction = 19,
                Name = "Khách Hàng",
                Operations = basicOperations
            };
            context.Functions.AddOrUpdate(x => x.MaFunction, func1, func2, func3, func4, func5, func6, func7, func8, func9, func10, func11, func12, func13, func14, func15, func16, func17, func18, func19);
            context.SaveChanges();

            //Loai Xuat Nhap
            var lxn1 = new LoaiXuatNhap()
            {
                TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho
            };
            var lxn2 = new LoaiXuatNhap()
            {
                TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.XuatBan
            };
            var lxn3 = new LoaiXuatNhap()
            {
                TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang
            };
            var lxn4 = new LoaiXuatNhap()
            {
                TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap
            };

            var lxn5 = new LoaiXuatNhap()
            {
                TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe
            };          

            context.LoaiXuatNhaps.AddOrUpdate(x => x.MaLoaiXuatNhap, lxn1, lxn2, lxn3, lxn4, lxn5);
            context.SaveChanges();
        }
    }
}