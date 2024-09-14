using Microsoft.AspNetCore.Identity;

using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.CommonObjects.Requests;

namespace Mamlaka.API.Interfaces;
public interface IUserRepository
{
    Task<object> CreateUser(UserRegisterRequest registerRequest);
    Task<User> GetUser(string userId);
    Task<object> GetUserList();
    Task<object> DeleteUser(User user);
    Task<(bool successful, string token, long expires, UserDto user)> SignIn(string email, string password);
    (string token, long expires) GenerateUserJwt(User user, string userRole);
    Task<object> ResetPassword(PasswordResetRequest passwordResetRequest);
    Task<IEnumerable<IdentityRole>> GetAllRoles();
}
