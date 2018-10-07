using App.Common.MVC;
using sThuoc.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Med.Web.Areas.Production.Controllers
{
    public class DownloadFileController : BaseController
    {
        [HttpGet]
        [Authorize]
        public FileContentResult Docs(string id)
        {
            string fullPath = Server.MapPath("~/Docs") + "\\" + id;
            if (!System.IO.File.Exists(fullPath))
                return null;
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string contentType = string.Empty;
            if (fullPath.EndsWith(".pdf"))
            {
                contentType = "application/pdf";
            }
            else
            {
                contentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            }
            Response.AppendHeader("Content-Disposition", "inline; filename=" + id);
            return File(fileBytes, contentType);
        }
    }
}