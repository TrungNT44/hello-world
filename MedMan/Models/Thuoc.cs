using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sThuoc.DAL;
using System;
using App.Constants.Enums;

namespace sThuoc.Models
{
    public class Thuoc : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ThuocId { get; set; }
        [Display(Name = "Mã thuốc"), Required(ErrorMessage = "Mã thuốc không được bỏ trống")]
        public string MaThuoc { get; set; }
        [Display(Name = "Tên thuốc"), Required(ErrorMessage = "Tên thuốc không được bỏ trống")]
        public string TenThuoc { get; set; }
        [Display(Name = "Thông tin")]
        public string ThongTin { get; set; }
        [Display(Name = "Tên đầy đủ")]
        public string TenDayDu { get { return TenThuoc + " " + ThongTin; } }
        [Display(Name = "Hệ số"), Required(ErrorMessage = "hệ số không được bỏ trống")]
        public int HeSo { get; set; }

        [Display(Name = "Quy cách")]
        public string QuyCach
        {
            get
            {
                if (DonViThuNguyen != null && DonViThuNguyen.MaDonViTinh != DonViXuatLe.MaDonViTinh)
                {
                    return DonViThuNguyen.TenDonViTinh + " " + HeSo + " " + DonViXuatLe.TenDonViTinh;
                }
                return DonViXuatLe.TenDonViTinh;
            }
        }
        [Display(Name = "Giá nhập lẻ"), Required(ErrorMessage = "Giá nhập không được bỏ trống")]
        public decimal GiaNhap { get; set; }
        [Display(Name = "Giá bán sỉ"), Required(ErrorMessage = "Gái bán buôn không được bỏ trống")]
        public decimal GiaBanBuon { get; set; }
        [Display(Name = "Giá bán lẻ"), Required(ErrorMessage = "Gái bán lẻ không được bỏ trống")]
        public decimal GiaBanLe { get; set; }
        [Display(Name = "Số dư đầu kỳ")]
        public decimal SoDuDauKy { get; set; }
        [Display(Name = "Giá đầu kỳ")]
        public decimal GiaDauKy { get; set; }
        [Display(Name = "Giới hạn tồn")]
        public int? GioiHan { get; set; }
        [Display(Name = "Mã vạch")]
        public string BarCode { get; set; }
        [Display(Name = "Hoạt động")]
        [NotMapped]
        public bool HoatDong { get { return RecordStatusID == (byte)RecordStatus.Activated; } }
        [Display(Name = "Hàng tư vấn")]
        public bool HangTuVan { get; set; }
        [Display(Name = "Hạn dùng")]
        public DateTime? HanDung { get; set; }
        [Display(Name = "Dự trù")]
        public int DuTru { get; set; }
        public decimal PreFactors { get; set; }
        public int? PreRetailUnitID { get; set; }
        public int? PreUnitID { get; set; }
        public decimal PreInitQuantity { get; set; }
        public decimal PreInitPrice { get; set; }
        public DateTime? PreExpiredDate { get; set; }
        public string NhaThuoc_MaNhaThuocCreate { set; get; }
        public byte RecordStatusID { get; set; }
        public virtual NhaThuoc NhaThuocCreate { set; get; }
        public virtual DangBaoChe DangBaoChe { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public virtual DonViTinh DonViThuNguyen { get; set; }
        public virtual DonViTinh DonViXuatLe { get; set; }
        public virtual Nuoc Nuoc { get; set; }
        public virtual NhomThuoc NhomThuoc { get; set; }
        public virtual ICollection<PhieuNhapChiTiet> PhieuNhapChiTiets { get; set; }
        public virtual ICollection<PhieuXuatChiTiet> PhieuXuatChiTiets { get; set; }
        public virtual ICollection<PhieuKiemKeChiTiet> PhieuKiemKeChiTiets { get; set; }
    }

    public class InMaVachModel
    {
        public DateTime NgayTao { get; set; }
        public IList<InMaVachItemModel> Items { get; set; }
        public string ExtraPrintInfo { get; set; }
    }

    public class InMaVachItemModel
    {
        public int ThuocId { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal SoLuongTem { get; set; }
        public decimal Gia { get; set; }
        public string DonViXuatLe { get; set; }
        public string MaPhieuDaTonTai { get; set; }
        public string MaVach { get; set; }
    }
}