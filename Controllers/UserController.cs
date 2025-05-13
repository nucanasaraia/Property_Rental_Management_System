using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyRentalManagementSystem.CORE;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.Enums.UserEnum;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.SMTP;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly DataContext _context;
        
        public UserController(DataContext context)
        {
            _context = context;
        }


        [HttpGet("users-profile")]
        [Authorize]
        public ActionResult GetProfile(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

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
                if (user.Status == ACCOUNT_STATUS.VERIFIED)
                {
                    var response = new ApiResponse<User>
                    {
                        Data = user,
                        Message = null,
                        Status = StatusCodes.Status200OK,
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<bool>
                    {
                        Data = false,
                        Message = "User Not Verified",
                        Status = StatusCodes.Status400BadRequest,
                    };

                    return BadRequest(response);
                }
            }
        }


        [HttpGet("get-all-users")]
        public ActionResult getUsers()
        {
            var getAll = _context.Users;
            return Ok(getAll);
        }

        [HttpDelete("delete-user/{id}")]
        public ActionResult DeleteUser(int id)
        {
            var usertodelete = _context.Users.FirstOrDefault(u => u.Id == id);
            _context.Users.Remove(usertodelete);
            _context.SaveChanges();

            return Ok(usertodelete);
        }


        [HttpPost("get-reset-code")]
        public ActionResult GetResetCode(string userEmail)
        {

            var user = _context.Users.FirstOrDefault(x => x.Email == userEmail);
            if (user == null)
            {
                var response = new ApiResponse<bool>
                {
                    Data = false,
                    Message = "user not found",
                    Status = StatusCodes.Status404NotFound,
                };
                return NotFound(response);
            }
            else
            {
                if (user.Status == ACCOUNT_STATUS.VERIFIED)
                {
                    Random rand = new Random();
                    string randomCode = rand.Next(1000, 9999).ToString();

                    user.PasswordResetCode = randomCode;

                    SMTPService smtpService = new SMTPService();

                    smtpService.SendEmail(user.Email, "reset code", $"<p>{user.PasswordResetCode}</p>");

                    _context.SaveChanges();

                    var response = new ApiResponse<string>
                    {
                        Data = "code sent succesfully",
                        Status = StatusCodes.Status200OK,
                        Message = null
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<bool>
                    {
                        Data = false,
                        Status = StatusCodes.Status400BadRequest,
                        Message = " user not verified"
                    };
                    return BadRequest(response);
                }

            }
        }


        [HttpPut("reset-password")]
        public ActionResult ResetPassword(string email, string code, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                var response = new ApiResponse<bool>
                {
                    Data = false,
                    Message = "user not found",
                    Status = StatusCodes.Status404NotFound,
                };
                return NotFound(response);
            }
            else
            {
                if (user.PasswordResetCode == code)
                {
                    var newPasswordhash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    user.Password = newPasswordhash;
                    _context.SaveChanges();

                    var response = new ApiResponse<string>
                    {
                        Data = "password reset successfully",
                        Status = StatusCodes.Status200OK,
                        Message = null
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<string>
                    {
                        Data = "incorrect code",
                        Status = StatusCodes.Status400BadRequest,
                        Message = null
                    };
                    return BadRequest(response);
                }
            }
        }




    }
}
