using System.Web.UI.WebControls;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using MedMan.App_Start;
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
using sThuoc.Utils;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class KhachHangsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        //private readonly string this.GetNhaThuoc().MaNhaThuoc = UserService.GetMaNhaThuoc();
        private readonly UnitOfWork unitOfWork = new UnitOfWork();

        // GET: KhachHangs
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            ViewBag.NameSortParm2 = sortOrder == "tennhom" ? "tennhom_desc" : "tennhom";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilterTen = searchString;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var qry = db.KhachHangs.Where(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && !x.TenKhachHang.Equals("Điều chỉnh sau kiểm kê", StringComparison.OrdinalIgnoreCase)).Include(k => k.NhaThuoc).Include(k => k.NhomKhachHang);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenKhachHang.Contains(searchString));
            }
            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenKhachHang);
                    break;
                case "tennhom":
                    qry = qry.OrderBy(s => s.NhomKhachHang.TenNhomKhachHang);
                    break;
                case "tennhom_desc":
                    qry = qry.OrderByDescending(s => s.NhomKhachHang.TenNhomKhachHang);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenKhachHang);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        // GET: KhachHangs/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = await db.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Admin") && khachHang.MaNhaThuoc != this.GetNhaThuoc().MaNhaThuoc)
                throw new UnauthorizedAccessException();
            return View(khachHang);
        }

        // GET: KhachHangs/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhom = unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenNhomKhachHang).ToList();
            ViewBag.MaNhomKhachHang = new SelectList(nhom, "MaNhomKhachHang", "TenNhomKhachHang");
            return View();
        }

        // POST: KhachHangs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaKhachHang,TenKhachHang,DiaChi,SoDienThoai,NoDauKy,DonViCongTac,Email,GhiChu,MaNhaThuoc,MaNhomKhachHang")] KhachHang khachHang)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            if (ModelState.IsValid)
            {
                try
                {
                    var itemExist = unitOfWork.KhachHangRepository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.TenKhachHang.Trim().Equals(khachHang.TenKhachHang.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenKhachHang", "Tên khách hàng này đã tồn tại. Vui lòng nhập tên khách hàng khác.");
                    }
                    else
                    {
                        khachHang.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc);
                        khachHang.Created = DateTime.Now;
                        khachHang.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.KhachHangRepository.Insert(khachHang);
                        unitOfWork.Save();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {

                    ModelState.AddModelError("TenKhachHang", e.Message);
                }

            }

            var nhom = unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenNhomKhachHang).ToList();
            ViewBag.MaNhomKhachHang = new SelectList(nhom, "MaNhomKhachHang", "TenNhomKhachHang");
            return View(khachHang);
        }

        // GET: KhachHangs/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            KhachHang khachHang = unitOfWork.KhachHangRepository.GetMany(e => e.MaKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            var nhom = unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenNhomKhachHang).ToList();
            ViewBag.MaNhomKhachHang = new SelectList(nhom, "MaNhomKhachHang", "TenNhomKhachHang", khachHang.MaNhomKhachHang);
            return View(khachHang);
        }

        // POST: KhachHangs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaKhachHang,TenKhachHang,DiaChi,SoDienThoai,NoDauKy,DonViCongTac,Email,GhiChu,MaNhaThuoc,MaNhomKhachHang")] KhachHang khachHang)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (ModelState.IsValid)
            {
                try
                {
                    var itemExist = unitOfWork.KhachHangRepository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.MaKhachHang != khachHang.MaKhachHang && c.TenKhachHang.Trim().Equals(khachHang.TenKhachHang.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenKhachHang", "Tên khách hàng này đã tồn tại. Vui lòng nhập tên khách hàng khác.");
                    }
                    else
                    {
                        var kh = unitOfWork.KhachHangRepository.GetMany(e => e.MaKhachHang == khachHang.MaKhachHang && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
                        if (Constants.Default.ConstantEntities.KhachHangs.Contains(kh.TenKhachHang))
                        {
                            // khong cho phep sua ten khach hang mac dinh.
                            ModelState.AddModelError("TenKhachHang", "Không thể sửa tên khách hàng mặc định: " + kh.TenKhachHang);
                        }
                        if (kh != null && ModelState.IsValid)
                        {
                            kh.TenKhachHang = khachHang.TenKhachHang;
                            kh.DiaChi = khachHang.DiaChi;
                            kh.SoDienThoai = khachHang.SoDienThoai;
                            kh.NoDauKy = khachHang.NoDauKy;
                            kh.DonViCongTac = khachHang.DonViCongTac;
                            kh.Email = khachHang.Email;
                            kh.GhiChu = khachHang.GhiChu;
                            kh.MaNhomKhachHang = khachHang.MaNhomKhachHang;
                            kh.Modified = DateTime.Now;
                            kh.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                            unitOfWork.KhachHangRepository.Update(kh);
                            unitOfWork.Save();
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenKhachHang", e.Message);
                }

            }
            var nhom = unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenNhomKhachHang).ToList();
            ViewBag.MaNhomKhachHang = new SelectList(nhom, "MaNhomKhachHang", "TenNhomKhachHang");
            return View(khachHang);
        }

        // GET: KhachHangs/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            KhachHang khachHang =
                unitOfWork.KhachHangRepository.GetMany(e => e.MaKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .First();
            if (khachHang == null)
            {
                return HttpNotFound();
            }

            return View(khachHang);
        }

        // POST: KhachHangs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Delete(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            KhachHang khachHang =
                unitOfWork.KhachHangRepository.GetMany(e => e.MaKhachHang == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .First();
            if (Constants.Default.ConstantEntities.KhachHangs.Contains(khachHang.TenKhachHang))
            {
                // khong cho phep sua ten khach hang mac dinh.
                ViewBag.Message = "Không thể xóa khách hàng mặc định: " + khachHang.TenKhachHang;
                return View("Error");
            }
            if (khachHang != null)
            {

                try
                {
                    unitOfWork.KhachHangRepository.Delete(khachHang);
                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    ViewBag.Message = "Không thể xóa  khách hàng: " + khachHang.TenKhachHang +
                                      "<br/> Nguyên nhân có thể là do khách hàng đã được sử dụng";
                    return View("Error");

                }
            }

            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [SimpleAuthorize("Admin")]
        //public ActionResult Upload2(HttpPostedFileBase uploadFile)
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
        //                                var TenKhachHang = row[1].ToString().Trim();
        //                                var SoDienThoai = row[3].ToString().Trim();
        //                                if (!String.IsNullOrWhiteSpace(TenKhachHang))
        //                                {
        //                                    var KhachHang = new KhachHang();
        //                                    if (!String.IsNullOrWhiteSpace(SoDienThoai))
        //                                    {
        //                                        KhachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == TenKhachHang.ToUpper() && x.SoDienThoai.ToUpper() == SoDienThoai.ToUpper()).FirstOrDefault();
        //                                    }
        //                                    else
        //                                    {
        //                                        KhachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == TenKhachHang.ToUpper()).FirstOrDefault();
        //                                    }

        //                                    //Check if thuoc already exist
        //                                    if (KhachHang != null)
        //                                    {
        //                                        BindKhachHang(ref KhachHang, row);
        //                                    }
        //                                    else
        //                                    {
        //                                        KhachHang = new KhachHang()
        //                                        {
        //                                            MaKhachHang = 0,
        //                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
        //                                            Created = DateTime.Now,
        //                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
        //                                        };
        //                                        unitOfWork.KhachHangRepository.Insert(BindKhachHang(ref KhachHang, row));
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
                    string message = "<b>Thông tin khách hàng ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();
                    //A 32-bit provider which enables the use of

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
                                    var TenKhachHang = row.Cells[1].Text.Trim();
                                    //var SoDienThoai = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;

                                    var KhachHang = unitOfWork.KhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenKhachHang.ToUpper() == TenKhachHang.ToUpper()).FirstOrDefault();

                                    //Check if thuoc already exist
                                    if (KhachHang != null)
                                    {
                                        BindKhachHang(ref KhachHang, row);
                                        totalupdated++;
                                    }
                                    else
                                    {
                                        KhachHang = new KhachHang()
                                        {
                                            MaKhachHang = 0,
                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                            Created = DateTime.Now,
                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
                                        };
                                        unitOfWork.KhachHangRepository.Insert(BindKhachHang(ref KhachHang, row));

                                        totaladded++;
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
                        info.Title = "Thông tin upload khách hàng";
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
                ViewBag.Message = ex.Message;
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
                decimal tmp = 0;
                if (row.Cells[0] == null || string.IsNullOrEmpty(row.Cells[0].Text.Trim()))
                {
                    msg += "    - Nhóm khách hàng không được bỏ trống <br/>";
                }

                if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                {
                    msg += "    - Tên khách hàng không được bỏ trống <br/>";
                }

                if (row.Cells[4] != null && !decimal.TryParse(row.Cells[4].Text.Trim(), out tmp))
                {
                    msg += "    - Nợ đầu kỳ phải là số <br/>";
                }

                return string.IsNullOrEmpty(msg) ? Constants.Params.msgOk : msg;
            }
            else
            {
                return string.Empty;
            }
        }

        private KhachHang BindKhachHang(ref KhachHang khachhang, Excel.Row row)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //Nhom khach hang
            var tenNhomKhacHang = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[0].Text.Trim());
            var nhomKhachHang = unitOfWork.NhomKhachHangRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhomKhachHang.ToUpper() == tenNhomKhacHang.ToUpper()).FirstOrDefault();
            if (nhomKhachHang == null && !String.IsNullOrEmpty(tenNhomKhacHang))
            {
                nhomKhachHang = new NhomKhachHang
                {
                    TenNhomKhachHang = tenNhomKhacHang,
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc),
                    MaNhomKhachHang = 0,
                    GhiChu = "",
                    Created = DateTime.Now,
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId)
                };

                unitOfWork.NhomKhachHangRepository.Insert(nhomKhachHang);
                unitOfWork.Save();
            }
            if (nhomKhachHang != null) khachhang.NhomKhachHang = nhomKhachHang;
            //
            //Cac thong tin con lai            
            //
            var TenKhachHang = row.Cells[1].Text.Trim();
            if (!String.IsNullOrWhiteSpace(TenKhachHang)) khachhang.TenKhachHang = TenKhachHang;
            //
            var SoDienThoai = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(SoDienThoai)) khachhang.SoDienThoai = SoDienThoai;
            //
            var DiaChi = row.Cells[2] != null ? row.Cells[2].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(DiaChi)) khachhang.DiaChi = DiaChi;
            //
            var NodauKy = row.Cells[4] != null ? Convert.ToDecimal(row.Cells[4].Text.Trim()) : 0;
            khachhang.NoDauKy = NodauKy;
            //
            var DonViCongTac = row.Cells[5] != null ? row.Cells[5].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(DonViCongTac)) khachhang.DonViCongTac = DonViCongTac;
            //
            var Email = row.Cells[6] != null ? row.Cells[6].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(Email)) khachhang.Email = Email;
            //
            var GhiChu = row.Cells[7] != null ? row.Cells[7].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(GhiChu)) khachhang.GhiChu = GhiChu;

            return khachhang;
        }
        [SimpleAuthorize("Admin")]
        public ActionResult ExportToExcel()
        {
            var khachhangs = new DataTable("Khách hàng");
            khachhangs.Columns.Add("Nhóm khách hàng", typeof(string));
            khachhangs.Columns.Add("Tên khách hàng", typeof(string));
            khachhangs.Columns.Add("Địa chỉ", typeof(string));
            khachhangs.Columns.Add("Số điện thoại", typeof(string));
            khachhangs.Columns.Add("Nợ đầu kỳ", typeof(int));
            khachhangs.Columns.Add("Đơn vị công tác", typeof(string));
            khachhangs.Columns.Add("Email", typeof(string));
            khachhangs.Columns.Add("Ghi chú", typeof(string));
            //query all khachhangs
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var query = unitOfWork.KhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenKhachHang).ToList().Select(i =>
                        new
                        {
                            i.NhomKhachHang.TenNhomKhachHang,
                            i.TenKhachHang,
                            i.DiaChi,
                            i.SoDienThoai,
                            i.NoDauKy,
                            i.DonViCongTac,
                            i.Email,
                            i.GhiChu,
                        });
            //Add to rows
            foreach (var item in query)
            {
                DataRow dr = khachhangs.NewRow();
                dr["Nhóm khách hàng"] = item.TenNhomKhachHang;
                dr["Tên khách hàng"] = item.TenKhachHang;
                dr["Địa chỉ"] = item.DiaChi ?? "";
                dr["Số điện thoại"] = item.SoDienThoai ?? "";
                dr["Nợ đầu kỳ"] = item.NoDauKy ?? 0;
                dr["Đơn vị công tác"] = item.DonViCongTac ?? "";
                dr["Email"] = item.Email ?? "";
                dr["Ghi chú"] = item.GhiChu ?? "";
                khachhangs.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Khách hàng");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(khachhangs, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:O1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 5, 2 + khachhangs.Rows.Count, 7])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                using (ExcelRange col = ws.Cells[2, 10, 2 + khachhangs.Rows.Count, 13])
                {
                    col.Style.Numberformat.Format = "0.##0";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                var fileDownloadName = "KhachHang-" + DateTime.Now + ".xlsx";

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
