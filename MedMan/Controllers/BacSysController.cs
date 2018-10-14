
using System.Web.UI.WebControls;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Repositories;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using sThuoc.Models.ViewModels;
using Excel;
using MedMan.App_Start;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class BacSysController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();
        // GET: BacSys        
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentFilterTen = searchString;
            var qry = db.BacSys.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenBacSy.Contains(searchString));
            }
            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenBacSy);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenBacSy);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        // GET: BacSys/Details/5        
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            BacSy bacSy =
                await
                    unitOfWork.BacSyRespository.GetMany(e => e.MaBacSy == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                        .FirstAsync();
            if (bacSy == null)
            {
                return HttpNotFound();
            }
            return View(bacSy);
        }

        // GET: BacSys/Create        
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BacSys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Create([Bind(Include = "MaBacSy,TenBacSy,DiaChi,DienThoai,Email")] BacSy bacSy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var itemExist = unitOfWork.BacSyRespository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.TenBacSy.Trim().Equals(bacSy.TenBacSy.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenBacSy", "Tên bác sỹ này đã tồn tại. Vui lòng nhập tên bác sỹ khác.");
                    }
                    else
                    {
                        bacSy.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                        bacSy.Created = DateTime.Now;
                        bacSy.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.BacSyRespository.Insert(bacSy);
                        unitOfWork.Save();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenBacSy", e.Message);
                }
            }
            return View(bacSy);
        }

        // GET: BacSys/Edit/5        
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            BacSy bacSy =
                await
                    unitOfWork.BacSyRespository.GetMany(e => e.MaBacSy == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                        .FirstAsync();
            if (bacSy == null)
            {
                return HttpNotFound();
            }
            return View(bacSy);
        }

        // POST: BacSys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaBacSy,TenBacSy,DiaChi,DienThoai,Email")] BacSy bacSy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                    var itemExist = unitOfWork.BacSyRespository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.MaBacSy != bacSy.MaBacSy && c.TenBacSy.Trim().Equals(bacSy.TenBacSy.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenBacSy", "Tên bác sỹ này đã tồn tại. Vui lòng nhập tên bác sỹ khác.");
                    }
                    else
                    {
                        var bs = await unitOfWork.BacSyRespository.GetMany(e => e.MaBacSy == bacSy.MaBacSy && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstAsync();
                        bs.TenBacSy = bacSy.TenBacSy;
                        bs.DiaChi = bacSy.DiaChi;
                        bs.DienThoai = bacSy.DienThoai;
                        bs.Email = bacSy.Email;
                        bs.Modified = DateTime.Now;
                        bs.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.BacSyRespository.Update(bs);
                        unitOfWork.Save();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenBacSy", e.Message);
                }

            }

            return View(bacSy);
        }

        // GET: BacSys/Delete/5        
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            BacSy bacSy =
                await
                    unitOfWork.BacSyRespository.GetMany(e => e.MaBacSy == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                        .FirstAsync();
            if (bacSy == null)
            {
                return HttpNotFound();
            }
            return View(bacSy);
        }

        // POST: BacSys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Delete(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            BacSy bacSy =
                await
                    unitOfWork.BacSyRespository.GetMany(e => e.MaBacSy == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                        .FirstAsync();
            if (bacSy != null)
            {

                try
                {
                    unitOfWork.BacSyRespository.Delete(bacSy);
                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    ViewBag.Message = "Không thể xóa bác sỹ: " + bacSy.TenBacSy +
                                      "<br/> Nguyên nhân có thể là do bác sỹ đó đã được sử dụng";
                    ViewBag.FullMessage = e.Message;
                    return View("Error");

                }
            }
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [SimpleAuthorize("Admin")]
        //public ActionResult Upload(HttpPostedFileBase uploadFile)
        //{
        //    var strValidations = new StringBuilder(string.Empty);
        //    try
        //    {
        //        if (uploadFile.ContentLength > 0)
        //        {
        //            string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"),
        //                Path.GetFileName(uploadFile.FileName));

        //            uploadFile.SaveAs(filePath);
        //            var ds = new DataSet();

        //            //A 32-bit provider which enables the use of

        //            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath +
        //                                      ";Extended Properties=Excel 12.0;";
        //            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc; 
        //            using (var conn = new OleDbConnection(connectionString))
        //            {
        //                conn.Open();
        //                using (var dtExcelSchema = conn.GetSchema("Tables"))
        //                {
        //                    string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
        //                    string query = "SELECT * FROM [" + sheetName + "]";
        //                    var adapter = new OleDbDataAdapter(query, conn);
        //                    //DataSet ds = new DataSet();
        //                    adapter.Fill(ds, "Items");
        //                    if (ds.Tables.Count > 0)
        //                    {
        //                        if (ds.Tables[0].Rows.Count > 0)
        //                        {
        //                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                            {
        //                                var row = ds.Tables[0].Rows[i];
        //                                var tenbacsy = row[0].ToString().Trim();
        //                                var sodienthoai = row[2].ToString().Trim();
        //                                if (!String.IsNullOrWhiteSpace(tenbacsy))
        //                                {
        //                                    var bacsy = new BacSy();
        //                                    if (!String.IsNullOrWhiteSpace(sodienthoai))
        //                                    {
        //                                        bacsy = unitOfWork.BacSyRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenBacSy == tenbacsy && x.DienThoai == sodienthoai).FirstOrDefault();
        //                                    }
        //                                    else
        //                                    {
        //                                        bacsy =unitOfWork.BacSyRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenBacSy == tenbacsy).FirstOrDefault();
        //                                    }

        //                                    //Check if thuoc already exist
        //                                    if (bacsy != null)
        //                                    {
        //                                        Bindbacsy(ref bacsy, row);
        //                                    }
        //                                    else
        //                                    {
        //                                        bacsy = new BacSy()
        //                                        {
        //                                            MaBacSy = 0, 
        //                                            NhaThuoc =  unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
        //                                            Created = DateTime.Now,
        //                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
        //                                        };
        //                                        unitOfWork.BacSyRespository.Insert(Bindbacsy(ref bacsy, row)) ;
        //                                    }
        //                                }

        //                            }
        //                            unitOfWork.Save();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = ex.Message;
        //        return View("Error");
        //    }
        //    return RedirectToAction("Index");
        //}

        // [Audit]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            var strValidations = new StringBuilder(string.Empty);
            try
            {
                if (uploadFile.ContentLength > 0)
                {
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"),
                        Path.GetFileName(uploadFile.FileName));

                    uploadFile.SaveAs(filePath);
                    int totalupdated = 0;
                    int totaladded = 0;
                    int totalError = 0;
                    string message = "<b>Thông tin bác sỹ ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();
                    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

                    foreach (var worksheet in Workbook.Worksheets(filePath))
                    {
                        for (int i = 1; i < worksheet.Rows.Count(); i++)
                        {
                            var row = worksheet.Rows[i];
                            var msg = ValidateDataImport(row);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                if (msg == Constants.Params.msgOk)
                                {
                                    var tenbacsy = row.Cells[0].Text.Trim();
                                    var sodienthoai = row.Cells[2] != null ? row.Cells[2].Text.Trim() : string.Empty;

                                    if (!String.IsNullOrWhiteSpace(tenbacsy))
                                    {
                                        var bacsy = new BacSy();
                                        if (!String.IsNullOrWhiteSpace(sodienthoai))
                                        {
                                            bacsy = unitOfWork.BacSyRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenBacSy == tenbacsy && x.DienThoai == sodienthoai).FirstOrDefault();
                                        }
                                        else
                                        {
                                            bacsy = unitOfWork.BacSyRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenBacSy == tenbacsy).FirstOrDefault();
                                        }

                                        //Check if thuoc already exist
                                        if (bacsy != null)
                                        {
                                            Bindbacsy(ref bacsy, row);
                                            totalupdated++;
                                        }
                                        else
                                        {
                                            bacsy = new BacSy()
                                            {
                                                MaBacSy = 0,
                                                NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                                Created = DateTime.Now,
                                                CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
                                            };

                                            unitOfWork.BacSyRespository.Insert(Bindbacsy(ref bacsy, row));
                                            totaladded++;
                                        }
                                    }
                                }
                                else
                                {
                                    info.ErrorMsg.Add(string.Format(message, i, msg));
                                    totalError++;
                                }
                            }
                        }

                        unitOfWork.Save();

                        info.Title = "Thông tin upload bác sỹ";
                        info.TotalUpdated = totalupdated;
                        info.TotalAdded = totaladded;
                        info.TotalError = totalError;
                        Session["UploadMessage"] = info;

                        return RedirectToAction("index", "Upload");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "";
                ViewBag.FullMessage = ex.Message;
                return View("Error");
            }
            return RedirectToAction("Index");
        }

        private string ValidateDataImport(Excel.Row row)
        {
            bool flag = false;
            string msg = string.Empty;
            for (int i = 0; i < row.Cells.Count(); i++)
            {
                if (row.Cells[i] != null && !string.IsNullOrEmpty(row.Cells[i].Text))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                if (row.Cells[0] == null || string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                {
                    msg += "    - Tên bác sỹ không được bỏ trống <br/>";
                }

                return string.IsNullOrEmpty(msg) ? Constants.Params.msgOk : msg;
            }
            else
            {
                return string.Empty;
            }
        }


        private BacSy Bindbacsy(ref BacSy bacsy, Excel.Row row)
        {
            //
            //Bindding Cac thong tin
            //
            var tenbacsy = row.Cells[0] != null ? row.Cells[0].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(tenbacsy)) bacsy.TenBacSy = tenbacsy;
            //
            var sodienthoai = row.Cells[2] != null ? row.Cells[2].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(sodienthoai)) bacsy.DienThoai = sodienthoai;
            //
            var diachi = row.Cells[1] != null ? row.Cells[1].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(diachi)) bacsy.DiaChi = diachi;
            //     
            var email = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(email)) bacsy.Email = email;

            return bacsy;
        }
        [SimpleAuthorize("Admin")]
        public ActionResult ExportToExcel()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhacungcaps = new DataTable("Bác sỹ");
            nhacungcaps.Columns.Add("Tên bác sỹ", typeof(string));
            nhacungcaps.Columns.Add("Địa chỉ", typeof(string));
            nhacungcaps.Columns.Add("Số điện thoại", typeof(string));
            nhacungcaps.Columns.Add("Email", typeof(string));

            //query all nhacungcaps
            var query = unitOfWork.BacSyRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenBacSy).ToList().Select(i =>
                         new
                        {
                            i.TenBacSy,
                            i.DiaChi,
                            i.DienThoai,
                            i.Email,
                        });

            //Add to rows
            foreach (var item in query)
            {
                DataRow dr = nhacungcaps.NewRow();
                dr["Tên bác sỹ"] = item.TenBacSy;
                dr["Địa chỉ"] = item.DiaChi ?? "";
                dr["Số điện thoại"] = item.DienThoai ?? "";
                dr["Email"] = item.Email ?? "";
                nhacungcaps.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Bác sỹ");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(nhacungcaps, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:O1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 5, 2 + nhacungcaps.Rows.Count, 7])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                using (ExcelRange col = ws.Cells[2, 10, 2 + nhacungcaps.Rows.Count, 13])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                var fileDownloadName = "BacSy-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                pck.SaveAs(fileStream);
                fileStream.Position = 0;

                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;

                return fsr;
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
