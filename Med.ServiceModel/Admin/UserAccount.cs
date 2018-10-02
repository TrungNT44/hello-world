using App.Constants.Enums;
using Med.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
    
namespace Med.ServiceModel.Admin
{
    [Serializable()]
    public class UserAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public int? LanguageId { get; set; }
        public List<UserRoleId> RoleIds { get; set; }
        public Dictionary<HttpActionEnum, PermissionType> PermittedResources { get; set; }

        public bool HasRole(UserRoleId roleId)
        {
            return RoleIds.Contains(roleId);
        }
        public bool HasRoles(params UserRoleId[] roleIds)
        {
            return RoleIds.Any(roleIds.Contains);
        } 
        public bool HasPermission(HttpActionEnum resourceId)
        {
            return HasWritePermission(resourceId);
        }
        public bool HasPermission(int resourceId)
        {
            return HasPermission((HttpActionEnum)resourceId);
        }
        public bool IsSystemAdmin()
        {
            return HasRoles(UserRoleId.Super, UserRoleId.SysAdmin);
        }
        public bool HasWritePermission(HttpActionEnum resourceId)
        {
            return HasExactPermission(resourceId, PermissionType.Write);
        }


        public bool HasReadPermission(HttpActionEnum resourceId)
        {
            return HasExactPermission(resourceId, PermissionType.Read) || HasExactPermission(resourceId, PermissionType.Write);
        }

        public bool HasExactPermission(HttpActionEnum resourceId, PermissionType permission)
        {
            if (IsSystemAdmin())
            {
                return true;
            }
            
            return PermittedResources.ContainsKey(resourceId) && PermittedResources[resourceId] == permission;
        }
    }
}