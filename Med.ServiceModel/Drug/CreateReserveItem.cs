using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med.ServiceModel.Drug
{
    public class CreateReserveItem
    {
        public int STT { get; set; }
        public int ThuocId { get; set; }
        public string MaThuoc { get; set; }
        public string TenNhomThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DonViNguyen { get; set; }
        public string DonViLe { get; set; }
        public int SoLuongCanhBao { get; set; }
        public double TonKho { get; set; }
        public string DuTru { get; set; }
        public decimal DonGia { get; set; }
        public string DonGia_view { get; set; }
        public string ThanhTien { get; set; }
        public int HeSo { get; set; }
    }
}
