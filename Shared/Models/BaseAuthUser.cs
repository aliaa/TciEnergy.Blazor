using EasyMongoNet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.Models
{
    public abstract class BaseAuthUser : MongoEntity
    {
        [Required]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Display(Name = "مدیر سیستم است؟")]
        public bool IsAdmin { get; set; }

        [Required]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "غیر فعال شده")]
        public bool Disabled { get; set; }

        public string DisplayName
        {
            get { return FirstName + " " + LastName; }
        }

        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public bool HasPermission(Permission perm)
        {
            return IsAdmin || Permissions.Contains(perm);
        }
    }
}
