using MongoDB.Bson;
using Omu.ValueInjecter;
using System;
using System.Linq;

namespace TciEnergy.Blazor.Server.Configuration
{
    public static class ConfigureMapper
    {
        public static void Configure()
        {
            Mapper.DefaultMap = (src, resType, tag) =>
            {
                var res = Activator.CreateInstance(resType);
                res.InjectFrom(src);

                var srcTypeProps = src.GetType().GetProperties();
                var resTypeProps = resType.GetProperties();

                foreach (var resProp in resTypeProps.Where(p => p.PropertyType == typeof(ObjectId)))
                {
                    var matchSrcProp = srcTypeProps.FirstOrDefault(p => p.Name == resProp.Name && p.PropertyType == typeof(string));
                    if (matchSrcProp != null)
                    {
                        string id = (string)matchSrcProp.GetValue(src);
                        if (ObjectId.TryParse(id, out ObjectId objId))
                            resProp.SetValue(res, objId);
                    }
                }
                foreach (var srcProp in srcTypeProps.Where(p => p.PropertyType == typeof(ObjectId)))
                {
                    var matchResProp = resTypeProps.FirstOrDefault(p => p.Name == srcProp.Name && p.PropertyType == typeof(string));
                    if (matchResProp != null)
                    {
                        var objId = (ObjectId)srcProp.GetValue(src);
                        matchResProp.SetValue(res, objId.ToString());
                    }
                }
                return res;
            };
        }
    }
}
