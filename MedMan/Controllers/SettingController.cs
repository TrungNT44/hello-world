using sThuoc.DAL;
using sThuoc.Models;
using sThuoc.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Med.Web.Extensions;
using MedMan.App_Start;
using App.Common.MVC;
using System.Globalization;
using Med.Web.Filter;

namespace Med.Web.Controllers
{
    public class SettingController : BaseController
    {
        private SecurityContext db = new SecurityContext();
        private UnitOfWork unitOfWork = new UnitOfWork();
        public ActionResult Index()
        {
            var manhathuoc = this.GetNhaThuoc().MaNhaThuoc;
            var list = unitOfWork.SettingRepository.Get(c=>c.MaNhaThuoc==manhathuoc);
            foreach(Setting setting in list)
            {
                if (setting.Value.Contains("|"))
                    setting.Value = setting.Value.Replace("|", " - ");
            }
            return View(list);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        // [Audit]
        public ActionResult Create(Setting model)
        {
            if (ModelState.IsValid)
            {
                model.MaNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
                unitOfWork.SettingRepository.Insert(model);
                unitOfWork.Save();
                return Json(new { success = true, key = model.Key, value = model.Value });
            }

            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var maNhaThuoc = this.GetNhaThuoc().MaNhaThuoc;
            Setting item = unitOfWork.SettingRepository.Get(c => c.MaNhaThuoc == maNhaThuoc && c.Id == id.Value).FirstOrDefault();
            if (item == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (item.Value.Contains("|"))
                {
                    item.TuNgay = item.Value.Split('|')[0];
                    item.DenNgay = item.Value.Split('|')[1];
                }
            }
            return View(item);
        }

        [HttpPost]
        // [Audit]
        public ActionResult Edit(Setting model)
        {
            if (ModelState.IsValid)
            {
                var msg = ValidateSettingValue(model);
                if (string.IsNullOrEmpty(msg))
                {
                    if (model.Key == Constants.Settings.CanhBaoHangLoiNhuanAm)
                    {
                        model.Value = string.Format("{0}|{1}", model.TuNgay.Trim(), model.DenNgay.Trim());
                    }
                    else
                    {
                        model.Value = model.Value.Trim();
                    }
                    unitOfWork.SettingRepository.Update(model);
                    unitOfWork.Save();
                    return Json(new { success = true, id = model.Id, key = model.Key, value = model.Value });
                }
                else
                {
                    return Json(new { success = false, message = msg });
                }
            }
            else
            {
                return Json(new { success = false, message = "Giá trị không được để trống" });
            }
        }

        private string ValidateSettingValue(Setting model)
        {
            string key = model.Key;
            string value = model.Value;
            int tmp;
            value = value.Trim().ToLower();
            var msg = "";
            switch (key)
            {
                case Constants.Settings.SoNgayKhongCoGiaoDich:
                case Constants.Settings.SoNgayHetHan:
                    if (!int.TryParse(value, out tmp))
                    {
                        msg = "Giá trị nhập vào phải là số.";
                    }
                    break;
                case Constants.Settings.TuDongTaoMaThuoc:
                    if (value != "có" && value != "không")
                    {
                        msg = "Giá trị nhập vào phải có giá trị là 'Có' hoặc 'Không'.";
                    }
                    break;
                //case Constants.Settings.TuDongKhoiTaoHanDung:
                //    if (value != "có" && value != "không")
                //    {
                //        msg = "Giá trị nhập vào phải có giá trị là 'Có' hoặc 'Không'.";
                //    }
                //    break;
                case Constants.Settings.CanhBaoHangLoiNhuanAm:
                    {
                        model.TuNgay = model.TuNgay.Trim();
                        model.DenNgay = model.DenNgay.Trim();
                        if(string.IsNullOrEmpty(model.TuNgay) || string.IsNullOrEmpty(model.DenNgay))
                        {
                            msg = "Không được để trống từ ngày/đến ngày";
                        }
                        else
                        {
                            DateTime dTuNgay = new DateTime();
                            DateTime dDenNgay = new DateTime();
                            if(!DateTime.TryParseExact(model.TuNgay,"dd/MM/yyyy",null,DateTimeStyles.None,out dTuNgay))
                            {
                                msg = "Từ ngày sai định dạng dd/MM/yyyy";
                            }
                            else
                            {
                                if(!DateTime.TryParseExact(model.DenNgay, "dd/MM/yyyy", null, DateTimeStyles.None, out dDenNgay))
                                {
                                    msg = "Đến ngày sai định dạng dd/MM/yyyy";
                                }
                                else
                                {
                                    if(dTuNgay > dDenNgay)
                                    {
                                        msg = "Từ ngày phải nhỏ hơn Đến ngày";
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return msg;
        }
    }
}