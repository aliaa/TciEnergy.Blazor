using AliaaCommon.Models;
using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Models
{
    [CollectionOptions(Name = nameof(AuthUser))]
    [CollectionSave(WriteLog = true)]
    [CollectionIndex(Fields: new string[] { nameof(Username) }, Unique = true)]
    [BsonIgnoreExtraElements]
    public class AuthUserX : BaseAuthUser
    {
        public string HashedPassword { get; set; }

        [BsonIgnore]
        public string Password
        {
            set
            {
                HashedPassword = AuthUserDBExtention.GetHash(value);
            }
        }
    }
}
