using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

using AutoMapper;

using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.DAL.Enums;
using Mamlaka.API.Exceptions;
using Mamlaka.API.Interfaces;
using Mamlaka.API.Attributes;
using Mamlaka.API.DAL.Entities;
using Mamlaka.API.CommonObjects.Requests;

namespace Mamlaka.API.Controllers;

[
    ApiController,
    Route("api/users"),
    SwaggerOrder("A"),
    EnableCors("CorsPolicy"),
]

#nullable enable
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    public UserController(IUserRepository userRepository, IMapper mapper)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    /// <summary>
    /// register a new user to the system
    /// </summary>
    /// <param name="request"></param>
    /// <remarks>
    /// request format:
    ///    POST /api/users/register
    ///    {
    ///         "firstName": "Shawn",
    ///         "lastName": "Mbuvi",
    ///         "email": "seanmbuvi5@gmail.com",
    ///         "userRole": "SuperAdmin",
    ///         "password": "C@ptainJ@ck$parrow",
    ///         "confirmPassword": "C@ptainJ@ck$parrow"
    ///    }
    /// </remarks>
    /// <returns></returns>
    [HttpPost, Route("register"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> RegisterUser([FromBody, Required] UserRegisterRequest request)
    {
        if (request is null)
            return BadRequest();

        //validate user role
        if (!Enum.IsDefined(typeof(Roles), request.UserRole.Trim()))
        {
            throw new CustomException($"user role : {request.UserRole} is not pre-defined!", "ERR412", HttpStatusCode.PreconditionFailed);
        }
        return Ok(await _userRepository.CreateUser(request));
    }

    /// <summary>
    ///  user login to the system
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost, Route("signin"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> SignIn([FromBody, Required] SignInRequest request)
    {
        (bool successful, string token, long expires, UserDto user) = await _userRepository.SignIn(request.Email, request.Password);
        return Ok(new SignInDto { IsSuccessful = successful, AccessToken = token, Expires = expires, User = user });
    }

    /// <summary>
    /// reset user password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost, Route("reset-password"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> ResetUserPassword([FromBody, Required] PasswordResetRequest request)
    {
        if (request is null) return BadRequest("null parameters detected.");
        return Ok(await _userRepository.ResetPassword(request));
    }

    /// <summary>
    /// get the list of all users
    /// </summary>
    /// <returns></returns>
    [HttpGet("list"), Authorize(Policy = nameof(AuthPolicy.SuperRights))]    
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _userRepository.GetUserList());
    }

    /// <summary>
    /// get specific user by userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        if(string.IsNullOrWhiteSpace(userId)) return BadRequest("userId must be provided.");
        return Ok(await _userRepository.GetUser(userId));
    }    

    /// <summary>
    /// delete user from the database
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("delete/{userId}"), Authorize(Policy = nameof(AuthPolicy.SuperRights))]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        User? user = await _userRepository.GetUser(userId);
        if (user is null)
            return NotFound("user not found.");
        return Ok(await _userRepository.DeleteUser(user));
    }

    /// <summary>
    /// list all user roles
    /// </summary>
    /// <returns></returns>
    [HttpGet("roles")]
    public async Task<ActionResult> GetAllRoles()
    {
        return Ok(await _userRepository.GetAllRoles());
    }
}
