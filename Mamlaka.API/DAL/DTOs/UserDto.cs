namespace Mamlaka.API.DAL.DTOs;
public class UserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string NationalIdNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string CreatedAt { get; set; }
    public string ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public string UserName { get; set; }
    public string UserRole { get; set; }
}
