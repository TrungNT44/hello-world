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
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class NuocsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string _maNhaThuoc = UserService.GetMaNhaThuoc();
        // GET: Nuocs
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Read)]
        public async Task<ActionResult> Index()
        {
            return View(await db.Nuocs
                //.Where(x => x.MaNhaThuoc == this.GetNhaThuoc().MaNhaThuoc)
                .ToListAsync());
        }

        // GET: Nuocs/Details/5
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Read)]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nuoc nuoc = await db.Nuocs.FindAsync(id);
            if (nuoc == null)
            {
                return HttpNotFound();
            }
            //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
            //    throw new UnauthorizedAccessException();
            return View(nuoc);
        }

        // GET: Nuocs/Create
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Create)]
        public ActionResult Create()
        {
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            return View();
        }

        // POST: Nuocs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Create)]
        // [Audit]
        public async Task<ActionResult> Create([Bind(Include = "MaNuoc,TenNuoc,MaNhaThuoc")] Nuoc nuoc)
        {
            if (ModelState.IsValid)
            {
                //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
                //    throw new UnauthorizedAccessException();
                db.Nuocs.Add(nuoc);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(nuoc);
        }

        // GET: Nuocs/Edit/5
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Modify)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nuoc nuoc = await db.Nuocs.FindAsync(id);
            if (nuoc == null)
            {
                return HttpNotFound();
            }
            //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
            //    throw new UnauthorizedAccessException();
            ViewBag.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            return View(nuoc);
        }

        // POST: Nuocs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Modify)]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaNuoc,TenNuoc,MaNhaThuoc")] Nuoc nuoc)
        {
            if (ModelState.IsValid)
            {
                //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
                //    throw new UnauthorizedAccessException();
                db.Entry(nuoc).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(nuoc);
        }

        // GET: Nuocs/Delete/5
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Modify)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nuoc nuoc = await db.Nuocs.FindAsync(id);
            if (nuoc == null)
            {
                return HttpNotFound();
            }
            //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
            //    throw new UnauthorizedAccessException();
            return View(nuoc);
        }

        // POST: Nuocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize(FunctionResource.Nuoc, Operations.Modify)]
        // [Audit]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Nuoc nuoc = await db.Nuocs.FindAsync(id);
            //if (!User.IsInRole("Admin") && nuoc.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
            //    throw new UnauthorizedAccessException();
            db.Nuocs.Remove(nuoc);
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
