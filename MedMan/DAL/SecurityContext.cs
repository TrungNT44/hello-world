using System.Data.Entity;
using sThuoc.Models;

namespace sThuoc.DAL
{
    public class SecurityContext : DbContext
    {
        public SecurityContext()
            : base("SimpleSecurityConnection")
        {
            Database.SetInitializer<SecurityContext>(null);
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<OperationsToRoles> OperationsToRoles { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<NhaThuoc> NhaThuocs { get; set; }
        public DbSet<BacSy> BacSys { get; set; }
        public DbSet<DangBaoChe> DangBaoChes { get; set; }
        public DbSet<DonViTinh> DonViTinhs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<LoaiXuatNhap> LoaiXuatNhaps { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<NhomKhachHang> NhomKhachHangs { get; set; }
        public DbSet<NhomNhaCungCap> NhomNhaCungCaps { get; set; }
        public DbSet<NhomThuoc> NhomThuocs { get; set; }
        public DbSet<Nuoc> Nuocs { get; set; }
        //public DbSet<TinhThanhs> TinhThanhs { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<PhieuNhapChiTiet> PhieuNhapChiTiets { get; set; }
        public DbSet<PhieuXuat> PhieuXuats { get; set; }
        public DbSet<PhieuXuatChiTiet> PhieuXuatChiTiets { get; set; }
        public DbSet<PhieuKiemKe> PhieuKiemKes { get; set; }
        public DbSet<Thuoc> Thuocs { get; set; }
        public DbSet<PhieuThuChi> PhieuThuChis { get; set; }
        public DbSet<ChiNhanh> ChiNhanhs { get; set; }
        public DbSet<ThuocChiNhanh> ThuocChiNhanhs { get; set; }
        public DbSet<LoaiThuChi> LoaiThuChis { get; set; }
        public DbSet<NhaThuocThamSo> NhaThuocThamSos { get; set; }
        public DbSet<ThamSoNguoiDung> ThamSoNguoiDungs { get; set; }
        public DbSet<ThamSoNhaThuoc> ThamSoNhaThuocs { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<TongKetKy> TongKetKies { get; set; }
        public DbSet<TongKetKyChiTiet> TongKetKyChiTiets { get; set; }
        public DbSet<DrugMapping> DrugMappings { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperationsToRoles>().HasKey(e => new { e.UserId, e.FunctionId, e.RoleName, e.MaNhaThuoc });
            // Nha thuoc
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.BacSys)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            //modelBuilder.Entity<NhaThuoc>()
            //    .HasMany(a => a.Nuocs)
            //    .WithRequired(x => x.NhaThuoc)
            //    .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany<Thuoc>(t => t.Thuocs)
                .WithRequired(t => t.NhaThuoc);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany<Thuoc>(t => t.Thuocs_Sua)
                .WithRequired(t => t.NhaThuocCreate)
                .HasForeignKey(t=> t.NhaThuoc_MaNhaThuocCreate);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.NhomNhaCungCaps)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhomNhaCungCap>()
                .HasMany(a => a.NhaCungCaps)
                .WithRequired(x => x.NhomNhaCungCap)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.NhomKhachHangs)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhomKhachHang>()
                .HasMany(a => a.KhachHangs)
                .WithRequired(x => x.NhomKhachHang)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.DangBaoChes)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.DonViTinhs)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.Nhanviens)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.NhomThuocs)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhomThuoc>()
                .HasMany(a => a.Thuocs)
                .WithRequired(x => x.NhomThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.PhieuNhaps)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<PhieuNhap>()
                .HasMany(a => a.PhieuNhapChiTiets)
                .WithRequired(x => x.PhieuNhap)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.PhieuKiemKes)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasOptional(a => a.NhaThuocCha)
                .WithMany()
                .HasForeignKey(m => m.MaNhaThuocCha);
            //modelBuilder.Entity<PhieuKiemKe>()
            //    .HasMany(a => a.PhieuKiemKeChiTiets)
            //    .WithRequired(x => x.PhieuKiemKe)
            //    .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.PhieuThuChis)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.PhieuXuats)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            //modelBuilder.Entity<PhieuXuatChiTiet>()
            //    .HasRequired(x => x.PhieuXuat)
            //    .WithMany(a => a.PhieuXuatChiTiets)
                //.WillCascadeOnDelete(true);
            modelBuilder.Entity<NhaThuoc>()
                .HasMany(a => a.UserPermissions)
                .WithRequired(x => x.NhaThuoc)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<PhieuKiemKe>()
                .HasMany(a => a.PhieuKiemKeChiTiets)
                .WithRequired(x => x.PhieuKiemKe)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<UserProfile>()
                .HasMany(a => a.UserPermissions)
                .WithRequired(x => x.User)
                .WillCascadeOnDelete(true);
            
        }


    }

}
