using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mamlaka.API.DAL.Entities;

[Table("AspNetUsers")]
public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NationalIdNumber { get; set; }
    public string UserRole { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedAt { get; set; }

    [Column(TypeName = "nvarchar(256)")]
    public string ModifiedBy { get; set; }
}