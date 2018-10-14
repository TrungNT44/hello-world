
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
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
using sThuoc.Repositories;
using sThuoc.Models.ViewModels;
using Excel;
using sThuoc.Utils;
using App.Common.MVC;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class NhaCungCapsController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();

        // GET: NhaCungCaps
        [SimpleAuthorize("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "ten_desc" : "";
            ViewBag.NameSortParm2 = sortOrder == "tennhom" ? "tennhom_desc" : "tennhom";

            if (searchString != null)
            {
                if (page == null)
                {
                    page = 1;
                }
                ViewBag.CurrentSearchString = searchString;
            }           
            else
            {
                    if (page == null)
                    {
                        page = 1;
                    }
                    searchString = currentFilter;
            }
                
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ViewBag.CurrentFilter = searchString;
            var qry = db.NhaCungCaps.Where(x => x.MaNhaThuoc == maNhaThuoc).Include(k => k.NhaThuoc).Include(k => k.NhomNhaCungCap);

            if (!string.IsNullOrEmpty(searchString))
            {
                qry = qry.Where(x => x.TenNhaCungCap.Contains(searchString));
            }
            //sort the table 
            switch (sortOrder)
            {
                case "ten_desc":
                    qry = qry.OrderByDescending(s => s.TenNhaCungCap);
                    break;
                case "tennhom":
                    qry = qry.OrderBy(s => s.NhomNhaCungCap.TenNhomNhaCungCap);
                    break;
                case "tennhom_desc":
                    qry = qry.OrderByDescending(s => s.NhomNhaCungCap.TenNhomNhaCungCap);
                    break;
                default:
                    qry = qry.OrderBy(s => s.TenNhaCungCap);
                    break;
            }

            const int pageSize = 10;
            int pageNumber = (page ?? 1);
            var result = qry.ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        // GET: NhaCungCaps/Details/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            NhaCungCap nhaCungCap =
                unitOfWork.NhaCungCapRespository.GetMany(
                    e => e.MaNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).First();
            if (nhaCungCap == null)
            {
                return HttpNotFound();
            }
            return View(nhaCungCap);
        }

        // GET: NhaCungCaps/Create
        [SimpleAuthorize("Admin")]
        public ActionResult Create()
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhom = unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).ToList();
            ViewBag.MaNhomNhaCungCap = new SelectList(nhom, "MaNhomNhaCungCap", "TenNhomNhaCungCap");
            return View();
        }

        // POST: NhaCungCaps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public ActionResult Create([Bind(Include = "MaNhaCungCap,TenNhaCungCap,DiaChi,SoDienThoai,SoFax,MaSoThue,NguoiDaiDien,NguoiLienHe,Email,NoDauKy,MaNhaThuoc,MaNhomNhaCungCap")] NhaCungCap nhaCungCap)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (ModelState.IsValid)
            {
                try
                {
                    var itemExist = unitOfWork.NhaCungCapRespository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.TenNhaCungCap.Trim().Equals(nhaCungCap.TenNhaCungCap.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenNhaCungCap", "Tên nhà cung cấp này đã tồn tại. Vui lòng nhập tên nhà cung cấp khác.");
                    }
                    else
                    {
                        nhaCungCap.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc);
                        nhaCungCap.Created = DateTime.Now;
                        nhaCungCap.CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                        unitOfWork.NhaCungCapRespository.Insert(nhaCungCap);
                        unitOfWork.Save();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhaCungCap", e.Message);
                }

            }

            var nhom = unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).ToList();
            ViewBag.MaNhomNhaCungCap = new SelectList(nhom, "MaNhomNhaCungCap", "TenNhomNhaCungCap");
            return View(nhaCungCap);
        }

        // GET: NhaCungCaps/Edit/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var nhaCungCap = await
                unitOfWork.NhaCungCapRespository.GetMany(
                    e => e.MaNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstAsync();
            if (nhaCungCap == null)
            {
                return HttpNotFound();
            }

            var nhom = unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).ToList();
            ViewBag.MaNhomNhaCungCap = new SelectList(nhom, "MaNhomNhaCungCap", "TenNhomNhaCungCap", nhaCungCap.MaNhomNhaCungCap);
            return View(nhaCungCap);
        }

        // POST: NhaCungCaps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Edit([Bind(Include = "MaNhaCungCap,TenNhaCungCap,DiaChi,SoDienThoai,SoFax,MaSoThue,NguoiDaiDien,NguoiLienHe,Email,NoDauKy,MaNhaThuoc,MaNhomNhaCungCap")] NhaCungCap nhaCungCap)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            if (ModelState.IsValid)
            {
                try
                {
                    var itemExist = unitOfWork.NhaCungCapRespository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.MaNhaCungCap != nhaCungCap.MaNhaCungCap && c.TenNhaCungCap.Trim().Equals(nhaCungCap.TenNhaCungCap.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (itemExist != null)
                    {
                        ModelState.AddModelError("TenNhaCungCap", "Tên nhà cung cấp này đã tồn tại. Vui lòng nhập tên nhà cung cấp khác.");
                    }
                    else
                    {
                        var nhaCC = await unitOfWork.NhaCungCapRespository.GetMany(e => e.MaNhaCungCap == nhaCungCap.MaNhaCungCap && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstAsync();
                        if (Constants.Default.ConstantEntities.NhaCungCaps.Contains(nhaCC.TenNhaCungCap))
                        {
                            // khong cho phep sua ten khach hang mac dinh.
                            ModelState.AddModelError("TenKhachHang",
                                "Không thể sửa tên khách hàng mặc định: " + nhaCC.TenNhaCungCap);
                        }
                        else
                        {

                            nhaCC.TenNhaCungCap = nhaCungCap.TenNhaCungCap;
                            nhaCC.DiaChi = nhaCungCap.DiaChi;
                            nhaCC.SoDienThoai = nhaCungCap.SoDienThoai;
                            nhaCC.SoFax = nhaCungCap.SoFax;
                            nhaCC.MaSoThue = nhaCungCap.MaSoThue;
                            nhaCC.NguoiDaiDien = nhaCungCap.NguoiDaiDien;
                            nhaCC.NguoiLienHe = nhaCungCap.NguoiLienHe;
                            nhaCC.Email = nhaCungCap.Email;
                            nhaCC.NoDauKy = nhaCungCap.NoDauKy;
                            nhaCC.MaNhomNhaCungCap = nhaCungCap.MaNhomNhaCungCap;
                            nhaCC.NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc);
                            nhaCC.Modified = DateTime.Now;
                            nhaCC.ModifiedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                            unitOfWork.NhaCungCapRespository.Update(nhaCC);
                            unitOfWork.Save();
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("TenNhaCungCap", e.Message);
                }

            }
            var nhom = unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).ToList();
            ViewBag.MaNhomNhaCungCap = new SelectList(nhom, "MaNhomNhaCungCap", "TenNhomNhaCungCap");
            return View(nhaCungCap);
        }

        // GET: NhaCungCaps/Delete/5
        [SimpleAuthorize("Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            NhaCungCap nhaCungCap =
                await
                    unitOfWork.NhaCungCapRespository.GetMany(
                        e => e.MaNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstAsync();
            if (Constants.Default.ConstantEntities.NhaCungCaps.Contains(nhaCungCap.TenNhaCungCap))
            {
                // khong cho phep sua ten khach hang mac dinh.
                ViewBag.Message = "Không thể xóa khách hàng mặc định: " + nhaCungCap.TenNhaCungCap;
                return View("Error");
            }
            if (nhaCungCap == null)
            {
                return HttpNotFound();
            }
            return View(nhaCungCap);
        }

        // POST: NhaCungCaps/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SimpleAuthorize("Admin")]
        // [Audit]
        public async Task<ActionResult> Delete(int id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            NhaCungCap nhaCungCap =
                await
                    unitOfWork.NhaCungCapRespository.GetMany(
                        e => e.MaNhaCungCap == id && e.NhaThuoc.MaNhaThuoc == maNhaThuoc).FirstAsync();

            try
            {
                unitOfWork.NhaCungCapRespository.Delete(nhaCungCap);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                ViewBag.Message = "Không thể xóa nhà cung cấp: " + nhaCungCap.TenNhaCungCap +
                                  "<br/> Nguyên nhân có thể là do nhà cung cấp đã được sử dụng";
                ViewBag.FullMessage = e.Message;
                return View("Error");

            }
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [SimpleAuthorize("Admin")]
        //public ActionResult Upload(HttpPostedFileBase uploadFile)
        //{
        //    var strValidations = new StringBuilder(string.Empty);
        //    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
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
        //                                var tennhacungcap = row[1].ToString().Trim();
        //                                var sodienthoai = row[3].ToString().Trim();
        //                                if (!String.IsNullOrWhiteSpace(tennhacungcap))
        //                                {
        //                                    var nhacungcap = new NhaCungCap();
        //                                    if (!String.IsNullOrWhiteSpace(sodienthoai))
        //                                    {
        //                                        nhacungcap =unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap == tennhacungcap && x.SoDienThoai == sodienthoai).FirstOrDefault();
        //                                    }
        //                                    else
        //                                    {
        //                                        nhacungcap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap == tennhacungcap).FirstOrDefault();
        //                                    }

        //                                    //Check if thuoc already exist
        //                                    if (nhacungcap != null)
        //                                    {
        //                                        BindNhaCungCap(ref nhacungcap, row);
        //                                    }
        //                                    else
        //                                    {
        //                                        nhacungcap = new NhaCungCap()
        //                                        {
        //                                            MaNhaCungCap = 0,
        //                                            NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
        //                                            CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
        //                                            Created = DateTime.Now

        //                                        };
        //                                        unitOfWork.NhaCungCapRespository.Insert(BindNhaCungCap(ref nhacungcap, row));
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
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
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
                    string message = "<b>Thông tin nhà cung cấp ở dòng số {0} bị lỗi:</b><br/> {1}";
                    UploadObjectInfo info = new UploadObjectInfo();

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
                                    var tennhacungcap = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[1].Text.Trim());
                                    //var sodienthoai = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;
                                    if (!String.IsNullOrWhiteSpace(tennhacungcap))
                                    {
                                        var nhacungcap = unitOfWork.NhaCungCapRespository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.TenNhaCungCap.Equals(tennhacungcap, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                        //Check if thuoc already exist
                                        if (nhacungcap != null)
                                        {
                                            BindNhaCungCap(ref nhacungcap, row);
                                            totalupdated++;
                                        }
                                        else
                                        {
                                            nhacungcap = new NhaCungCap()
                                            {
                                                MaNhaCungCap = 0,
                                                NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                                                CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                                                Created = DateTime.Now

                                            };

                                            unitOfWork.NhaCungCapRespository.Insert(BindNhaCungCap(ref nhacungcap, row));
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

                        info.Title = "Thông tin upload nhà cung cấp";
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
                    msg += "    - Nhóm nhà cung cấp không được bỏ trống <br/>";
                }

                if (row.Cells[1] == null || string.IsNullOrEmpty(row.Cells[1].Text.Trim()))
                {
                    msg += "    - Tên nhà cung cấp không được bỏ trống <br/>";
                }

                if (row.Cells[4] != null && !string.IsNullOrEmpty(row.Cells[4].Text.Trim()) && !decimal.TryParse(row.Cells[4].Value.Trim(), out tmp))
                {
                    msg += "    - Số dư đầu kỳ phải là số <br/>";
                }

                return string.IsNullOrEmpty(msg) ? Constants.Params.msgOk : msg;
            }
            else
            {
                return string.Empty;
            }
        }


        private NhaCungCap BindNhaCungCap(ref NhaCungCap nhacungcap, Excel.Row row)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //Nhom nha cung cap
            var tennhomnhacungcap = sThuoc.Utils.Helpers.RemoveEncoding(row.Cells[0].Text.Trim());
            var nhomnhacungcap = unitOfWork.NhomNhaCungCapRepository.GetMany(x => x.MaNhaThuoc == maNhaThuoc && x.TenNhomNhaCungCap == tennhomnhacungcap).FirstOrDefault();
            if (nhomnhacungcap == null && !String.IsNullOrEmpty(tennhomnhacungcap))
            {
                nhomnhacungcap = new NhomNhaCungCap
                {
                    TenNhomNhaCungCap = tennhomnhacungcap,
                    NhaThuoc = unitOfWork.NhaThuocRepository.GetById(maNhaThuoc),
                    MaNhomNhaCungCap = 0,
                    GhiChu = "",
                    CreatedBy = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId),
                    Created = DateTime.Now
                };

                unitOfWork.NhomNhaCungCapRepository.Insert(nhomnhacungcap);
                unitOfWork.Save();
            }
            if (nhomnhacungcap != null) nhacungcap.NhomNhaCungCap = nhomnhacungcap;
            //
            //Bindding Cac thong tin
            //
            var tennhacungcap = row.Cells[1].Text.Trim();
            if (!String.IsNullOrWhiteSpace(tennhacungcap)) nhacungcap.TenNhaCungCap = tennhacungcap;
            //
            var sodienthoai = row.Cells[3] != null ? row.Cells[3].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(sodienthoai)) nhacungcap.SoDienThoai = sodienthoai;
            //
            var diachi = row.Cells[2] != null ? row.Cells[2].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(diachi)) nhacungcap.DiaChi = diachi;
            //
            var nodauky = (row.Cells[4] != null && !string.IsNullOrEmpty(row.Cells[4].Text.Trim())) ? Convert.ToDecimal(row.Cells[4].Value.Trim()) : 0;
            nhacungcap.NoDauKy = nodauky;
            //
            var sofax = row.Cells[5] != null ? row.Cells[5].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(sofax)) nhacungcap.SoFax = sofax;
            //
            var masothue = row.Cells[6] != null ? row.Cells[6].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(masothue)) nhacungcap.MaSoThue = masothue;
            //
            var nguoidaidien = row.Cells[7] != null ? row.Cells[7].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(nguoidaidien)) nhacungcap.NguoiDaiDien = nguoidaidien;
            //
            var nguoilienhe = row.Cells[8] != null ? row.Cells[8].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(nguoilienhe)) nhacungcap.NguoiLienHe = nguoilienhe;
            //
            var email = row.Cells[9] != null ? row.Cells[9].Text.Trim() : string.Empty;
            if (!String.IsNullOrWhiteSpace(email)) nhacungcap.Email = email;

            return nhacungcap;
        }
        [SimpleAuthorize("Admin")]
        public ActionResult ExportToExcel()
        {
            var nhacungcaps = new DataTable("Nhà cung cấp");
            nhacungcaps.Columns.Add("Nhóm nhà cung cấp", typeof(string));
            nhacungcaps.Columns.Add("Tên nhà cung cấp", typeof(string));
            nhacungcaps.Columns.Add("Địa chỉ", typeof(string));
            nhacungcaps.Columns.Add("Số điện thoại", typeof(string));
            nhacungcaps.Columns.Add("Nợ đầu kỳ", typeof(int));
            nhacungcaps.Columns.Add("Số fax", typeof(string));
            nhacungcaps.Columns.Add("Mã số thuế", typeof(string));
            nhacungcaps.Columns.Add("Người đại diện", typeof(string));
            nhacungcaps.Columns.Add("Người liên hệ", typeof(string));
            nhacungcaps.Columns.Add("Email", typeof(string));
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            //query all nhacungcaps
            var query = unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc).OrderBy(e => e.TenNhaCungCap).Select(i =>
                         new
                        {
                            i.NhomNhaCungCap.TenNhomNhaCungCap,
                            i.TenNhaCungCap,
                            i.DiaChi,
                            i.SoDienThoai,
                            i.NoDauKy,
                            i.SoFax,
                            i.NguoiDaiDien,
                            i.NguoiLienHe,
                            i.Email,
                        });
            //Add to rows
            foreach (var item in query)
            {
                DataRow dr = nhacungcaps.NewRow();
                dr["Nhóm nhà cung cấp"] = item.TenNhomNhaCungCap;
                dr["Tên nhà cung cấp"] = item.TenNhaCungCap;
                dr["Địa chỉ"] = item.DiaChi ?? "";
                dr["Số điện thoại"] = item.SoDienThoai ?? "";
                dr["Nợ đầu kỳ"] = item.NoDauKy ?? 0;
                dr["Số fax"] = item.SoFax ?? "";
                dr["Người đại diện"] = item.NguoiDaiDien ?? "";
                dr["Người liên hệ"] = item.NguoiLienHe ?? "";
                dr["Email"] = item.Email ?? "";
                nhacungcaps.Rows.Add(dr);
            }

            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Nhà cung cấp");

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

                var fileDownloadName = "NhaCungCap-" + DateTime.Now + ".xlsx";

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
