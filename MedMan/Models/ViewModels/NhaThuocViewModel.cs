using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MedMan.App_Start;

namespace sThuoc.Models.ViewModels
{
	public class NhaThuocViewModel
	{
        [Key]
        [Display(Name = "Mã nhà thuốc")]
        public string MaNhaThuoc { get; set; }
        [Display(Name = "Nhà thuốc q.lý")]
        public string MaNhaThuocCha { get; set; }
        public String TenNhaThuocQuanLy { set; get; }
        [Display(Name = "Tên nhà thuốc"), Required]
        public string TenNhaThuoc { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }
        [Display(Name = "Số kinh doanh")]        
        public string SoKinhDoanh { get; set; }
        [Display(Name = "Điện thoại")]
        public string DienThoai { get; set; }
        [Display(Name = "Người đại diện")]
        public string NguoiDaiDien { get; set; }
        [Display(Name = "Hoạt động")]
        public bool HoatDong { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Di động")]
        public string Mobile { get; set; }
        [Display(Name = "Dược sỹ")]
        public string DuocSy { get; set; }
        [Required]
        [Display(Name = "Tài khoản q.lý")]
        public Int32 Administrator { get; set; }
        [Display(Name ="Tỉnh thành")]
        public int? TinhThanhId { get; set; }
        public string AdminName { get; set; }
        public string AdminUsername { get; set; }
        public DateTime Modified { get; set; }
	    public NhaThuocViewModel()
	    {
	        
	    }
	    public NhaThuocViewModel(NhaThuoc nhaThuoc)
	    {
	        MaNhaThuoc = nhaThuoc.MaNhaThuoc;
            MaNhaThuocCha = nhaThuoc.MaNhaThuocCha;
	        TenNhaThuoc = nhaThuoc.TenNhaThuoc;
	        DiaChi = nhaThuoc.DiaChi;
	        SoKinhDoanh = nhaThuoc.SoKinhDoanh;
	        DienThoai = nhaThuoc.DienThoai;
	        NguoiDaiDien = nhaThuoc.NguoiDaiDien;
	        Email = nhaThuoc.Email;
            TinhThanhId = nhaThuoc.TinhThanhId;
	        Mobile = nhaThuoc.Mobile;
	        DuocSy = nhaThuoc.DuocSy;
	        HoatDong = nhaThuoc.HoatDong;
	        Modified = nhaThuoc.Modified.HasValue ? nhaThuoc.Modified.Value : nhaThuoc.Created.HasValue?nhaThuoc.Created.Value:DateTime.MinValue;
	        if (nhaThuoc.Nhanviens.Any(c => c.Role == Constants.Security.Roles.Admin.Value))
	        {
	            var quanly = nhaThuoc.Nhanviens.First(c => c.Role == Constants.Security.Roles.Admin.Value);
	            Administrator = quanly.User.UserId;
	            AdminName = quanly.User.TenDayDu;
	            AdminUsername = quanly.User.UserName;
	        }
            else
            {
                Administrator = 0;
            }
            if (!String.IsNullOrEmpty(MaNhaThuocCha))
            {
                TenNhaThuocQuanLy = nhaThuoc.NhaThuocCha.TenNhaThuoc;
            }
            else
            {
                MaNhaThuocCha = null;
            }
	    }

	    public NhaThuoc ToDomainModel(NhaThuoc nhathuoc = null)
	    {
            if(nhathuoc== null)
	        return new NhaThuoc()
	        {
	            MaNhaThuoc = MaNhaThuoc,
                DiaChi = DiaChi,
                DienThoai = DienThoai,
                NguoiDaiDien = NguoiDaiDien,
                Email = Email,
                Mobile = Mobile,
                TinhThanhId = TinhThanhId,
                SoKinhDoanh = SoKinhDoanh,
                DuocSy = DuocSy,
                TenNhaThuoc = TenNhaThuoc,
                //HoatDong =  true,//Mặc định khi tạo mới là hoạt động
                MaNhaThuocCha = (String.IsNullOrEmpty(MaNhaThuocCha)) ? MaNhaThuoc : MaNhaThuocCha
            };
            else
            {
                //nhathuoc.MaNhaThuoc = MaNhaThuoc;
                nhathuoc.DiaChi = DiaChi;
                nhathuoc.DienThoai = DienThoai;
                nhathuoc.NguoiDaiDien = NguoiDaiDien;
                nhathuoc.Email = Email;
                nhathuoc.Mobile = Mobile;
                nhathuoc.TinhThanhId = TinhThanhId;
                nhathuoc.SoKinhDoanh = SoKinhDoanh;
                nhathuoc.DuocSy = DuocSy;
                nhathuoc.TenNhaThuoc = TenNhaThuoc;
                //HoatDong =  true,//Mặc định khi tạo mới là hoạt động
                //nhathuoc.MaNhaThuocCha = (String.IsNullOrEmpty(MaNhaThuocCha)) ? MaNhaThuoc : MaNhaThuocCha;
            }
            return nhathuoc;
	    }
	}

    public class NhaThuocSessionModel
    {
        public string MaNhaThuoc { get; set; }
        public string MaNhaThuocCha { set; get; }
        public string TenNhaThuoc { get; set; }
        public int? TinhThanhId { set; get; }
        public string DiaChi { get; set; }
        
        public string SoKinhDoanh { get; set; }
        
        public string DienThoai { get; set; }
        
        public string NguoiDaiDien { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        
        public string Mobile { get; set; }
        
        public string DuocSy { get; set; }
        public string Role { get; set; }
        public int? DrugStoreID { get; set; }

        public NhaThuocSessionModel()
	    {
	        
	    }
        public NhaThuocSessionModel(NhaThuoc nhaThuoc)
	    {
	        MaNhaThuoc = nhaThuoc.MaNhaThuoc;
	        MaNhaThuocCha = nhaThuoc.MaNhaThuocCha;
            TenNhaThuoc = nhaThuoc.TenNhaThuoc;
	        DiaChi = nhaThuoc.DiaChi;
	        SoKinhDoanh = nhaThuoc.SoKinhDoanh;
	        DienThoai = nhaThuoc.DienThoai;
	        NguoiDaiDien = nhaThuoc.NguoiDaiDien;
	        Email = nhaThuoc.Email;
	        Mobile = nhaThuoc.Mobile;
	        DuocSy = nhaThuoc.DuocSy;
            TinhThanhId = nhaThuoc.TinhThanhId;
            DrugStoreID = nhaThuoc.ID;
	    }

	    public NhaThuoc ToDomainModel()
	    {
	        return new NhaThuoc()
	        {
                MaNhaThuocCha = MaNhaThuocCha,
	            MaNhaThuoc = MaNhaThuoc,
                DiaChi = DiaChi,
                DienThoai = DienThoai,
                NguoiDaiDien = NguoiDaiDien,
                TinhThanhId = TinhThanhId,
                Email = Email,
                Mobile = Mobile,
                SoKinhDoanh = SoKinhDoanh,
                TenNhaThuoc = TenNhaThuoc,
                ID = DrugStoreID
	        };
	    }
    }
}