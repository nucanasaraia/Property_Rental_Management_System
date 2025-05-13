using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        public readonly DataContext _context;
        public readonly IMapper _mapper;

        public ReviewsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("reviews-property/{propertyId}")]
        public ActionResult GetPropertyReviews(int propertyId)
        {
            var reviews = _context.Reviews
            .Where(r => r.PropertyId == propertyId)
            .ToList();

            var returnInfo = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(returnInfo);


        }

        [HttpPost("property/{propertyId}")]
        [Authorize(Policy = "TenantOnly")]
        public IActionResult PostReviewForProperty(AddReviewForOwner request, int propertyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int reviewerId = int.Parse(userIdClaim.Value);

            var propertyExists = _context.Properties.Any(p => p.Id == propertyId);
            if (!propertyExists)
                return NotFound($"Property with ID {propertyId} not found.");

            var reviewedTenantExists = _context.Users.Any(u => u.Id == request.ReviewedTenantId);
            if (!reviewedTenantExists)
                return NotFound($"Reviewed tenant with ID {request.ReviewedTenantId} not found.");

            var review = new Review
            {
                PropertyId = propertyId,
                ReviewerId = reviewerId,
                ReviewedTenantId = request.ReviewedTenantId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewedOn = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Ok(review);
        }



        [HttpGet("owner-by-property/{propertyId}")]
        public IActionResult GetReviewsAboutOwnerByProperty(int propertyId)
        {
            var reviews = _context.Reviews
                .Where(r => r.PropertyId == propertyId)
                .ToList();

            var returnInfo = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(returnInfo);
        }

        [HttpPost("review-owner-by-property/{propertyId}")]
        [Authorize(Policy = "TenantOnly")]
        public IActionResult PostReviewAboutOwnerByProperty(int propertyId, AddReview request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int reviewerId = int.Parse(userIdClaim.Value);

            var property = _context.Properties.FirstOrDefault(p => p.Id == propertyId);
            if (property == null)
                return NotFound("Property not found.");

            var review = new Review
            {
                PropertyId = propertyId,
                ReviewerId = reviewerId,
                ReviewedTenantId = property.UserId, 
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewedOn = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Ok(review);
        }



        [HttpGet("tenant/{tenantId}")]
        public IActionResult GetReviewsAboutTenant(int tenantId)
        {
            var reviews = _context.Reviews
                .Where(r => r.ReviewedTenantId == tenantId)
                .ToList();

            var returnInfo = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(returnInfo);
        }


        [HttpPost("add-tenant/{tenantId}")]
        [Authorize(Policy = "OwnerOnly")]
        public IActionResult PostReviewAboutTenant(AddReview request, int tenantId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.NameId);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int reviewerId = int.Parse(userIdClaim.Value);

            var rental = _context.Rentals.FirstOrDefault(r => r.UserId == tenantId);

            if (rental == null)
                return NotFound("No rental found for tenant.");

            var review = new Review
            {
                PropertyId = rental.PropertyId,
                ReviewerId = reviewerId,
                ReviewedTenantId = tenantId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewedOn = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Ok(review);
        }


    }
}
