using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.CommonObjects.Requests;
public class SignInRequest
{
    /// <summary>
    /// Email address for the user logged in
    /// </summary>
    [Required(ErrorMessage = "email is required")]
    [StringLength(maximumLength: 128, MinimumLength = 8, ErrorMessage = "username should be between 8 - 128 characters")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// user account password
    /// </summary>
    [Required(ErrorMessage = "password is required")]
    public string Password { get; set; } = null!;
}
