using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Med.ServiceModel.Response;

namespace Med.ServiceModel.Admin
{
    public class ResourcePermission
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public int PermissionId { get; set; }
    }
    public class RolePermissionResponse
    {       
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<ResourcePermission> PermittedResources { get; set; }
        public List<ResourcePermission> NonePermittedResources { get; set; }        
    }
}
