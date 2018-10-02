using System.Collections.Generic;
using Med.Entity.Registration;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Admin;

namespace Med.Service.Admin
{
    public interface IAdminService
    {
        List<Med.Entity.Admin.Role> GetAllRoles(bool onlyRolesForDrugStore = true);
        bool PushRemainResourcesToDB();
        RolePermissionResponse LoadRoleActions(int roleId, int? drugStoreId);
        bool AddRoleAction(int roleId, int resourceId, int permissionId, int? drugStoreId);
        bool UpdatePermission(int roleId, int resourceId, int permissionId, int? drugStoreId);
        bool RemoveRoleAction(int roleId, int resourceId, int? drugStoreId);
    }
}
