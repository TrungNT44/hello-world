using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using System;
using sThuoc.Repositories;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class DonViTinhsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string _maNhaThuoc = UserService.GetMaNhaThuoc();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();
        // GET: DonViTinhs
        
        public ActionResult Index()
        {
            var nhathuoc = this.GetNhaThuoc();
            var maNhaThuoc = nhathuoc.MaNhaThuoc;
            var maNhaThuocCha = nhathuoc.MaNhaThuocCha;
            return View( db.DonViTinhs.Where(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc || e.NhaThuoc.MaNhaThuoc == maNhaThuocCha).ToList());
        }

        // GET: DonViTinhs/Details/5
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DonViTinh donViTinh =  unitOfWork.DonViTinhRepository.GetMany(e=> e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDonViTinh==id).First();
            if (donViTinh == null)
            {
                return HttpNotFound();
            }
            return View(donViTinh);
        }

        // GET: DonViTinhs/Create
        
        public ActionResult Create()
        {
            return View();
        }

        // POST: DonViTinhs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaDonViTinh,TenDonViTinh")] DonViTinh donViTinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(string.IsNullOrEmpty(donViTinh.TenDonViTinh))
                        ModelState.AddModelError("TenDonViTinh","Đơn vị tính không thể bỏ trống");
                    if (ModelState.IsValid)
                    {
                        // kiem tra da ton tai don vi tinh hay chua
                        var maNhaThuoc = this.GetNhaThuoc().MaNhaThuocCha;
                        var dvt =
                            unitOfWork.DonViTinhRepository.GetMany(
                                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.TenDonViTinh == donViTinh.TenDonViTinh)
                                .FirstOrDefault();
                        if(dvt!=null)
                            ModelState.AddModelError("TenDonViTinh","Đơn vị tính đã tồn tại");
                        if (ModelState.IsValid) { 
                            donViTinh.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                            unitOfWork.DonViTinhRepository.Insert(donViTinh);
                            unitOfWork.Save();
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenDonViTinh",e.Message);
                }
            }
            if (donViTinh.MaDonViTinh > 0)
                return Json(new { success = true, id = donViTinh.MaDonViTinh, title = donViTinh.TenDonViTinh });

            return Json(new { success = false, message = ModelState.GetFirstErrorMessage() });
        }

        // GET: DonViTinhs/Edit/5
        
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DonViTinh donViTinh =
                await
                    unitOfWork.DonViTinhRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDonViTinh == id).FirstAsync();
            if (donViTinh == null)
            {
                return HttpNotFound();
            }

            return View(donViTinh);
        }

        // POST: DonViTinhs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaDonViTinh,TenDonViTinh")] DonViTinh donViTinh)
        {
            if (ModelState.IsValid)
            {
                if(string.IsNullOrEmpty(donViTinh.TenDonViTinh))
                        ModelState.AddModelError("TenDonViTinh","Đơn vị tính không thể bỏ trống");
                if (ModelState.IsValid)
                {
                    // kiem tra da ton tai don vi tinh hay chua
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var dvt =
                        unitOfWork.DonViTinhRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.TenDonViTinh == donViTinh.TenDonViTinh&& e.MaDonViTinh!=donViTinh.MaDonViTinh)
                            .FirstOrDefault();
                    if (dvt != null)
                        ModelState.AddModelError("TenDonViTinh", "Đơn vị tính đã tồn tại");
                    if (ModelState.IsValid)
                    {
                        dvt = unitOfWork.DonViTinhRepository.GetMany(e=> e.NhaThuoc.MaNhaThuoc ==maNhaThuoc&& e.MaDonViTinh== donViTinh.MaDonViTinh).FirstOrDefault();
                        if (dvt != null)
                        {
                            dvt.TenDonViTinh = donViTinh.TenDonViTinh;
                            unitOfWork.DonViTinhRepository.Update(dvt);
                            unitOfWork.Save();
                            return Json(new { success = true, id = dvt.MaDonViTinh, title = dvt.TenDonViTinh });
                        }
                        

                    }
                }

            }
            return Json(new { success = false, message = ModelState.GetFirstErrorMessage() });
        }

        // GET: DonViTinhs/Delete/5
        
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            DonViTinh donViTinh =
                await
                    unitOfWork.DonViTinhRepository.GetMany(
                        e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDonViTinh == id).FirstAsync();
            if (donViTinh == null)
            {
                return HttpNotFound();
            }
            return View(donViTinh);
        }

        // POST: DonViTinhs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // [Audit]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                DonViTinh donViTinh =
                    await
                        unitOfWork.DonViTinhRepository.GetMany(
                            e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.MaDonViTinh == id).FirstAsync();

                var deletedId = donViTinh.MaDonViTinh;
                unitOfWork.DonViTinhRepository.Delete(donViTinh);
                unitOfWork.Save();
                return Json(new { success = true, id = deletedId });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Đơn vị này đã được sử dụng. Bạn không thể xóa" });
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
