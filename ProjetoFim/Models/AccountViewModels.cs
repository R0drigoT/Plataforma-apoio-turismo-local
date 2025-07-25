using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [Display(Name = "Nome de Utilizador")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Lembrar-me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [Display(Name = "Nome de Utilizador")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirme a password")]
        [Compare("Password", ErrorMessage = "A password e a sua confirmação não correspondem.")]
        public string ConfirmPassword { get; set; }
    }
}