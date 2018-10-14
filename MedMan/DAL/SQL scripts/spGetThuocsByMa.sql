USE [aspnet-SeedSimple-20130125152905]
GO
/****** Object:  StoredProcedure [dbo].[GetThuocsByMa]    Script Date: 19/03/2015 12:59:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetThuocsByMa] 
	-- Add the parameters for the stored procedure here
	@Term as nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT th.[ThuocId] as [value], th.[TenThuoc] as [label], th.ThongTin + ' ' +bc.TenDangBaoChe as [desc],
	ISNULL((Select SUM(SoLuong) FROM PhieuNhapChiTiets pn RIGHT JOIN PhieuNhaps pn2 ON pn2.MaPhieuNhap = pn.MaPhieuNhap WHERE pn.ThuocId = th.ThuocId AND pn2.Xoa = 0) -(Select SUM(SoLuong) FROM PhieuXuatChiTiets px RIGHT JOIN PhieuXuats px2 ON px2.MaPhieuXuat = px.MaPhieuXuat WHERE px.ThuocId = th.ThuocId AND px2.Xoa = 0),0) as soLuong, 
	th.[MaThuoc] as maThuoc,th.HeSo as heSo, th.MaDonViXuat as donViXuat, th.MaDonViThuNguyen as donViTN, th.GiaBanLe as giaBan,th.GiaBanBuon as giaBuon,th.GiaNhap as giaNhap
	FROM Thuocs th 	
	INNER JOIN DangBaoChes bc ON th.MaDangBaoChe = bc.MaDangBaoChe	
	WHERE th.[MaThuoc] like '%' + @Term +'%'
	GROUP BY th.ThuocId,th.TenThuoc,bc.TenDangBaoChe,th.ThongTin,th.[MaThuoc],th.HeSo,th.MaDonViXuat,th.MaDonViThuNguyen,th.GiaBanLe,th.GiaBanBuon,th.GiaNhap;	
END
