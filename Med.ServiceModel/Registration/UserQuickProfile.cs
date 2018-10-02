using App.Common.Mapping;
using System;
using Med.Entity.Registration;
using App.Common.Data;
using Med.Entity.Common;

namespace Med.ServiceModel.Registration
{
    public class UserQuickProfile: BaseEntity, IMappedFrom<User>
    {
        public UserQuickProfile(User user)
        {
            this.EntityId = user.EntityId;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
            this.LastLoggedInDate = user.LastLoggedInDate;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime LastLoggedInDate { get; set; }
        public string LanguageCode { get; set; }
    }
}
