namespace BackgammonR.WebUI.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        public string Name { get; set; }
    }
}