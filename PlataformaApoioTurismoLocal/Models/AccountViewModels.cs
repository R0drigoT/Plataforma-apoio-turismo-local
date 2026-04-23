using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ProjetoFim.Resources;

namespace ProjetoFim.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Email")]
        [Display(Name = "Auth_Email", ResourceType = typeof(Strings))]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        public string Provider { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [Display(Name = "Auth_Code", ResourceType = typeof(Strings))]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Auth_RememberBrowser", ResourceType = typeof(Strings))]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Email")]
        [Display(Name = "Auth_Email", ResourceType = typeof(Strings))]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [Display(Name = "Auth_Username", ResourceType = typeof(Strings))]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Auth_Password", ResourceType = typeof(Strings))]
        public string Password { get; set; }

        [Display(Name = "Auth_RememberMe", ResourceType = typeof(Strings))]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [Display(Name = "Auth_Username", ResourceType = typeof(Strings))]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Email")]
        [Display(Name = "Auth_Email", ResourceType = typeof(Strings))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Password_Length")]
        [DataType(DataType.Password)]
        [Display(Name = "Auth_Password", ResourceType = typeof(Strings))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Auth_ConfirmPassword", ResourceType = typeof(Strings))]
        [System.ComponentModel.DataAnnotations.Compare("Password",
       ErrorMessageResourceType = typeof(Strings),
       ErrorMessageResourceName = "Validation_Password_Confirm")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [DataType(DataType.Date)]
        [Display(Name = "Auth_DateOfBirth", ResourceType = typeof(Strings))]
        public DateTime DateOfBirth { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Email")]
        [Display(Name = "Auth_Email", ResourceType = typeof(Strings))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Password_Length")]
        [DataType(DataType.Password)]
        [Display(Name = "Auth_Password", ResourceType = typeof(Strings))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Auth_ConfirmPassword", ResourceType = typeof(Strings))]
        [System.ComponentModel.DataAnnotations.Compare("Password",
      ErrorMessageResourceType = typeof(Strings),
      ErrorMessageResourceName = "Validation_Password_Confirm")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Required")]
        [EmailAddress(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "Validation_Email")]
        [Display(Name = "Auth_Email", ResourceType = typeof(Strings))]
        public string Email { get; set; }
    }
}
