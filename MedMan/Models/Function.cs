using System.ComponentModel.DataAnnotations;

namespace sThuoc.Models
{
    public class Function
    {
        [Key]        
        public int MaFunction { get; set; }
        [Display(Name = "Tên Chức Năng")]
        public string Name { get; set; }
        [Display(Name = "Tên Thao Tác")]
        public Operations Operations { get; set; }
    }
}
