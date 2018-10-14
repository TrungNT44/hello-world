using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using PagedList;
using MedMan.App_Start;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using sThuoc.Repositories;
using WebGrease.Css.Extensions;
using WebMatrix.WebData;
using WebSecurity = sThuoc.Filter.WebSecurity;
using sThuoc.Utils;
using App.Common.MVC;
using Med.Web.Filter;
using System.Linq.Expressions;
using Med.Service.Drug;
using App.Common.DI;
using Med.Common.Enums;

namespace Med.Web.Controllers
{
    public class NhaThuocController : BaseController
    {
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        // GET: NhaThuoc
        [Authorize(Roles = "SuperUser")]
        public ActionResult Index(String currentFilter, string searchString, String OrderColumn, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.SearchString = searchString;
            ViewBag.CurrentSort = OrderColumn;
            ViewBag.TenNhaThuoc = OrderColumn == "TenNhaThuoc" ? "TenNhaThuoc_desc" : "TenNhaThuoc";
            ViewBag.MaNhaThuoc = String.IsNullOrEmpty(OrderColumn) ? "MaNhaThuoc_desc" : ""; ;
            ViewBag.HoatDong = OrderColumn == "HoatDong" ? "HoatDong_desc" : "HoatDong"; ;
            var qry = unitOfWork.NhaThuocRepository.GetMany();//;
            if (!string.IsNullOrEmpty(searchString))
                qry = qry.Where(e => e.TenNhaThuoc.Contains(searchString) || e.DiaChi.Contains(searchString) || e.MaNhaThuoc.Contains(searchString));
            switch (OrderColumn)
            {
                case "TenNhaThuoc":
                    qry = qry.OrderBy(x => x.TenNhaThuoc);
                    break;
                case "TenNhaThuoc_desc":
                    qry = qry.OrderByDescending(x => x.TenNhaThuoc);
                    break;
                case "HoatDong":
                    qry = qry.OrderBy(x => x.HoatDong);
                    break;
                case "HoatDong_desc":
                    qry = qry.OrderByDescending(x => x.HoatDong);
                    break;
                case "MaNhaThuoc_desc":
                    qry = qry.OrderByDescending(x => x.MaNhaThuoc);
                    break;
                case "MaNhaThuoc":
                default:
                    qry = qry.OrderBy(x => x.MaNhaThuoc);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            //qry = qry.OrderBy(e => e.TenNhaThuoc);
            var result = qry.ToPagedList(pageNumber, pageSize);
            //var model = result.Select(e => new NhaThuocViewModel(e)).ToList();
            return View(result);
        }

        // GET: NhaThuoc/Details/5
        [Authorize(Roles = "SuperUser")]
        public ActionResult Details(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("index");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (doiTuongNhaThuoc != null)
            {

                return View(new NhaThuocViewModel(doiTuongNhaThuoc));
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }

            return View("Error");
        }
        private bool checkGiaoDich(String MaNhaThuoc)
        {
            int count = 0;
            count += unitOfWork.NhaThuocRepository.GetById(MaNhaThuoc).PhieuKiemKes.Count;
            count += unitOfWork.NhaThuocRepository.GetById(MaNhaThuoc).PhieuNhaps.Count;
            count += unitOfWork.NhaThuocRepository.GetById(MaNhaThuoc).PhieuThuChis.Count;
            count += unitOfWork.NhaThuocRepository.GetById(MaNhaThuoc).PhieuXuats.Count;

            if (count > 0)
                return false;
            return true;//Được phép cập nhật, xóa
        }
        [SimpleAuthorize("User")]
        public ActionResult ThongTin(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc \"" + id + "\" hoặc bạn không có quyền truy cập nhà thuốc này";
                return View("Error");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (doiTuongNhaThuoc != null)
            {

                return View(new NhaThuocViewModel(doiTuongNhaThuoc));
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc \"" + id + "\" hoặc bạn không có quyền truy cập nhà thuốc này";
            }

            return View("Error");
        }

        [HttpGet]
        [SimpleAuthorize("Admin")]
        public ActionResult EditInfo(string id)
        {
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (doiTuongNhaThuoc != null)
            {

                return View(new NhaThuocViewModel(doiTuongNhaThuoc));
            }

            return View("Error");
        }

        [HttpPost]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult EditInfo(NhaThuocViewModel formCollection)
        {
            if (ModelState.IsValid)
            {
                var nhaThuoc = formCollection.ToDomainModel(unitOfWork.NhaThuocRepository.GetById(formCollection.MaNhaThuoc));
                nhaThuoc.Modified = DateTime.Now;
                nhaThuoc.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                unitOfWork.NhaThuocRepository.Update(nhaThuoc);
                unitOfWork.Save();
                return View("thongtin", formCollection);
            }

            return View("Error");
        }
        [Authorize(Roles = "SuperUser")]
        // GET: NhaThuoc/Create
        public ActionResult Create()
        {

            var model = new NhaThuocViewModel() { MaNhaThuoc = unitOfWork.NhaThuocRepository.GenereateNextId("NT") };
            //var items = new TinhThanhServices().getMany().ToList();
            //ViewBag.LstTinhThanh = items;
            return View(model);
        }

        // POST: NhaThuoc/Create
        [Authorize(Roles = "SuperUser")]
        [HttpPost]
        // [Audit]
        public ActionResult Create(NhaThuocViewModel formCollection)
        {
            try
            {
                UserProfile tkQuanLy = null;
                // check administrator account.
                if (formCollection.Administrator <= 0)
                    ModelState.AddModelError("Administrator", "Chưa chọn tài khoản quản lý");
                else
                    tkQuanLy = unitOfWork.UserProfileRepository.GetById(formCollection.Administrator);
                if (tkQuanLy == null)
                {
                    formCollection.Administrator = 0;
                    ModelState.AddModelError("Administrator", "Tài khoản quản lý không tồn tại");
                }
                if (ModelState.IsValid)
                {
                    var nhaThuoc = formCollection.ToDomainModel();
                    nhaThuoc.HoatDong = true; // Mặc định là true khi tạo mới
                    nhaThuoc.Created = DateTime.Now;
                    nhaThuoc.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);

                    unitOfWork.NhaThuocRepository.Insert(nhaThuoc);
                    var nhanVienNhaThuoc = new NhanVienNhaThuoc()
                    {
                        NhaThuoc = nhaThuoc,
                        User = tkQuanLy,
                        Role = Constants.Security.Roles.Admin.Value
                    };
                    unitOfWork.NhanVienNhaThuocRespository.Insert(nhanVienNhaThuoc);
                    // default entities
                    taoDuLieuBanDauChoNhaThuoc(nhaThuoc);
                    unitOfWork.Save();
                    // assign Admin role to the user.
                    Roles.Provider.AddUsersToRoles(new[] { tkQuanLy.UserName }, new[] { Constants.Security.Roles.Admin.Value });

                }
                else
                {
                    //var items = new TinhThanhServices().getMany().ToList();
                    //ViewBag.LstTinhThanh = items;
                    return View(formCollection);
                }


                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMsg = "Có lỗi xảy ra, vui lòng liên hệ quản trị hệ thống!";
                return RedirectToAction("Index");
            }
        }

