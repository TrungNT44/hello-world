using System.Collections.Generic;
using Med.Entity.Registration;
using Med.ServiceModel.Registration;
using Med.Entity;
using Med.ServiceModel.Admin;

namespace Med.Service.Registration
{
    public interface IUserService
    {
        UserSignInResponse SignIn(UserSignInRequest request);
        void CreateIfNotExist(IList<User> users);

        void SignOut(string token);

        bool IsValidToken(string authenticationToken);

        IList<BacSy> GetAllBacSy();

        UserAccount GetUserAccount(int userId);
    }
}
