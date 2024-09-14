namespace Mamlaka.API.DAL.DTOs;
public class SignInDto
{
    /// <summary>
    /// Successful sign in result
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// User JWT token to be used in the subsequent calls for authorization
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Timestamp at which the token expires
    /// </summary>
    public long Expires { get; set; }

    /// <summary>
    /// Logged in user
    /// </summary>
#nullable enable
    public UserDto? User { get; set; }
}
