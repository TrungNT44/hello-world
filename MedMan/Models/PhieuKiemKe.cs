using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using sThuoc.DAL;
using WebGrease.Css.Extensions;

namespace sThuoc.Models
{
    public class PhieuKiemKe : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuKiemKe { get; set; }
        [Display(Name = "Số Phiếu")]
        public int SoPhieu { get; set; }
        [Display(Name = "Phiếu Nhập")]
        public virtual PhieuNhap PhieuNhap { get; set; }
        [Display(Name = "Phiếu Xuất")]
        public virtual PhieuXuat PhieuXuat { get; set; }
        public virtual IList<PhieuKiemKeChiTiet> PhieuKiemKeChiTiets { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public bool DaCanKho { get; set; }
        public virtual ICollection<ChiNhanh> ChiNhanhs { get; set; }
    }

    public class PhieuKiemKeEditModel
    {
        public int MaPhieuKiemKe { get; set; }
        public bool DaCanKho { get; set; }
        public DateTime NgayTao { get; set; }
        public ICollection<PhieuKiemKeItemEditModel> Items { get; set; }
        public int SoPhieu { get; set; }
        public PhieuKiemKeEditModel()
        {

        }
        public PhieuKiemKeEditModel(PhieuKiemKe phieuKiemKe)
        {
            MaPhieuKiemKe = phieuKiemKe.MaPhieuKiemKe;
            SoPhieu = phieuKiemKe.SoPhieu;
            if (phieuKiemKe.Created.HasValue)
            {
                NgayTao = phieuKiemKe.Created.Value;
            }
            Items = new List<PhieuKiemKeItemEditModel>();
            if (phieuKiemKe.PhieuKiemKeChiTiets != null && phieuKiemKe.PhieuKiemKeChiTiets.Any())
            {
                phieuKiemKe.PhieuKiemKeChiTiets.ForEach(e =>
                {
                    Items.Add(new PhieuKiemKeItemEditModel()
                    {
                        TenNhomThuoc = e.Thuoc.NhomThuoc.TenNhomThuoc,
                        MaThuoc = e.Thuoc.MaThuoc,
                        SoLuongThucTe = e.ThucTe,
                        SoLuongHeThong = e.TonKho,
                        TenThuoc = e.Thuoc.TenThuoc,
                        TenDonViTinhXuatLe = e.Thuoc.DonViXuatLe.TenDonViTinh
                    });
                });
            }

        }
    }

    public class PhieuKiemKeItemEditModel
    {
        public string TenNhomThuoc { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string TenDonViTinhXuatLe { get; set; }
        public decimal? SoLuongThucTe { get; set; }
        public decimal SoLuongHeThong { get; set; }
    }

    public class PhieuCanKhoItem
    {
        public int MaPhieu;
        public long SoPhieu;
        public string LoaiPhieu;
        public decimal SoLuong;
    }
}