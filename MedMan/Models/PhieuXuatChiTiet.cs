using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    public class PhieuXuatChiTiet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuXuatCt { get; set; }
        public virtual PhieuXuat PhieuXuat { get; set; }
        [Display(Name = "Đơn Vị Tính"), Required]
        public virtual DonViTinh DonViTinh { get; set; }
        public virtual Thuoc Thuoc { get; set; }
        //public DateTime HanDung { get; set; }
        [Display(Name = "Chiết Khấu")]
        public decimal ChietKhau { get; set; }
        [Display(Name = "Giá Xuất"), Required]
        public decimal GiaXuat { get; set; }
        [Display(Name = "Số Lượng"), Required]
        public decimal SoLuong { get; set; }        
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public double RetailQuantity { get; set; }
        public double RetailPrice { get; set; }
        public bool IsModified { get; set; }
        public string ReduceNoteItemIds { get; set; }
        public double? ReduceQuantity { get; set; }
        public int HandledStatusId { get; set; }
        public byte RecordStatusID { get; set; }
        
        [NotMapped]
        public Nullable<int> Thuoc_ThuocId { get; set; }
        [NotMapped]
        public string NhaThuoc_MaNhaThuoc { get; set; }
        [NotMapped]
        public int DonViTinh_MaDonViTinh { get; set; }
        [NotMapped]
        public PhieuXuatChiTiet Original { get; set; }
        [NotMapped]
        public bool NeedUpdate { get; set; }
    }    
}