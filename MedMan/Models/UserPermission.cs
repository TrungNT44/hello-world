using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace sThuoc.Models
{
    public class UserPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Title { get; set; }
        public virtual UserProfile User { get; set; }
        public virtual NhaThuoc NhaThuoc { get; set; }
        public string Controller { get; set; }        
        public string Action { get; set; }
    }
}