using System.ComponentModel.DataAnnotations;

namespace TciEnergy.Blazor.Shared.ViewModels
{
    public class NewUserVM
    {
        [Required(ErrorMessage = "نام اجباریست!")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی اجباریست!")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "نام کاربری اجباریست!")]
        [StringLength(maximumLength: 20, MinimumLength = 5, ErrorMessage = "نام کاربری بایستی بین 5 تا 20 کاراکتر باشد!")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور اجباریست!")]
        [StringLength(maximumLength: 20, MinimumLength = 5, ErrorMessage = "رمز عبور بایستی بین 5 الی 20 کاراکتر باشد!")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تکرار رمز عبور اجباریست!")]
        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن بایستی برابر باشند!")]
        [Display(Name = "تکرار رمز عبور")]
        public string RetypePassword { get; set; }
    }
}
