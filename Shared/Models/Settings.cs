using EasyMongoNet;

namespace TciEnergy.Blazor.Shared.Models
{
    [CollectionIndex(new string[] { nameof(Key) }, Unique = true)]
    [CollectionSave(WriteLog = true)]
    public class Settings : MongoEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
