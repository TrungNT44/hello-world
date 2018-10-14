using sThuoc.Models;
namespace sThuoc.Models
{
    public class OperationsToRoles
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public int FunctionId { get; set; }
        public Operations Operations { get; set; }
        public string MaNhaThuoc {get; set;}
    }
}
