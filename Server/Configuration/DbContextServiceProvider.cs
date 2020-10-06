using AliaaCommon;
using EasyMongoNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TciCommon.Models;
using TciCommon.ServerUtils;

namespace TciEnergy.Blazor.Server.Configuration
{
    public static class DbContextServiceProvider
    {
        public static void AddMongoDbContexts(this IServiceCollection services, IConfiguration config)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), StringNormalizer.JSON_FILE_NAME);
            var stringNormalizer = new StringNormalizer(filePath);
            services.AddSingleton(stringNormalizer);

            var dbName = config.GetValue<string>("DBName");
            var connString = config.GetValue<string>("MongoConnString");
            var customConnections = config.GetSection("CustomConnections").Get<List<CustomMongoConnection>>();

            foreach (var cc in customConnections)
                if (cc.ConnectionString == null)
                    cc.ConnectionString = connString;
            var provinceDbInfo = customConnections.Where(c => c.Type == nameof(Province)).FirstOrDefault();
            var provinceDb = new MongoDbContext(provinceDbInfo.DBName, provinceDbInfo.ConnectionSettings);

            var dbs = new ProvinceDBs { CommonDb = provinceDb };
            foreach (var p in provinceDb.FindGetResults<Province>(p => p.Applications.Contains("Energy")))
            {
                var db = new MongoDbContext(dbName + "-" + p.Prefix, connString,
                    customConnections: customConnections,
                    objectPreprocessor: stringNormalizer);
                dbs.Add(p.Prefix, db);
            }

            services.AddSingleton(dbs);

            services.AddTransient<IReadOnlyDbContext>(serviceProvider => GetFromServices(serviceProvider));
            services.AddTransient<IDbContext>(serviceProvider => GetFromServices(serviceProvider));
        }

        private static IDbContext GetFromServices(IServiceProvider serviceProvider)
        {
            var dbs = serviceProvider.GetService<ProvinceDBs>();
            var httpContext = serviceProvider.GetService<IHttpContextAccessor>();
            var province = httpContext.HttpContext.User.FindFirst(nameof(Province))?.Value;
            if (province == null)
                return null;
            return dbs[province];
        }
    }
}
