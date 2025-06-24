using System.ComponentModel.DataAnnotations;

namespace GreenAgriApp.Models.ViewModels
{
public class RegisterUserViewModel
{
    [Required]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

}