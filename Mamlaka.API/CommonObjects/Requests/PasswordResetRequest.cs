using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.CommonObjects.Requests;
#nullable enable
public class PasswordResetRequest
{
    [Required(ErrorMessage = "email must be provided")]
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "password must be provided")]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }

    [JsonIgnore]
    public string? Token { get; set; }
}
