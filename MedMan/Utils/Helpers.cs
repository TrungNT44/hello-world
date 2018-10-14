using System.Text.RegularExpressions;
using MedMan.App_Start;
using sThuoc.Models;
using sThuoc.Models.ViewModels;
using sThuoc.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Med.Web.Controllers;
using Med.Common;

namespace sThuoc.Utils
{
    public static class Helpers
    {
        private static int _internalCounter = 0;
        public static string GenerateUniqueBarcodeV1()
        {
            var now = DateTime.Now;

            var days = (int)(now - new DateTime(2070, 1, 1)).TotalDays;
            var seconds = (int)(now - DateTime.Today).TotalSeconds;

            var counter = _internalCounter++ % 1000;
            var barcode = days.ToString("0000") + seconds.ToString("00000") + counter.ToString("000");

            return barcode;
        }

        public static string GenerateUniqueBarcodeV2()
        {
            var temp = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var barcode = Regex.Replace(temp, "[a-zA-Z]", string.Empty).Substring(0, 12);
            return barcode;
        }

        public static bool ConvertToDateTime(string value, ref DateTime dt)
        {
            string[] arr = null;
            value = value.Replace(" ", "");
            if (value.Contains("/"))
            {
                arr = value.Split('/');
            }
            else if (value.Contains("-"))
            {
                arr = value.Split('-');
            }

            try
            {
                if (arr != null)
                {
                    dt = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                }
                else
                {
                    dt = DateTime.FromOADate(Convert.ToDouble(value));
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public static String GetSettingValue(String manhathuoc, String key, String defaultValue, UnitOfWork uow)
        {
            try
            {
                String rst = "";
                var val = uow.SettingRepository.Get(c => c.MaNhaThuoc.Equals(manhathuoc) && c.Key.Equals(key)).Select(s => s.Value).ToList();
                if (val.Count() > 0)
                    rst = val[0].ToString();
                else
                {
                    var item4 = new Setting
                    {
                        Key = Constants.Settings.CapNhatGiaPN,
                        Value = Constants.Settings.CapNhatGiaPN_value,
                        MaNhaThuoc = manhathuoc
                    };
                    uow.SettingRepository.Insert(item4);
                    uow.Save();
                    return defaultValue;
                }
                return rst;
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }
        public static void AddDefaultSettingForAdmin(string manhathuoc, UnitOfWork unitOfWork)
        {
            var count = unitOfWork.SettingRepository.Get(c => c.MaNhaThuoc == manhathuoc).Count();
            if (count == 0)
            {
                var item1 = new Setting
                {
                    Key = Constants.Settings.SoNgayHetHan,
                    Value = Constants.Settings.SoNgayHetHan_Value,
                    MaNhaThuoc = manhathuoc
                };

                var item2 = new Setting
                {
                    Key = Constants.Settings.SoNgayKhongCoGiaoDich,
                    Value = Constants.Settings.SoNgayKhongCoGiaoDich_Value,
                    MaNhaThuoc = manhathuoc
                };
                //var item3 = new Setting
                //{
                //    Key = Constants.Settings.TuDongKhoiTaoHanDung,
                //    Value = Constants.Settings.TuDongKhoiTaoHanDung_Value,
                //    MaNhaThuoc = manhathuoc
                //};
                var item3 = new Setting
                {
                    Key = Constants.Settings.TuDongTaoMaThuoc,
                    Value = Constants.Settings.TuDongTaoMaThuoc_Value,
                    MaNhaThuoc = manhathuoc
                };
                var item4 = new Setting
                {
                    Key = Constants.Settings.CapNhatGiaPN,
                    Value = Constants.Settings.CapNhatGiaPN_value,
                    MaNhaThuoc = manhathuoc
                };
                var item5 = new Setting
                {
                    Key = Constants.Settings.TuDongTaoMaVachThuoc,
                    //Value = Constants.Settings.TuDongTaoMaVachThuoc_Value,
                    Value = "Không",
                    MaNhaThuoc = manhathuoc
                };

                var item6 = new Setting
                {
                    Key = MedSettingKey.AllowToChangeTotalAmountInDeliveryNoteKey,
                    Value = "Không",
                    MaNhaThuoc = manhathuoc
                };

                unitOfWork.SettingRepository.Insert(item1);
                unitOfWork.SettingRepository.Insert(item2);
                unitOfWork.SettingRepository.Insert(item3);
                unitOfWork.SettingRepository.Insert(item4);
                unitOfWork.SettingRepository.Insert(item5);
                unitOfWork.SettingRepository.Insert(item6);
                unitOfWork.Save();
            }
        }

        public static decimal GetLoiNhuanLoaiThuoc(List<PhieuNhapChiTiet> listNhapChiTiet, Thuoc thuoc, int dvt, decimal soluong, decimal giaxuat, decimal chietkhau, int vat, List<PhieuNhapMoiNhat> list = null)
        {
            decimal tongnhap = 0;
            decimal tongxuat = soluong * giaxuat * (1 + vat / 100) * (1 - chietkhau / 100);
            var tmp = listNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
            var lastItem = list.FirstOrDefault(c => c.ThuocId == thuoc.ThuocId);

            if (lastItem == null)
            {
                if (tmp.Any())
                {
                    var tmplast = tmp.LastOrDefault();
                    if (thuoc.DonViThuNguyen != null && tmplast.DonViTinh == thuoc.DonViThuNguyen && thuoc.HeSo > 0)
                        lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = tmplast.GiaNhap / thuoc.HeSo, VAT = tmplast.PhieuNhap.VAT, ChietKhau = tmplast.ChietKhau };
                    else
                        lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = tmplast.GiaNhap, VAT = tmplast.PhieuNhap.VAT, ChietKhau = tmplast.ChietKhau };
                }
                else
                {
                    lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = thuoc.GiaNhap, VAT = 0, ChietKhau = 0 };
                }

                list.Add(lastItem);
            }

            if (thuoc.DonViThuNguyen != null && dvt == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
            {
                soluong = soluong * thuoc.HeSo;
            }

            if (tmp.Count == 0)
            {
                tongnhap = lastItem.GiaNhap * soluong * (1 + lastItem.VAT / 100) * (1 - lastItem.ChietKhau / 100);
            }
            else
            {
                //var lastItem = new { GiaNhap = tmp.LastOrDefault().GiaNhap, VAT = tmp.LastOrDefault().PhieuNhap.VAT, ChietKhau = tmp.LastOrDefault().ChietKhau };
                foreach (var item in tmp)
                {
                    if (thuoc.DonViThuNguyen != null && item.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                    {
                        item.SoLuong = item.SoLuong * thuoc.HeSo;
                        item.GiaNhap = item.GiaNhap / thuoc.HeSo;
                        item.DonViTinh = thuoc.DonViXuatLe;
                    }

                    if (item.SoLuong > soluong)
                    {
                        item.SoLuong = item.SoLuong - soluong;
                        tongnhap += soluong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100) * (1 - item.ChietKhau / 100);
                        soluong = 0;
                        break;
                    }
                    else
                    {
                        soluong = soluong - item.SoLuong;
                        tongnhap += item.SoLuong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100) * (1 - item.ChietKhau / 100);
                        listNhapChiTiet.Remove(item);
                    }
                }

                if (soluong > 0)
                {
                    tongnhap += lastItem.GiaNhap * soluong * (1 + lastItem.VAT / 100) * (1 - lastItem.ChietKhau / 100);
                }
            }

            return tongxuat - tongnhap;
        }

        public static decimal GetLoiNhuanAm(List<PhieuNhapChiTiet> listNhapChiTiet, Thuoc thuoc, int dvt, decimal soluong, decimal giaxuat, decimal chietkhau, int vat, List<long> listPhieuNhap, List<long> listMaPhieuNhap, List<PhieuNhapMoiNhat> list = null)
        {
            decimal tongnhap = 0;
            decimal tongxuat = soluong * giaxuat * (1 + vat / 100) * (1 - chietkhau / 100);
            var tmp = listNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
            var lastItem = list.FirstOrDefault(c => c.ThuocId == thuoc.ThuocId);

            if (lastItem == null)
            {
                if (tmp.Any())
                {
                    var tmplast = tmp.LastOrDefault();
                    if (thuoc.DonViThuNguyen != null && tmplast.DonViTinh == thuoc.DonViThuNguyen && thuoc.HeSo > 0)
                        lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = tmplast.GiaNhap / thuoc.HeSo, VAT = tmplast.PhieuNhap.VAT, ChietKhau = tmplast.ChietKhau };
                    else
                        lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = tmplast.GiaNhap, VAT = tmplast.PhieuNhap.VAT, ChietKhau = tmplast.ChietKhau };
                }
                else
                {
                    lastItem = new PhieuNhapMoiNhat { ThuocId = thuoc.ThuocId, GiaNhap = thuoc.GiaNhap, VAT = 0, ChietKhau = 0 };
                }

                list.Add(lastItem);
            }

