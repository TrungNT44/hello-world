using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class DangBaoChesController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        private UnitOfWork unitOfWork = new UnitOfWork();
        // GET: DangBaoChes
        public ActionResult Index()
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var maNhaThuocCha = nhathuoc.MaNhaThuocCha;
            var rs = db.DangBaoChes.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc || x.NhaThuoc.MaNhaThuoc == maNhaThuocCha).ToList();
            return View(rs);
        }

        // GET: DangBaoChes/Details/5
        //[SimpleAuthorize(FunctionResource.DangBaoChe, Operations.Read)]
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DangBaoChe dangBaoChe = await db.DangBaoChes.FindAsync(id);
        //    if (dangBaoChe == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (!User.IsInRole("Admin") && dangBaoChe.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
        //        throw new UnauthorizedAccessException();
        //    return View(dangBaoChe);
        //}

        // GET: DangBaoChes/Create
        [SimpleAuthorize(FunctionResource.DangBaoChe, Operations.Create)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: DangBaoChes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]     
        public async Task<ActionResult> Create([Bind(Include = "MaDangBaoChe,TenDangBaoChe")] DangBaoChe dangBaoChe)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(dangBaoChe.TenDangBaoChe))
                        ModelState.AddModelError("TenDonViTinh", "Đơn vị tính không thể bỏ trống");
                    if (ModelState.IsValid)
                    {
                        // kiem tra da ton tai don vi tinh hay chua
                        var maNhaThuoc = this.GetNhaThuoc().MaNhaThuocCha;
                        var dvt =
                            unitOfWork.DangBaoCheRepository.GetMany(
                                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.TenDangBaoChe == dangBaoChe.TenDangBaoChe && e.MaDangBaoChe != dangBaoChe.MaDangBaoChe)
                                .FirstOrDefault();
                        if (dvt != null)
                            ModelState.AddModelError("TenDangBaoChe", "Dạng bào chế đã tồn tại");
                        if (ModelState.IsValid)
                        {
                            dangBaoChe.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                            unitOfWork.DangBaoCheRepository.Insert(dangBaoChe);
                            unitOfWork.Save();
                        }
                    }

                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenDangBaoChe", e.Message);
                }
            }
            if (dangBaoChe.MaDangBaoChe > 0)
                return Json(new { success = true, id = dangBaoChe.MaDangBaoChe, title = dangBaoChe.TenDangBaoChe });

            return Json(new { success = false, message = ModelState.GetFirstErrorMessage() });
        }

        // GET: DangBaoChes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DangBaoChe dangBaoChe =
                await
                    unitOfWork.DangBaoCheRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDangBaoChe == id).FirstAsync();
            if (dangBaoChe == null)
            {
                return HttpNotFound();
            }
            return View(dangBaoChe);
        }

        // POST: DangBaoChes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaDangBaoChe,TenDangBaoChe")] DangBaoChe dangBaoChe)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(dangBaoChe.TenDangBaoChe))
                    ModelState.AddModelError("TenDangBaoChe", "Tên dạng bào chế không thể bỏ trống");
                if (ModelState.IsValid)
                {
                    // kiem tra da ton tai don vi tinh hay chua
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var dbc =
                        unitOfWork.DangBaoCheRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.TenDangBaoChe == dangBaoChe.TenDangBaoChe && e.MaDangBaoChe != dangBaoChe.MaDangBaoChe)
                            .FirstOrDefault();
                    if (dbc != null)
                        ModelState.AddModelError("TenDangBaoChe", "Tên dạng bào chế đã tồn tại");
                    if (ModelState.IsValid)
                    {
                        dbc = unitOfWork.DangBaoCheRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDangBaoChe == dangBaoChe.MaDangBaoChe).FirstOrDefault();
                        if (dbc != null)
                        {
                            dbc.TenDangBaoChe = dangBaoChe.TenDangBaoChe;
                            unitOfWork.DangBaoCheRepository.Update(dbc);
                            unitOfWork.Save();
                            return Json(new { success = true, id = dbc.MaDangBaoChe, title = dbc.TenDangBaoChe });
                        }


                    }
                }

            }
            return Json(new { success = false, message = ModelState.GetFirstErrorMessage() });
        }

        // GET: DangBaoChes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DangBaoChe dangBaoChe =
                await
                    unitOfWork.DangBaoCheRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDangBaoChe == id).FirstAsync();
            if (dangBaoChe == null)
            {
                return HttpNotFound();
            }
            return View(dangBaoChe);
        }

        // POST: DangBaoChes/Delete/5
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                DangBaoChe dangBaoChe =
                    await
                        unitOfWork.DangBaoCheRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDangBaoChe == id).FirstAsync();

                var deletedId = dangBaoChe.MaDangBaoChe;
                unitOfWork.DangBaoCheRepository.Delete(dangBaoChe);
                unitOfWork.Save();
                return Json(new { success = true, id = deletedId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Dạng bào chế này đã được sử dụng. Bạn không thể xóa" });
            }
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
