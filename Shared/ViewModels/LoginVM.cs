using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "نام کاربری اجباریست!")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور اجباریست!")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "لطفا استان خود را انتخاب نمائید.")]
        [Display(Name = "استان")]
        public string Province { get; set; }

        public bool RememberMe { get; set; }
    }
}
