using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.DTOs.PropertiesDTOs;
using PropertyRentalManagementSystem.Enums.RentalEnum;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.SMTP;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public RentalsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }



        [Authorize]
        [HttpGet("rentals/my-properties")]
        public IActionResult GetRentalsOnMyProperties()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid User ID format.");

            var rentals = _context.Rentals
                .Include(r => r.Property)
                .Include(r => r.User)
                .Where(r => r.Property != null && r.Property.UserId == userId)
                .ToList();

            return Ok(rentals);
        }


        [Authorize]
        [HttpGet("my-rentals")]
        public IActionResult GetMyRentals()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            if (!int.TryParse(userIdClaim.Value, out int renterId))
                return Unauthorized("Invalid User ID format.");

            var rentals = _context.Rentals
                .Where(r => r.UserId == renterId && r.RentalStatus == Enums.RentalEnum.RENTAL_STATUS.CONFIRMED)
                .Include(r => r.Property)
                .ToList();

            if (!rentals.Any())
                return NotFound("No confirmed rentals found.");

            var rentalDtos = _mapper.Map<List<RentalDto>>(rentals);

            return Ok(rentalDtos);
        }
       

        [HttpPost("request/{propertyId}")]
        [Authorize(Policy = "TenantOnly")]
        public ActionResult RequestRental(int propertyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid User ID format.");
            }

            var property = _context.Properties.Find(propertyId);
            if (property == null)
            {
                return NotFound("Property not found.");
            }

            var existingRental = _context.Rentals
                .FirstOrDefault(r => r.PropertyId == propertyId && r.UserId == userId && r.RentalStatus != Enums.RentalEnum.RENTAL_STATUS.CANCELLED);

            if (existingRental != null)
            {
                return BadRequest("You have already requested this property.");
            }

            var rental = new Rental
            {
                PropertyId = propertyId,
                UserId = userId,
                StartDate = DateTime.UtcNow,
                RentalStatus = Enums.RentalEnum.RENTAL_STATUS.REQUESTED,
                Amount = property.Price,
                IsPaid = false
            };

            _context.Rentals.Add(rental);
            _context.SaveChanges();


            var owner = _context.Users.FirstOrDefault(u => u.Id == property.UserId);
            if (owner != null)
            {
                SMTPService smtp = new SMTPService();
                smtp.SendEmail(owner.Email, "New Rental Request",
                    $"<p>You have a new rental request for your property (ID: {property.Id}) from user ID: {userId}.</p>");
            }

            var rentalDto = _mapper.Map<RentalDto>(rental);
            return Ok(rentalDto);
        }


        [HttpPut("rentals/{id}/approve")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult ApproveRentalRequest(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("User ID not found or invalid in token.");

            var rental = _context.Rentals
                .Include(r => r.Property)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null)
                return NotFound("Rental request not found.");

            if (rental.Property.UserId != userId)
                return Forbid("You are not the owner of this property.");

            if (rental.RentalStatus != Enums.RentalEnum.RENTAL_STATUS.REQUESTED)
                return BadRequest("Only requested rentals can be approved.");

            rental.RentalStatus = Enums.RentalEnum.RENTAL_STATUS.CONFIRMED;
            _context.Rentals.Update(rental);
            _context.SaveChanges();

            var tenant = _context.Users.FirstOrDefault(u => u.Id == rental.UserId);
            if (tenant != null)
            {
                SMTPService smtp = new SMTPService();
                smtp.SendEmail(tenant.Email, "Rental Request Approved",
                    $"<p>Your rental request for property ID {rental.PropertyId} has been approved by the owner.</p>");
            }

            var rentalDto = _mapper.Map<RentalDto>(rental);
            return Ok(rentalDto);
        }

        [HttpPut("rentals/{id}/reject")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult RejectRentalRequest(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("User ID not found or invalid in token.");

            var rental = _context.Rentals
                .Include(r => r.Property)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null)
                return NotFound("Rental request not found.");

            if (rental.Property.UserId != userId)
                return Forbid("You are not authorized to reject this request.");

            rental.RentalStatus = Enums.RentalEnum.RENTAL_STATUS.CANCELLED;
            _context.Rentals.Update(rental);
            _context.SaveChanges();

            var tenant = _context.Users.FirstOrDefault(u => u.Id == rental.UserId);
            if (tenant != null)
            {
                SMTPService smtp = new SMTPService();
                smtp.SendEmail(tenant.Email, "Rental Request Rejected",
                    $"<p>Your rental request for property ID {rental.PropertyId} has been rejected by the owner.</p>");
            }

            var rentalDto = _mapper.Map<RentalDto>(rental);
            return Ok(rentalDto);
        }

        [HttpPut("rentals/{id}/terminate")]
        [Authorize(Policy = "OwnerOnly")]
        public IActionResult TerminateRental(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("User ID not found or invalid in token.");

            var rental = _context.Rentals
                .Include(r => r.Property)
                .FirstOrDefault(r => r.Id == id);

            if (rental == null)
                return NotFound("Rental not found.");

            if (rental.Property.UserId != userId)
                return Forbid("You are not authorized to terminate this rental.");

            rental.RentalStatus = Enums.RentalEnum.RENTAL_STATUS.TERMINATED;
            rental.EndDate = DateTime.UtcNow;

            _context.Rentals.Update(rental);
            _context.SaveChanges();

            var tenant = _context.Users.FirstOrDefault(u => u.Id == rental.UserId);
            if (tenant != null)
            {
                SMTPService smtp = new SMTPService();
                smtp.SendEmail(tenant.Email, "Rental Terminated",
                    $"<p>Your rental for property ID {rental.PropertyId} has been terminated by the owner.</p>");
            }

            var rentalDto = _mapper.Map<RentalDto>(rental);
            return Ok(rentalDto);
        }


    }
}
