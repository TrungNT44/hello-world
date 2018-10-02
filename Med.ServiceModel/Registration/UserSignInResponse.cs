using App.Common;

namespace Med.ServiceModel.Registration
{
    public class UserSignInResponse
    {
        public UserSignInResponse(AuthenticationToken token, UserQuickProfile userProfile)
        {
            this.Token = token;
            this.Profile = userProfile;
        }
        public AuthenticationToken Token { get; set; }
        public UserQuickProfile Profile { get; set; }
    }
}
