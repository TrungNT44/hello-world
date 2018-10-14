using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Security;
using MedMan.App_Start;
using sThuoc.DAL;
using sThuoc.Repositories;
using WebMatrix.WebData;

namespace sThuoc.Models
{
    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Chưa nhập mật khẩu mới")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự và bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự và bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không trùng.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(12, ErrorMessage = "{0} phải có ít nhất {2} ký tự..", MinimumLength = 10)]
        [Display(Name = "SĐT")]
        public string Phone { get; set; }

    }

    public class ResetPasswordConfirmModel
    {

        public string Token { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự..", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không trùng.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "Tài khoản đăng nhập")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }


        public bool IsConfirmed { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Chưa nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [RegularExpression(@"^\w+(\.\w+)*(@\w+(\.\w+)*){0,1}$", ErrorMessage = "Tên đăng nhập chỉ được chữ cái, chữ số, và dấu _")]
        public string UserName { get; set; }
        

        [Required(ErrorMessage = "Chưa nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự và bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,14}$", ErrorMessage = "Mật khẩu phải từ 8 đến 15 ký tự và bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        [Display(Name = "Xác nhận mật khẩu")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Mật khẩu và xác nhận không trùng.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Chưa nhập họ tên")]
        [Display(Name = "Họ và Tên")]
        public string TenDayDu { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
        [Display(Name = "Số CMT")]
        public string SoCMT { get; set; }

        [Display(Name = "Nhà thuốc")]
        public string MaNhaThuoc { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class NhanVienViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Họ và Tên")]
        public string TenDayDu { get; set; }
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
        [EmailAddress, Display(Name = "Thư điện tử")]
        public string Email { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Trạng thái hoạt động")]
        public string Role { get; set; }
        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; }

        public NhanVienViewModel(UserProfile user, NhaThuoc nhaThuoc)
        {
            UserId = user.UserId;
            UserName = user.UserName;
            TenDayDu = user.TenDayDu;
            Email = user.Email;
            SoDienThoai = user.SoDienThoai;
            MaNhaThuoc = nhaThuoc.MaNhaThuoc;
            HoatDong = user.HoatDong;
            Role = Roles.Provider.IsUserInRole(user.UserName, Constants.Security.Roles.SuperUser.Value)
                ? Constants.Security.Roles.SuperUser.Value
                : "";
            if (string.IsNullOrEmpty(Role))
            {
                var nhanVien = nhaThuoc.Nhanviens.First(e => e.User.UserId == user.UserId);
                if (nhanVien != null)
                    Role = nhanVien.Role;
            }

        }

    }
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {

        }

        // Allow Initialization with an instance of ApplicationUser:
        public EditUserViewModel(UserProfile user)
        {
            UserId = user.UserId;
            UserName = user.UserName;
            TenDayDu = user.TenDayDu;
            Email = user.Email;
            SoDienThoai = user.SoDienThoai;
            HoatDong = user.HoatDong;
            SoCMT = user.SoCMT;
            if (user.NhanVienNhaThuocs.Count > 0)
            {
                DsNhaThuoc =
                    user.NhanVienNhaThuocs.Select(e => string.Concat(e.NhaThuoc.TenNhaThuoc, " - ", Constants.Security.Roles.RoleTexts[e.Role])).ToArray();
            }
        }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Họ và Tên")]
        public string TenDayDu { get; set; }
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
        [EmailAddress, Display(Name = "Thư điện tử")]
        public string Email { get; set; }
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Số CMT")]
        public string SoCMT { get; set; }
        [Display(Name = "Trạng thái hoạt động")]
        public int RoleId { get; set; }
        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; }
        public string[] DsNhaThuoc { get; set; }

    }

    public class SelectUserRolesViewModel
    {

        public SelectUserRolesViewModel()
        {
            Roles = new List<SelectListItem>();
        }


        // Enable initialization with an instance of ApplicationUser:
        public SelectUserRolesViewModel(UserProfile user, bool isAdmin = false)
            : this()
        {
            UserId = user.UserId;
            UserName = user.UserName;

            var uow = new UnitOfWork();
            var roleProvider = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
            //Set the current row to the one fetched
            RoleName = roleProvider.GetRolesForUser(user.UserName)[0];
            //Set the rows list
            foreach (var role in roleProvider.GetAllRoles())
                Roles.Add(new SelectListItem { Text = role, Value = role });
            //Bind the functions information            
            Functions =
                uow.FunctionRepository.GetAll().Select(function => new SelectFunctionViewModel(user.UserId, function.MaFunction, user.MaNhaThuoc, RoleName)).ToList();
        }
        [Required]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public List<SelectListItem> Roles { get; set; }
        public List<SelectFunctionViewModel> Functions { get; set; }
    }


    public class SelectFunctionViewModel
    {
        public SelectFunctionViewModel() { Operations = new List<SelectOperationViewModel>(); }

        public SelectFunctionViewModel(int userId, int functionId, string maNhaThuoc, string roleName)
            : this()
        {
            FunctionId = functionId;
            var uow = new UnitOfWork();
            var function = uow.FunctionRepository.GetById(functionId);
            FunctionName = function.Name;
            var functionOperations = function.Operations;
            foreach (var operation in functionOperations.ToString().Split(','))
            {
                // An EditorViewModel will be used by Editor Template:
                var rvm = new SelectOperationViewModel(operation.Trim());
                Operations.Add(rvm);
            }

            // Set the Selected property to true for those roles for 
            // which the current user is a member:
            var db = new SecurityContext();
            var firstOrDefault = db.OperationsToRoles.FirstOrDefault(x => x.UserId == userId && x.FunctionId == functionId && x.MaNhaThuoc == maNhaThuoc && x.RoleName == roleName);
            if (firstOrDefault != null)
            {
                var operationsToRoles = firstOrDefault.Operations;
                foreach (var activeOperation in operationsToRoles.ToString().Split(','))
                {
                    var checkFuctionOperation =
                        Operations.Find(r => r.OperationName == activeOperation.Trim());
                    if (checkFuctionOperation != null) checkFuctionOperation.Selected = true;
                }
            }
        }
        [Required]
        public int FunctionId { get; set; }

        [Display(Name = "Tên chức năng")]
        public string FunctionName { get; set; }
        public List<SelectOperationViewModel> Operations { get; set; }
    }

    public class SelectOperationViewModel
    {
        public SelectOperationViewModel() { }

        public SelectOperationViewModel(string operation)
        {
            OperationName = operation;
        }
        public bool Selected { get; set; }
        [Required]
        public string OperationName { get; set; }
    }

}
