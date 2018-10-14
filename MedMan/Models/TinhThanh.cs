using sThuoc.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    public class TinhThanhs : BaseModel
    {
        [Key]
        public int IdTinhThanh { set; get; }
        public string MaTinhThanh { set; get; }
        public string TenTinhThanh { set; get; }
    }
}