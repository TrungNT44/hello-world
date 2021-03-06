use WebNTDB
go

alter table NhaThuocs
ADD MaNhaThuocCha nvarchar(128);
go
update NhaThuocs set MaNhaThuocCha = MaNhaThuoc Where MaNhaThuocCha is Null;
go
alter table UserProfile
ADD Enable_NT bit default 1 
GO
alter table thuocs
add NhaThuoc_MaNhaThuocCreate nvarchar(128) null
go
update thuocs set NhaThuoc_MaNhaThuocCreate = NhaThuoc_MaNhaThuoc where NhaThuoc_MaNhaThuocCreate is null
go
CREATE TABLE [dbo].[TraoDoiHangHoa](
 [Id] [int]  IDENTITY(1,1) NOT NULL,
 [MaNhaThuoc] [nchar](10)  NULL,
 [ThuocId] [int]  NULL,
 [GiaBan] [float]  NULL,
 [ChietKhau] [float]  NULL,
 [SoLuong] [float]  NULL,
 [MaDonViTinh] [int]  NULL,
 [HanDung] [datetime]  NULL,
 [GhiChu] [nchar](1000) NULL,
 CONSTRAINT [PK_TraoDoiHangHoa] PRIMARY KEY CLUSTERED 
(
 Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
