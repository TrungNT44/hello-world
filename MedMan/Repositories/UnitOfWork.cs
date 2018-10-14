using System;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class UnitOfWork  : IDisposable
    {
        private  SecurityContext _context = new SecurityContext();
        private  UserProfileRepository _userProfileRepository;
        private  MembershipRepository _membershipRepository;
        private  FunctionRepository _functionRepository;
        private  OperationsToRolesRepository _operationsToRolesRepository;
        private  NhaThuocRepository _nhaThuocRepository;
        private  NhanVienNhaThuocRespository _nhanVienNhaThuocRespository;
        private  UserPermissionsRespository _userPermissionsRespository;
        private  NhomKhachHangRepository _nhomKhachHangRepository;
        private  KhachHangRepository _khachHangRepository;
        private  NhomNhaCungCapRepository _nhomNhaCungCapRepository;
        private  NhaCungCapRespository _nhaCungCapRespository;
        private  BacSyRespository _bacSyRespository;
        private  NhomThuocRepository _nhomThuocRepository;
        private  DonViTinhRepository _donViTinhRepository;
        private  DangBaoCheRepository _dangBaoCheRepository;
        private  NuocRepository _nuocRepository;
        private  ThuocRepository _thuocRepository;
        private  LoaiXuatNhapRepository _loaiXuatNhapRepository;
        private  PhieuNhapRepository _phieuNhapRepository;
        private  PhieuNhapChiTietRespository _phieuNhapChiTietRespository;
        private  PhieuXuatRepository _phieuXuatRepository;
        private  PhieuXuatChiTietRepository _phieuXuatChiTietRepository;
        private  PhieuThuChiRepository _phieuThuChiRepository;
        private  PhieuKiemKeRepository _phieuKiemKeRepository;
        private PhieuKiemKeChiTietRepository _phieuKiemKeChiTietRepository;
        private SettingRepository _settingRepository;
        private TongKetKyRepository _tongKetKyRepository;
        private TongKetKyChiTietRepository _tongKetKyChiTietRepository;
        private DrugMappingRepository _thuocMappingRepository;
        public UnitOfWork()
        {
            //_userProfileRepository = new UserProfileRepository(_context);
            //_membershipRepository = new MembershipRepository(_context);
            //_operationsToRolesRepository = new OperationsToRolesRepository(_context);
            //_functionRepository = new FunctionRepository(_context);
            //_nhaThuocRepository= new NhaThuocRepository(_context);
            //_nhanVienNhaThuocRespository = new NhanVienNhaThuocRespository(_context);
            //_userPermissionsRespository = new UserPermissionsRespository(_context);
            //_nhomKhachHangRepository = new NhomKhachHangRepository(_context);
            //_khachHangRepository = new KhachHangRepository(_context);
            //_nhomNhaCungCapRepository = new NhomNhaCungCapRepository(_context);
            //_nhaCungCapRespository = new NhaCungCapRespository(_context);
            //_bacSyRespository = new BacSyRespository(_context);
            //_nhomThuocRepository = new NhomThuocRepository(_context);
            //_donViTinhRepository = new DonViTinhRepository(_context);
            //_dangBaoCheRepository = new DangBaoCheRepository(_context);
            //_nuocRepository = new NuocRepository(_context);
            //_thuocRepository = new ThuocRepository(_context);
            //_loaiXuatNhapRepository = new LoaiXuatNhapRepository(_context);
            //_phieuNhapRepository = new PhieuNhapRepository(_context);
            //_phieuNhapChiTietRespository = new PhieuNhapChiTietRespository(_context);
            //_phieuXuatRepository = new PhieuXuatRepository(_context);
            //_phieuXuatChiTietRepository = new PhieuXuatChiTietRepository(_context);
            //_phieuThuChiRepository = new PhieuThuChiRepository(_context);
            

        }
        public DrugMappingRepository ThuocMappingRepository
        {
            get
            {
                if (_thuocMappingRepository == null)
                {
                    _thuocMappingRepository = new DrugMappingRepository(_context);
                }
                return _thuocMappingRepository;
            }
        }
        public UnitOfWork(SecurityContext context)
        {
            context.Database.CommandTimeout = Int32.MaxValue;
            this._context = context;
        }

        public PhieuKiemKeRepository PhieuKiemKeRepository
        {
            get
            {
                if (_phieuKiemKeRepository==null)
                {
                    _phieuKiemKeRepository = new PhieuKiemKeRepository(_context);
                }
                return _phieuKiemKeRepository;
            }
        }
        public PhieuKiemKeChiTietRepository PhieuKiemKeChiTietRepository
        {
            get
            {
                if (_phieuKiemKeChiTietRepository == null)
                {
                    _phieuKiemKeChiTietRepository = new PhieuKiemKeChiTietRepository(_context);
                }
                return _phieuKiemKeChiTietRepository;
            }
        }

        public PhieuThuChiRepository PhieuThuChiRepository
        {
            get
            {
                if(_phieuThuChiRepository==null)
                    _phieuThuChiRepository = new PhieuThuChiRepository(_context);
                return _phieuThuChiRepository;
            }
        }

        public PhieuXuatChiTietRepository PhieuXuatChiTietRepository
        {
            get
            {
                if(_phieuXuatChiTietRepository==null)
                    _phieuXuatChiTietRepository = new PhieuXuatChiTietRepository(_context);
                return _phieuXuatChiTietRepository;
            }
        }

        public PhieuXuatRepository PhieuXuatRepository
        {
            get
            {
                if(_phieuXuatRepository==null)
                    _phieuXuatRepository = new PhieuXuatRepository(_context);
                return _phieuXuatRepository;
            }
        }

        public PhieuNhapRepository PhieuNhapRepository
        {
            get
            {
                if(_phieuNhapRepository==null)
                    _phieuNhapRepository = new PhieuNhapRepository(_context);
                return _phieuNhapRepository;
            }
        }

        public PhieuNhapChiTietRespository PhieuNhapChiTietRepository
        {
            get
            {
                if(_phieuNhapChiTietRespository==null)
                    _phieuNhapChiTietRespository=new PhieuNhapChiTietRespository(_context);
                return _phieuNhapChiTietRespository;
            }
        }

        public LoaiXuatNhapRepository LoaiXuatNhapRepository
        {
            get
            {
                if(_loaiXuatNhapRepository==null)
                    _loaiXuatNhapRepository= new LoaiXuatNhapRepository(_context);
                return _loaiXuatNhapRepository;
            }
        }

        public ThuocRepository ThuocRepository
        {
            get
            {
                if(_thuocRepository==null)
                    _thuocRepository = new ThuocRepository(_context);
                return _thuocRepository;
            }
        }

        public NuocRepository NuocRepository
        {
            get
            {
                if(_nuocRepository==null)
                    _nuocRepository=new NuocRepository(_context);
                return _nuocRepository;
            }
        }

        public DangBaoCheRepository DangBaoCheRepository
        {
            get
            {
                if(_dangBaoCheRepository==null)
                    _dangBaoCheRepository= new DangBaoCheRepository(_context);
                return _dangBaoCheRepository;
            }
        }

        public DonViTinhRepository DonViTinhRepository
        {
            get
            {
                if(_donViTinhRepository==null)
                    _donViTinhRepository=new DonViTinhRepository(_context);
                return _donViTinhRepository;
            }
        }

        public NhomThuocRepository NhomThuocRepository
        {
            get
            {
                if(_nhomThuocRepository==null)
                    _nhomThuocRepository = new NhomThuocRepository(_context);
                return _nhomThuocRepository;
            }
        }

        public BacSyRespository BacSyRespository
        {
            get
            {
                if(_bacSyRespository==null)
                    _bacSyRespository=new BacSyRespository(_context);
                return _bacSyRespository;
            }
        }

        public NhaCungCapRespository NhaCungCapRespository
        {
            get
            {
                if(_nhaCungCapRespository==null)
                    _nhaCungCapRespository=new NhaCungCapRespository(_context);
                return _nhaCungCapRespository;
            }
        }

        public NhomNhaCungCapRepository NhomNhaCungCapRepository
        {
            get
            {
                if(_nhomNhaCungCapRepository==null)
                    _nhomNhaCungCapRepository=new NhomNhaCungCapRepository(_context);
                return _nhomNhaCungCapRepository;
            }
        }

        public KhachHangRepository KhachHangRepository
        {
            get
            {
                if(_khachHangRepository==null)
                    _khachHangRepository=new KhachHangRepository(_context);
                return _khachHangRepository;
            }
        }

        public NhomKhachHangRepository NhomKhachHangRepository
        {
            get
            {
                if(_nhomKhachHangRepository==null)
                    _nhomKhachHangRepository=new NhomKhachHangRepository(_context);
                return _nhomKhachHangRepository;
            }
        }
        public UserPermissionsRespository UserPermissionsRespository
        {
            get
            {
                if(_userPermissionsRespository==null)
                    _userPermissionsRespository = new UserPermissionsRespository(_context);
                return _userPermissionsRespository;
            }
        }
        public NhanVienNhaThuocRespository NhanVienNhaThuocRespository
        {
            get
            {
                if(_nhanVienNhaThuocRespository==null)
                    _nhanVienNhaThuocRespository = new NhanVienNhaThuocRespository(_context);
                return _nhanVienNhaThuocRespository;
            }
        }
        public UserProfileRepository UserProfileRepository
        {
            get
            {
                if(_userProfileRepository==null)
                    _userProfileRepository=new UserProfileRepository(_context);
                return _userProfileRepository;
            }
        }

        public MembershipRepository MembershipRepository
        {
            get
            {
                if(_membershipRepository==null)
                    _membershipRepository=new MembershipRepository(_context);
                return _membershipRepository;
            }
        }

        public FunctionRepository FunctionRepository
        {
            get
            {
                if(_functionRepository==null)
                    _functionRepository=new FunctionRepository(_context);
                return _functionRepository;
            }
        }

         public OperationsToRolesRepository OperationsToRolesRepository
        {
             get
             {
                 if(_operationsToRolesRepository==null)
                     _operationsToRolesRepository=new OperationsToRolesRepository(_context);
                 return _operationsToRolesRepository;
             }
        }

        public NhaThuocRepository NhaThuocRepository
        {
            get
            {
                if(_nhaThuocRepository==null)
                    _nhaThuocRepository=new NhaThuocRepository(_context);
                return _nhaThuocRepository;
            }
        }

        public SettingRepository SettingRepository
        {
            get
            {
                if (_settingRepository == null)
                    _settingRepository = new SettingRepository(_context);
                return _settingRepository;
            }
        }
        public TongKetKyRepository TongKetKyRepository
        {
            get
            {
                if (_tongKetKyRepository == null)
                    _tongKetKyRepository = new TongKetKyRepository(_context);
                return _tongKetKyRepository;
            }
        }
        public TongKetKyChiTietRepository TongKetKyChiTietRepository
        {
            get
            {
                if (_tongKetKyChiTietRepository == null)
                    _tongKetKyChiTietRepository = new TongKetKyChiTietRepository(_context);
                return _tongKetKyChiTietRepository;
            }
        }

        public int Save()
         {
            return _context.SaveChanges();
         }

        public SecurityContext Context
        {
            
            get{
                return _context;
            }
        }

       private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
   
}
