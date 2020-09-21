using System;
using System.Collections.Generic;
using System.Text;

namespace TciEnergy.Blazor.Shared.Models
{
    public class ClientAuthUser : BaseAuthUser
    {
        public string ProvincePrefix { get; set; }
    }
}
