using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Auth;
using BidSphere.Models.Enums;
using BidSphere.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidSphere.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RegisterDtoValidator _registerValidator;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RegisterDtoValidator registerValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _registerValidator = registerValidator;
        }

        /// <summary>
        /// Register a new user with custom fields (Role, CreatedAt)
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            // Validate input
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Parse role from request or default to User
            UserRole userRole = UserRole.User;
            if (!string.IsNullOrEmpty(registerDto.Role))
            {
                if (Enum.TryParse<UserRole>(registerDto.Role, true, out var parsedRole))
                {
                    userRole = parsedRole;
                }
                else
                {
                    return BadRequest(new { message = "Invalid role. Valid roles: Admin, User, Guest" });
                }
            }

            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Role = userRole,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            // Add user to Identity role (syncs with User.Role property)
            await _userManager.AddToRoleAsync(user, user.Role.ToString());

            return Ok(new { message = $"User registered successfully with role: {user.Role}. Please login to get your token." });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Update user profile (email and role)
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateProfile(UserDto userDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return NotFound();
            }

            // Update email
            user.Email = userDto.Email;
            user.UserName = userDto.Email;

            // Update role if provided
            if (!string.IsNullOrEmpty(userDto.Role))
            {
                if (Enum.TryParse<UserRole>(userDto.Role, true, out var newRole))
                {
                    // Remove from old Identity role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    // Update enum property
                    user.Role = newRole;

                    // Add to new Identity role
                    await _userManager.AddToRoleAsync(user, newRole.ToString());
                }
                else
                {
                    return BadRequest(new { message = "Invalid role. Valid roles: Admin, User, Guest" });
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new UserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }
    }
}