        private void taoDuLieuBanDauChoNhaThuoc(NhaThuoc nhathuoc)
        {
            var createdBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
            Constants.Default.ConstantEntities.NhomKhachHangs.ForEach(e =>
                unitOfWork.NhomKhachHangRepository.Insert(new NhomKhachHang()
                {
                    TenNhomKhachHang = e,
                    Created = DateTime.Now,
                    CreatedBy = createdBy,
                    NhaThuoc = nhathuoc
                }));
            Constants.Default.ConstantEntities.KhachHangs.ForEach(e => unitOfWork.KhachHangRepository.Insert(new KhachHang()
            {
                TenKhachHang = e,
                Created = DateTime.Now,
                CreatedBy = createdBy,
                NhomKhachHang = nhathuoc.NhomKhachHangs.First(),
                NhaThuoc = nhathuoc,
                CustomerTypeId = Constants.Default.ConstantEntities.KhachHangLe == e ? (int)CustomerType.Retail: (int)CustomerType.InventoryAdjustment
            }));
            Constants.Default.ConstantEntities.NhomNhaCungCaps.ForEach(e => unitOfWork.NhomNhaCungCapRepository.Insert(new NhomNhaCungCap()
            {
                TenNhomNhaCungCap = e,
                Created = DateTime.Now,
                CreatedBy = createdBy,
                NhaThuoc = nhathuoc,
                IsDefault = true
            }));
            Constants.Default.ConstantEntities.NhaCungCaps.ForEach(e => unitOfWork.NhaCungCapRespository.Insert(new NhaCungCap()
            {
                TenNhaCungCap = e,
                Created = DateTime.Now,
                CreatedBy = createdBy,
                NhomNhaCungCap = nhathuoc.NhomNhaCungCaps.First(),
                NhaThuoc = nhathuoc
            }));

        }
        public ActionResult Settings()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            sThuoc.Utils.Helpers.AddDefaultSettingForAdmin(maNhaThuoc, unitOfWork);
            return View();
        }
        // GET: NhaThuoc/Edit/5
        [Authorize(Roles = "SuperUser")]
        public ActionResult Edit(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("index");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (doiTuongNhaThuoc != null)
            {

                return View("Create", new NhaThuocViewModel(doiTuongNhaThuoc));
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }

            return View("Error");
        }

