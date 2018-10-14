using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using App.Common.DI;
using Med.Service.Drug;
using PagedList;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using App.Common.MVC;
using Med.Web.Data.Session;
using Med.Web.Filter;
using MedMan;

namespace Med.Web.Controllers
{
    public class NhomThuocsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string _maNhaThuoc = UserService.GetMaNhaThuoc();
        private UnitOfWork unitOfWork = new UnitOfWork();

        // GET: NhomThuocs
        [SimpleAuthorize("Admin")]
        //public async Task<ActionResult> Index(string searchString, int? page)
        //{
        //    ViewBag.SearchString = searchString;
        //    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
        //    var qry =  unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);//
        //    if (!string.IsNullOrEmpty(searchString))
        //        qry = qry.Where(e => e.TenNhomThuoc.Contains(searchString));                                           
        //    const int pageSize = 10;
        //    int pageNumber = (page ?? 1);
        //    qry = qry.OrderBy(e => e.TenNhomThuoc);
        //    var result = qry.ToPagedList(pageNumber, pageSize);
        //    return View(result);
        //}
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            //ViewBag.CurrentFilterTen = searchString;
            //var qry = unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);//
            //if (!string.IsNullOrEmpty(searchString))
            //    qry = qry.Where(e => e.TenNhomThuoc.Contains(searchString));

            ViewBag.CurrentFilterTen = searchString;
            var qry = db.NhomThuocs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenNhomThuoc.Contains(searchString));
            }

            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenNhomThuoc);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenNhomThuoc);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        // GET: NhomThuocs/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            NhomThuoc nhomThuoc =
                await
                    unitOfWork.NhomThuocRepository.GetMany(
                        e => e.MaNhomThuoc == id && (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)).FirstAsync();
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            return View(nhomThuoc);
        }

        // GET: NhomThuocs/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: NhomThuocs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaNhomThuoc,TenNhomThuoc,KyHieuNhomThuoc")] NhomThuoc nhomThuoc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nhaThuoc = this.GetNhaThuoc();
                    var maNhaThuoc = nhaThuoc.MaNhaThuoc;
                    var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
                    var item = unitOfWork.NhomThuocRepository.Get(c => (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha) && c.TenNhomThuoc == nhomThuoc.TenNhomThuoc).FirstOrDefault();
                    if (item != null)
                    {
                        ModelState.AddModelError("TenNhomThuoc", "Tên nhóm thuốc này đã tồn tại. Vui lòng nhập tên nhóm thuốc khác");
                    }
                    else
                    {
                        nhomThuoc.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuocCha);
                        nhomThuoc.TenNhomThuoc = nhomThuoc.TenNhomThuoc;
                        nhomThuoc.KyHieuNhomThuoc = nhomThuoc.KyHieuNhomThuoc;
                        nhomThuoc.Created = DateTime.Now;
                        nhomThuoc.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.NhomThuocRepository.Insert(nhomThuoc);
                        unitOfWork.Save();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhomThuoc", e.Message);
                }
            }

            return View(nhomThuoc);
        }

        // GET: NhomThuocs/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            NhomThuoc nhomThuoc =
                await
                    unitOfWork.NhomThuocRepository.GetMany(
                        e => e.MaNhomThuoc == id && (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)).FirstAsync();
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            return View(nhomThuoc);
        }

        // POST: NhomThuocs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaNhomThuoc,TenNhomThuoc,KyHieuNhomThuoc,MaNhaThuoc")] NhomThuoc nhomThuoc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nhaThuoc = this.GetNhaThuoc();
                    var maNhaThuoc = nhaThuoc.MaNhaThuoc;
                    var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
                    var existItem = unitOfWork.NhomThuocRepository.Get(c => c.TenNhomThuoc == nhomThuoc.TenNhomThuoc && c.MaNhomThuoc != nhomThuoc.MaNhomThuoc && (c.NhaThuoc.MaNhaThuoc == maNhaThuoc || c.NhaThuoc.MaNhaThuoc == maNhaThuocCha)).FirstOrDefault();
                    if (existItem != null)
                    {
                        ModelState.AddModelError("TenNhomThuoc", "Tên nhóm thuốc này đã tồn tại. Vui lòng nhập tên nhóm thuốc khác.");
                        return View(nhomThuoc);
                    }
                    else
                    {
                        var nThuoc = await unitOfWork.NhomThuocRepository.GetMany(e => e.MaNhomThuoc == nhomThuoc.MaNhomThuoc && (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)).FirstAsync();
                        if (nThuoc != null)
                        {
                            nThuoc.TenNhomThuoc = nhomThuoc.TenNhomThuoc;
                            nThuoc.KyHieuNhomThuoc = nhomThuoc.KyHieuNhomThuoc;
                            nThuoc.Modified = DateTime.Now;
                            nThuoc.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                            unitOfWork.NhomThuocRepository.Update(nThuoc);
                            unitOfWork.Save();
                        }
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhomThuoc", e.Message);
                    return View(nhomThuoc);
                }

                return RedirectToAction("Index");
            }
            return View(nhomThuoc);
        }

        // GET: NhomThuocs/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var nhaThuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhaThuoc.MaNhaThuoc;
            var maNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            var nhomThuoc = await unitOfWork.NhomThuocRepository.GetMany(e => e.MaNhomThuoc == id && (e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha)).FirstAsync();
            if (nhomThuoc == null)
            {
                return HttpNotFound();
            }
            return View(nhomThuoc);
        }

        // POST: NhomThuocs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Delete(int id)
        {
            var service = IoC.Container.Resolve<IDrugManagementService>();
            if (!service.DeleteDrugGroups(WebSessionManager.Instance.CurrentDrugStoreCode, id))
            {
                ViewBag.Message = "Không thể xóa nhóm thuốc này vì đã tồn tại giao dịch.";
                return View("Error");
            }
            MainApp.Instance.ReloadCacheDrugs(MedSessionManager.CurrentDrugStoreCode);

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
