using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TciEnergy.Blazor.Shared;

namespace TciEnergy.Blazor.Server.Controllers
{
    public class BaseController : ControllerBase
    {
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
    }
}
