using AliaaCommon.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using TciEnergy.Blazor.Server.Models;
using EasyMongoNet.Driver2;
using TciCommon.Models;

namespace TciEnergy.Blazor.Server.Utils
{
    public static class AuthUserXExtentions
    {
        public static async Task<AuthUserX> CheckAuthentication(this IMongoCollection<AuthUserX> userCol, IMongoCollection<LoginLog> loginLogCol, string username, string password, bool passwordIsHashed = false)
        {
            string hash;
            if (passwordIsHashed)
                hash = password;
            else
                hash = AuthUserDBExtention.GetHash(password);
            string ip = null; //TODO
            var user = await userCol.FindFirstAsync(u => u.Username == username && u.HashedPassword == hash && u.Disabled != true);

            if (user != null)
            {
                await loginLogCol.InsertOneAsync(new LoginLog { Sucess = true, UserId = user.Id, Username = user.Username, IP = ip });
                return user;
            }

            await loginLogCol.InsertOneAsync(new LoginLog { Sucess = false, Username = username, IP = ip });
            return null;
        }

        public static async Task<AuthUserX> FindByUsername(this IMongoCollection<AuthUserX> userCol, string username)
        {
            return await userCol.FindFirstAsync(u => u.Username == username);
        }
    }
}
