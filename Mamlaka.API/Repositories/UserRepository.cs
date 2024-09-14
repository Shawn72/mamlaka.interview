using System.Net;
using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;
using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.Interfaces;
using Mamlaka.API.Exceptions;
using Mamlaka.API.Extensions;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.DAL.Constants;
using Mamlaka.API.DAL.DbContexts;
using Mamlaka.API.CommonObjects.Requests;
using static Mamlaka.API.Helpers.Helpers;

namespace Mamlaka.API.Repositories;
public class UserRepository : IUserRepository
{
    private readonly IMapper _mapper;
    private readonly Constants _constants;
    private readonly IConfiguration _configuration;
    private readonly MySqlDbContext _databaseContext;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    public  readonly RoleManager<IdentityRole> _roleManager;
    public UserRepository(
        IMapper mapper,
        IConfiguration configuration,
        MySqlDbContext databaseContext,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _databaseContext = databaseContext;
        _constants = new Constants(configuration);
    }
    public async Task<object> CreateUser(UserRegisterRequest registerRequest)
    {
        try
        {
            if (registerRequest is not null)
            {
                User _member = new()
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Email = registerRequest.Email,
                    UserName = registerRequest.Email,
                    NationalIdNumber = registerRequest.NationalIdNumber,
                    PhoneNumber = registerRequest.PhoneNumber,
                    UserRole = registerRequest.UserRole,
                    CreatedAt = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt")),
                    ModifiedAt = DateTime.Parse(DateTime.UtcNow.ToEastAfricanTime().ToString("yyyy-MM-dd HH:mm:ss tt")),
                    ModifiedBy = registerRequest.ModifiedBy is not null ? registerRequest.ModifiedBy : _constants._defaultActor
                };

                IdentityResult result = await _userManager.CreateAsync(_member, registerRequest.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(_member, registerRequest.UserRole);
                    return $"success*user successfully registered.";
                }
                else
                {
                    StringBuilder errors = new StringBuilder();
                    foreach (var error in result.Errors)
                    {
                        errors.Append($"{error.Description}");
                    }
                    return $"error*{errors}";
                }
            }
            else
                return $"error*{HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> DeleteUser(User user)
    {
        try
        {
            if (user is not null)
            {
                _databaseContext.AspNetUsers.Remove(user);
                if (await _databaseContext.SaveChangesAsync() > 0) { return $"success*user removed successfully."; }
                else
                    return $"error*Error occured while deleting user.";
            }
            else
                return $"error*bad request:{HttpStatusCode.BadRequest}";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public (string token, long expires) GenerateUserJwt(User member, string userRole)
    {
        try
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_configuration["Security:Key"]));
            SigningCredentials signingCredentials = new(key, SecurityAlgorithms.HmacSha256Signature);

            DateTime baseDate = DateTime.UtcNow.ToEastAfricanTime();
            DateTime expiryDate = baseDate.AddMinutes(_configuration.GetValue<int>("Security:SessionLifeTimeInMinutes"));

            List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Exp, expiryDate.ToEpoch().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, $"{member.FirstName}{" "}{member.LastName}"),
            new Claim(JwtRegisteredClaimNames.Email, member.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, member.Id),
            new Claim(ClaimTypes.Role, userRole)
        };

            JwtSecurityToken token = new(
                issuer: _configuration["Security:Issuer"],
                audience: _configuration["Security:Audience"],
                claims: claims,
                notBefore: baseDate, //overwrite nbf
                expires: expiryDate,
                signingCredentials: signingCredentials
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), expiryDate.ToEpoch());
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<User> GetUser(string userId) {
        try
        {
#nullable enable
            User? user = await _databaseContext.AspNetUsers.FirstOrDefaultAsync(p => p.Id.Equals(userId));
            if (user is not null)
            {
                return user;
            }
            else return null;
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception occured: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> GetUserList()
    {
        try
        {
#nullable enable
            List<User> _members = await _databaseContext.AspNetUsers.ToListAsync();
            if (_members.Any())
            {
                return _members;
            }
            else return "no users found.";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception occured: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<object> ResetPassword(PasswordResetRequest passwordResetRequest)
    {
        try
        {
#nullable enable
            User? member = await _userManager.FindByEmailAsync(passwordResetRequest.Email);
#nullable disable
            if (member is null)
            {
                return "error*user not found.";
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(member);
            passwordResetRequest.Token = token;
            IdentityResult resetPassResult = await _userManager.ResetPasswordAsync(member, passwordResetRequest.Token, passwordResetRequest.Password);
            if (resetPassResult.Succeeded)
            {
                return "success*user password reset successfully.";
            }
            return "error*operation failed.";
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<(bool successful, string token, long expires, UserDto user)> SignIn(string email, string password)
    {
        try
        {
            ValidatedParameter("Password", password, out password, throwException: true);
            ValidatedParameter(nameof(User.Email), email, out email, throwException: true);

#nullable enable
            User? userDetails = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email).ConfigureAwait(false);
#nullable disable

            if (userDetails is null) throw new CustomException("Invalid login attempt, user not found!", "ERR404", HttpStatusCode.NotFound);

            string _userRole = string.Empty;
            IList<string> _allTheRoles = await _userManager.GetRolesAsync(userDetails);
            if (_allTheRoles.Count > 0)
            {
                //incase there are several roles, just take first one role, for simplicity
                _userRole = _allTheRoles.First();
            }
            else
            {
                _userRole = userDetails.UserRole;
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(userDetails, password, isPersistent: false, lockoutOnFailure: false);

            if (!signInResult.Succeeded) throw new CustomException("Unauthorized login attempt", "ERR401", HttpStatusCode.Unauthorized);

            UserDto memberModel = new()
            {
                Id = userDetails.Id,
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Email = email,
                NationalIdNumber = userDetails.NationalIdNumber,
                PhoneNumber = userDetails.PhoneNumber,
                UserRole = _userRole,
                CreatedAt = userDetails.CreatedAt.ToEastAfricanTime().ToString("yyyy-MM-dd"),
                ModifiedAt = userDetails.ModifiedAt.ToEastAfricanTime().ToString("yyyy-MM-dd"),
                ModifiedBy = userDetails.ModifiedBy is not null ? userDetails.ModifiedBy : _constants._defaultActor
            };

            (string token, long expires) = GenerateUserJwt(userDetails, _userRole);

            return (true, token, expires, memberModel);
        }
        catch (Exception ex)
        {
            throw new CustomException($"exception: {ex.Message}", "ERR500", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<IdentityRole>> GetAllRoles()
    {
        return await _roleManager.Roles.ToListAsync();
    }
}
