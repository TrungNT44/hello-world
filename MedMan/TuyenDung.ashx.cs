using App.Common.DI;
using Med.Entity;
using Med.Service.Recruitment;
using Med.ServiceModel.Recruitment;
using Med.ServiceModel.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;

namespace Med.Web
{
    /// <summary>
    /// Summary description for TuyenDung
    /// </summary>
    public class TuyenDung : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string sMethod = context.Request.Params["method"];
            var service = IoC.Container.Resolve<IRecruitService>();
            string res = "";
            switch (sMethod)
            {
                case "GetListActive":
                    {
                        string page = context.Request.Params["page"];
                        string pageSize = context.Request.Params["page_size"];
                        string tieude = context.Request.Params["tieude"];
                        string tinhthanh = context.Request.Params["tinhthanh"];
                        int? itinhthanh = null;
                        if (!string.IsNullOrEmpty(tinhthanh))
                        {
                            itinhthanh = int.Parse(tinhthanh.Trim());
                        }
                        if (string.IsNullOrEmpty(page))
                            page = "0";
                        if (string.IsNullOrEmpty(pageSize))
                            pageSize = "20";
                        var data = service.GetListRecruitActive(tieude, itinhthanh, page, pageSize);
                        res = JsonConvert.SerializeObject(data);
                    }
                    break;
                case "GetTinhThanh":
                    {
                        var data = service.GetListProvinces();
                        res = JsonConvert.SerializeObject(data);
                    }
                    break;
                case "GetRecruitInfo":
                    {
                        int recruitId = 0;
                        string recruitIdString = context.Request.Params["recruitId"];
                        int.TryParse(recruitIdString, out recruitId);
                        var data = service.GetRecruitInfo(recruitId);
                        res = JsonConvert.SerializeObject(data);
                    }
                    break;
            }
            context.Response.AddHeader("Access-Control-Allow-Origin", @"http://webnhathuoc.com");
            context.Response.ContentType = "text/plain";
            context.Response.Write(res);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}