using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Configuration;
using Med.Web.Data.Session;
using PagedList;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI.WebControls;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using sThuoc.Repositories;
using WebGrease.Css.Extensions;
using WebMatrix.WebData;
using WebSecurity = sThuoc.Filter.WebSecurity;
using System.Net.Mail;
using System.Text;
using sThuoc.Utils;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    [Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //
        // GET: /Account/Login
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        //[AllowAnonymous]
        //public ActionResult Login(string returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    LoginModel model = new LoginModel() { IsConfirmed = true };
        //    return View(model);
        //}
        [SimpleAuthorize("SuperUser,Admin")]
        // [Audit]
        public ActionResult ResetPasswordForce(long? id, int type = 0)
        {
            var user = unitOfWork.UserProfileRepository.GetById(id);

            ViewBag.Type = type;
            if (user == null)
            {
                ViewBag.Message = "Tài khoản không tồn tại";
                return View("Error");
            }
            ViewBag.UserName = user.UserName;
            ViewBag.User = user;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("SuperUser,Admin")]
        // [Audit]
        public ActionResult ResetPasswordForce(long? id, int? type, LocalPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = unitOfWork.UserProfileRepository.GetById(id);
                if (user == null)
                {
                    ViewBag.Message = "Tài khoản không tồn tại";
                    return View("Error");
                }

                if (Roles.Provider.IsUserInRole(user.UserName, Constants.Security.Roles.SuperUser.Value))
                {
                    ViewBag.Message = "Không thể thay đổi mật khẩu cho tài khoản hệ thống ở đây";
                    return View("Error");
                }
                //var result = WebSecurity.ChangePassword(user.UserName, model.OldPassword, model.NewPassword);
                var result = WebSecurity.ResetPasswordForce(user.UserName, model.NewPassword);
                if (result)
                {
                    return RedirectToAction("ResetPasswordForceSuccess", new { userName = user.UserName, @type = type });
                }
                ViewBag.Message = "Có lỗi sảy ra trong quá trình thay đổi mật khẩu";
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ResetPasswordForceSuccess(string userName, int type = 0)
        {
            ViewBag.UserName = userName;
            ViewBag.Type = type;
            return View();
        }
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Thay đổi mật khẩu thành công."
                : message == ManageMessageId.ChangePasswordFailure ? "Thay đổi mật khẩu thất bại."
                : "";
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (ModelState.IsValid)
            {
                var db = new SecurityContext();
                var result = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (result)
                {
                    return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordFailure });
            }


            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            string errorMsg = "Sai mật khẩu hoặc tên đăng nhập.";

            if (model.IsConfirmed)
            {
                if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    var user = unitOfWork.UserProfileRepository.GetUserByName(model.UserName);
                    if (!user.HoatDong)
                    {
                        errorMsg = "Tài khoản bị khóa. Bạn liên hệ với WEB NHÀ THUỐC hoặc chủ nhà thuốc của bạn.";
                        ModelState.AddModelError("", errorMsg);
                        WebSecurity.Logout();
                        return View(model);
                    }
                    if (user.Enable_NT.HasValue && !user.Enable_NT.Value)//tài khoản bị khóa bởi nhà thuốc
                    {
                        errorMsg = "Nhà thuốc không tồn tại!";
                        WebSecurity.Logout();//Tientd sửa lỗi đăng nhập 2 lần 1 tài khoản bị khóa
                        return View(model);
                    }

                    WebSessionManager.Instance.CurrentUserId = user.UserId;
                    return RedirectToAction("ChonNhaThuocMacDinh", new { returnUrl = returnUrl });
                    //return View("ChonNhaThuocMacDinh",outputModel);
                    //return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", errorMsg);
            return View(model);
        }
        [Authorize]
        public ActionResult ChonNhaThuocMacDinh(string returnUrl)
        {
            var loggedInUser = WebSecurity.GetCurrentUser();
            var roles = Roles.Provider.GetRolesForUser(loggedInUser.UserName);
            // dang nhap xong can lua chon 1 nha thuoc de khoi tao phien lam viec

            // Lay danh sach nha thuoc ma nguoi dung thuoc vao.

            var dsNhaThuoc = new List<NhaThuoc>();
            if (roles.Any(r => r == Constants.Security.Roles.SuperUser.Value))
            {
                //Đối với user quản trị hệ thống
                dsNhaThuoc = unitOfWork.NhaThuocRepository.GetMany(e => e.MaNhaThuoc == e.MaNhaThuocCha && e.HoatDong).ToList();
            }
            else
            {
                //Đối với user quản trị các nhà thuốc cha
                dsNhaThuoc =
                    unitOfWork.NhaThuocRepository.Get(
                        e => e.HoatDong &&
                        e.MaNhaThuoc== e.MaNhaThuocCha &&
                        e.Nhanviens.Any(f => f.User.UserId == WebSecurity.GetCurrentUserId)).ToList();
                if (!dsNhaThuoc.Any() && roles.All(r => r != Constants.Security.Roles.SuperUser.Value))
                {
                    //Đối với user quản lý nhà thuốc và các user nhân viên
                    dsNhaThuoc =
                    unitOfWork.NhaThuocRepository.Get(
                        e => e.HoatDong &&
                        //e.MaNhaThuoc == e.MaNhaThuocCha &&
                        e.Nhanviens.Any(f => f.User.UserId == WebSecurity.GetCurrentUserId)).ToList();
                }

            }
            if (!dsNhaThuoc.Any() && roles.All(r => r != Constants.Security.Roles.SuperUser.Value))
            {
                ViewBag.Message = "Tài khoản này chưa được gán quyền vào bất kỳ nhà thuốc nào";
                WebSecurity.Logout();
                return View("Error");
            }
            // lay quyen cho tung nha thuoc.
            var outputModel = dsNhaThuoc.Select(e => new NhaThuocViewModel(e)).ToList();
            foreach (var nt in outputModel)
            {
                if (roles.Any(r => r == Constants.Security.Roles.SuperUser.Value))
                    nt.AdminName = Constants.Security.Roles.SuperUser.Text;
                else
                    nt.AdminName = dsNhaThuoc.First(e => e.MaNhaThuoc == nt.MaNhaThuoc).Nhanviens.First(e => e.User.UserId == loggedInUser.UserId).Role;
            }
            if (roles.Contains(Constants.Security.Roles.Admin.Value))
                foreach (var item in dsNhaThuoc)
                {
                    if (item.NhaThuocCons.Count > 0)
                    {
                        outputModel.AddRange(item.NhaThuocCons.Where(e=> e.MaNhaThuocCha != e.MaNhaThuoc && e.HoatDong).Select(e => new NhaThuocViewModel(e)).ToList());
                    }
                }
            if (outputModel.Count == 1)
            {
                return RedirectToAction("ChonNhaThuoc", new { id = outputModel.First().MaNhaThuoc, returnUrl });
            }
            WebSessionManager.Instance.NumberOfDrugStores = outputModel.Count;
            ViewBag.returnUrl = returnUrl;
            return View("ChonNhaThuocMacDinh", outputModel);
            //return RedirectToLocal(Request.QueryString["returnUrl"]);
        }
        [Authorize]
        public ActionResult ChonNhaThuoc(string id, string returnUrl)
        {
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (nhaThuoc != null)
            {
                this.SetNhaThuoc(nhaThuoc);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ViewBag.Message = string.Format("Nhà thuốc #{0} không tồn tại", id);
                return View("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            WebSessionManager.Instance.Clear();

            return Redirect("/Account/Login");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginModel() { IsConfirmed = true});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            WebSessionManager.Instance.Clear();
            return Redirect("/Account/Login");
            //WebSecurity.Logout();
            //Session.Clear();
            //return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/Register

        [Authorize(Roles = "SuperUser")]
        public ActionResult Register()
        {
            var db = new SecurityContext();
            ViewBag.MaNhaThuoc = new SelectList(db.NhaThuocs, "MaNhaThuoc", "TenNhaThuoc", null);
            return View();
        }
        [SimpleAuthorize("Admin")]
        public ActionResult CreateStaff()
        {
            ViewBag.AccountSuffix = "@" + this.GetNhaThuoc().MaNhaThuoc;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult CreateStaff(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var usernameexit = FormatUserName(model.UserName, manhathuoc, true);
                    var itemExist = unitOfWork.UserProfileRepository.Get(c => c.MaNhaThuoc == manhathuoc && c.TenDayDu.Trim().Equals(model.TenDayDu.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    var UserExist = unitOfWork.UserProfileRepository.Get(c => c.MaNhaThuoc == manhathuoc && c.UserName.Equals(usernameexit, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenDayDu", "Tên nhân viên này đã tồn tại. Vui lòng nhập tên nhân viên khác");
                    }
                    else
                    {
                        if (UserExist != null)
                        {
                            ModelState.AddModelError("UserName", "Tài khoản này đã tồn tại. Vui lòng nhập tài khoản khác");
                        }
                        else
                        {
                            model.UserName = FormatUserName(model.UserName, manhathuoc, true);
                            string confirmationToken =
                                WebSecurity.CreateUserAndAccount(model.UserName, model.Password, model.TenDayDu, model.Email, model.SoDienThoai, model.SoCMT, manhathuoc);

                            var newUser = unitOfWork.UserProfileRepository.GetUserByName(model.UserName);

                            Roles.Provider.AddUsersToRoles(new[] { newUser.UserName }, new[] { Constants.Security.Roles.User.Value });
                            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(manhathuoc);
                            nhaThuoc.Nhanviens.Add(new NhanVienNhaThuoc()
                            {
                                NhaThuoc = nhaThuoc,
                                Role = Constants.Security.Roles.User.Value,
                                User = newUser
                            });
                            // phan quyen mac dinh cho nhan vien
                            SetDefaultUserPermissions(unitOfWork, newUser, nhaThuoc);

                            unitOfWork.Save();

                            return RedirectToAction("Staff", "Account");
                        }

                    }
                }
                catch (MembershipCreateUserException e)
                {
                    var errorMsg = ErrorCodeToString(e.StatusCode);
                    switch (e.StatusCode)
                    {
                        case MembershipCreateStatus.DuplicateUserName:
                            ModelState.AddModelError("UserName", errorMsg);
                            break;
                        case MembershipCreateStatus.DuplicateEmail:
                        case MembershipCreateStatus.InvalidEmail:
                            ModelState.AddModelError("Email", errorMsg);
                            break;
                        case MembershipCreateStatus.InvalidPassword:
                            ModelState.AddModelError("Password", errorMsg);
                            break;
                        default:
                            ModelState.AddModelError("", errorMsg);
                            break;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void SetDefaultUserPermissions(UnitOfWork uow, UserProfile newUser, NhaThuoc nhaThuoc)
        {
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "index",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "Details",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "Edit",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "Delete",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "Create",
                User = newUser,
                NhaThuoc = nhaThuoc
            });


            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "index",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "Details",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "Edit",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "Delete",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "Create",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "khachhangs",
                Action = "ExportToExcel",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            // nha cung cap
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomnhacungcaps",
                Action = "index",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomkhachhangs",
                Action = "Details",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomnhacungcaps",
                Action = "Edit",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomnhacungcaps",
                Action = "Delete",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhomnhacungcaps",
                Action = "Create",
                User = newUser,
                NhaThuoc = nhaThuoc
            });


            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "index",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "Details",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "Edit",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "Delete",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "Create",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "nhacungcaps",
                Action = "ExportToExcel",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            // bac sy
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "index",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "Details",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "Edit",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "Delete",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "Create",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
            uow.UserPermissionsRespository.Insert(new UserPermission()
            {
                Controller = "bacsys",
                Action = "ExportToExcel",
                User = newUser,
                NhaThuoc = nhaThuoc
            });
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        // [Audit]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string confirmationToken =
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, model.TenDayDu, model.Email, model.SoDienThoai, model.SoCMT, model.MaNhaThuoc);

                    return RedirectToAction("Index", "Account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string id)
        {
            if (WebSecurity.ConfirmAccount(id))
            {
                return RedirectToAction("ConfirmationSuccess");
            }
            return RedirectToAction("ConfirmationFailure");
        }

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        // [Audit]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var user = WebSecurity.GetUser(model.UserName);
            if (user != null)
            {
                var roleName = unitOfWork.MembershipRepository.GetRoleName(user.UserId);
                if (!string.IsNullOrEmpty(roleName))
                {
                    if (roleName.Equals(Constants.Security.Roles.Admin.Value))
                    {
                        string emailAddress = WebSecurity.GetEmail(model.UserName);
                        string fromEmail = "webnhathuoc.hotro@gmail.com";
                        if (!string.IsNullOrEmpty(emailAddress))
                        {
                            UserProfile user_info = WebSecurity.GetUser(model.UserName);
                            if (user_info.SoDienThoai == model.Phone)
                            {
                                string newPassword = sThuoc.Utils.Helpers.GeneratePassword(5, 1, 1);
                                Console.WriteLine(newPassword);
                                WebSecurity.ResetPasswordForce(user.UserName, newPassword);
                                StringBuilder body = new StringBuilder();
                                body.Append("Xin chào bạn,<br/>");
                                body.Append("Mật khẩu mới của bạn là: " + newPassword + "<br/>");
                                body.Append("Trân trọng,<br/>");
                                body.Append("Web nhà thuốc");
                                var message = new MailMessage();
                                message.To.Add(new MailAddress(emailAddress));  // replace with valid value 
                                message.From = new MailAddress(fromEmail);  // replace with valid value

                                //Todo: se cc den ho tro nha thuoc khi co tai khoan email chinh xac
                                //message.CC.Add(new MailAddress(ccEmail));
                                message.Subject = "Khôi phục mật khẩu";
                                message.Body = body.ToString();
                                message.IsBodyHtml = true;

                                using (var smtp = new SmtpClient())
                                {
                                    var credential = new NetworkCredential
                                    {
                                        UserName = fromEmail,  // replace with valid value
                                        Password = "webnhathuoc@123"  // replace with valid value
                                    };

                                    smtp.Credentials = credential;
                                    smtp.Host = "smtp.gmail.com";
                                    smtp.Port = 587;
                                    smtp.EnableSsl = true;
                                    //Todo: se bo ra khi tai khoan email chinh xac
                                    smtp.Send(message);
                                };

                                return RedirectToAction("ResetPwStepTwo");
                            }
                            else
                            {
                                return RedirectToAction("InvalidUserName");
                            }
                        }
                    }
                    else if (roleName.Equals(Constants.Security.Roles.User.Value))
                    {
                        return RedirectToAction("ResetPasswordForStaff");
                    }
                }
                else
                {
                    return RedirectToAction("InvalidUserName");
                }
            }

            return RedirectToAction("InvalidUserName");
        }

        [Authorize(Roles = "SuperUser")]
        public ActionResult Index(int? page)
        {

            var roleProvider = Roles.Provider;
            var usersQuery = unitOfWork.UserProfileRepository.GetMany();
            // processing filter : Role, NhaThuoc
            var filterRole = Request.QueryString["FilterRole"] ?? "";
            var filterNhaThuoc = Request.QueryString["FilterNhaThuoc"] ?? "";
            var filterUserName = Request.QueryString["FilterUserName"] ?? "";
            if (!string.IsNullOrEmpty(filterNhaThuoc))
            {
                usersQuery = usersQuery.Where(e => e.NhanVienNhaThuocs.Any(f => f.NhaThuoc.MaNhaThuoc == filterNhaThuoc));
            }
            if (!string.IsNullOrEmpty(filterUserName))
            {
                usersQuery = usersQuery.Where(e => e.UserName.Contains(filterUserName));
            }
            var users = usersQuery.AsEnumerable();
            if (!string.IsNullOrEmpty(filterRole))
            {
                users = users.Where(e => roleProvider.GetRolesForUser(e.UserName).Any(r => r == filterRole));
            }
            var model = users.Select(user => new EditUserViewModel(user)).ToList();
            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = model.ToPagedList(pageNumber, pageSize);


            // lay danh sach nhom quyen

            ViewBag.Roles = roleProvider.GetAllRoles();
            // lay danh sach nha thuoc 
            ViewBag.DsNhaThuoc = unitOfWork.NhaThuocRepository.GetAll();
            return View(result);
        }
        [SimpleAuthorize("Admin")]
        public ActionResult Staff(string searchString)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var users =
                unitOfWork.NhaThuocRepository.GetById(maNhaThuoc).Nhanviens.Select(e => e.User);
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(e => e.TenDayDu.ToLower().Contains(searchString.ToLower()));
            }
            var result = users.OrderBy(e => e.TenDayDu).ToList();

            var model = result.Select(user => new NhanVienViewModel(user, unitOfWork.NhaThuocRepository.GetById(maNhaThuoc))).ToList();

            return View(model);
        }
        //public ActionResult Staff(string sortOrder, string currentFilter, string searchString, int? page)
        //{
        //    ViewBag.CurrentSort = sortOrder;
        //    ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
        //    //ViewBag.NameSortParm2 = sortOrder == "tennhom" ? "tennhom_desc" : "tennhom";

        //    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
        //    var qry =
        //        unitOfWork.NhaThuocRepository.GetById(maNhaThuoc).Nhanviens.Select(e => e.User);
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        qry = qry.Where(e => e.TenDayDu.ToLower().Contains(searchString.ToLower()));
        //    }

        //    //sort the table 
        //    switch (sortOrder)
        //    {
        //        case "ten_desc":
        //            qry = qry.OrderByDescending(s => s.TenDayDu);
        //            break;                
        //        default:
        //            qry = qry.OrderBy(s => s.TenDayDu);
        //            break;
        //    }

        //    var result = qry.OrderBy(e => e.TenDayDu).ToList();

        //    var model = result.Select(user => new NhanVienViewModel(user, unitOfWork.NhaThuocRepository.GetById(maNhaThuoc))).ToList();
        //    return View(result);
        //}

        [Authorize(Roles = "SuperUser")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = unitOfWork.UserProfileRepository.GetById(id);
            // prevent editing other super account.
            var permissionView = PreventEditingAnotherSuperAccount(user);
            if (permissionView != null)
            {
                return permissionView;
            }
            //return new HttpUnauthorizedResult();
            var model = new EditUserViewModel(user);

            return View(model);
        }
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult EditStaff(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // chi cho phep sua user trong nha thuoc.
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
            if (nhaThuoc.Nhanviens.Any(e => e.User.UserId == id))
            {
                var manhathuoc = nhaThuoc.MaNhaThuoc;
                var user = unitOfWork.UserProfileRepository.GetById(id);
                // prevent editing other super account.
                var permissionView = PreventEditingAnotherSuperAccount(user);
                if (permissionView != null)
                {
                    return permissionView;
                }
                //return new HttpUnauthorizedResult();
                var model = new EditUserViewModel(user);
                //model.UserName = FormatUserName(model.UserName, manhathuoc, false);
                ViewBag.AccountSuffix = "@" + manhathuoc;
                return View(model);
            }
            ViewBag.Message = "Không thể sửa thông tin nhân viên này";
            return View("Error");
        }
        [HttpPost]
        [SimpleAuthorize("Admin")]
        [ValidateAntiForgeryToken]
        // [Audit]
        public ActionResult EditStaff(int id, EditUserViewModel model)
        {
            try
            {
                // chi cho phep sua user trong nha thuoc.
                var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
                var itemExist = unitOfWork.UserProfileRepository.Get(c => c.MaNhaThuoc == manhathuoc && c.UserId != model.UserId && c.TenDayDu.Trim().Equals(model.TenDayDu.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (itemExist != null)
                {
                    ModelState.AddModelError("TenDayDu", "Tên nhân viên này đã tồn tại. Vui lòng nhập tên nhân viên khác");
                    return View(model);
                }
                else
                {
                    var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                    if (nhaThuoc.Nhanviens.Any(e => e.User.UserId == id))
                    {
                        if (ModelState.IsValid)
                        {
                            //model.UserName = FormatUserName(model.UserName, manhathuoc, true);
                            var user = unitOfWork.UserProfileRepository.GetById(id);
                            // prevent editing other super account.
                            var permissionView = PreventEditingAnotherSuperAccount(user);
                            if (permissionView != null)
                            {
                                return permissionView;
                            }
                            user.UserName = model.UserName;
                            user.TenDayDu = model.TenDayDu;
                            user.Email = model.Email;
                            user.SoDienThoai = model.SoDienThoai;
                            user.MaNhaThuoc = manhathuoc;
                            user.HoatDong = model.HoatDong;
                            user.SoCMT = model.SoCMT;
                            unitOfWork.UserProfileRepository.Update(user);
                            unitOfWork.Save();
                        }
                    }
                }
            }
            catch (MembershipCreateUserException e)
            {
                var errorMsg = ErrorCodeToString(e.StatusCode);
                switch (e.StatusCode)
                {
                    case MembershipCreateStatus.DuplicateUserName:
                        ModelState.AddModelError("UserName", errorMsg);
                        break;
                    case MembershipCreateStatus.DuplicateEmail:
                    case MembershipCreateStatus.InvalidEmail:
                        ModelState.AddModelError("Email", errorMsg);
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        ModelState.AddModelError("Password", errorMsg);
                        break;
                    default:
                        ModelState.AddModelError("", errorMsg);
                        break;
                }
            }

            return RedirectToAction("Staff");
        }

        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        [ValidateAntiForgeryToken]
        // [Audit]
        public async Task<ActionResult> Edit(int id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = unitOfWork.UserProfileRepository.GetById(id);
                // prevent editing other super account.
                var permissionView = PreventEditingAnotherSuperAccount(user);
                if (permissionView != null)
                {
                    return permissionView;
                }

                //var db = new SecurityContext();

                user.TenDayDu = model.TenDayDu;
                user.Email = model.Email;
                user.SoDienThoai = model.SoDienThoai;
                user.MaNhaThuoc = model.MaNhaThuoc;
                user.HoatDong = model.HoatDong;
                user.SoCMT = model.SoCMT;
                //db.Entry(user).State = EntityState.Modified;
                unitOfWork.UserProfileRepository.Update(user);
                unitOfWork.Save();
                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        // [Audit]
        public ActionResult Delete(int id, EditUserViewModel form)
        {
            var user = unitOfWork.UserProfileRepository.GetById(id);
            // prevent editing other super account.
            var permissionView = PreventEditingAnotherSuperAccount(user, "delete");
            if (permissionView != null)
            {
                return permissionView;
            }
            try
            {
                unitOfWork.NhanVienNhaThuocRespository.GetMany(e => e.User.UserId == id).ToList().ForEach(nv => unitOfWork.NhanVienNhaThuocRespository.Delete(nv));
                WebSecurity.DeleteUser(user.UserName, true);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                ViewBag.Message = "Không thể xóa user này, liên hệ administrator để biết thêm chi tiết";
                ViewBag.FullMessage = e.Message;
                return View("Error");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SuperUser")]
        // [Audit]
        public ActionResult Delete(int id)
        {
            var user = unitOfWork.UserProfileRepository.GetById(id);
            // prevent editing other super account.
            var permissionView = PreventEditingAnotherSuperAccount(user, "delete");
            if (permissionView != null)
            {
                return permissionView;
            }
            return View("ConfirmDeleteAccount", new EditUserViewModel(user));
        }

        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult DeleteStaff(int? id)
        {
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
            if (nhaThuoc.Nhanviens.Any(e => e.User.UserId == id))
            {
                var user = unitOfWork.UserProfileRepository.GetById(id);
                // prevent editing other super account.
                var permissionView = PreventEditingAnotherSuperAccount(user, "delete");
                if (permissionView != null)
                {
                    return permissionView;
                }

                return View("DeleteStaff", new EditUserViewModel(user));
            }
            ViewBag.Message = "Không thể xóa nhân viên này";
            return View("Error");
        }

        [HttpPost]
        [SimpleAuthorize("SuperUser")]
        // [Audit]
        public ActionResult DeleteStaff(int id, EditUserViewModel form)
        {
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
            if (nhaThuoc.Nhanviens.Any(e => e.User.UserId == id))
            {
                var user = unitOfWork.UserProfileRepository.GetById(id);
                // prevent editing other super account.
                var permissionView = PreventEditingAnotherSuperAccount(user, "delete");
                if (permissionView != null)
                {
                    return permissionView;
                }

                try
                {
                    if (unitOfWork.NhanVienNhaThuocRespository.CheckStaffExist(id))
                    {
                        ViewBag.Message = "Không thể xóa nhân viên này vì đã tồn tại giao dịch. Bạn chỉ có thể dừng hoạt động đối với nhân viên này.";
                        return View("Error");
                    }
                    else
                    {
                        // delete userpermission                        
                        unitOfWork.UserPermissionsRespository.GetMany(c => c.User.UserId == id).ToList().ForEach(c => unitOfWork.UserPermissionsRespository.Delete(c));
                        // delete userinroles
                        unitOfWork.MembershipRepository.DeleteRoleInUsers(id);
                        // delete userprofile
                        unitOfWork.UserProfileRepository.Delete(user);
                        var nv = unitOfWork.NhanVienNhaThuocRespository.Get(c => c.User.UserId == id).FirstOrDefault();
                        // delete nhanvien                        
                        //unitOfWork.Save();

                        unitOfWork.NhanVienNhaThuocRespository.Delete(nv);
                        unitOfWork.Save();

                    }

                    //unitOfWork.NhanVienNhaThuocRespository.GetMany(e => e.User.UserId == id).ToList().ForEach(nv => unitOfWork.NhanVienNhaThuocRespository.Delete(nv));
                    //WebSecurity.DeleteUser(user.UserName, true);                   
                }
                catch (Exception e)
                {
                    ViewBag.Message = "Không thể xóa user này, liên hệ administrator để biết thêm chi tiết";
                    ViewBag.FullMessage = e.Message;
                    return View("Error");
                }
                return RedirectToAction("Staff");
            }
            else
            {
                ViewBag.Message = "Không thể xóa nhân viên này";
                return View("Error");
            }
        }
        [SimpleAuthorize("Admin")]
        public ActionResult StaffPermission(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
            var nhanVien = nhaThuoc.Nhanviens.FirstOrDefault(e => e.User.UserId == id);
            if (nhanVien == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ViewBag.UserId = nhanVien.User.UserId;
            ViewBag.UserName = nhanVien.User.UserName;
            var listPermission = _buildPhanQuyenEditModelFromUserData(unitOfWork.UserPermissionsRespository.GetMany(e => e.User.UserId == nhanVien.User.UserId && e.NhaThuoc.MaNhaThuoc == nhaThuoc.MaNhaThuoc).AsEnumerable());
            var model = listPermission.Where(c => c.Permissions.Where(x => x.Visible).Count() > 0).ToList();
            return View(model);
        }
        [SimpleAuthorize("Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult StaffPermission(int id, ICollection<Permission> form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list = unitOfWork.UserPermissionsRespository.GetMany(e => e.User.UserId == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).AsEnumerable();
            _updatePhanQuyenEditModelToUserData(id, maNhaThuoc, list, form);


            return RedirectToAction("staff");
        }
        private List<Permission> _buildPhanQuyenEditModelFromUserData(
            IEnumerable<UserPermission> userPermissions)
        {
            var model = _buildBlankPhanQuyenEditModel();
            for (var i = 0; i < model.Count; i++)
            {
                var item = model[i];
                userPermissions.Where(e => e.Controller.Equals(item.Controller, StringComparison.OrdinalIgnoreCase)).ForEach(e =>
                {
                    _updateModelPermissionItem(e.Action.ToLower(), e.Title, item.Permissions);
                    //if (e.Action.ToLower() == "index" || e.Action.ToLower()=="details")
                    //{
                    //    _updateModelPermissionItem(e.Action.ToLower(), QuyenEnum.View,item.Permissions);
                    //}
                    //else if (e.Action.ToLower() == "edit")
                    //{
                    //    _updateModelPermissionItem(e.Action.ToLower(), QuyenEnum.Modify, item.Permissions);
                    //}
                    //else if (e.Action.ToLower() == "delete")
                    //{
                    //    _updateModelPermissionItem(e.Action.ToLower(), QuyenEnum.Delete, item.Permissions);
                    //}
                    //else if (e.Action.ToLower() == "create")
                    //{
                    //    _updateModelPermissionItem(e.Action.ToLower(), QuyenEnum.Add, item.Permissions);
                    //}
                });
            }
            return model;
        }

        private void _updateModelPermissionItem(string action, string quyen, List<PermissionItem> permissions)
        {
            permissions.Where(p => p.Permission == quyen).ForEach(e => { e.Checked = true; e.CheckedValue = "checked"; });
        }
        private void _updatePhanQuyenEditModelToUserData(Int32 userId, string maNhaThuoc, IEnumerable<UserPermission> userPermissions, IEnumerable<Permission> formCollection)
        {
            var model = _buildBlankPhanQuyenEditModel();
            for (var i = 0; i < model.Count; i++)
            {
                var item = model[i];
                formCollection.Where(e => e.Controller.ToLower() == item.Controller.ToLower()).ForEach(e =>
                {
                    e.Permissions.Where(p => p.Checked).ForEach(p =>
                    {
                        item.Permissions.Where(pp => pp.Permission == p.Permission).ForEach(f => f.Checked = true);
                    });
                });
            }
            //model.ForEach(e =>
            //{
            //    e.Permissions.ForEach(p =>
            //    {
            //        updatePermissionItem(userPermissions, maNhaThuoc, userId, e.Controller.ToLower(), p.action, p.Permission, p.Checked);                    
            //    });
            //});

            foreach (var item in model)
            {
                foreach (var p in item.Permissions)
                {
                    updatePermissionItem(userPermissions, maNhaThuoc, userId, p.controller.ToLower(), p.action, p.Permission, p.Checked);
                }
            }

            unitOfWork.Save();
        }

        private void updatePermissionItem(IEnumerable<UserPermission> userPermissions, string maNhaThuoc, Int32 userId, string controller, string action, string title, bool hasPermission)
        {
            var permissionItem =
                userPermissions.FirstOrDefault(e => e.Controller == controller.ToLower() && e.Action == action.ToLower() && e.User.UserId == userId &&
                                                    e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.Title == title);
            if (permissionItem == null && hasPermission)
            {
                //insert
                unitOfWork.UserPermissionsRespository.Insert(new UserPermission()
                {
                    User = unitOfWork.UserProfileRepository.GetById(userId),
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                    Action = action.ToLower(),
                    Controller = controller.ToLower(),
                    Title = title
                });
            }

            if (permissionItem != null && !hasPermission)
            {
                //delete
                userPermissions.Where(e => e.Controller == controller.ToLower() && e.Action == action.ToLower() && e.User.UserId == userId &&
                                                    e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.Title == title).ForEach(up => unitOfWork.UserPermissionsRespository.Delete(up));



            }
        }
        private List<Permission> _buildBlankPhanQuyenEditModel()
        {
            return PermissionConfig.GetBlankPermissions();
        }
        [Authorize(Roles = "Admin,SuperUser")]
        public ActionResult UserRoles(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = unitOfWork.UserProfileRepository.GetById(id);
            //if (!User.IsInRole("Admin") && !UserService.CanAccess(user.MaNhaThuoc))
            // return new HttpUnauthorizedResult();
            //TODO: check access
            var model = User.IsInRole("Admin") ? new SelectUserRolesViewModel(user, true) : new SelectUserRolesViewModel(user);



            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,SuperUser")]
        [ValidateAntiForgeryToken]
        public ActionResult UserRoles(SelectUserRolesViewModel model)
        {
            if (ModelState.IsValid)
            {

                var roleProvider = (SimpleRoleProvider)Roles.Provider;
                var user = unitOfWork.UserProfileRepository.GetUserByName(model.UserName);
                //if (!User.IsInRole("Admin") && !UserService.CanAccess(user.MaNhaThuoc))
                //  return new HttpUnauthorizedResult();
                //TODO: check access
                roleProvider.RemoveUsersFromRoles(new[] { model.UserName }, roleProvider.GetRolesForUser(model.UserName));
                roleProvider.AddUsersToRoles(new[] { model.UserName }, new[] { model.RoleName });

                //Bind the function permission
                if (model.Functions != null)
                    foreach (var item in model.Functions)
                    {
                        var operations = Operations.None;
                        var selectedOperations = item.Operations.Where(x => x.Selected).Select(y => y.OperationName).ToArray();
                        foreach (var operation in selectedOperations)
                        {
                            Operations operationEnum;
                            Enum.TryParse(operation, out operationEnum);
                            operations |= operationEnum;
                        }
                        FunctionsService.InsertOrUpdateOperationToRole(model.UserId, item.FunctionId, operations, "User", user.MaNhaThuoc);
                    }

                return RedirectToAction("Index");
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult InvalidUserName()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPwStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordForStaff()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        // [Audit]
        public ActionResult ResetPasswordConfirmation(ResetPasswordConfirmModel model)
        {
            if (WebSecurity.ResetPassword(model.Token, model.NewPassword))
            {
                return RedirectToAction("PasswordResetSuccess");
            }
            return RedirectToAction("PasswordResetFailure");
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(string id)
        {
            ResetPasswordConfirmModel model = new ResetPasswordConfirmModel() { Token = id };
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PasswordResetFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PasswordResetSuccess()
        {
            return View();
        }

        public ActionResult UserPicker()
        {
            var model = unitOfWork.UserProfileRepository.Get(e => e.HoatDong).OrderBy(e => e.UserName).Select(e => new EditUserViewModel(e)).AsEnumerable();

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Unauthorised()
        {
            return View();
        }

        #region Helpers
        private void AddErrors(List<ModelError> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            ChangePasswordFailure
        }


        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Tên tài khoản đã tồn tại";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Email đã được dùng vui lòng chon email khác";

                case MembershipCreateStatus.InvalidPassword:
                    return "Mật khẩu không đúng";

                case MembershipCreateStatus.InvalidEmail:
                    return "Email không đúng";

                case MembershipCreateStatus.InvalidAnswer:
                    return "Câu trả lời để khôi phục mật khẩu không đúng";

                case MembershipCreateStatus.InvalidQuestion:
                    return "Câu hỏi khôi phục mật khẩu không đúng";

                case MembershipCreateStatus.InvalidUserName:
                    return "Tài khoản không đúng";

                case MembershipCreateStatus.ProviderError:
                    return "Có lỗi sảy ra";

                case MembershipCreateStatus.UserRejected:
                    return "Tạo tài khoản thất bại, vui lòng thử lại";

                default:
                    return "Có lỗi sảy ra";
            }
        }

        private ActionResult PreventEditingAnotherSuperAccount(UserProfile inputUser, string action = "")
        {
            // kiem tra tai khoan dang sua co phai tai khoan hien tai khong?
            var modifyingUserRoles = Roles.Provider.GetRolesForUser(inputUser.UserName);
            if (modifyingUserRoles.Any(r => r == Constants.Security.Roles.SuperUser.Value))
            {
                if (WebSecurity.GetCurrentUserId != inputUser.UserId)
                {
                    ViewBag.Message = "Không thể sửa tài khoản khác có quyền " + Constants.Security.Roles.SuperUser.Text;
                    return View("NoPermission");
                }

                if (action.ToLower() == "delete")
                {
                    ViewBag.Message = "Không thể xóa tài khoản có quyền " + Constants.Security.Roles.SuperUser.Text;
                    return View("NoPermission");
                }
            }
            return null;
        }

        private string FormatUserName(string username, string manhathuoc, bool flag)
        {
            if (flag)
            {
                return string.Concat(username, "@", manhathuoc);
            }
            else
            {
                var index = username.LastIndexOf("@");
                return index > -1 ? username.Substring(0, index) : username;
            }
        }
        #endregion
    }
}
