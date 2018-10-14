using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using sThuoc.DAL;
using sThuoc.Models;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class LoaiXuatNhapsController : BaseController
    {
        private SecurityContext db = new SecurityContext();

        // GET: LoaiXuatNhaps
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Index()
        {
            return View(await db.LoaiXuatNhaps.ToListAsync());
        }

        // GET: LoaiXuatNhaps/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiXuatNhap loaiXuatNhap = await db.LoaiXuatNhaps.FindAsync(id);
            if (loaiXuatNhap == null)
            {
                return HttpNotFound();
            }
            return View(loaiXuatNhap);
        }

        // GET: LoaiXuatNhaps/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoaiXuatNhaps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // [Audit]
        public async Task<ActionResult> Create([Bind(Include = "MaLoaiXuatNhap,TenLoaiXuatNhap")] LoaiXuatNhap loaiXuatNhap)
        {
            if (ModelState.IsValid)
            {
                db.LoaiXuatNhaps.Add(loaiXuatNhap);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(loaiXuatNhap);
        }

        // GET: LoaiXuatNhaps/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiXuatNhap loaiXuatNhap = await db.LoaiXuatNhaps.FindAsync(id);
            if (loaiXuatNhap == null)
            {
                return HttpNotFound();
            }
            return View(loaiXuatNhap);
        }

        // POST: LoaiXuatNhaps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaLoaiXuatNhap,TenLoaiXuatNhap")] LoaiXuatNhap loaiXuatNhap)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loaiXuatNhap).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(loaiXuatNhap);
        }

        // GET: LoaiXuatNhaps/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiXuatNhap loaiXuatNhap = await db.LoaiXuatNhaps.FindAsync(id);
            if (loaiXuatNhap == null)
            {
                return HttpNotFound();
            }
            return View(loaiXuatNhap);
        }

        // POST: LoaiXuatNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // [Audit]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LoaiXuatNhap loaiXuatNhap = await db.LoaiXuatNhaps.FindAsync(id);
            db.LoaiXuatNhaps.Remove(loaiXuatNhap);
            await db.SaveChangesAsync();
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
