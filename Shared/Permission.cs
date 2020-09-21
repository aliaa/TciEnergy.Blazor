using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared
{
    public enum Permission
    {
        [Display(Name = "تغییر مشترکین")]
        ChangeSubscribers,

        [Display(Name = "وارد سازی قبوض")]
        ImportBills,

        [Display(Name = "مدیریت کاربران")]
        ManageUsers,
    }
}
