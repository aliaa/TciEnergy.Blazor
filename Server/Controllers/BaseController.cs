﻿using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TciCommon.Models;
using TciCommon.Server;
using TciEnergy.Blazor.Server.Models;
using TciEnergy.Blazor.Shared;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly ProvinceDBs dbs;

        public BaseController(ProvinceDBs dbs)
        {
            this.dbs = dbs;
            dbs.CommonDb.GetUserNameFunc = () => Username;
            foreach (var k in dbs.Keys)
                dbs[k].GetUserNameFunc = () => Username;
        }

        protected string ProvincePrefix => HttpContext.User.FindFirst(nameof(Province)).Value;

        protected Province Province => dbs.CommonDb.FindFirst<Province>(p => p.Prefix == ProvincePrefix);

        protected IDbContext db => dbs[ProvincePrefix];

        protected string Username => HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        protected ObjectId? UserId
        {
            get
            {
                if (ObjectId.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value, out ObjectId val))
                    return val;
                return null;
            }
        }

        protected IEnumerable<Permission> UserPermissions
        {
            get
            {
                if (User == null)
                    return Enumerable.Empty<Permission>();
                Claim claim = User.Claims.FirstOrDefault(c => c.Type == nameof(Permission));
                if (claim == null)
                    return Enumerable.Empty<Permission>();
                return claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => (Permission)Enum.Parse(typeof(Permission), c));
            }
        }

        protected AuthUserX GetUser()
        {
            var id = UserId;
            if (id != null)
                return db.FindById<AuthUserX>(id.Value);
            return null;
        }

        protected IEnumerable<City> Cities => db.Find<City>(c => c.Province == Province.Id).SortBy(c => c.Name).ToEnumerable();

        protected async Task<FileContentResult> CreateExcelFile(DataTable table, string sheetName, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var memStream = new MemoryStream();
            using var package = new ExcelPackage(memStream);
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add(sheetName);
            sheet.Cells["A1"].LoadFromDataTable(table, true);
            await package.SaveAsync();
            var file = memStream.ToArray();
            return File(file, "application/octet-stream", fileName);
        }

        private ObjectId _mainCityId;
        public ObjectId MainCityId
        {
            get
            {
                if (_mainCityId == ObjectId.Empty)
                    _mainCityId = ObjectId.Parse(db.Find<Settings>(s => s.Key == "MainCity").Project(s => s.Value).FirstOrDefault());
                return _mainCityId;
            }
        }
    }
}
