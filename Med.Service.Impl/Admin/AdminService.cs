using System;
using System.Linq;
using App.Common;
using App.Common.Helpers;
using App.Common.Data;
using App.Common.Validation;
using Med.DbContext;
using Med.Entity.Registration;
using App.Common.DI;
using System.Collections.Generic;
using Med.Repository.Factory;
using Med.ServiceModel.Registration;
using Med.Service.Registration;
using Med.Entity;
using Med.Repository.Registration;
using Med.Service.Admin;
using Med.Service.Base;
using Med.Common;
using Med.Common.Enums;
using Med.ServiceModel.Admin;
using App.Constants.Enums;

namespace Med.Service.Impl.Admin
{
    public class AdminService : MedBaseService, IAdminService
    {       
        public List<Med.Entity.Admin.Role> GetAllRoles(bool onlyRolesForDrugStore = true)
        {
            var roles = _dataFilterService.GetValidRoles();
            if (onlyRolesForDrugStore)
            {
                roles = roles.Where(i => i.RoleId > (int)UserRoleId.MinIdForDrugStore && i.RoleId < (int)UserRoleId.MaxIdForDrugStore);
            }
            var results = roles.ToList();

            return results;
        }
        public bool PushRemainResourcesToDB()
        {
            var result = true;
            var actions = HttpActionEnumHelper.GetAllActions();           
            var repository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.Resource >>();
            var existingResources = repository.GetAll().ToList();
            var existingResourceIds = existingResources.Select(i => i.ResourceId).ToList();
            var newActions = actions.Where(i => !existingResourceIds.Contains(i.Key));
            var updateResources = new List<Med.Entity.Admin.Resource>();
            existingResources.ForEach(i =>
            {
                if (actions.ContainsKey(i.ResourceId))
                {
                    var act = actions[i.ResourceId];
                    if (!i.ResourceName.Equals(act.ActionName))
                    {
                        i.ResourceName = act.ActionName;
                        updateResources.Add(i);
                    }                    
                }
            });
            var newResources = newActions.Select(i => new Med.Entity.Admin.Resource()
            {
                ResourceId = i.Key,
                ResourceName = i.Value.ActionName,
                ApplicationId = (int)AppProductionType.MedMan,
                ResourceLevel = (int)ResourceLevelType.None
            }).ToList();

            if (newResources.Any())
            {
                repository.InsertMany(newResources);
            }
            if (updateResources.Any())
            {
                repository.UpdateMany(updateResources);
            }

            return result;
        }

        public RolePermissionResponse LoadRoleActions(int roleId, int? drugStoreId)
        {
            var result = new RolePermissionResponse()
            {
                RoleId = roleId,
                RoleName = string.Empty,
                PermittedResources = new List<ResourcePermission>(),
                NonePermittedResources = new List<ResourcePermission>()
            };
            var role = _dataFilterService.GetValidRoles().FirstOrDefault(i => i.RoleId == roleId);
            if (role == null) return result;
                       
            var actions = HttpActionEnumHelper.GetAllActions();
            var rolePerRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.RolePermission>>();
            var rolePermises = rolePerRepo.GetAll().Where(i => i.RoleId == roleId);
            if (drugStoreId > 0 && rolePermises.Any(i => i.DrugStoreID == drugStoreId))
            {
                rolePermises = rolePermises.Where(i => i.DrugStoreID == drugStoreId);
            }
            var permittedResources = rolePermises
                .Select(i => new ResourcePermission()
                {
                    PermissionId = i.PermissionId,
                    ResourceId = i.ResourceId                 
                }).ToList();
            permittedResources.ForEach(i =>
            {
                i.ResourceName = actions[i.ResourceId].ActionName;
            });
            var permittedResourceIds = permittedResources.Select(i => i.ResourceId).ToList();
            var nonPermittedResources = actions.Where(i => !permittedResourceIds.Contains(i.Key))
                .Select(i => new ResourcePermission()
                {
                    PermissionId = (int)PermissionType.None,
                    ResourceId = i.Key,
                    ResourceName = i.Value.ActionName
                }).ToList();

            result.RoleName = role.RoleName;
            result.PermittedResources = permittedResources;
            result.NonePermittedResources = nonPermittedResources;

            return result;
        }
        public bool AddRoleAction(int roleId, int resourceId, int permissionId, int? drugStoreId)
        {
            var retVal = true;
            var rolePerRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.RolePermission>>();
            rolePerRepo.Insert(new Med.Entity.Admin.RolePermission()
            {
                PermissionId = permissionId,
                DrugStoreID = drugStoreId,
                ResourceId = resourceId,
                RoleId = roleId
            });
            rolePerRepo.Commit();

            return retVal;
        }
        public bool UpdatePermission(int roleId, int resourceId, int permissionId, int? drugStoreId)
        {
            var retVal = true;
            var rolePerRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.RolePermission>>();
            rolePerRepo.UpdateMany(i => i.RoleId == roleId && i.ResourceId == resourceId && i.DrugStoreID == drugStoreId,
                i => new Med.Entity.Admin.RolePermission() { PermissionId = permissionId });

            return retVal;
        }
    
        public bool RemoveRoleAction(int roleId, int resourceId, int? drugStoreId)
        {
            var retVal = true;
            var rolePerRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Med.Entity.Admin.RolePermission>>();
            rolePerRepo.Delete(i => i.RoleId == roleId && i.ResourceId == resourceId && i.DrugStoreID == drugStoreId);

            return retVal;
        }
    }
}
