using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyRentalManagementSystem.CORE;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.Enums.UserEnum;
using PropertyRentalManagementSystem.FluentValidations;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.Requests;
using PropertyRentalManagementSystem.Services.Implementation;
using PropertyRentalManagementSystem.Services.Interfaces;
using PropertyRentalManagementSystem.SMTP;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly DataContext _context;
        private readonly IJWTService _jwtService;
        private readonly UserValidator _userValidator;

        public AuthenticationController(DataContext context, IJWTService jwtService, UserValidator validations)
        {
            _context = context;
            _jwtService = jwtService;
            _userValidator = validations;

        }
       

        [HttpPost("auth-register")]
        public ActionResult Register(AddUser request)
        {
            var userExists = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (userExists == null)
            {
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    UserRole = request.UserRole,
                };

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                Random rand = new Random();
                string randomCode = rand.Next(1000, 9999).ToString();

                user.VerificationCode = randomCode;

                var result = _userValidator.Validate(user);

                if (!result.IsValid)
                {
                    var responses = new ApiResponse<List<string>>
                    {
                        Data = result.Errors.Select(e => e.ErrorMessage).ToList(),
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Validation failed"
                    };

                    return BadRequest(responses);
                }

                SMTPService smtpService = new SMTPService();

                smtpService.SendEmail(user.Email, "Verification", $"<p>{user.VerificationCode}</p>");


                _context.Users.Add(user);
                _context.SaveChanges();

                var response = new ApiResponse<User>
                {
                    Data = user,
                    Status = StatusCodes.Status200OK,
                    Message = null,
                };

                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    Data = false,
                    Status = StatusCodes.Status409Conflict,
                    Message = "User Already Exists",
                };

                return BadRequest(response);
            }
        }

        [HttpPost("verify-email/{email}/{code}")]
        public ActionResult Verify(string email, string code)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                var response = new ApiResponse<bool>
                {
                    Data = false,
                    Message = "User Not Found",
                    Status = StatusCodes.Status404NotFound,
                };

                return NotFound(response);
            }
            else
            {
                if (user.VerificationCode == code)
                {
                    user.Status = ACCOUNT_STATUS.VERIFIED;
                    user.VerificationCode = null;

                    _context.SaveChanges();
                    var response = new ApiResponse<bool>
                    {
                        Data = true,
                        Message = "User Verified",
                        Status = StatusCodes.Status200OK,
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<bool>
                    {
                        Data = false,
                        Message = "Wrong Verification Code",
                        Status = StatusCodes.Status400BadRequest,
                    };
                    return BadRequest(response);
                }
            }
        }



        [HttpPost("login")]
        public ActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                var response = new ApiResponse<bool>
                {
                    Data = false,
                    Message = "User Not Found",
                    Status = StatusCodes.Status404NotFound,
                };

                return NotFound(response);
            }
            else
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password) && user.Status == ACCOUNT_STATUS.VERIFIED)
                {
                    var response = new ApiResponse<UserToken>
                    {
                        Data = _jwtService.GetUserToken(user),
                        Status = 200,
                        Message = ""
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<bool>
                    {
                        Data = false,
                        Status = StatusCodes.Status401Unauthorized,
                        Message = "Something went wrong",
                    };

                    return Unauthorized(response);
                }

            }
        }


    }
}
