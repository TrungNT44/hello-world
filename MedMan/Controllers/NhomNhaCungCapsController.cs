using PagedList;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class NhomNhaCungCapsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();

        [SimpleAuthorize("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            //ViewBag.NameSortParm2 = sortOrder == "tennhom" ? "tennhom_desc" : "tennhom";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilterTen = searchString;
            var qry = db.NhomNhaCungCaps.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenNhomNhaCungCap.Contains(searchString));
            }
            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenNhomNhaCungCap);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenNhomNhaCungCap);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        // GET: NhomNhaCungCaps/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NhomNhaCungCap nhomNhaCungCap = await db.NhomNhaCungCaps.FindAsync(id);
            if (nhomNhaCungCap == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Admin") && nhomNhaCungCap.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
                throw new UnauthorizedAccessException();
            return View(nhomNhaCungCap);
        }

        // GET: NhomNhaCungCaps/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: NhomNhaCungCaps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaNhomNhaCungCap,TenNhomNhaCungCap,GhiChu,MaNhaThuoc")] NhomNhaCungCap nhomNhaCungCap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var itemExist = unitOfWork.NhomNhaCungCapRepository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.TenNhomNhaCungCap.Trim().Equals(nhomNhaCungCap.TenNhomNhaCungCap.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenNhomNhaCungCap", "Tên nhóm nhà cung cấp này đã tồn tại. Vui lòng nhập tên nhóm nhà cung cấp khác.");
                    }
                    else
                    {
                        nhomNhaCungCap.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                        nhomNhaCungCap.Created = DateTime.Now;
                        nhomNhaCungCap.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.NhomNhaCungCapRepository.Insert(nhomNhaCungCap);
                        unitOfWork.Save();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhomNhaCungCap", e.Message);
                }
            }

            return View(nhomNhaCungCap);
        }

        // GET: NhomNhaCungCaps/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            NhomNhaCungCap nhomNhaCungCap =
                unitOfWork.NhomNhaCungCapRepository.GetMany(
                    e => e.MaNhomNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (nhomNhaCungCap == null)
            {
                return HttpNotFound();
            }
            return View(nhomNhaCungCap);
        }

        // POST: NhomNhaCungCaps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Edit([Bind(Include = "MaNhomNhaCungCap,TenNhomNhaCungCap,GhiChu,MaNhaThuoc")] NhomNhaCungCap nhomNhaCungCap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var itemExist = unitOfWork.NhomNhaCungCapRepository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.MaNhomNhaCungCap != nhomNhaCungCap.MaNhomNhaCungCap && c.TenNhomNhaCungCap.Trim().Equals(nhomNhaCungCap.TenNhomNhaCungCap.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenNhomNhaCungCap", "Tên nhóm nhà cung cấp này đã tồn tại. Vui lòng nhập tên nhóm nhà cung cấp khác.");
                    }
                    else
                    {
                        var nhom =
                            unitOfWork.NhomNhaCungCapRepository.GetMany(
                                e =>
                                    e.MaNhomNhaCungCap == nhomNhaCungCap.MaNhomNhaCungCap &&
                                    e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
                        if (Constants.Default.ConstantEntities.NhomNhaCungCaps.Contains(nhom.TenNhomNhaCungCap))
                        {
                            ModelState.AddModelError("TenNhomNhaCungCap", "Không thể sửa tên nhóm nhà cung cấp mặc định + " + nhom.TenNhomNhaCungCap);
                        }
                        else
                        {
                            nhom.TenNhomNhaCungCap = nhomNhaCungCap.TenNhomNhaCungCap;
                            nhom.GhiChu = nhomNhaCungCap.GhiChu;
                            nhom.Modified = DateTime.Now;
                            nhom.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                            unitOfWork.NhomNhaCungCapRepository.Update(nhom);
                            unitOfWork.Save();
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhomNhaCungCap", e.Message);
                }

            }
            return View(nhomNhaCungCap);
        }

        // GET: NhomNhaCungCaps/Delete/5
        [SimpleAuthorize("Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhomNhaCungCap =
                unitOfWork.NhomNhaCungCapRepository.GetMany(
                    e => e.MaNhomNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (nhomNhaCungCap == null)
            {
                return HttpNotFound();
            }
            return View(nhomNhaCungCap);
        }

        // POST: NhomNhaCungCaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Delete(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhomNhaCungCap =
                unitOfWork.NhomNhaCungCapRepository.GetMany(
                    e => e.MaNhomNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (Constants.Default.ConstantEntities.NhomNhaCungCaps.Contains(nhomNhaCungCap.TenNhomNhaCungCap))
            {
                ViewBag.Message = "Không thể xóa nhóm nhà cung cấp mặc đinh: " + nhomNhaCungCap.TenNhomNhaCungCap;
                return View("Error");
            }

            if (nhomNhaCungCap != null)
            {

                try
                {
                    unitOfWork.NhomNhaCungCapRepository.Delete(nhomNhaCungCap);
                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    ViewBag.Message = "Không thể xóa nhóm nhà cung cấp: " + nhomNhaCungCap.TenNhomNhaCungCap +
                                      "<br/> Nguyên nhân có thể là do nhóm nhà cung cấp đã được sử dụng";
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