            if (thuoc.DonViThuNguyen != null && dvt == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
            {
                soluong = soluong * thuoc.HeSo;
            }

            if (tmp.Count == 0)
            {
                tongnhap = lastItem.GiaNhap * soluong * (1 + lastItem.VAT / 100) * (1 - lastItem.ChietKhau / 100);
            }
            else
            {
                foreach (var item in tmp)
                {
                    if (item.PhieuNhap.MaPhieuNhap > 0)
                    {
                        listPhieuNhap.Add(item.PhieuNhap.SoPhieuNhap);
                        listMaPhieuNhap.Add(item.PhieuNhap.MaPhieuNhap);
                    }

                    if (thuoc.DonViThuNguyen != null && item.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                    {
                        item.SoLuong = item.SoLuong * thuoc.HeSo;
                        item.GiaNhap = item.GiaNhap / thuoc.HeSo;
                        item.DonViTinh = thuoc.DonViXuatLe;
                    }

                    if (item.SoLuong > soluong)
                    {
                        item.SoLuong = item.SoLuong - soluong;
                        tongnhap += soluong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100) * (1 - item.ChietKhau / 100);
                        soluong = 0;
                        break;
                    }
                    else
                    {
                        soluong = soluong - item.SoLuong;
                        tongnhap += item.SoLuong * item.GiaNhap * (1 + item.PhieuNhap.VAT / 100) * (1 - item.ChietKhau / 100);
                        listNhapChiTiet.Remove(item);
                    }
                }

                if (soluong > 0)
                {
                    tongnhap += lastItem.GiaNhap * soluong * (1 + lastItem.VAT / 100) * (1 - lastItem.ChietKhau / 100);
                }
            }

            return tongxuat - tongnhap;
        }

