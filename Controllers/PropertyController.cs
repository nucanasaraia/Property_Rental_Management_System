using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.DTOs.PropertiesDTOs;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public PropertyController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("properties")]
        public ActionResult GetProperties()
        {
            var properties = _context.Properties.ToList();

            var returnInfo = _mapper.Map<List<PropertyListDto>>(properties);

            return Ok(returnInfo);
        }


        [HttpGet("properties-by-price")]
        public ActionResult GetByPrice()
        {
            var MostToLeast = _context.Properties
                .OrderByDescending(x => x.Price)
                .ToList();

            var returnInfo = _mapper.Map<List<PropertyListDto>>(MostToLeast);

            return Ok(returnInfo);
        }


        [HttpGet("properties-by-roomNumber")]
        public ActionResult GetByRoomNumbers()
        {
            var MostToLeast = _context.Properties
                .OrderByDescending(x => x.NumberOfRooms)
                .ToList();

            var returnInfo = _mapper.Map<List<PropertyListDto>>(MostToLeast);

            return Ok(returnInfo);
        }


        [HttpGet("properties-by-address")]
        public ActionResult GetByAddress()
        {
            var sorted = _context.Properties
                .OrderBy(p => p.Address)
                .ToList();

            var result = _mapper.Map<List<PropertyListDto>>(sorted);

            return Ok(result);
        }


        [HttpGet("properties/{id}")]
        
        public ActionResult GetById(int id)
        {
            var property = _context.Properties
                .FirstOrDefault(x => x.Id == id);

            if (property == null)
                return NotFound();

            var result = _mapper.Map<PropertyDetailDto>(property);

            return Ok(result);
        }


        [HttpPost("add-properties")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult AddPropery(AddProperty requests)
        {
            var property = _mapper.Map<Property>(requests);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            property.UserId = int.Parse(userId); 

            _context.Properties.Add(property);
            _context.SaveChanges();

            return Ok(property);
        }


        [HttpPut("change-property/{id}")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult ChangeProperty(int id, AddProperty requests)
        {
            var property = _context.Properties.FirstOrDefault(x => x.Id == id);

            if (property == null)
            {
                return NotFound($"Property with ID {id} not found.");
            }

            _mapper.Map(requests, property);

            _context.SaveChanges();

            var returnInfo = _mapper.Map<PropertyListDto>(property);
            return Ok(returnInfo);
        }

        [HttpDelete("delete-property/{id}")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult DeleteProperty(int id)
        {
            var property = _context.Properties.FirstOrDefault(x => x.Id == id);

            if (property == null)
            {
                return NotFound($"Property with ID {id} not found.");
            }

            _context.Properties.Remove(property);

            _context.SaveChanges();
            return Ok($"Property with ID {id} has been deleted.");
        }

        [Authorize]
        [HttpGet("properties/my")]
        public ActionResult GetMyOwnedProperties()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int userId = int.Parse(userIdClaim.Value);



            var properties = _context.Properties
                .Where(p => p.UserId == userId)
                .ToList();

            return Ok(properties);
        }


    }
}
