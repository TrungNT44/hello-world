using System.Collections.Generic;
using System.Linq;
using sThuoc.DAL;
using sThuoc.Models;

namespace sThuoc.Repositories
{
    public class UserProfileRepository : GenericRepository<UserProfile>
    {
        public UserProfileRepository(SecurityContext context) : base(context) { }

        public UserProfile GetUserByName(string userName)
        {
            return Context.UserProfiles.FirstOrDefault(x => x.UserName == userName);
        }

        public List<UserProfile> GetUserByNhaThuoc(string maNhaThuoc)
        {
            return Context.UserProfiles.Where(x => x.MaNhaThuoc == maNhaThuoc).ToList();
        }
    }
}
