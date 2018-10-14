using Med.Common.Enums;
using OfficeOpenXml;
using MedMan.App_Start;
using sThuoc.DAL;
using Med.Web.Extensions;
using sThuoc.Filter;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using sThuoc.Repositories;
using sThuoc.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using App.Common.MVC;
using Med.Common.Enums;
using App.Constants.Enums;

namespace Med.Web.Controllers
{
    public class BaoCaoController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();
        [SimpleAuthorize("Admin")]
        public ActionResult Index(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            //
            //Tinh Ton kho
            //var theokhohang = new TheoKhoHangViewModel() ;
            //theokhohang.Type = "all";            
            // processing query.
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc 
                && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated && c.PhieuNhap.LoaiXuatNhap != null && c.PhieuNhap.LoaiXuatNhap.MaLoaiXuatNhap != (int)NoteInOutType.InitialInventory);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc 
                && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated && e.PhieuXuat.MaLoaiXuatNhap != (int)NoteInOutType.InitialInventory);

            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            var thuocQuery = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);

            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            DateTime? fromDate = null;
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                fromDate = form.From.Value.Date;
                phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(e => e.PhieuNhap.NgayNhap < toDate);
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
            // Loc tat cac phieu khach hang tra & tra nha cung cap           
            var listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);
            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);
            // order items
            thuocQuery = thuocQuery.OrderBy(e => e.TenThuoc);
            //
            decimal TongNhap = 0, TongXuat = 0, TongKho = 0;

            var theokhohangitemview = new TheoKhoHangItemViewModel();
            foreach (var thuoc in thuocQuery)
            {
                theokhohangitemview = sThuoc.Utils.Helpers.CalculateKho(thuoc, listPhieuNhapChiTiet, listPhieuXuatChiTiet, fromDate);
                TongNhap = TongNhap + theokhohangitemview.TongGiaTriNhap;
                TongXuat =TongXuat + theokhohangitemview.TongGiaTriXuat;
                TongKho = TongKho + theokhohangitemview.TongGiaTriTonCuoi;
            }
           
            // Tong no khach hang
            ViewBag.TongNoKhachHang = GetTongNoKhachHang(form);

            // Tong no nha cung cap
            ViewBag.TongNoNhaCungCap = GetTongNoNhaCungCap(form);

            // Tong thu = tong ban hang + tong thu khac
            // Tong ban hang
            ViewBag.TongBanHang = GetTongBanHang(form);
            // tong thu khac
            ViewBag.TongThuKhac = GetTongThuKhac(form);
            ViewBag.TongThu = ViewBag.TongBanHang + ViewBag.TongThuKhac;

            // Tong chi = tong mua hang hoa + tong chi phi kinh doanh + tong cac loai chi khac
            // Tong mua hang hoa
            ViewBag.TongMuaHangHoa = GetTongMuaHang(form);
            // Tong cac loai chi khac
            ViewBag.TongChiKhac = GetTongChiKhac(form);
            // Tong chi phi kinh doanh
            ViewBag.TongChiPhiKinhDoanh = GetTongChiPhiKinhDoanh(form);
            ViewBag.TongChi = ViewBag.TongMuaHangHoa + ViewBag.TongChiKhac + ViewBag.TongChiPhiKinhDoanh;

            // Loi nhuan gop

            ViewBag.LoiNhuanGop = GetLoiNhuanGop(form) + ViewBag.TongThuKhac;

            // Loi nhuan rong
            ViewBag.LoiNhuanRong = ViewBag.LoiNhuanGop - ViewBag.TongChiPhiKinhDoanh - ViewBag.TongChiKhac;
            // Quy tien mat = Tong Thu - Tong Chi
            ViewBag.QuyTienMat = ViewBag.TongThu - ViewBag.TongChi;

            // Tong Nhap            
            ViewBag.TongNhap = TongNhap;
            
            // Tong Xuat            
            ViewBag.TongXuat = TongXuat;

            // Tong Kho
            ViewBag.TongKho = TongKho;
            
            // Tien Von
            var query3 = unitOfWork.PhieuXuatChiTietRepository
                .GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated && e.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.Default.ConstantEntities.LoaiXuatNhapKiemKe);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                query3 = query3.Where(e => e.PhieuXuat.NgayXuat >= form.From && e.PhieuXuat.NgayXuat < toDate);
            }

            ViewBag.TienVon = 0;//query3.AsEnumerable().Sum(s => unitOfWork.PhieuXuatChiTietRepository.GetTienVon(s));


            return View(form);
        }

        //public ActionResult Index(BaoCaoTongHopViewModel form)
        //{
        //    var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

        //    var tongNhapQuery = unitOfWork.PhieuNhapRepository
        //        .GetMany(e =>
        //            e.NhaThuoc.MaNhaThuoc == maNhaThuoc
        //            && !e.Xoa && e.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang);

        //    var listPhieXuatQuery = unitOfWork.PhieuXuatChiTietRepository
        //        .GetMany(e =>
        //            e.NhaThuoc.MaNhaThuoc == maNhaThuoc
        //            && !e.PhieuXuat.Xoa
        //            && e.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap);

        //    var tongTraNhaCungCapQuery = unitOfWork.PhieuXuatRepository
        //        .GetMany(e =>
        //            e.NhaThuoc.MaNhaThuoc == maNhaThuoc
        //            && !e.Xoa
        //            && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap);

        //    var tongKhachHangTraQuery = unitOfWork.PhieuNhapRepository
        //        .GetMany(e =>
        //            e.NhaThuoc.MaNhaThuoc == maNhaThuoc
        //            && !e.Xoa
        //            && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang);

        //    //var listPhieuNhapQuery = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && !c.PhieuNhap.Xoa && c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang);

        //    List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
        //    List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
        //    var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && !c.PhieuNhap.Xoa);
        //    var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.PhieuXuat.Xoa);

        //    var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
        //    var thuocQuery = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);

        //    foreach (var item in listDuDauKy)
        //    {
        //        listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
        //        {
        //            Thuoc = new Thuoc() { ThuocId = item.ThuocId },
        //            PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
        //            GiaNhap = item.GiaDauKy,
        //            DonViTinh = item.DonViXuatLe,
        //            SoLuong = item.SoDuDauKy,
        //            ChietKhau = 0
        //        });
        //    }

        //    DateTime? fromDate = null;

        //    if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
        //    {
        //        var toDate = form.To.Value.AddDays(1);
        //        fromDate = form.From.Value.Date;
        //        tongNhapQuery = tongNhapQuery.Where(e => e.NgayNhap >= form.From && e.NgayNhap < toDate);
        //        listPhieXuatQuery = listPhieXuatQuery.Where(e => e.PhieuXuat.NgayXuat >= form.From && e.PhieuXuat.NgayXuat < toDate);
        //        tongTraNhaCungCapQuery = tongTraNhaCungCapQuery.Where(e => e.NgayXuat >= form.From && e.NgayXuat < toDate);
        //        tongKhachHangTraQuery = tongKhachHangTraQuery.Where(e => e.NgayNhap >= form.From && e.NgayNhap < toDate);

        //        phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(e => e.PhieuNhap.NgayNhap < toDate);
        //        phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);

        //        ViewBag.TongSoDuDauKy = GetSoDuDauKy(form.From.Value);
        //    }
        //    else
        //    {
        //        // tong so du dau ky kho hang
        //        //var listDuDauKy = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);
        //        ViewBag.TongSoDuDauKy = listDuDauKy.Count() > 0 ? listDuDauKy.Sum(e => e.SoDuDauKy * e.GiaDauKy) : 0;
        //    }

        //    // Tong no khach hang
        //    ViewBag.TongNoKhachHang = GetTongNoKhachHang(form);

        //    // Tong no nha cung cap
        //    ViewBag.TongNoNhaCungCap = GetTongNoNhaCungCap(form);

        //    // Tong thu = tong ban hang + tong thu khac
        //    // Tong ban hang
        //    ViewBag.TongBanHang = GetTongBanHang(form);
        //    // tong thu khac
        //    ViewBag.TongThuKhac = GetTongThuKhac(form);
        //    ViewBag.TongThu = ViewBag.TongBanHang + ViewBag.TongThuKhac;

        //    // Tong chi = tong mua hang hoa + tong chi phi kinh doanh + tong cac loai chi khac
        //    // Tong mua hang hoa
        //    ViewBag.TongMuaHangHoa = GetTongMuaHang(form);
        //    // Tong cac loai chi khac
        //    ViewBag.TongChiKhac = GetTongChiKhac(form);
        //    // Tong chi phi kinh doanh
        //    ViewBag.TongChiPhiKinhDoanh = GetTongChiPhiKinhDoanh(form);
        //    ViewBag.TongChi = ViewBag.TongMuaHangHoa + ViewBag.TongChiKhac + ViewBag.TongChiPhiKinhDoanh;

        //    // Loi nhuan gop
        //    ViewBag.LoiNhuanGop = GetLoiNhuanGop(form) + ViewBag.TongThuKhac;

        //    // Loi nhuan rong
        //    ViewBag.LoiNhuanRong = ViewBag.LoiNhuanGop - ViewBag.TongChiPhiKinhDoanh - ViewBag.TongChiKhac;
        //    // Quy tien mat = Tong Thu - Tong Chi
        //    ViewBag.QuyTienMat = ViewBag.TongThu - ViewBag.TongChi;

        //    // Tong Nhap
        //    var tongTraNhaCungCap = tongTraNhaCungCapQuery.Sum(e => (decimal?)e.TongTien) ?? 0;
        //    ViewBag.TongNhap = (tongNhapQuery.Sum(e => (decimal?)e.TongTien) ?? 0) - tongTraNhaCungCap;

        //    // Tong Xuat
        //    listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
        //    var tongKhachHangTra = tongKhachHangTraQuery.Sum(e => (decimal?)e.TongTien) ?? 0;
        //    ViewBag.TongXuat = GetTongXuat(fromDate, listPhieuNhapChiTiet, phieuXuatChiTietQuery, listPhieuXuatChiTiet) - tongKhachHangTra;//(listPhieXuatQuery.Sum(e => (decimal?)e.TongTien) ?? 0) - tongKhachHangTra;

        //    // Tien Von 
        //    // Tien Von
        //    var query3 = unitOfWork.PhieuXuatChiTietRepository
        //        .GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.PhieuXuat.Xoa && e.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.Default.ConstantEntities.LoaiXuatNhapKiemKe);
        //    if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
        //    {
        //        var toDate = form.To.Value.AddDays(1);
        //        query3 = query3.Where(e => e.PhieuXuat.NgayXuat >= form.From && e.PhieuXuat.NgayXuat < toDate);
        //    }

        //    ViewBag.TienVon = query3.AsEnumerable().Sum(s => unitOfWork.PhieuXuatChiTietRepository.GetTienVon(s));


        //    return View(form);
        //}

        private decimal GetTongChiPhiKinhDoanh(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var query = unitOfWork.PhieuThuChiRepository.GetMany(
                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChiPhiKinhDoanh);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                query = query.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }
            return query.Sum(e => (decimal?)e.Amount) ?? 0;
        }

        private decimal GetTongMuaHang(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var tongCacPhieuMuaHangQuery = unitOfWork.PhieuNhapRepository.GetMany(
                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapKho);

            var tongPhieuChiQuery = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChi);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongCacPhieuMuaHangQuery = tongCacPhieuMuaHangQuery.Where(e => e.NgayNhap >= form.From && e.NgayNhap < toDate);
                tongPhieuChiQuery = tongPhieuChiQuery.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }

            var tongCacPhieuMuaHang = tongCacPhieuMuaHangQuery.Sum(e => (decimal?)e.DaTra) ?? 0;
            var tongPhieuChi = tongPhieuChiQuery.Sum(e => (decimal?)e.Amount) ?? 0;
            return tongCacPhieuMuaHang + tongPhieuChi;
        }

        private decimal GetTongChiKhac(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var query = unitOfWork.PhieuThuChiRepository.GetMany(
                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && (e.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChiKhac));
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                query = query.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }
            return query.Sum(e => (decimal?)e.Amount) ?? 0;
        }

        private decimal GetTongBanHang(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var tongCacPhieuBanHangQuery = unitOfWork.PhieuXuatRepository.GetMany(
                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan);

            var tongPhieuThuQuery = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongCacPhieuBanHangQuery = tongCacPhieuBanHangQuery.Where(e => e.NgayXuat >= form.From && e.NgayXuat < toDate);
                tongPhieuThuQuery = tongPhieuThuQuery.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }

            var tongCacPhieuBanHang = tongCacPhieuBanHangQuery.Sum(e => (decimal?)e.DaTra) ?? 0;
            var tongPhieuThu = tongPhieuThuQuery.Sum(e => (decimal?)e.Amount) ?? 0;
            return tongCacPhieuBanHang + tongPhieuThu;
        }

        private decimal GetTongThuKhac(BaoCaoTongHopViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var query = unitOfWork.PhieuThuChiRepository.GetMany(
                e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && (e.LoaiPhieu == 3));
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                query = query.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }
            return query.Sum(e => (decimal?)e.Amount) ?? 0;
        }

        private decimal GetTongNoNhaCungCap(BaoCaoTongHopViewModel form)
        {
            decimal tongNo = 0;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // lay tong tien khach hang no tu phieu xuat
            var tongNoNhaCungCapQuery = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.KhachHang == null && e.NhaCungCap != null && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapKho);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongNoNhaCungCapQuery = tongNoNhaCungCapQuery.Where(e => e.NgayNhap >= form.From && e.NgayNhap < toDate);
            }
            var tongTienNoNhaCungCapTuPhieuNhaps = tongNoNhaCungCapQuery.Sum(e => (decimal?)e.TongTien > (decimal?)e.DaTra ? (decimal?)e.TongTien - (decimal?)e.DaTra : 0) ?? 0;

            // lay tong tien no khach hang tu viec khach hang tra lai.
            //var tongTienNhaCungCapNoTuViecTraLai = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.Xoa && e.KhachHang == null && e.NhaCungCap != null)
            //        .Sum(e => (decimal?)e.TongTien > (decimal?)e.DaTra ? (decimal?)e.TongTien - (decimal?)e.DaTra : 0) ?? 0;
            // lay tong tien thu tu khach hang tu cac phieu thu.
            var tongCacPhieuChiQuery = unitOfWork.PhieuThuChiRepository.GetMany(
                    e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.KhachHang == null && e.NhaCungCap != null && e.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChi);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongCacPhieuChiQuery = tongCacPhieuChiQuery.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }
            var tongTienChiChoNhaCungCap = tongCacPhieuChiQuery.Sum(e => (decimal?)e.Amount) ?? 0;
            tongNo = tongTienNoNhaCungCapTuPhieuNhaps - tongTienChiChoNhaCungCap;



            return tongNo;
        }

        private decimal GetTongNoKhachHang(BaoCaoTongHopViewModel form)
        {
            decimal tongNo = 0;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // lay tong tien khach hang no tu phieu xuat
            var tongNoMuaHangQuery = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatBan && e.KhachHang != null && e.NhaCungCap == null);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongNoMuaHangQuery = tongNoMuaHangQuery.Where(e => e.NgayXuat >= form.From && e.NgayXuat < toDate);
            }
            var tongTienKhachHangNoTuPhieuXuats = tongNoMuaHangQuery.Sum(e => (decimal?)e.TongTien > (decimal?)e.DaTra ? (decimal?)e.TongTien - (decimal?)e.DaTra : 0) ?? 0;
           
            // lay tong tien no khach hang tu viec khach hang tra lai.
            //var tongTienNoKhachHangTraLai = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && !e.Xoa && e.KhachHang!=null&& e.NhaCungCap==null)
            //        .Sum(e => (decimal?)e.TongTien > (decimal?)e.DaTra ? (decimal?)e.TongTien - (decimal?)e.DaTra : 0)??0;
            // lay tong tien thu tu khach hang tu cac phieu thu.
            var tongPhieuThuQuery = unitOfWork.PhieuThuChiRepository.GetMany(
                    e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.KhachHang != null && e.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu && e.NhaCungCap == null);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                tongPhieuThuQuery = tongPhieuThuQuery.Where(e => e.NgayTao >= form.From && e.NgayTao < toDate);
            }

            var tongTienThuTuKhachHang = tongPhieuThuQuery.Sum(e => (decimal?)e.Amount) ?? 0;
            tongNo = tongTienKhachHangNoTuPhieuXuats - tongTienThuTuKhachHang;

            return tongNo;
        }
        private decimal GetTongXuat(DateTime? fromDate, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, IQueryable<PhieuXuatChiTiet> phieuXuatChiTietQuery, List<PhieuXuatChiTiet> listPhieuXuatChiTiet)
        {

            // Loc tat cac phieu khach hang tra & tra nha cung cap           
            var listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            var listThuoc = listPhieuXuatChiTiet.Select(c => c.Thuoc).Distinct();
            decimal tongGiaTriXuat = 0M;
            foreach (var thuoc in listThuoc)
            {
                var tmpNhap = listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
                var tmpXuat = fromDate.HasValue ? listPhieuXuatChiTiet.Where(c => string.IsNullOrEmpty(c.Option1) && c.Thuoc.ThuocId == thuoc.ThuocId && c.PhieuXuat.NgayXuat > fromDate.Value.AddDays(-1)).ToList() : listPhieuXuatChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
                // tinh toan tong xuat
                var soluongXuat = 0M;
                foreach (var xuat in tmpXuat)
                {
                    if (thuoc.DonViThuNguyen != null && xuat.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                    {
                        xuat.SoLuong = xuat.SoLuong * thuoc.HeSo;
                    }

                    soluongXuat += xuat.SoLuong;

                    foreach (var nhap in tmpNhap)
                    {
                        if (thuoc.DonViThuNguyen != null && nhap.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                        {
                            nhap.SoLuong = nhap.SoLuong * thuoc.HeSo;
                            nhap.GiaNhap = nhap.GiaNhap / thuoc.HeSo;
                            nhap.DonViTinh = thuoc.DonViXuatLe;
                        }

                        if (nhap.SoLuong > soluongXuat)
                        {
                            nhap.SoLuong = nhap.SoLuong - soluongXuat;
                            tongGiaTriXuat += soluongXuat * nhap.GiaNhap;
                            soluongXuat = 0;
                            xuat.Option1 = "Edit";
                            break;
                        }

                        if (nhap.SoLuong == soluongXuat)
                        {
                            soluongXuat = 0;
                            tongGiaTriXuat += nhap.SoLuong * nhap.GiaNhap;
                            nhap.SoLuong = 0;

                            xuat.Option1 = "Edit";
                            listPhieuNhapChiTiet.Remove(nhap);
                        }
                        else
                        {
                            soluongXuat = soluongXuat - nhap.SoLuong;
                            tongGiaTriXuat += nhap.SoLuong * nhap.GiaNhap;
                            nhap.SoLuong = 0;

                            listPhieuNhapChiTiet.Remove(nhap);
                        }
                    }
                }
            }

            return tongGiaTriXuat;
        }

        private decimal GetLoiNhuanGop(BaoCaoTongHopViewModel form)
        {
            unitOfWork = new UnitOfWork();
            decimal loinhuan = 0;
            var formNhanVien = new TheoNhanVienViewModel();  
            if (User.IsInRole(Constants.Security.Roles.User.Value))
                formNhanVien.UserId = WebSecurity.GetCurrentUserId;
            formNhanVien.Type = TheoNhanVienTypes.All;      
            // admin
            if (User.IsInRole(Constants.Security.Roles.Admin.Value))
            {
                ViewBag.NhanViens = new SelectList(
                    unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc).Nhanviens.Select(e => e.User).OrderBy(e => e.TenDayDu).ToList(),
                    "UserId", "TenDayDu", ViewBag.SelectedNhanVien != null ? ViewBag.SelectedNhanVien.UserId : "");

            }
            //super user
            if (User.IsInRole(Constants.Security.Roles.SuperUser.Value))
            {
                var nhanviens =
                    unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc)
                        .Nhanviens.Select(e => e.User)
                        .OrderBy(e => e.TenDayDu).ToList();
                nhanviens.Insert(0, unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId));
                ViewBag.NhanViens = new SelectList(nhanviens, "UserId", "TenDayDu", ViewBag.SelectedNhanVien != null ? ViewBag.SelectedNhanVien.UserId : "");

            }

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }
            // processing query.
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang);
            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            var toDate = DateTime.MinValue;
            var fromDate = DateTime.MinValue;

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
           
            var listKhachHangTra = new List<PhieuNhapChiTiet>();

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                toDate = form.To.Value.Date.AddDays(1);
                fromDate = form.From.Value.Date;

                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.NgayNhap.Value < toDate && e.NgayNhap.Value >= fromDate);
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.NgayNhap < toDate).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }
            else
            {
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }

            if (formNhanVien.UserId.HasValue && formNhanVien.UserId.Value > 0)
            {
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.CreatedBy.UserId == formNhanVien.UserId);
            }

            // Loc tat cac phieu khach hang tra & tra nha cung cap      

            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();
            // order items
            formNhanVien.Items = new List<TheoNhanVienItemViewModel>();
            foreach (var item in listPhieuXuatChiTiet)
            {
                formNhanVien.Items.Add(new TheoNhanVienItemViewModel(item, listPhieuNhapChiTiet, 0, listPhieuNhapMoiNhat));
            }

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                formNhanVien.Items = formNhanVien.Items.Where(c => c.Date < toDate && c.Date >= fromDate).ToList();
            }

            formNhanVien.Items = formNhanVien.Items.GroupBy(c => c.MaPhieu).Select(c => new TheoNhanVienItemViewModel()
            {
                MaPhieu = c.Key,
                SoPhieu = c.First().SoPhieu,
                MaNhanVien = c.First().MaNhanVien,
                TenNhanVien = c.First().TenNhanVien,
                TenKhachHang = c.First().TenKhachHang,
                TongTien = c.First().TongTien,
                DaTra = c.First().DaTra,
                LoiNhuan = c.Sum(f => f.LoiNhuan),
                Date = c.First().Date
            }).OrderBy(i => i.MaPhieu).ToList();

            // Them nhung phieu khach hang tra vao de hien thi
            foreach (var item in phieuKhachHangTraQuery)
            {
                formNhanVien.Items.Add(new TheoNhanVienItemViewModel { MaNhanVien = item.CreatedBy.UserId, TenNhanVien = item.CreatedBy.TenDayDu, Date = item.NgayNhap.Value, SoPhieu = item.SoPhieuNhap, MaPhieu = item.MaPhieuNhap, MaLoai = item.LoaiXuatNhap.MaLoaiXuatNhap, TenKhachHang = item.KhachHang.TenKhachHang, TongTien = -1 * item.TongTien });
            }

            if (currentId != 0)
            {
                formNhanVien.Items = formNhanVien.Items.Where(c => c.MaNhanVien == currentId).ToList();
            }


            // bao cao tat ca nhan vien
            formNhanVien.Items = formNhanVien.Items.GroupBy(e =>
               new
               {
                   // Date = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day),
                   User = e.TenNhanVien
               }).Select(f => new TheoNhanVienItemViewModel
               {
                   TongTien = f.Sum(g => g.TongTien),
                   DaTra = f.Sum(g => g.DaTra),
                   LoiNhuan = f.Sum(g => g.LoiNhuan),
                   TenNhanVien = f.Key.User
                   // Date = f.Key.Date
               }).ToList();
            // bao cao tat ca nhan vien          

            loinhuan = 0;
            foreach (var item in formNhanVien.Items)
            {
                loinhuan = loinhuan + item.LoiNhuan;
            }            
            return loinhuan;       
           
        }

        private decimal GetLoiNhuanTheoPhieuXuat(List<PhieuNhapChiTiet> phieuNhaps, List<PhieuXuatChiTiet> phieuXuats)
        {
            decimal loinhuan = 0;

            foreach (var item in phieuXuats)
            {
                loinhuan += sThuoc.Utils.Helpers.GetLoiNhuanLoaiThuoc(phieuNhaps, item.Thuoc, item.DonViTinh.MaDonViTinh, item.SoLuong, item.GiaXuat, item.ChietKhau, item.PhieuXuat.VAT);
            }

            return loinhuan;
        }

        private decimal GetSoDuDauKy(DateTime dt)
        {
            decimal loinhuan = 0;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var phieuNhaps = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayNhap < dt).ToList();
            var phieuXuats = unitOfWork.PhieuXuatRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.RecordStatusID == (byte)RecordStatus.Activated && c.NgayXuat < dt).ToList();
            var listDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            if (phieuNhaps != null)
            {
                loinhuan += phieuNhaps.Sum(c => c.TongTien);
            }

            if (listDauKy != null)
            {
                loinhuan += listDauKy.Sum(c => c.SoDuDauKy * c.GiaDauKy);
            }

            if (phieuXuats != null)
            {
                loinhuan -= phieuXuats.Sum(c => c.TongTien);
            }

            return loinhuan;
        }

        //private decimal GetLoiNhuanLoaiThuoc(List<PhieuNhapChiTiet> list, Thuoc thuoc, int dvt, decimal soluong, decimal giaxuat, decimal chietkhau, int vat)
        //{
        //    decimal tongnhap = 0;
        //    decimal tongxuat = soluong * giaxuat * (1 + vat / 100 - chietkhau / 100);
        //    var tmp = list.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
        //    if (dvt == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
        //    {
        //        soluong = soluong * thuoc.HeSo;
        //    }

        //    foreach (var item in tmp)
        //    {
        //        if (item.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
        //        {
        //            item.SoLuong = item.SoLuong * thuoc.HeSo;
        //            item.GiaNhap = item.GiaNhap / thuoc.HeSo;
        //        }

        //        if (item.SoLuong > soluong)
        //        {
        //            item.SoLuong = item.SoLuong - soluong;
        //            tongnhap += soluong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100 - item.ChietKhau / 100);
        //            break;
        //        }
        //        else
        //        {
        //            tongnhap += item.SoLuong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100 - item.ChietKhau / 100);
        //            list.Remove(item);
        //        }
        //    }

        //    return tongxuat - tongnhap;
        //}

        //// GET: BaoCao
        //public ActionResult TongKetDoanhThu()
        //{
        //    ViewBag.searchTen = new SelectList(db.KhachHangs, "MaKhachHang", "TenKhachHang");
        //    return View();
        //}
        [SimpleAuthorize("User")]
        public ActionResult TheoNhanVien(TheoNhanVienViewModel form)
        {
            if (User.IsInRole(Constants.Security.Roles.User.Value))
                form.UserId = WebSecurity.GetCurrentUserId;
            form.Type = TheoNhanVienTypes.All;
            if (!form.UserId.HasValue || form.UserId <= 0)
            {
                if (User.IsInRole(Constants.Security.Roles.User.Value))
                {
                    form.Type = TheoNhanVienTypes.Individual;
                    ViewBag.SelectedNhanVien = unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId);
                }

            }
            else
            {
                ViewBag.SelectedNhanVien = unitOfWork.UserProfileRepository.GetById(form.UserId);
                form.Type = TheoNhanVienTypes.Individual;
            }
            // admin
            if (User.IsInRole(Constants.Security.Roles.Admin.Value))
            {
                ViewBag.NhanViens = new SelectList(
                    unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc).Nhanviens.Select(e => e.User).OrderBy(e => e.TenDayDu).ToList(),
                    "UserId", "TenDayDu", ViewBag.SelectedNhanVien != null ? ViewBag.SelectedNhanVien.UserId : "");

            }
            //super user
            if (User.IsInRole(Constants.Security.Roles.SuperUser.Value))
            {
                var nhanviens =
                    unitOfWork.NhaThuocRepository.GetById(this.GetNhaThuoc().MaNhaThuoc)
                        .Nhanviens.Select(e => e.User)
                        .OrderBy(e => e.TenDayDu).ToList();
                nhanviens.Insert(0, unitOfWork.UserProfileRepository.GetById(WebSecurity.GetCurrentUserId));
                ViewBag.NhanViens = new SelectList(nhanviens, "UserId", "TenDayDu", ViewBag.SelectedNhanVien != null ? ViewBag.SelectedNhanVien.UserId : "");

            }

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            // processing query.
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang);
            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            var toDate = DateTime.MinValue;
            var fromDate = DateTime.MinValue;

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
            var listKhachHangTra = new List<PhieuNhapChiTiet>();

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                toDate = form.To.Value.Date.AddDays(1);
                fromDate = form.From.Value.Date;

                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.NgayNhap.Value < toDate && e.NgayNhap.Value >= fromDate);
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.NgayNhap < toDate).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }
            else
            {
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }

            if (form.UserId.HasValue && form.UserId.Value > 0)
            {
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.CreatedBy.UserId == form.UserId);
            }

            // Loc tat cac phieu khach hang tra & tra nha cung cap      

            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();
            // order items
            form.Items = new List<TheoNhanVienItemViewModel>();
            foreach (var item in listPhieuXuatChiTiet)
            {
                form.Items.Add(new TheoNhanVienItemViewModel(item, listPhieuNhapChiTiet, 0, listPhieuNhapMoiNhat));
            }

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                form.Items = form.Items.Where(c => c.Date < toDate && c.Date >= fromDate).ToList();
            }

            form.Items = form.Items.GroupBy(c => c.MaPhieu).Select(c => new TheoNhanVienItemViewModel()
            {
                MaPhieu = c.Key,
                SoPhieu = c.First().SoPhieu,
                MaNhanVien = c.First().MaNhanVien,
                TenNhanVien = c.First().TenNhanVien,
                TenKhachHang = c.First().TenKhachHang,
                TongTien = c.First().TongTien,
                DaTra = c.First().DaTra,
                LoiNhuan = c.Sum(f => f.LoiNhuan),
                Date = c.First().Date
            }).OrderBy(i=>i.MaPhieu).ToList();

            // Them nhung phieu khach hang tra vao de hien thi
            foreach (var item in phieuKhachHangTraQuery)
            {
                form.Items.Add(new TheoNhanVienItemViewModel { MaNhanVien = item.CreatedBy.UserId, TenNhanVien = item.CreatedBy.TenDayDu, Date = item.NgayNhap.Value, SoPhieu = item.SoPhieuNhap, MaPhieu = item.MaPhieuNhap, MaLoai = item.LoaiXuatNhap.MaLoaiXuatNhap, TenKhachHang = item.KhachHang.TenKhachHang, TongTien = -1 * item.TongTien });
            }

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.MaNhanVien == currentId).ToList();
            }

            if (form.UserId.HasValue && form.UserId.Value > 0)
            {
                // Bao cao theo 1 nhan vien
                form.Items = form.Items.Where(c => c.MaNhanVien == form.UserId.Value).OrderBy(c => c.Date).ToList();
            }
            else
            {
                // bao cao tat ca nhan vien
                form.Items = form.Items.GroupBy(e =>
                   new
                   {
                       // Date = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day),
                       User = e.TenNhanVien
                   }).Select(f => new TheoNhanVienItemViewModel
                   {
                       TongTien = f.Sum(g => g.TongTien),
                       DaTra = f.Sum(g => g.DaTra),
                       LoiNhuan = f.Sum(g => g.LoiNhuan),
                       TenNhanVien = f.Key.User
                       // Date = f.Key.Date
                   }).ToList();
            }
            if (form.Export == 1)
            {
                return GenerateExcelTheoNhanVien(form);
            }
            return View(form);
        }

        private ActionResult GenerateExcelTheoNhanVien(TheoNhanVienViewModel form)
        {
            var title = "Báo cáo theo nhân viên";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Nhân viên: {0}", form.UserId > 0 ? unitOfWork.UserProfileRepository.GetById(form.UserId).TenDayDu : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Ngày";
                ws.Cells[rowIndex, colIndex++].Value = form.Type == TheoNhanVienTypes.All ? "Tên Nhân Viên" : "Tên Khách Hàng";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng tiền";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền trả";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền nợ";
                ws.Cells[rowIndex, colIndex++].Value = "Lợi nhuận";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.Date.ToString("dd/MM/yyyy");
                    ws.Cells[rowIndex, colIndex++].Value = form.Type == TheoNhanVienTypes.All ? item.TenNhanVien : item.TenKhachHang;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.TongTien;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.DaTra;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TienNo;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.LoiNhuan;
                    cells.Style.Numberformat.Format = "#,##0";

                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tổng tiền:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongTien);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng lợi nhuận:";
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 2, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.LoiNhuan);
                cells.Style.Numberformat.Format = "#,##0";



                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }

        [SimpleAuthorize("Admin")]
        public ActionResult TheoMatHang(TheoMatHangViewModel form)
        {
            bool tinhToanTheoKy = false;//ConfigurationManager.AppSettings["TinhToanBaoCaoTheoKy"] != null && ConfigurationManager.AppSettings["TinhToanBaoCaoTheoKy"] == "1";

            if (string.IsNullOrEmpty(form.Type))
                form.Type = "all";

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // nhom khach hang
            ViewBag.NhomThuocs =
                unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenNhomThuoc)
                    .AsEnumerable();

            // DS khach hang
            ViewBag.Thuocs =
                unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenThuoc)
                    .AsEnumerable();

            // Get current user

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            List<int> listThuoc;

            DateTime dt = DateTime.MinValue;
            var toDate = DateTime.MaxValue;
            var fromDate = DateTime.MinValue;

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                dt = form.From.Value.Date;
                toDate = form.To.Value.Date.AddDays(1);
                fromDate = form.From.Value.Date;
            }

            // processing query.
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);

            if (form.Type == "byname")
            {
                listThuoc = new List<int>() { unitOfWork.ThuocRepository.Get(c => c.ThuocId == form.ThuocId).FirstOrDefault().ThuocId };
                phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(c => c.Thuoc.ThuocId == form.ThuocId);
                listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == form.ThuocId).ToList();
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(c => c.Thuoc.ThuocId == form.ThuocId);
            }
            else if (form.Type == "bygroup")
            {
                listThuoc = unitOfWork.ThuocRepository.Get(c => c.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc).Select(c => c.ThuocId).ToList();
            }
            else
            {
                if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
                {
                    listThuoc =
                        unitOfWork.PhieuXuatChiTietRepository.Get(c => c.PhieuXuat.NgayXuat < toDate)
                            .Select(c => c.Thuoc.ThuocId)
                            .ToList();
                }
                else
                {
                    listThuoc = unitOfWork.ThuocRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc).Select(c => c.ThuocId).ToList();
                }
            }

            if (tinhToanTheoKy)
            {
                var listKyChiTiet = sThuoc.Utils.Helpers.GetTongKetKy(unitOfWork, maNhaThuoc, listThuoc, ref dt);
                if (listKyChiTiet.Any())
                {
                    foreach (var item in listKyChiTiet)
                    {
                        listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                        {
                            Thuoc = new Thuoc() { ThuocId = item.Thuoc.ThuocId },
                            PhieuNhap =
                                new PhieuNhap()
                                {
                                    NgayNhap = DateTime.MinValue,
                                    VAT = 0,
                                    LoaiXuatNhap =
                                        new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho }
                                },
                            GiaNhap = item.Gia,
                            DonViTinh = item.Thuoc.DonViXuatLe,
                            SoLuong = item.SoLuong,
                            ChietKhau = 0
                        });
                    }

                    phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(c => c.PhieuNhap.NgayNhap >= dt);
                }
                else
                {
                    var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
                    foreach (var item in listDuDauKy)
                    {
                        listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                        {
                            Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                            PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                            GiaNhap = item.GiaDauKy,
                            DonViTinh = item.DonViXuatLe,
                            SoLuong = item.SoDuDauKy,
                            ChietKhau = 0
                        });
                    }
                }
            }
            else
            {
                var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
                foreach (var item in listDuDauKy)
                {
                    listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                    {
                        Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                        PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                        GiaNhap = item.GiaDauKy,
                        DonViTinh = item.DonViXuatLe,
                        SoLuong = item.SoDuDauKy,
                        ChietKhau = 0
                    });
                }
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
            var listKhachHangTra = new List<PhieuNhapChiTiet>();

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.PhieuNhap.NgayNhap.Value < toDate && e.PhieuNhap.NgayNhap.Value >= dt);
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.NgayNhap >= dt && c.PhieuNhap.NgayNhap < toDate).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }
            else
            {
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }

            // Loc tat cac phieu khach hang tra & tra nha cung cap           

            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // Tinh toan dua tren phieu nhap xuat 
            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();

            form.Items = new List<TheoMatHangItemViewModel>();

            foreach (var item in listPhieuXuatChiTiet)
            {
                form.Items.Add(new TheoMatHangItemViewModel(item, unitOfWork, listPhieuNhapChiTiet, 0, listPhieuNhapMoiNhat));
            }

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.MaNhanVien == currentId).ToList();
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(c => c.PhieuNhap.CreatedBy.UserId == currentId);
            }

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                form.Items = form.Items.Where(c => c.Date < toDate && c.Date >= fromDate).ToList();
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(c => DbFunctions.TruncateTime(c.PhieuNhap.NgayNhap.Value) < toDate && DbFunctions.TruncateTime(c.PhieuNhap.NgayNhap.Value) >= fromDate);
            }

            if (form.Type == "byname")
            {
                form.Items = form.Items.Where(c => c.ThuocId == form.ThuocId).ToList();
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(c => c.Thuoc.ThuocId == form.ThuocId);
            }
            else if (form.Type == "bygroup")
            {
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(c => c.Thuoc.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc);
                form.Items = form.Items.Where(c => c.MaNhomThuoc == form.MaNhomThuoc).ToList();
                form.Items = form.Items.GroupBy(e =>
                new
                {
                    MaThuoc = e.MaThuoc                 
                }).Select(f => new TheoMatHangItemViewModel
                {
                    TongTien = f.Sum(g => g.TongTienTruocXL),
                    DaTra = f.Sum(g => g.DaTra),
                    LoiNhuan = f.Sum(g => g.LoiNhuan),
                    MaThuoc = f.Key.MaThuoc,
                    TenThuoc = f.First().TenThuoc,
                    GiaNhap = f.First().GiaNhap,
                    GiaXuat = f.First().GiaXuat,
                    SoLuong = f.Sum(g => g.SoLuongTruocXL),
                    DonViXuatLe = f.First().DonViXuatLe,
                    TenDonViTinh = f.First().DonViXuatLe.TenDonViTinh
                }).ToList();
            }
            else
            {
                form.Items = form.Items.GroupBy(e =>
                new
                {
                    e.MaThuoc
                }).Select(f => new TheoMatHangItemViewModel
                {
                    TongTien = f.Sum(g => g.TongTienTruocXL),
                    DaTra = f.Sum(g => g.DaTra),
                    LoiNhuan = f.Sum(g => g.LoiNhuan),
                    MaThuoc = f.Key.MaThuoc,
                    TenThuoc = f.First().TenThuoc,
                    GiaNhap = f.First().GiaNhap, //getGiaNhapTB(f.First().MaThuoc, form),
                    GiaXuat = f.First().GiaXuat,
                    SoLuong = f.Sum(g => g.SoLuongTruocXL),
                    DonViXuatLe = f.First().DonViXuatLe,
                    TenDonViTinh = f.First().DonViXuatLe.TenDonViTinh
                }).ToList();
            }

           

            if (form.Type == "byname")
            {
                // Them nhung phieu khach hang tra vao de hien thi
                //foreach (var item in phieuKhachHangTraQuery)
                //{
                //    form.Items.Add(new TheoMatHangItemViewModel { ThuocId = item.Thuoc.ThuocId, Date = item.PhieuNhap.NgayNhap.Value, MaThuoc = item.Thuoc.MaThuoc, TenThuoc = item.Thuoc.TenThuoc, SoLuongTruocXL = -1 * item.SoLuong, TongTienTruocXL = -1 * (item.SoLuong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100) * (1 - item.ChietKhau / 100)), TenDonViTinh = item.DonViTinh.TenDonViTinh, LoiNhuan = 0 });
                //}
                form.Items = form.Items.Where(c => c.ThuocId == form.ThuocId).OrderBy(c => c.Date).ToList();
            }

            // bao cao

            if (form.Export == 1)
            {
                return GenerateExcelTheoMatHang(form);
            }
            return View(form);
        }

        [SimpleAuthorize("Admin")]
        public ActionResult TheoNgay(string Date)
        {
            DateTime dt;
            if (!DateTime.TryParse(Date, out dt))
                dt = DateTime.Now.Date;

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            TheoNgayViewModel form = new TheoNgayViewModel();

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.RecordStatusID == (byte)RecordStatus.Activated && DbFunctions.TruncateTime(c.NgayNhap) == dt);
            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());

            // Loc tat cac phieu khach hang tra & tra nha cung cap           
            var listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // Tinh toan dua tren phieu nhap xuat 
            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();

            form.Items = new List<TheoNgayItemViewModel>();

            foreach (var item in listPhieuXuatChiTiet)
            {
                form.Items.Add(new TheoNgayItemViewModel(item, listPhieuNhapChiTiet, listPhieuNhapMoiNhat));
            }

            form.Items = form.Items.Where(c => c.Date.Date == dt).ToList();

            foreach (var item in phieuKhachHangTraQuery)
            {
                form.Items.Add(new TheoNgayItemViewModel() { CreatedBy = item.CreatedBy.UserId, SoPhieu = (int)item.SoPhieuNhap, MaPhieu = item.MaPhieuNhap, MaLoai = item.LoaiXuatNhap.MaLoaiXuatNhap, TenKhachHang = item.KhachHang.TenKhachHang, TongTien = -1 * item.TongTien });
            }

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.CreatedBy == currentId).ToList();
            }

            form.Items = form.Items.GroupBy(f => f.SoPhieu).Select(f => new TheoNgayItemViewModel() { SoPhieu = f.Key, MaPhieu = f.First().MaPhieu, MaLoai = f.First().MaLoai, TenKhachHang = f.First().TenKhachHang, TongTien = f.First().TongTien, TienTra = f.First().TienTra, TienNo = f.First().TienNo, LoiNhuan = f.Sum(g => g.LoiNhuan) }).ToList();
            //form.Items = form.Items.Where(c => c.SoPhieu == 4921).ToList();
            form.Items.ForEach(c => { form.TongTien += c.TongTien; form.TongLoiNhuan += c.LoiNhuan; });

            form.SoLuongKhach = form.Items.Count;
            form.Date = dt;

            return View(form);
        }

        public ActionResult ChiTietTheoNgay(string Date)
        {
            DateTime dt = DateTime.MinValue;
            if (!DateTime.TryParse(Date, out dt))
                dt = DateTime.Now.Date;
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            ChiTietTheoNgayViewModel form = new ChiTietTheoNgayViewModel();
            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated && DbFunctions.TruncateTime(c.PhieuNhap.NgayNhap) == dt).ToList();

            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();
            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());

            // Loc tat cac phieu khach hang tra & tra nha cung cap           
            var listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // Tinh toan dua tren phieu nhap xuat 
            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();
            form.Items = new List<ChiTietTheoNgayItemViewModel>();
            foreach (var item in listPhieuXuatChiTiet)
            {
                var tmp = new ChiTietTheoNgayItemViewModel(item, listPhieuNhapChiTiet, listPhieuNhapMoiNhat);
                if (form.Items.Any(c => c.MaPhieu == tmp.MaPhieu))
                    tmp.SoPhieu = 0;

                form.Items.Add(tmp);
            }

            form.Items = form.Items.Where(c => c.Date.Date == dt).ToList();

            foreach (var item in phieuKhachHangTraQuery)
            {
                //item.SoLuong = !string.IsNullOrEmpty(item.Option1) ? decimal.Parse(item.Option1) : item.SoLuong;
                //if (item.DonViTinh.MaDonViTinh == item.Thuoc.DonViThuNguyen.MaDonViTinh && item.Thuoc.HeSo != 0)
                //{
                //    item.SoLuong = item.SoLuong / item.Thuoc.HeSo;
                //}
                var tmp = new ChiTietTheoNgayItemViewModel()
                {
                    CreatedBy = item.PhieuNhap.CreatedBy.UserId,
                    SoPhieu = (int)item.PhieuNhap.SoPhieuNhap,
                    MaPhieu = item.PhieuNhap.MaPhieuNhap,
                    MaLoai = item.PhieuNhap.LoaiXuatNhap.MaLoaiXuatNhap,
                    TenKhachHang = item.PhieuNhap.KhachHang.TenKhachHang,
                    MaThuoc = item.Thuoc.MaThuoc,
                    TenThuoc = item.Thuoc.TenThuoc,                    
                    SoLuongTruocXL = item.SoLuong,
                    DonGia = item.GiaNhap,
                    DonVi = item.DonViTinh.TenDonViTinh,
                    ChietKhau = item.ChietKhau,
                    VAT = item.PhieuNhap.VAT,
                    ThanhTien = -1 * item.SoLuong * item.GiaNhap * (1 - item.ChietKhau / 100) * (1 + item.PhieuNhap.VAT / 100),
                    LoiNhuan = 0
                };

                if (form.Items.Any(c => c.MaPhieu == tmp.MaPhieu))
                {
                    tmp.SoPhieu = 0;
                }

                form.Items.Add(tmp);
            }

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.CreatedBy == currentId).ToList();
            }

            form.Items.Where(c => c.ThanhTien > 0).ToList().ForEach(c => { form.TongTien += c.ThanhTien; form.TongLoiNhuan += c.LoiNhuan; });

            form.SoLuongKhach = form.Items.Count;

            form.Date = dt;

            return View(form);
        }

        private ActionResult GenerateExcelTheoMatHang(TheoMatHangViewModel form)
        {
            var title = "Báo cáo theo mặt hàng";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                var ten = "";
                switch (form.Type)
                {
                    default:
                    case "all":
                        ten = "Tổng kết hết";
                        break;
                    case "byname":
                        var item = unitOfWork.ThuocRepository.GetById(form.ThuocId);
                        if (item != null)
                            ten = item.TenThuoc;
                        break;
                    case "bygroup":
                        var item1 = unitOfWork.NhomThuocRepository.GetById(form.MaNhomThuoc);
                        if (item1 != null)
                            ten = item1.TenNhomThuoc;
                        break;

                }
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Mặt hàng: {0}", ten);
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Mã thuốc";
                ws.Cells[rowIndex, colIndex++].Value = "Tên thuốc";
                ws.Cells[rowIndex, colIndex++].Value = "Đơn vị";
                ws.Cells[rowIndex, colIndex++].Value = "Giá nhập";
                ws.Cells[rowIndex, colIndex++].Value = "Giá xuất";
                ws.Cells[rowIndex, colIndex++].Value = "Số lượng";
                
                ws.Cells[rowIndex, colIndex++].Value = "Tổng tiền";
                ws.Cells[rowIndex, colIndex++].Value = "Lợi nhuận";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.MaThuoc;
                    ws.Cells[rowIndex, colIndex++].Value = item.TenThuoc;
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.DonViXuatLe.TenDonViTinh;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.GiaNhap;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.GiaXuat;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.SoLuong;
                    cells.Style.Numberformat.Format = "#,##0";
                    
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TongTien;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.LoiNhuan;
                    cells.Style.Numberformat.Format = "#,##0";

                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tổng tiền:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongTien);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng lợi nhuận:";
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 2, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.LoiNhuan);
                cells.Style.Numberformat.Format = "#,##0";



                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }

        private ActionResult GenerateExcelTheoKhoHang(TheoKhoHangViewModel form)
        {
            var title = "Báo cáo theo kho hàng";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                var ten = "";
                switch (form.Type)
                {
                    default:
                    case "all":
                        ten = "Tổng kết hết";
                        break;
                    case "byname":
                        var item = unitOfWork.ThuocRepository.GetById(form.ThuocId);
                        if (item != null)
                            ten = item.TenThuoc;
                        break;
                    case "bygroup":
                        var item1 = unitOfWork.NhomThuocRepository.GetById(form.MaNhomThuoc);
                        if (item1 != null)
                            ten = item1.TenNhomThuoc;
                        break;

                }
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Kho hàng: {0}", ten);
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Mã thuốc";
                ws.Cells[rowIndex, colIndex++].Value = "Tên thuốc";
                ws.Cells[rowIndex, colIndex++].Value = "Tồn đầu";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng giá trị";
                ws.Cells[rowIndex, colIndex++].Value = "Nhập";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng giá trị";
                ws.Cells[rowIndex, colIndex++].Value = "Xuất";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng giá trị";
                ws.Cells[rowIndex, colIndex++].Value = "Tồn cuối";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng giá trị";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.MaThuoc;
                    ws.Cells[rowIndex, colIndex++].Value = item.TenThuoc;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.TonDau;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TongGiaTriTonDau;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.Nhap;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TongGiaTriNhap;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.Xuat;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TongGiaTriXuat;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TonCuoi;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TongGiaTriTonCuoi;
                    cells.Style.Numberformat.Format = "#,##0";
                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tồn ĐK:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Style.Font.Bold = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongGiaTriTonDau);
                cells.Style.Numberformat.Format = "#,##0";
                cells.Style.Font.Bold = true;

                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng nhập:";
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Style.Font.Bold = true;
                cells = ws.Cells[rowIndex + 2, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongGiaTriNhap);
                cells.Style.Numberformat.Format = "#,##0";
                cells.Style.Font.Bold = true;

                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Value = "Tổng xuất:";
                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Merge = true;
                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Style.Font.Bold = true;
                cells = ws.Cells[rowIndex + 3, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongGiaTriXuat);
                cells.Style.Numberformat.Format = "#,##0";
                cells.Style.Font.Bold = true;

                ws.Cells[rowIndex + 4, colIndex, rowIndex + 4, colIndex + 1].Value = "Tồn CK:";
                ws.Cells[rowIndex + 4, colIndex, rowIndex + 4, colIndex + 1].Merge = true;
                ws.Cells[rowIndex + 4, colIndex, rowIndex + 4, colIndex + 1].Style.Font.Bold = true;
                cells = ws.Cells[rowIndex + 4, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongGiaTriTonCuoi);
                cells.Style.Numberformat.Format = "#,##0";
                cells.Style.Font.Bold = true;

                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }
        [SimpleAuthorize("Admin")]
        public ActionResult TheoKhoHang(TheoKhoHangViewModel form)
        {
            if (string.IsNullOrEmpty(form.Type))
                form.Type = "all";

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // nhom khach hang
            ViewBag.NhomThuocs =
                unitOfWork.NhomThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenNhomThuoc)
                    .AsEnumerable();
            // DS khach hang
            ViewBag.Thuocs =
                unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenThuoc)
                    .AsEnumerable();
            // processing query.
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated
                && c.PhieuNhap.LoaiXuatNhap != null && c.PhieuNhap.LoaiXuatNhap.MaLoaiXuatNhap != (int)NoteInOutType.InitialInventory);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc 
                && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated && e.PhieuXuat.MaLoaiXuatNhap != (int)NoteInOutType.InitialInventory);

            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc).ToList();
            var thuocQuery = unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc);

            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId, NhomThuoc = item.NhomThuoc },                    
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            DateTime? fromDate = null;
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                fromDate = form.From.Value.Date;
                phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(e => e.PhieuNhap.NgayNhap < toDate);
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
            }

            if (form.Type == "byname")
            {
                thuocQuery = thuocQuery.Where(e => e.ThuocId == form.ThuocId);
                phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(c => c.Thuoc.ThuocId == form.ThuocId);
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(c => c.Thuoc.ThuocId == form.ThuocId);
                listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == form.ThuocId).ToList();
            }

            if (form.Type == "bygroup")
            {
                thuocQuery = thuocQuery.Where(e => e.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc);

                phieuNhapChiTietQuery = phieuNhapChiTietQuery.Where(c => c.Thuoc.NhomThuoc != null && c.Thuoc.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc);
                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(c => c.Thuoc.NhomThuoc != null && c.Thuoc.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc);

                listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.Thuoc.NhomThuoc != null && c.Thuoc.NhomThuoc.MaNhomThuoc == form.MaNhomThuoc).ToList();
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());

            // Loc tat cac phieu khach hang tra & tra nha cung cap           
            var listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // order items
            thuocQuery = thuocQuery.OrderBy(e => e.TenThuoc);


            // bao cao
            form.Items = new List<TheoKhoHangItemViewModel>();
            foreach (var thuoc in thuocQuery)
            {
                form.Items.Add(sThuoc.Utils.Helpers.CalculateKho(thuoc, listPhieuNhapChiTiet, listPhieuXuatChiTiet, fromDate));
            }

            if (form.Export == 1)
            {
                return GenerateExcelTheoKhoHang(form);
            }

            return View(form);
        }

        private decimal getGiaNhapTB(string maThuoc, TheoMatHangViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var query = unitOfWork.PhieuNhapChiTietRepository.GetMany(e => e.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated && e.Thuoc.MaThuoc == maThuoc);
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.AddDays(1);
                query = query.Where(e => e.PhieuNhap.NgayNhap >= form.From && e.PhieuNhap.NgayNhap < toDate);
            }
            var avg =
                query.Average(
                    e => e.DonViTinh.MaDonViTinh == e.Thuoc.DonViXuatLe.MaDonViTinh ?

                (decimal?)e.ChietKhau > 0 ? (decimal?)e.GiaNhap - (decimal?)e.GiaNhap * e.ChietKhau / 100 : (decimal?)e.GiaNhap
            :
                (decimal?)e.ChietKhau > 0 ? (decimal?)e.GiaNhap - (decimal?)e.GiaNhap * e.ChietKhau / 100 / (decimal?)e.Thuoc.HeSo : (decimal?)e.GiaNhap / (decimal?)e.Thuoc.HeSo) ?? 0;
            return avg;
            //TODO: tinh gia nhap tb them truong hop co VAT 
        }

        [SimpleAuthorize("Admin")]
        public ActionResult TheoKhachHang(TheoKhachHangViewModel form)
        {
            if (string.IsNullOrEmpty(form.Type))
                form.Type = "all";

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // nhom khach hang
            ViewBag.NhomKhachHangs =
                unitOfWork.NhomKhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenNhomKhachHang)
                    .AsEnumerable();
            // DS khach hang
            ViewBag.KhachHangs =
                unitOfWork.KhachHangRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenKhachHang)
                    .AsEnumerable();

            // Kiem tra xem user co phai la admin ko? neu ko phai admin thi chi hien thi du lieu do user do tao ra 
            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            // processing query.
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.RecordStatusID == (byte)RecordStatus.Activated);
            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();

            var phieuThuQuery = unitOfWork.PhieuThuChiRepository.Get(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuThu);

            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());

            //Loc tat cac phieu khach hang tra
            var listKhachHangTra = new List<PhieuNhapChiTiet>();

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                var fromDate = form.From.Value.Date;

                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.NgayNhap.Value < toDate && e.NgayNhap.Value >= fromDate);
                phieuThuQuery = phieuThuQuery.Where(c => c.NgayTao < toDate && c.NgayTao >= fromDate);

                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.NgayNhap < toDate).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }
            else
            {
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }

            //Loc tat cac phieu tra nha cung cap                       
            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // bao cao
            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();

            form.Items = new List<TheoKhachHangItemViewModel>();
            foreach (var item in listPhieuXuatChiTiet)
            {
                form.Items.Add(new TheoKhachHangItemViewModel(item, listPhieuNhapChiTiet, 0, listPhieuNhapMoiNhat));
            }

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                var fromDate = form.From.Value.Date;
                form.Items = form.Items.Where(c => c.Date < toDate && c.Date >= fromDate).ToList();
            }

            form.Items = form.Items.GroupBy(c => c.MaPhieu).Select(c => new TheoKhachHangItemViewModel
            {
                MaPhieu = c.First().MaPhieu,
                MaNhanVien = c.First().MaNhanVien,
                MaKhachHang = c.First().MaKhachHang,
                TenKhachHang = c.First().TenKhachHang,
                SoPhieu = c.First().SoPhieu,
                TongTien = c.First().TongTien,
                DaTra = c.First().DaTra,
                LoiNhuan = c.Sum(f => f.LoiNhuan),
                Date = c.First().Date
            }).ToList();

            foreach (var item in phieuKhachHangTraQuery)
            {
                form.Items.Add(new TheoKhachHangItemViewModel() { MaNhanVien = item.CreatedBy.UserId, SoPhieu = item.SoPhieuNhap, Date = item.NgayNhap.Value, MaKhachHang = item.KhachHang.MaKhachHang, TenKhachHang = item.KhachHang.TenKhachHang, TongTien = -1 * item.TongTien });
            }

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.MaNhanVien == currentId).ToList();
            }

            if (form.Type == "byname")
            {
                form.Items = form.Items.Where(e => e.MaKhachHang == form.MaKhachHang).ToList();
                phieuThuQuery = phieuThuQuery.Where(c => c.KhachHang.MaKhachHang == form.MaKhachHang);
            }
            else if (form.Type == "bygroup")
            {
                form.Items = form.Items.Where(c => c.MaNhomKhachHang == form.MaNhomKhachHang).ToList();
                phieuThuQuery = phieuThuQuery.Where(c => c.KhachHang.NhomKhachHang.MaNhomKhachHang == form.MaNhomKhachHang);
            }
            else
            {
                form.Items = form.Items.GroupBy(e =>
                 new
                 {
                     e.MaKhachHang
                 }).Select(f => new TheoKhachHangItemViewModel
                 {
                     TongTien = f.Sum(g => g.TongTien),
                     DaTra = f.Sum(g => g.DaTra),
                     TongTraSau = phieuThuQuery.Where(c => c.KhachHang.MaKhachHang ==  f.FirstOrDefault().MaKhachHang).Sum(c => c.Amount),
                     LoiNhuan = f.Sum(g => g.LoiNhuan),
                     TenKhachHang = f.FirstOrDefault().TenKhachHang
                 }).ToList();
            }

            ViewBag.TongTraSau = phieuThuQuery.Sum(c => c.Amount);

            if (form.Export == 1)
            {
                return GenerateExcelTheoKhachHang(form);
            }
            return View(form);
        }

        private ActionResult GenerateExcelTheoKhachHang(TheoKhachHangViewModel form)
        {
            var title = "Báo cáo theo khách hàng";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                var ten = "";
                switch (form.Type)
                {
                    default:
                    case "all":
                        ten = "Tổng kết hết";
                        break;
                    case "byname":
                        var item = unitOfWork.KhachHangRepository.GetById(form.MaKhachHang);
                        if (item != null)
                            ten = item.TenKhachHang;
                        break;
                    case "bygroup":
                        var item1 = unitOfWork.NhomKhachHangRepository.GetById(form.MaNhomKhachHang);
                        if (item1 != null)
                            ten = item1.TenNhomKhachHang;
                        break;

                }
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Khách hàng: {0}", ten);
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Ngày";
                ws.Cells[rowIndex, colIndex++].Value = "Tên Khách Hàng";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng tiền";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền trả";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền nợ";
                ws.Cells[rowIndex, colIndex++].Value = "Lợi nhuận";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.Date.ToString("dd/MM/yyyy");
                    ws.Cells[rowIndex, colIndex++].Value = item.TenKhachHang;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.TongTien;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.DaTra;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TienNo;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.LoiNhuan;
                    cells.Style.Numberformat.Format = "#,##0";

                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tổng tiền:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongTien);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng trả:";
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 2, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.DaTra);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Value = "Tổng lợi nhuận:";
                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 3, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.LoiNhuan);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 4, colIndex, rowIndex + 4, colIndex + 1].Value = "Tổng nợ:";
                ws.Cells[rowIndex + 4, colIndex, rowIndex + 4, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 4, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TienNo);
                cells.Style.Numberformat.Format = "#,##0";



                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }

        private decimal getDebt(long maKhachHang)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            decimal? result = unitOfWork.PhieuXuatRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.KhachHang.MaKhachHang == maKhachHang)
                .Sum(x => (decimal?)x.TongTien - (decimal?)x.DaTra);
            var slThuChi = unitOfWork.PhieuThuChiRepository.GetMany(x => x.NhaThuoc.MaNhaThuoc == maNhaThuoc && x.KhachHang.MaKhachHang == maKhachHang)
                .Sum(x => (int?)x.Amount) ?? 0;
            result = result - slThuChi;

            if (result != null) return result.Value;
            return 0;
        }

        [SimpleAuthorize("Admin")]
        public ActionResult TheoNhaCungCap(TheoNhaCungCapViewModel form)
        {
            if (string.IsNullOrEmpty(form.Type))
                form.Type = "all";

            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            // nhom nha cung cap
            ViewBag.NhomNhaCungCaps =
                unitOfWork.NhomNhaCungCapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenNhomNhaCungCap)
                    .AsEnumerable();
            // DS nha cung cap
            ViewBag.NhaCungCaps =
                unitOfWork.NhaCungCapRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.TenNhaCungCap != "Điều chỉnh sau kiểm kê")
                    .OrderBy(e => e.TenNhaCungCap)
                    .AsEnumerable();

            // Get current user

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            // processing query.

            var phieuChiQuery = unitOfWork.PhieuThuChiRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiPhieu == Constants.LoaiPhieuThuChi.PhieuChi);
            var phieuNhapQuery = unitOfWork.PhieuNhapRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.RecordStatusID == (byte)RecordStatus.Activated && (e.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapKho));
            
            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                var fromDate = form.From.Value.Date;
                phieuChiQuery = phieuChiQuery.Where(c => c.NgayTao < toDate && c.NgayTao >= fromDate);
                //phieuTraNhaCungCap = phieuTraNhaCungCap.Where(c => c.NgayXuat < toDate && c.NgayXuat >= fromDate);
                phieuNhapQuery = phieuNhapQuery.Where(e => e.NgayNhap >= fromDate && e.NgayNhap < toDate);
            }

            if (form.Type == "byname")
            {
                phieuChiQuery = phieuChiQuery.Where(e => e.NhaCungCap.MaNhaCungCap == form.MaNhaCungCap);
                phieuNhapQuery = phieuNhapQuery.Where(e => e.NhaCungCap.MaNhaCungCap == form.MaNhaCungCap);
                //phieuTraNhaCungCap = phieuTraNhaCungCap.Where(c => c.NhaCungCap.MaNhaCungCap == form.MaNhaCungCap);
            }

            if (form.Type == "bygroup")
            {
                phieuChiQuery = phieuChiQuery.Where(e => e.NhaCungCap.MaNhomNhaCungCap == form.MaNhomNhaCungCap);
                phieuNhapQuery = phieuNhapQuery.Where(e => e.NhaCungCap.MaNhomNhaCungCap == form.MaNhomNhaCungCap);
                //phieuTraNhaCungCap = phieuTraNhaCungCap.Where(e => e.NhaCungCap.MaNhomNhaCungCap == form.MaNhomNhaCungCap);
            }

            // bao cao
            form.Items = new List<TheoNhaCungCapItemViewModel>();
            // Phieu nhap tu nha cung cap
            foreach (var item in phieuNhapQuery.OrderBy(c => c.NgayNhap))
            {
                var tempPhieuChiQuery = phieuChiQuery.Where(c => c.NhaCungCap.MaNhaCungCap == item.NhaCungCap.MaNhaCungCap);
                decimal traSau = tempPhieuChiQuery != null && tempPhieuChiQuery.Count() > 0 ? tempPhieuChiQuery.Sum(c => c.Amount) : 0;
                form.Items.Add(new TheoNhaCungCapItemViewModel(item, traSau));
            }

            // Them phieu tra nha cung cap
            //foreach (var item in phieuTraNhaCungCap.OrderBy(c => c.NgayXuat))
            //{
            //    form.Items.Add(new TheoNhaCungCapItemViewModel() { MaPhieu = item.MaPhieuXuat, MaNhanVien = item.CreatedBy.UserId, SoPhieu = item.SoPhieuXuat, Date = item.NgayXuat.Value, MaNhaCungCap = item.NhaCungCap.MaNhaCungCap, TenNhaCungCap = item.NhaCungCap.TenNhaCungCap, TongTien = item.TongTien * -1 });
            //}

            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.MaNhanVien == currentId).ToList();
            }

            if (form.Type == "all" || form.Type == "bygroup")
            {
                form.Items = form.Items.GroupBy(c => c.MaNhaCungCap).Select(g => new TheoNhaCungCapItemViewModel() 
                                            { TenNhaCungCap = g.FirstOrDefault().TenNhaCungCap, 
                                              TongTien = g.Sum(c => Math.Round(c.TongTien,0)), 
                                              DaTra = g.Sum(c => Math.Round(c.DaTra,0)), 
                                              TongTraSau = g.FirstOrDefault().TongTraSau }).ToList();
                //for (int i = 0; i < form.Items.Count; i++)
                //{
                //    var tempPhieuChiQuery = phieuChiQuery.Where(c => c.NhaCungCap.MaNhaCungCap == form.Items[i].MaNhaCungCap);
                //    form.Items[i].TongTraSau = tempPhieuChiQuery != null && tempPhieuChiQuery.Count() > 0 ? tempPhieuChiQuery.Sum(c => c.Amount) : 0;
                //}
            }
            else
            {
                form.Items = form.Items.Where(c => c.MaNhaCungCap == form.MaNhaCungCap).OrderBy(c => c.Date).ToList();
            }

            //ViewBag.TongTraSau = phieuChiQuery != null && phieuChiQuery.Count() > 0 ? phieuChiQuery.Sum(c => c.Amount) : 0;

            if (form.Export == 1)
            {
                return GenerateExcelTheoNhaCungCap(form);
            }

            return View(form);
        }

        private ActionResult GenerateExcelTheoNhaCungCap(TheoNhaCungCapViewModel form)
        {
            var title = "Báo cáo theo nhà cung cấp";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                // xac dinh loai bao cao
                var nhaCungCap = "";
                switch (form.Type)
                {
                    default:
                    case "all":
                        nhaCungCap = "Tổng kết hết";
                        break;
                    case "byname":
                        var item = unitOfWork.NhaCungCapRespository.GetById(form.MaNhaCungCap);
                        if (item != null)
                            nhaCungCap = item.TenNhaCungCap;
                        break;
                    case "bygroup":
                        var item1 = unitOfWork.NhomNhaCungCapRepository.GetById(form.MaNhomNhaCungCap);
                        if (item1 != null)
                            nhaCungCap = item1.TenNhomNhaCungCap;
                        break;

                }
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Nhà cung cấp: {0}", nhaCungCap);
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Ngày";
                ws.Cells[rowIndex, colIndex++].Value = "Mã số";
                ws.Cells[rowIndex, colIndex++].Value = "Tên nhà cung cấp";
                ws.Cells[rowIndex, colIndex++].Value = "VAT";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng tiền";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền trả";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền nợ";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.Date.ToString("dd/MM/yyyy");
                    ws.Cells[rowIndex, colIndex++].Value = item.SoPhieu;
                    ws.Cells[rowIndex, colIndex++].Value = item.TenNhaCungCap;
                    ws.Cells[rowIndex, colIndex++].Value = item.VAT;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.TongTien;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.DaTra;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TienNo;
                    cells.Style.Numberformat.Format = "#,##0";


                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tổng tiền:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongTien);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng trả:";
                ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 2, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.DaTra);
                cells.Style.Numberformat.Format = "#,##0";

                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Value = "Tổng nợ:";
                ws.Cells[rowIndex + 3, colIndex, rowIndex + 3, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 3, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TienNo);
                cells.Style.Numberformat.Format = "#,##0";



                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }

        [SimpleAuthorize("Admin")]
        public ActionResult TheoBacSy(TheoBacSyViewModel form)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;

            // lay ds bac sy.
            ViewBag.BacSys =
                unitOfWork.BacSyRespository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc)
                    .OrderBy(e => e.TenBacSy)
                    .AsEnumerable();

            // processing query.
            List<PhieuNhapChiTiet> listPhieuNhapChiTiet = new List<PhieuNhapChiTiet>();
            List<PhieuXuatChiTiet> listPhieuXuatChiTiet = new List<PhieuXuatChiTiet>();
            var phieuNhapChiTietQuery = unitOfWork.PhieuNhapChiTietRepository.GetMany(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.PhieuNhap.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuXuatChiTietQuery = unitOfWork.PhieuXuatChiTietRepository.GetMany(e => e.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.PhieuXuat.RecordStatusID == (byte)RecordStatus.Activated);
            var phieuKhachHangTraQuery = unitOfWork.PhieuNhapRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.RecordStatusID == (byte)RecordStatus.Activated);
            var listDuDauKy = unitOfWork.ThuocRepository.GetMany(c => c.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.GiaDauKy > 0 && c.SoDuDauKy > 0).ToList();

            // Get current user

            var currentId = 0;
            if (User.IsInRole(Constants.Security.Roles.User.Value))
            {
                currentId = WebSecurity.GetCurrentUserId;
            }

            foreach (var item in listDuDauKy)
            {
                listPhieuNhapChiTiet.Add(new PhieuNhapChiTiet()
                {
                    Thuoc = new Thuoc() { ThuocId = item.ThuocId },
                    PhieuNhap = new PhieuNhap() { NgayNhap = DateTime.MinValue, VAT = 0, LoaiXuatNhap = new LoaiXuatNhap() { TenLoaiXuatNhap = Constants.LoaiPhieuXuatNhap.NhapKho } },
                    GiaNhap = item.GiaDauKy,
                    DonViTinh = item.DonViXuatLe,
                    SoLuong = item.SoDuDauKy,
                    ChietKhau = 0
                });
            }

            listPhieuNhapChiTiet.AddRange(phieuNhapChiTietQuery.OrderBy(c => c.PhieuNhap.NgayNhap).ToList());
            var listKhachHangTra = new List<PhieuNhapChiTiet>();

            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                var fromDate = form.From.Value.Date;

                phieuXuatChiTietQuery = phieuXuatChiTietQuery.Where(e => e.PhieuXuat.NgayXuat < toDate);
                phieuKhachHangTraQuery = phieuKhachHangTraQuery.Where(e => e.NgayNhap.Value < toDate);
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang && c.PhieuNhap.NgayNhap < toDate).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }
            else
            {
                listKhachHangTra = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            }


            // Loc tat cac phieu khach hang tra & tra nha cung cap           

            var listTraNhaCC = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap || c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap == Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();
            listPhieuNhapChiTiet = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.NhapLaiTuKhachHang).OrderBy(c => c.PhieuNhap.NgayNhap).ToList();
            listPhieuXuatChiTiet = phieuXuatChiTietQuery.Where(c => c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.XuatVeNhaCungCap && c.PhieuXuat.LoaiXuatNhap.TenLoaiXuatNhap != Constants.LoaiPhieuXuatNhap.DieuChinhKiemKe).OrderBy(c => c.PhieuXuat.NgayXuat).ToList();

            // Tinh cho phieu khach hang tra
            sThuoc.Utils.Helpers.CalculatePhieuKhachHangTra(listKhachHangTra, listPhieuXuatChiTiet);

            // Tinh cho phieu xuat nha cung cap
            sThuoc.Utils.Helpers.CalculatePhieuTraNhaCungCap(listPhieuNhapChiTiet, listTraNhaCC);

            // bao cao tat ca nhan vien
            var listPhieuNhapMoiNhat = new List<PhieuNhapMoiNhat>();

            form.Items = new List<TheoBacSyItemViewModel>();
            foreach (var item in listPhieuXuatChiTiet)
            {
                form.Items.Add(new TheoBacSyItemViewModel(item, listPhieuNhapChiTiet, 0, listPhieuNhapMoiNhat));
            }

            // Bo di nhung item khong co bac sy
            form.Items = form.Items.Where(c => !string.IsNullOrEmpty(c.TenBacSy)).ToList();
            // If current user is not admin then filter data by current user
            if (currentId != 0)
            {
                form.Items = form.Items.Where(c => c.MaNhanVien == currentId).ToList();
            }

            form.Items = form.Items.GroupBy(c => c.MaPhieu).Select(f => new TheoBacSyItemViewModel
            {
                MaPhieu = f.First().MaPhieu,
                SoPhieu = f.First().SoPhieu,
                TongTien = f.First().TongTien,
                DaTra = f.First().DaTra,
                LoiNhuan = f.Sum(c => c.LoiNhuan),
                MaBacSy = f.FirstOrDefault() != null ? f.FirstOrDefault().MaBacSy : 0,
                TenBacSy = f.FirstOrDefault() != null ? f.FirstOrDefault().TenBacSy : "",
                Date = f.First().Date
            }).ToList();


            if (form.Period == "period" && form.From.HasValue && form.To.HasValue)
            {
                var toDate = form.To.Value.Date.AddDays(1);
                var fromDate = form.From.Value.Date.AddDays(-1);
                form.Items = form.Items.Where(e => e.Date < toDate && e.Date > fromDate).ToList();
                
            }

            if (form.BacSyId.HasValue && form.BacSyId.Value > 0)
            {
                form.Items = form.Items.Where(e => e.MaBacSy == form.BacSyId.Value).ToList();
            }
            else
            {
                form.Items = form.Items.GroupBy(e =>
               new
               {
                   e.MaBacSy
               }).Select(f => new TheoBacSyItemViewModel
               {
                   TongTien = f.Sum(g => g.TongTien),
                   DaTra = f.Sum(g => g.DaTra),
                   LoiNhuan = f.Sum(g => g.LoiNhuan),
                   TenBacSy = f.FirstOrDefault().TenBacSy,
               }).ToList();
            }

            if (form.Export == 1)
            {
                return GenerateExcelTheoBacsy(form);
            }
            return View(form);
        }

        private ActionResult GenerateExcelTheoBacsy(TheoBacSyViewModel form)
        {
            var title = "Báo cáo theo bác sỹ";
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Author = this.GetNhaThuoc().TenNhaThuoc;
                p.Workbook.Properties.Title = title;
                //Create a sheet
                p.Workbook.Worksheets.Add(title);
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = title; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
                int colIndex = 1;
                int rowIndex = 1;

                // set title
                ws.Cells[1, 1, 1, colIndex + 5].Merge = true;
                ws.Cells[1, 1, 1, colIndex + 5].Value = title;
                ws.Cells[1, 1, 1, colIndex + 5].Style.Font.Size = 15;

                // set input parameters
                rowIndex += 3;
                ws.Cells[rowIndex, 2, rowIndex, 5].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 5].Value = string.Format("Bác sỹ: {0}", form.BacSyId > 0 ? unitOfWork.BacSyRespository.GetById(form.BacSyId).TenBacSy : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Size = 12;
                rowIndex += 1;
                ws.Cells[rowIndex, 2, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 2, rowIndex, 8].Value = string.Format("Kỳ báo cáo: {0}", form.Period == "period" ? string.Format("Từ ngày {0:dd/MM/yyyy} đến ngày {1:dd/MM/yyyy}", form.From.Value, form.To.Value) : "Tổng kết hết");
                ws.Cells[rowIndex, 2, rowIndex, 8].Style.Font.Size = 12;

                rowIndex += 3;
                colIndex = 1;
                // columns header
                ws.Cells[rowIndex, colIndex++].Value = "STT";
                ws.Cells[rowIndex, colIndex++].Value = "Ngày";
                ws.Cells[rowIndex, colIndex++].Value = "Tên bác sỹ";
                ws.Cells[rowIndex, colIndex++].Value = "Tổng tiền";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền trả";
                ws.Cells[rowIndex, colIndex++].Value = "Tiền nợ";
                ws.Cells[rowIndex, colIndex++].Value = "Lợi nhuận";
                // format header 
                ws.Cells[rowIndex, 1, rowIndex, colIndex].Style.Font.Bold = true;
                var itemIndex = 0;
                ExcelRange cells;
                foreach (var item in form.Items)
                {
                    itemIndex++;
                    rowIndex++;
                    colIndex = 1;
                    ws.Cells[rowIndex, colIndex++].Value = itemIndex;
                    ws.Cells[rowIndex, colIndex++].Value = item.Date.ToString("dd/MM/yyyy");
                    ws.Cells[rowIndex, colIndex++].Value = item.TenBacSy;
                    cells = ws.Cells[rowIndex, colIndex++];
                    cells.Value = item.TongTien;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.DaTra;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.TienNo;
                    cells.Style.Numberformat.Format = "#,##0";
                    cells = ws.Cells[rowIndex, colIndex++]; cells.Value = item.LoiNhuan;
                    cells.Style.Numberformat.Format = "#,##0";

                }
                // set width of data columns.
                for (int i = 1; i < 7; i++)
                {
                    ws.Column(i).AutoFit();
                }
                rowIndex += 2;
                colIndex = 1;
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Value = "Tổng tiền:";
                ws.Cells[rowIndex + 1, colIndex, rowIndex + 1, colIndex + 1].Merge = true;
                cells = ws.Cells[rowIndex + 1, colIndex + 2];
                cells.Value = form.Items.Sum(e => e.TongTien);
                cells.Style.Numberformat.Format = "#,##0";

                //ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Value = "Tổng lợi nhuận:";
                //ws.Cells[rowIndex + 2, colIndex, rowIndex + 2, colIndex + 1].Merge = true;
                //cells = ws.Cells[rowIndex + 2, colIndex + 2];
                //cells.Value = form.Items.Sum(e => e.LoiNhuan);
                //cells.Style.Numberformat.Format = "#,##0";



                var fileDownloadName = "baocao-" + DateTime.Now + ".xlsx";

                var fileStream = new MemoryStream();
                p.SaveAs(fileStream);
                fileStream.Position = 0;
                var fsr = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fsr.FileDownloadName = fileDownloadName;
                return fsr;
            }
        }

        public async Task<ActionResult> DialogDetail(int? id)
        {
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            var thuoc = new Thuoc();
            //if (id.HasValue)
            //{
            //    thuoc = await
            //        unitOfWork.ThuocRepository.GetMany(e => e.NhaThuoc.MaNhaThuoc == maNhaThuoc && e.ThuocId == id)
            //            .FirstOrDefaultAsync();
            //    if (thuoc != null)
            //    {
            //        var thuocnhap = unitOfWork.PhieuNhapChiTietRepository.Get(c => c.PhieuNhap.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == thuoc.ThuocId).OrderByDescending(c => c.MaPhieuNhapCt).FirstOrDefault();
            //        var thuocxuat = unitOfWork.PhieuXuatChiTietRepository.Get(c => c.PhieuXuat.NhaThuoc.MaNhaThuoc == maNhaThuoc && c.Thuoc.ThuocId == thuoc.ThuocId).OrderByDescending(c => c.MaPhieuXuatCt).FirstOrDefault();
            //        //ViewBag.GiaNhap = GetGiaNhapHienTai(thuoc);
            //        //ViewBag.GiaXuat = GetGiaXuatHienTai(thuoc);
            //        //ViewBag.SoLuong = GetSoLuongHienTai(thuoc);
            //        ViewBag.GiaNhapDonGanNhat = thuocnhap != null ? thuocnhap.GiaNhap.ToString("#,##0") : string.Empty;
            //        ViewBag.GiaNhapXuatGanNhat = thuocxuat != null ? thuocxuat.GiaXuat.ToString("#,##0") : string.Empty;
            //        ViewBag.DonViLe = thuoc.DonViXuatLe.TenDonViTinh;
            //        ViewBag.QuyCach = string.Empty;
            //        if (thuoc.DonViThuNguyen != null)
            //        {
            //            ViewBag.QuyCach = thuoc.DonViThuNguyen.TenDonViTinh + " = " + thuoc.HeSo.ToString() + " " + thuoc.DonViXuatLe.TenDonViTinh;
            //        }
            //        ViewBag.GiaNhap = thuoc.GiaNhap.ToString("#,##0");
            //        ViewBag.GiaXuat = thuoc.GiaBanLe.ToString("#,##0");
            //        ViewBag.GiaSi = thuoc.GiaBanBuon.ToString("#,##0");
            //        ViewBag.SoLuong = GetSoLuongHienTai(thuoc);
            //        ViewBag.GioiHan = thuoc.GioiHan.ToString();
            //        ViewBag.HanDung = thuoc.HanDung.ToString();
            //    }

            //}
            return View();
        }
    }

    public class LoiNhuanByPhieuXuatChiTiet
    {
        public Thuoc Thuoc { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaTra { get; set; }
        public decimal LoiNhuan { get; set; }
    }

    public class LoiNhuanByPhieuXuat
    {
        public PhieuXuat PhieuXuat { get; set; }
        public IEnumerable<LoiNhuanByPhieuXuatChiTiet> LoiNhuanByPhieuXuatChiTiets { get; set; }
    }

    public class ByDoiTac
    {
        public string NhaCungCap { get; set; }
        public string KhachHang { get; set; }
        public IEnumerable<DateTime?> NgayThang { get; set; }
        public IEnumerable<long> SoPhieu { get; set; }
        public IEnumerable<int> CK { get; set; }
        public IEnumerable<decimal> TongTien { get; set; }
        public IEnumerable<int> TienTra { get; set; }
        public IEnumerable<decimal> TienNo { get; set; }
        public IEnumerable<decimal> LoiNhuan { get; set; }
    }

    public class ByThuoc
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public float GiaNhap { get; set; }
        public float GiaXuat { get; set; }
        public float SoLuong { get; set; }
        public float TongTien { get; set; }
        public float LoiNhuan { get; set; }
    }

    public class BaoCaoModel
    {
        public IEnumerable<PhieuNhap> PhieuNhaps { get; set; }
        public IEnumerable<PhieuXuat> PhieuXuats { get; set; }
        public IList<LoiNhuanByPhieuXuat> LoiNhuanByPhieuXuats { get; set; }
    }

    public class LoiNhuanGopModel
    {
        public decimal LoiNhuan;
        public DateTime Date;
    }
}