        // POST: NhaThuoc/Edit/5
        [Authorize(Roles = "SuperUser")]
        [HttpPost]
        // [Audit]
        public ActionResult Edit(string id, NhaThuocViewModel formCollection)
        {
            try
            {
                UserProfile tkQuanLy = null;
                // check administrator account.
                if (formCollection.Administrator <= 0)
                    ModelState.AddModelError("Administrator", "Chưa chọn tài khoản quản lý");
                else
                    tkQuanLy = unitOfWork.UserProfileRepository.GetById(formCollection.Administrator);
                if (tkQuanLy == null)
                {
                    ModelState.AddModelError("Administrator", "Tài khoản quản lý không tồn tại");
                }
                var nhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
                if (nhaThuoc == null)
                {
                    ViewBag.Message = "Không tồn tại nhà thuốc; ID= " + id;
                    return View("Error");
                }
                if (ModelState.IsValid)
                {
                    nhaThuoc.Mobile = formCollection.Mobile;
                    nhaThuoc.DiaChi = formCollection.DiaChi;
                    nhaThuoc.DienThoai = formCollection.DienThoai;
                    nhaThuoc.DuocSy = formCollection.DuocSy;
                    nhaThuoc.Email = formCollection.Email;
                    nhaThuoc.NguoiDaiDien = formCollection.NguoiDaiDien;
                    nhaThuoc.SoKinhDoanh = formCollection.SoKinhDoanh;
                    nhaThuoc.TenNhaThuoc = formCollection.TenNhaThuoc;
                    nhaThuoc.TinhThanhId = formCollection.TinhThanhId;
                    //nhaThuoc.HoatDong = formCollection.HoatDong;
                    nhaThuoc.Modified = DateTime.Now;
                    nhaThuoc.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                    if (checkGiaoDich(formCollection.MaNhaThuoc))
                    {
                        nhaThuoc.MaNhaThuocCha = formCollection.MaNhaThuocCha;
                    }
                    unitOfWork.NhaThuocRepository.Update(nhaThuoc);
                    var nhanVienNhaThuoc = nhaThuoc.Nhanviens.FirstOrDefault(e => e.Role == Constants.Security.Roles.Admin.Value);
                    if (nhanVienNhaThuoc == null)
                    {
                        nhanVienNhaThuoc = new NhanVienNhaThuoc()
                        {
                            NhaThuoc = nhaThuoc,
                            User = tkQuanLy,
                            Role = Constants.Security.Roles.Admin.Value
                        };
                        unitOfWork.NhanVienNhaThuocRespository.Insert(nhanVienNhaThuoc);
                    }
                    else
                    {
                        nhanVienNhaThuoc.User = tkQuanLy;
                        unitOfWork.NhanVienNhaThuocRespository.Update(nhanVienNhaThuoc);
                    }

                    unitOfWork.Save();
                    // assign Admin role to the user.
                    if (!Roles.Provider.IsUserInRole(tkQuanLy.UserName, Constants.Security.Roles.Admin.Value))
                        Roles.Provider.AddUsersToRoles(new[] { tkQuanLy.UserName }, new[] { Constants.Security.Roles.Admin.Value });

                }
                else
                {
                    return View("Create", formCollection);
                }


                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("Create");
            }
        }
        [Authorize(Roles = "SuperUser")]
        // GET: NhaThuoc/Delete/5
        public ActionResult Delete(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("index");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);
            if (doiTuongNhaThuoc != null)
            {
                ViewBag.OnDeleting = true;
                return View("Details", new NhaThuocViewModel(doiTuongNhaThuoc));
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }

            return View("Error");
        }
        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        // [Audit]
        public ActionResult ConfirmDelete(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("index");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);

            if (doiTuongNhaThuoc != null)
            {
                var service = IoC.Container.Resolve<IDrugStoreService>();
                service.DeleteDrugStore(doiTuongNhaThuoc.MaNhaThuoc);
           
                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }

            return View("Error");
        }
       
        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public ActionResult ConfirmRollback(string id)
        {
            // tro lai trang ds nha thuoc neu ko truyen vao id
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("index");
            }
            var doiTuongNhaThuoc = unitOfWork.NhaThuocRepository.GetById(id);

            if (doiTuongNhaThuoc != null)
            {
                var service = IoC.Container.Resolve<IDrugStoreService>();
                service.RollbackDrugStore(doiTuongNhaThuoc.MaNhaThuoc);

                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Message = "Không tìm thấy nhà thuốc theo mã \"" + id + "\"";
            }

            return View("Error");
        }
        public ActionResult NhaThuocPicker()
        {
            List<NhaThuoc> model = unitOfWork.NhaThuocRepository.Get(e => e.HoatDong && e.MaNhaThuoc == e.MaNhaThuocCha).OrderBy(e => e.TenNhaThuoc).Select(e => e).AsEnumerable().ToList<NhaThuoc>();

            return View(model);
        }
    }
}
