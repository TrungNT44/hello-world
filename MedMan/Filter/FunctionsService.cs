using System.Linq;
using System.Web;
using System.Web.Security;
using Microsoft.SqlServer.Server;
using MedMan.App_Start;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using sThuoc.Repositories;
using WebMatrix.WebData;
using System.Collections.Generic;

namespace sThuoc.Filter
{
    public static class FunctionsService
    {
        public static int AddFunction(int id, string name, Operations operation)
        {
            var uow = new UnitOfWork();
            var resource = new Function()
            {
                MaFunction = id,
                Name = name,
                Operations = operation
            };

            uow.FunctionRepository.Insert(resource);
            uow.Save();
            return resource.MaFunction;
        }

        public static void InsertOrUpdateOperationToRole(int userId, int functionId, Operations operation, string role, string maNhaThuoc = "")
        {
            var uow = new UnitOfWork();
            var row = uow.OperationsToRolesRepository.GetOperationToRolesByUser(userId, functionId, role, maNhaThuoc);
            if (row != null)
            {
                row.Operations = operation;
                uow.OperationsToRolesRepository.Update(row);
                uow.Save();
            }
            else
            {
                row = new OperationsToRoles()
               {
                   UserId = userId,
                   FunctionId = functionId,
                   Operations = operation,
                   RoleName = role,
                   MaNhaThuoc = maNhaThuoc
               };
                uow.OperationsToRolesRepository.Insert(row);
                uow.Save();
            }
        }

        public static bool Authorize(int userId, int functionId, Operations operation)
        {
            bool authorized = false;

            var roleProvider = (SimpleRoleProvider)Roles.Provider;
            var uow = new UnitOfWork();
            var user = uow.UserProfileRepository.GetById(userId);
            string[] roles = roleProvider.GetRolesForUser(user.UserName);

            var companyId = uow.UserProfileRepository.GetUserByName(user.UserName).MaNhaThuoc;
            foreach (string role in roles)
            {
                int count = uow.OperationsToRolesRepository.Get(o => o.UserId == userId && o.RoleName == role && o.FunctionId == functionId && o.MaNhaThuoc == companyId &&
                     (o.Operations & operation) != 0).Count();
                if (count > 0)
                {
                    authorized = true;
                    break;
                }
            }
            return authorized;
        }
        public static bool Authorize(int functionId, Operations operation, string[] roles)
        {
            var uow = new UnitOfWork();
            var roleProvider = (SimpleRoleProvider)Roles.Provider;
            var userId = WebMatrix.WebData.WebSecurity.CurrentUserId;
            bool authorized = false;
            if (roles[0] == "SuperUser" || roles[0] == "Admin")
            {
                authorized = true;
            }
            else
            {
                foreach (string role in roles)
                {
                    int count = uow.OperationsToRolesRepository.Get(o => o.UserId == userId && o.RoleName == role && o.FunctionId == functionId &&
                         (o.Operations & operation) != 0).Count();
                    if (count > 0)
                    {
                        authorized = true;
                        break;
                    }
                }
            }

            return authorized;
        }
        public static bool Authorize(string controller, string action, NhaThuocSessionModel nhaThuoc,string[] checkRoles =null)
        {
            if (nhaThuoc == null || string.IsNullOrEmpty(nhaThuoc.Role))
                return false;
            if (HttpContext.Current.User.IsInRole(Constants.Security.Roles.SuperUser.Value))
            {
                return true;
            }
            if (nhaThuoc.Role == Constants.Security.Roles.Admin.Value)
                return true;
            if (checkRoles!=null&&checkRoles.Contains(nhaThuoc.Role))
                return true;

            // kiem tra co quyen tren tung trang khong?
            var uow = new UnitOfWork();
            if (controller.ToLower() == "inventory")
            {
                controller = "Phieukiemkes";
            }
            var permission = uow.UserPermissionsRespository.Get(
                e => e.Controller.ToLower() == controller.ToLower() && e.Action.ToLower() == action.ToLower() && e.NhaThuoc.MaNhaThuoc == nhaThuoc.MaNhaThuoc&&e.User.UserId==WebSecurity.GetCurrentUserId);
            if (permission.Any())
                return true;


            return false;
        }

        public static string GetRolesAsCsv(int resourceId, Operations operation)
        {
            string rolesCsv = string.Empty;
            UnitOfWork uow = new UnitOfWork();
            var roles = uow.OperationsToRolesRepository.Get(o => o.FunctionId == resourceId &&
                (o.Operations & operation) != 0).Select(o => o.RoleName).Distinct();
            bool firstItem = true;
            foreach (string role in roles)
            {
                if (firstItem)
                {
                    rolesCsv += role;
                    firstItem = false;
                }
                else
                {
                    rolesCsv += "," + role;
                }
            }
            return rolesCsv;

        }

        public static bool AuthorizeInputBill(int userId, string maNhaThuoc, int maPhieuNhap = 0 )
        {
            var isExisted = true;
            bool authorized = false;
            var uow = new UnitOfWork();
            if (maPhieuNhap > 0)
            {
                isExisted = uow.PhieuNhapRepository.GetMany(
               e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc
                   && e.MaPhieuNhap == maPhieuNhap
                   && e.LoaiXuatNhap.MaLoaiXuatNhap == 3).Any();
            }

            if (isExisted)
            {
                var controllers = new[] {"phieunhaps", "phieuxuats"};
                var actions = new[] {"create", "edit", "delete", "details"};
                var permissions = uow.UserPermissionsRespository.Get(
                    e => controllers.Contains(e.Controller)
                         && actions.Contains(e.Action)
                         && e.NhaThuoc.MaNhaThuoc == maNhaThuoc
                         && e.User.UserId == userId);
                authorized = permissions.Any();
            }

            return authorized;
        }

    }

    //Mapping MaFunction with Function's name
    public class FunctionResource
    {
        public const int NhomThuoc = 1;
        public const int Thuoc = 2;
        public const int DonViTinh = 3;
        public const int NhomKhachHang = 4;
        public const int NhomNhaCungCap = 5;
        public const int NhaCungCap = 6;
        public const int BacSy = 7;
        public const int NhanVien = 8;
        public const int BanHang = 9;
        public const int NhapHang = 10;
        public const int KhachTraHang = 11;
        public const int TraCungCap = 12;
        public const int PhieuThuKhach = 13;
        public const int PhieuChiCungCap = 14;
        public const int PhieuKhac = 15;
        public const int BaoCao = 16;
        public const int DangBaoChe = 17;
        public const int Nuoc = 18;
        public const int KhachHang = 19;
        public const int PhieuKiemKe = 20;
    }
}
