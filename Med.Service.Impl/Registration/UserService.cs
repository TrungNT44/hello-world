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
using Med.Service.Base;
using Med.ServiceModel.Admin;
using Med.Entity.Admin;
using Med.Common.Enums;
using App.Constants.Enums;

namespace Med.Service.Impl.Registration
{
    public class UserService : MedBaseService, IUserService
    {
        public void CreateIfNotExist(IList<User> users)
        {
            if (users == null) { return; }
            using (IUnitOfWork uow = new App.Common.Data.UnitOfWork(new MedDbContext(IOMode.Write)))
            {
                IUserRepository userRepository = IoC.Container.Resolve<IUserRepository>();
                foreach (User user in users)
                {
                    User existUser = userRepository.GetByEmail(user.Email);
                    if (existUser != null) { continue; }
                    //userRepository.Add(user);
                }
                uow.Commit();
            }
        }

        public UserSignInResponse SignIn(UserSignInRequest request)
        {
            AuthenticationToken token;
            ValidateUserLoginRequest(request);
            User user;
            using (IUnitOfWork uow = new App.Common.Data.UnitOfWork(new MedDbContext(IOMode.Write)))
            {
                IUserRepository userRepository = IoC.Container.Resolve<IUserRepository>();

                token = new App.Common.AuthenticationToken(Guid.NewGuid(), DateTimeHelper.GetAuthenticationTokenExpiredUtcDateTime());
                user = userRepository.GetByEmail(request.Email);

                user.Token = token.Value;
                user.TokenExpiredAfter = token.ExpiredAfter;
                //userRepository.Update(user);
                uow.Commit();
            }
            UserQuickProfile profile = new UserQuickProfile(user);
            return new UserSignInResponse(token, profile);
        }

        private void ValidateUserLoginRequest(UserSignInRequest request)
        {
            if (request == null)
            {
                throw new ValidationException("Common.InvalidRequest");
            }
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                throw new ValidationException("Registration.SignIn.InvalidEmail");
            }
            if (String.IsNullOrWhiteSpace(request.Pwd))
            {
                throw new ValidationException("Registration.SignIn.InvalidPwd");
            }
            IUserRepository userRepository = IoC.Container.Resolve<IUserRepository>();
            User userProfile = userRepository.GetByEmail(request.Email);

            if (userProfile == null || EncodeHelper.EncodePassword(request.Pwd) != userProfile.Password)
            {
                throw new ValidationException("Registration.SignIn.InvalidEmailOrPwd");
            }

        }

        public void SignOut(string token)
        {
            using (IUnitOfWork uow = new App.Common.Data.UnitOfWork(new MedDbContext(IOMode.Write)))
            {
                IUserRepository userRepository = IoC.Container.Resolve<IUserRepository>();
                User user = userRepository.GetByToken(token);
                if (user == null) { return; }
                user.Token = string.Empty;
                user.TokenExpiredAfter = DateTime.UtcNow;
                //userRepository.Update(user);
                uow.Commit();
            }
        }
        public bool IsValidToken(string authenticationToken)
        {
            IUserRepository userRepository = IoC.Container.Resolve<IUserRepository>();
            User user = userRepository.GetByToken(authenticationToken);
            return user != null && !DateTimeHelper.IsExpired(user.TokenExpiredAfter);
        }

        public IList<BacSy> GetAllBacSy()
        {
            var userRepository = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, BacSy>>();

            var lst = userRepository.GetAll().ToList();

            return lst;
        }

        public UserAccount GetUserAccount(int userId)
        {
            var userRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, UserProfile>>();
            var resourceRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, RolePermission>>();
            var roleRepo = IoC.Container.Resolve<BaseRepositoryV2<MedDbContext, Entity.Admin.UserRole>>();

            var user = userRepo.GetAll().Where(i => i.UserId == userId).Select(i => new UserAccount()
            {
                Id = i.UserId,
                Name = i.UserName,
                Email = i.Email                
            }).FirstOrDefault();
            var roleIds = roleRepo.GetAll().Where(i => i.UserId == userId).Select(i => i.RoleId).Distinct().ToList();
            
            if (roleIds.Any())
            {
                user.RoleIds = roleIds.Select(i => (UserRoleId)i).ToList();
                var permittedResources = resourceRepo.GetAll().Where(i => roleIds.Contains(i.RoleId))
                    .ToDictionary(i => (HttpActionEnum)i.ResourceId, i => (PermissionType)i.PermissionId);
                user.PermittedResources = permittedResources;
            }
            else
            {
                user.RoleIds = new List<UserRoleId>();
            }

            return user;
        }
    }
}
