using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class NhomKhachHangsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();


        // GET: NhomKhachHangs
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            var _maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.NhaThuocSession = this.GetNhaThuoc();
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            //ViewBag.NameSortParm2 = sortOrder == "tennhom" ? "tennhom_desc" : "tennhom";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilterTen = searchString;
            var qry = db.NhomKhachHangs.Where(x => x.NhaThuoc.MaNhaThuoc == _maNhaThuoc);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenNhomKhachHang.Contains(searchString));
            }
            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenNhomKhachHang);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenNhomKhachHang);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }
        // GET: NhomKhachHangs/Details/5
        [SimpleAuthorize(FunctionResource.NhomKhachHang, Operations.Read)]
        public async Task<ActionResult> Details(int? id)
        {
            var _maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NhomKhachHang nhomKhachHang = await db.NhomKhachHangs.FindAsync(id);
            if (nhomKhachHang == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Admin") && nhomKhachHang.NhaThuoc.MaNhaThuoc != _maNhaThuoc)
                throw new UnauthorizedAccessException();
            return View(nhomKhachHang);
        }

        // GET: NhomKhachHangs/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: NhomKhachHangs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Create([Bind(Include = "MaNhomKhachHang,TenNhomKhachHang,GhiChu")] NhomKhachHang nhomKhachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var item = unitOfWork.NhomKhachHangRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.TenNhomKhachHang.Equals(nhomKhachHang.TenNhomKhachHang.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (item != null)
                    {
                        ModelState.AddModelError("TenNhomKhachHang", "Tên nhóm khách hàng này đã tồn tại. Vui lòng nhập tên nhóm khách hàng khác.");
                    }
                    else
                    {
                        nhomKhachHang.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                        nhomKhachHang.Created = DateTime.Now;
                        nhomKhachHang.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.NhomKhachHangRepository.Insert(nhomKhachHang);
                        unitOfWork.Save();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhomKhachHang", "Error: " + e.Message);
                }

            }

            return View(nhomKhachHang);
        }

        // GET: NhomKhachHangs/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhomKhachHang =
                unitOfWork.NhomKhachHangRepository.GetMany(
                    e => e.MaNhomKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (nhomKhachHang == null)
            {
                return HttpNotFound();
            }
            return View(nhomKhachHang);
        }

        // POST: NhomKhachHangs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Edit([Bind(Include = "MaNhomKhachHang,TenNhomKhachHang,GhiChu")] NhomKhachHang nhomKhachHang)
        {
            if (ModelState.IsValid)
            {
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                
                var itemExits = unitOfWork.NhomKhachHangRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.MaNhomKhachHang != nhomKhachHang.MaNhomKhachHang && c.TenNhomKhachHang.Equals(nhomKhachHang.TenNhomKhachHang.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();                
                if (itemExits != null)
                {
                    ModelState.AddModelError("TenNhomKhachHang", "Tên nhóm khách hàng này đã tồn tại. Vui lòng nhập tên nhóm khách hàng khác.");
                    return View(nhomKhachHang);
                }

                var nhom =
                    unitOfWork.NhomKhachHangRepository.GetMany(
                        e => e.MaNhomKhachHang == nhomKhachHang.MaNhomKhachHang && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                        .First();
                if (Constants.Default.ConstantEntities.NhomKhachHangs.Contains(nhom.TenNhomKhachHang))
                {
                    // khong cho phep sua ten khach hang mac dinh.
                    ModelState.AddModelError("TenNhomKhachHang", "Không thể sửa tên nhóm khách hàng mặc định: " + nhom.TenNhomKhachHang);
                }                

                if (nhom != null && ModelState.IsValid)
                {
                    nhom.TenNhomKhachHang = nhomKhachHang.TenNhomKhachHang;
                    nhom.GhiChu = nhomKhachHang.GhiChu;
                    nhom.Modified = DateTime.Now;
                    nhom.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                    unitOfWork.NhomKhachHangRepository.Update(nhom);
                    unitOfWork.Save();
                    return RedirectToAction("Index");
                }


            }
            return View(nhomKhachHang);
        }

        // GET: NhomKhachHangs/Delete/5
        [SimpleAuthorize("Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhom =
                unitOfWork.NhomKhachHangRepository.GetMany(
                    e => e.MaNhomKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (nhom == null)
            {
                return HttpNotFound();
            }
            return View(nhom);
        }

        // POST: NhomKhachHangs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Delete(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            NhomKhachHang nhomKhachHang =
                unitOfWork.NhomKhachHangRepository.GetMany(
                    e => e.MaNhomKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (Constants.Default.ConstantEntities.NhomKhachHangs.Contains(nhomKhachHang.TenNhomKhachHang))
            {
                // khong cho phep sua ten khach hang mac dinh.
                ViewBag.Message = "Không thể xóa nhóm khách hàng mặc định: " + nhomKhachHang.TenNhomKhachHang;
                return View("Error");
            }

            if (nhomKhachHang != null)
            {
                try
                {
                    var flag = unitOfWork.NhomKhachHangRepository.CheckCustomerExist(nhomKhachHang.MaNhomKhachHang);
                    if (!flag)
                    {
                        unitOfWork.NhomKhachHangRepository.Delete(nhomKhachHang);
                        unitOfWork.Save();
                    }
                    else
                    {
                        ViewBag.Message = "Không thể xóa nhóm khách hàng: " + nhomKhachHang.TenNhomKhachHang +
                                     "<br/> Nguyên nhân có thể là do nhóm khách hàng đã được sử dụng";                        
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Message = "Không thể xóa nhóm khách hàng: " + nhomKhachHang.TenNhomKhachHang +
                                      "<br/> Nguyên nhân có thể là do nhóm khách hàng đã được sử dụng";
                    ViewBag.Exception = e.Message;
                    return View("Error");

                }

            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