        public static void CalculatePhieuKhachHangTra(List<PhieuNhapChiTiet> listPhieuNhap, List<PhieuXuatChiTiet> listPhieuXuat)
        {
            foreach (var phieunhap in listPhieuNhap)
            {
                var thuoc = phieunhap.Thuoc;
                var tmpsoluong = phieunhap.SoLuong;
                var tmp = listPhieuXuat.Where(c => c.PhieuXuat.NgayXuat < phieunhap.PhieuNhap.NgayNhap.Value.AddDays(1) && c.Thuoc.ThuocId == thuoc.ThuocId && string.IsNullOrEmpty(c.Option1)).OrderByDescending(c => c.PhieuXuat.NgayXuat).ToList();
                if (thuoc.DonViThuNguyen != null && phieunhap.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                {
                    phieunhap.SoLuong = phieunhap.SoLuong * thuoc.HeSo;
                    phieunhap.Option2 = "Edited";
                }

                foreach (var phieuxuat in tmp)
                {
                    var tmpThuoc = phieuxuat.Thuoc;
                    if (tmpThuoc.DonViThuNguyen != null && phieuxuat.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && tmpThuoc.HeSo > 0)
                    {
                        phieuxuat.SoLuong = phieuxuat.SoLuong * thuoc.HeSo;
                        phieuxuat.GiaXuat = phieuxuat.GiaXuat / thuoc.HeSo;
                        phieuxuat.DonViTinh = tmpThuoc.DonViXuatLe;
                        phieuxuat.Option2 = "Edited";
                    }

                    if (phieunhap.SoLuong > phieuxuat.SoLuong)
                    {
                        phieunhap.Option1 = phieunhap.SoLuong.ToString(CultureInfo.InvariantCulture);
                        phieunhap.SoLuong = phieunhap.SoLuong - phieuxuat.SoLuong;

                        phieuxuat.Option1 = "XL";
                        phieuxuat.Option2 = "Edited";
                    }
                    else
                    {
                        phieuxuat.Option3 = phieuxuat.SoLuong.ToString();
                        phieuxuat.SoLuong = phieuxuat.SoLuong - phieunhap.SoLuong;
                        phieuxuat.Option2 = "Edited";
                        break;
                    }

                    if (phieunhap.SoLuong == 0)
                        break;
                }
                phieunhap.SoLuong = tmpsoluong;
            }
        }
        public static void CalculatePhieuTraNhaCungCap(List<PhieuNhapChiTiet> listPhieuNhap, List<PhieuXuatChiTiet> listPhieuXuat)
        {
            foreach (var phieuxuat in listPhieuXuat)
            {
                var thuoc = phieuxuat.Thuoc;
                var tmp = listPhieuNhap.Where(c => c.PhieuNhap.NgayNhap < phieuxuat.PhieuXuat.NgayXuat.Value.AddDays(1) && c.Thuoc.ThuocId == thuoc.ThuocId).OrderByDescending(c => c.PhieuNhap.NgayNhap).ToList();
                if (thuoc.DonViThuNguyen != null && phieuxuat.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                {
                    phieuxuat.SoLuong = phieuxuat.SoLuong * thuoc.HeSo;
                    phieuxuat.DonViTinh = thuoc.DonViXuatLe;
                    phieuxuat.Option2 = "Edited";
                }

                foreach (var phieunhap in tmp)
                {
                    var tmpThuoc = phieuxuat.Thuoc;
                    if (tmpThuoc.DonViThuNguyen != null && phieunhap.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && tmpThuoc.HeSo > 0)
                    {
                        phieunhap.SoLuong = phieunhap.SoLuong * thuoc.HeSo;
                        phieunhap.GiaNhap = phieunhap.GiaNhap / thuoc.HeSo;
                        phieunhap.DonViTinh = thuoc.DonViXuatLe;
                        phieunhap.Option2 = "Edited";
                    }

                    if (phieuxuat.SoLuong >= phieunhap.SoLuong)
                    {
                        phieuxuat.Option3 = phieuxuat.SoLuong.ToString();
                        phieuxuat.SoLuong = phieuxuat.SoLuong - phieunhap.SoLuong;
                        phieuxuat.Option2 = "Edited";
                        listPhieuNhap.Remove(phieunhap);
                    }
                    else
                    {
                        phieunhap.Option1 = phieunhap.SoLuong.ToString();
                        phieunhap.SoLuong = phieunhap.SoLuong - phieuxuat.SoLuong;
                        phieunhap.Option2 = "Edited";
                        break;
                    }

                    if (phieuxuat.SoLuong == 0)
                        break;
                }
            }
        }

        public static TheoKhoHangItemViewModel CalculateKho(Thuoc thuoc, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, List<PhieuXuatChiTiet> listPhieuXuatChiTiet, DateTime? fromDate)
        {
            TheoKhoHangItemViewModel result = new TheoKhoHangItemViewModel() { MaThuoc = thuoc.MaThuoc, TenThuoc = thuoc.TenThuoc };
            if (fromDate.HasValue)
            {
                var tmpNhap = listPhieuNhapChiTiet.Where(c => c.PhieuNhap.NgayNhap < fromDate && c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
                var tmpXuat = listPhieuXuatChiTiet.Where(c => c.PhieuXuat.NgayXuat < fromDate && c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
                decimal soluongXuat = 0;
                if (tmpNhap.Count > 0)
                {
                    var lastItem = new { GiaNhap = tmpNhap.LastOrDefault().GiaNhap, VAT = tmpNhap.LastOrDefault().PhieuNhap.VAT, ChietKhau = tmpNhap.LastOrDefault().ChietKhau };
                    foreach (var xuat in tmpXuat)
                    {
                        if (thuoc.DonViThuNguyen != null && xuat.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                        {
                            xuat.SoLuong = xuat.SoLuong * thuoc.HeSo;
                        }

                        soluongXuat += xuat.SoLuong;

                        foreach (var nhap in tmpNhap)
                        {
                            if (thuoc.DonViThuNguyen != null && nhap.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                            {
                                nhap.SoLuong = nhap.SoLuong * thuoc.HeSo;
                                nhap.GiaNhap = nhap.GiaNhap / thuoc.HeSo;
                                nhap.DonViTinh = thuoc.DonViXuatLe;
                            }

                            if (nhap.SoLuong > soluongXuat)
                            {
                                nhap.SoLuong = nhap.SoLuong - soluongXuat;
                                soluongXuat = 0;
                                listPhieuXuatChiTiet.Remove(xuat);
                                break;
                            }
                            else if (nhap.SoLuong == soluongXuat)
                            {
                                nhap.SoLuong = 0;
                                soluongXuat = 0;
                                listPhieuXuatChiTiet.Remove(xuat);
                                listPhieuNhapChiTiet.Remove(nhap);
                            }
                            else
                            {
                                soluongXuat = soluongXuat - nhap.SoLuong;
                                nhap.SoLuong = 0;
                                listPhieuNhapChiTiet.Remove(nhap);
                            }
                        }
                    }

                    if (soluongXuat > 0)
                    {
                        result.TonDau = -1 * soluongXuat;
                        result.TongGiaTriTonDau = result.TonDau * lastItem.GiaNhap;
                    }
                    else
                    {
                        tmpNhap = tmpNhap.Where(c => c.SoLuong > 0).ToList();

                        foreach (var item in tmpNhap)
                        {
                            if (thuoc.DonViThuNguyen != null && item.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                            {
                                item.SoLuong = item.SoLuong * thuoc.HeSo;
                                item.GiaNhap = item.GiaNhap / thuoc.HeSo;
                            }

                            result.TonDau += item.SoLuong;
                            result.TongGiaTriTonDau += item.SoLuong * item.GiaNhap;

                            //listPhieuNhapChiTiet.Remove(item);
                        }
                    }
                }
                else
                {
                    //result.TonDau = thuoc.SoDuDauKy;
                    //result.TongGiaTriTonDau = thuoc.SoDuDauKy * thuoc.GiaDauKy;
                    //Da tru het ca ton dau ky
                    result.TonDau = 0;
                    result.TongGiaTriTonDau = 0;
                }
            }
            else
            {
                var tmpNhap = listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
                if (tmpNhap.Count > 0)
                {
                    result.TonDau = tmpNhap.FirstOrDefault().SoLuong;
                    result.TongGiaTriTonDau = tmpNhap.FirstOrDefault().SoLuong * tmpNhap.FirstOrDefault().GiaNhap;
                }
                else
                {
                    result.TonDau = 0;
                    result.TongGiaTriTonDau = 0;
                }

            }

            return CalculateTongNhapXuatTon(result, thuoc, listPhieuNhapChiTiet, listPhieuXuatChiTiet, fromDate);
        }

        public static TheoKhoHangItemViewModel CalculateTongNhapXuatTon(TheoKhoHangItemViewModel result, Thuoc thuoc, List<PhieuNhapChiTiet> listPhieuNhapChiTiet, List<PhieuXuatChiTiet> listPhieuXuatChiTiet, DateTime? fromDate)
        {
            var tmpNhap = fromDate.HasValue ? listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId && c.PhieuNhap.NgayNhap > fromDate.Value.AddDays(-1)).ToList() : listPhieuNhapChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
            var tmpXuat = fromDate.HasValue ? listPhieuXuatChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId && c.PhieuXuat.NgayXuat > fromDate.Value.AddDays(-1)).ToList() : listPhieuXuatChiTiet.Where(c => c.Thuoc.ThuocId == thuoc.ThuocId).ToList();
            decimal tmpSoLuong = 0;
            decimal tmpGiaNhap = 0;
            for (int i = 0; i < tmpNhap.Count; i++)
            {
                if (tmpNhap[i].PhieuNhap.NgayNhap != DateTime.MinValue)
                {
                    //var nhap = tmpNhap[i];
                    tmpSoLuong = tmpNhap[i].SoLuong;
                    tmpGiaNhap = tmpNhap[i].GiaNhap;
                    if (thuoc.DonViThuNguyen != null && tmpNhap[i].DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                    {
                        tmpSoLuong = tmpSoLuong * thuoc.HeSo;
                        tmpGiaNhap = tmpGiaNhap / thuoc.HeSo;
                    }

                    result.Nhap += tmpSoLuong;
                    result.TongGiaTriNhap += tmpSoLuong * tmpGiaNhap * (1 + tmpNhap[i].PhieuNhap.VAT / 100) * (1 - tmpNhap[i].ChietKhau / 100);
                }
            }

            // tinh toan tong xuat
            var soluongXuat = 0M;
            foreach (var xuat in tmpXuat)
            {
                if (thuoc.DonViThuNguyen != null && xuat.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                {
                    xuat.SoLuong = xuat.SoLuong * thuoc.HeSo;
                }

                soluongXuat += xuat.SoLuong;
                result.Xuat += xuat.SoLuong;

                if (tmpNhap.Count > 0)
                {
                    foreach (var nhap in tmpNhap)
                    {
                        if (thuoc.DonViThuNguyen != null && nhap.DonViTinh.MaDonViTinh == thuoc.DonViThuNguyen.MaDonViTinh && thuoc.HeSo > 0)
                        {
                            nhap.SoLuong = nhap.SoLuong * thuoc.HeSo;
                            nhap.GiaNhap = nhap.GiaNhap / thuoc.HeSo;
                            nhap.DonViTinh = thuoc.DonViXuatLe;
                        }

                        if (nhap.SoLuong > soluongXuat)
                        {
                            nhap.SoLuong = nhap.SoLuong - soluongXuat;
                            result.TongGiaTriXuat += soluongXuat * nhap.GiaNhap;
                            soluongXuat = 0;
                            listPhieuXuatChiTiet.Remove(xuat);
                            break;
                        }
                        else if (nhap.SoLuong == soluongXuat)
                        {
                            soluongXuat = 0;
                            result.TongGiaTriXuat += nhap.SoLuong * nhap.GiaNhap;
                            nhap.SoLuong = 0;

                            listPhieuXuatChiTiet.Remove(xuat);
                            listPhieuNhapChiTiet.Remove(nhap);
                        }
                        else
                        {
                            soluongXuat = soluongXuat - nhap.SoLuong;
                            result.TongGiaTriXuat += nhap.SoLuong * nhap.GiaNhap;
                            nhap.SoLuong = 0;

                            listPhieuNhapChiTiet.Remove(nhap);
                        }
                    }
                }
                else
                {
                    if (result.TonDau != 0)
                        result.TongGiaTriXuat += xuat.SoLuong * result.TongGiaTriTonDau / result.TonDau;
                    else
                        result.TongGiaTriXuat += xuat.SoLuong * thuoc.GiaNhap;
                }


            }

            result.TonCuoi = result.TonDau + result.Nhap - result.Xuat;
            result.TongGiaTriTonCuoi = result.TongGiaTriTonDau + result.TongGiaTriNhap - result.TongGiaTriXuat;

            return result;
        }

        public static List<TongKetKyChiTiet> GetTongKetKy(UnitOfWork _unitOfWork, string manhathuoc, List<int> listThuoc, ref DateTime dt)
        {
            List<TongKetKyChiTiet> list = new List<TongKetKyChiTiet>();
            var kytinhtoan =
                _unitOfWork.TongKetKyRepository.Get(
                    c => c.NhaThuoc.MaNhaThuoc == manhathuoc && listThuoc.Contains(c.Thuoc.ThuocId) && c.TrangThai).OrderBy(c => c.KyTinhToan).FirstOrDefault();

            DateTime dtKyTinhToan = dt;
            if (kytinhtoan != null)
            {
                dtKyTinhToan = kytinhtoan.KyTinhToan.AddMonths(-1);
                if (dt != DateTime.MinValue && dt.AddMonths(-1) < dtKyTinhToan)
                {
                    dtKyTinhToan = new DateTime(dt.Year, dt.Month - 1, 1);
                }
            }
            else
            {
                if (dt != DateTime.MinValue)
                {
                    dtKyTinhToan = new DateTime(dt.Year, dt.Month - 1, 1);
                    kytinhtoan = _unitOfWork.TongKetKyRepository.Get(c => c.NhaThuoc.MaNhaThuoc == manhathuoc && c.KyTinhToan == dtKyTinhToan).FirstOrDefault();
                    if (kytinhtoan == null)
                    {
                        return list;
                    }
                }
                else
                {

                }


                if (kytinhtoan == null) return list;
                dtKyTinhToan = kytinhtoan.KyTinhToan;

                if (dt != DateTime.MinValue && dt.AddMonths(-1) < dtKyTinhToan)
                {
                    dtKyTinhToan = new DateTime(dt.Year, dt.Month - 1, 1);
                }
            }

            list =
            _unitOfWork.TongKetKyChiTietRepository.Get(
                c =>
                    c.NhaThuoc.MaNhaThuoc == manhathuoc && listThuoc.Contains(c.Thuoc.ThuocId) &&
                    c.Date == dtKyTinhToan).ToList();

            if (list.Any()) dt = dtKyTinhToan.AddMonths(1);

            return list;
        }

        public static string RemoveEncoding(string input)
        {
            return input.Normalize();
        }

        public static string GeneratePassword(int lowercase, int uppercase, int numerics)
        {
            string lowers = "abcdefghijklmnopqrstuvwxyz";
            string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string specials = "@!%";
            string number = "0123456789";

            Random random = new Random();

            string generated = "!";
            for (int i = 1; i <= lowercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    lowers[random.Next(lowers.Length - 1)].ToString()
                );

            for (int i = 1; i <= uppercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    uppers[random.Next(uppers.Length - 1)].ToString()
                );

            for (int i = 1; i <= numerics; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    number[random.Next(number.Length - 1)].ToString()
                );

            generated = generated.Insert(
                random.Next(generated.Length),
                specials[random.Next(specials.Length - 1)].ToString());

            return generated.Replace("!", string.Empty);

        }

        public static string FormatDecimal(string s)
        {
            decimal dTmp;
            if (decimal.TryParse(s, out dTmp))
            {
                var arr = s.Split('.');
                if (arr.Length == 2)
                {
                    var tmp = arr[1];
                    if (int.Parse(tmp) > 0)
                    {
                        while (tmp.Length > 0 && tmp[tmp.Length - 1] == '0')
                        {
                            tmp = tmp.Remove(tmp.Length - 1);
                        }
                        s = arr[0] + "." + tmp;

                    }
                    else
                    {
                        s = arr[0];
                    }
                }
            }
            return s;
        }
    }
}