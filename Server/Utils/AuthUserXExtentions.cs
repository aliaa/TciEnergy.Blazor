using AliaaCommon.Models;
using TciEnergy.Blazor.Server.Models;
using TciCommon.Models;
using EasyMongoNet;

namespace TciEnergy.Blazor.Server.Utils
{
    public static class AuthUserXExtentions
    {
        public static AuthUserX CheckAuthentication(this IDbContext db, string username, string password, bool passwordIsHashed = false)
        {
            string hash;
            if (passwordIsHashed)
                hash = password;
            else
                hash = AuthUserDBExtention.GetHash(password);
            string ip = null; //TODO
            var user = db.FindFirst<AuthUserX>(u => u.Username == username && u.HashedPassword == hash && u.Disabled != true);

            if (user != null)
            {
                db.Save(new LoginLog { Sucess = true, UserId = user.Id, Username = user.Username, IP = ip });
                return user;
            }

            db.Save(new LoginLog { Sucess = false, Username = username, IP = ip });
            return null;
        }

        public static AuthUserX FindByUsername(this IReadOnlyDbContext db, string username)
        {
            return db.FindFirst<AuthUserX>(u => u.Username == username);
        }
    }
}
