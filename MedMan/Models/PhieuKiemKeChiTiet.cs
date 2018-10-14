using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sThuoc.Models
{
    public class PhieuKiemKeChiTiet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhieuKiemKeCt { get; set; }
        public virtual Thuoc Thuoc { get; set; }
        [Display(Name = "Tồn Kho")]
        public decimal TonKho { get; set; }
        [Display(Name = "Thực Tế")]
        public decimal? ThucTe { get; set; }
        public virtual PhieuKiemKe PhieuKiemKe { get; set; }
        
    }
}