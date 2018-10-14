using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class PhieuNhapChiTiet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuNhapCt { get; set; }
        public virtual PhieuNhap PhieuNhap { get; set; }
        [Display(Name = "Đơn Vị Tính")]
        public virtual DonViTinh DonViTinh { get; set; }
        public virtual Thuoc Thuoc { get; set; }
        [Display(Name = "Chiết Khấu")]
        public decimal ChietKhau { get; set; }
        [Display(Name = "Giá Nhập")]
        public decimal GiaNhap { get; set; }
        [Display(Name = "Số Lượng")]
        public DateTime? HanDung { get; set; }  
        public string SoLo { get; set; }    
        public decimal SoLuong { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public decimal GiaBanLe { get; set; }
        public bool IsModified { get; set; }
        public double RetailQuantity { get; set; }
        public double RetailPrice { get; set; }
        public double RetailOutPrice { get; set; }
        public string ReduceNoteItemIds { get; set; }
        public double? ReduceQuantity { get; set; }
        public double RemainRefQuantity { get; set; }
        public int HandledStatusId { get; set; }
        public byte RecordStatusID { get; set; }

        [NotMapped]
        public string NhaThuoc_MaNhaThuoc { get; set; }
        [NotMapped]
        public Nullable<int> Thuoc_ThuocId { get; set; }
        [NotMapped]
        public Nullable<int> DonViTinh_MaDonViTinh { get; set; }
        [NotMapped]
        public PhieuNhapChiTiet Original { get; set; }
        [NotMapped]
        public bool NeedUpdate { get; set; }
    }
}