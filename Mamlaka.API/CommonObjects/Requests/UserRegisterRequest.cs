using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.CommonObjects.Requests;

#nullable enable
public class UserRegisterRequest
{

    [Required(ErrorMessage = "firstName must be provided")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "lastName must be provided")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "phoneNumber must be provided")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "nationalIdNumber must be provided")]
    public required string NationalIdNumber { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "email must be provided")]
    public required string Email { get; set; }

    /// <summary>
    /// predefined user roles: SuperAdmin, Admin, User
    /// </summary>
    [Required(ErrorMessage = "userRole must be provided")]
    public required string UserRole { get; set; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "password must be provided")]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }

    public string? ModifiedBy { get; set; }
}
