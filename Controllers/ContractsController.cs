using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.Enums;
using PropertyRentalManagementSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;



namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public ContractsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("contracts/{rentalId}")]
        public ActionResult GetContractByRentalId(int rentalId)
        {
             var contract = _context.Contracts
                .Include(c => c.Rental)
                .ThenInclude(r => r.Property)
                .FirstOrDefault(c => c.RentalId == rentalId);

            if (contract == null)
            {
                return NotFound("Contract not found for the given rental ID.");
            }

            var contractDto = _mapper.Map<ContractDto>(contract);
            return Ok(contractDto);
        }


        [HttpPost("contracts/{rentalId}")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult CreateContract(int rentalId, ContractDto contractDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not found in token.");

            int ownerId = int.Parse(userIdClaim.Value);

            var rental = _context.Rentals
                .Include(r => r.Property)
                .FirstOrDefault(r => r.Id == rentalId);

            if (rental == null)
                return NotFound("Rental not found.");

            if (rental.Property.UserId != ownerId)
                return Forbid("Only the owner of this property can create a contract.");

            var contract = new Contract
            {
                RentalId = rentalId,
                Terms = contractDto.Terms,
                Conditions = contractDto.Conditions,
                Amount = contractDto.Amount,
                ContractStatus = CONTRACT_STATUS.DRAFT
            };

            _context.Contracts.Add(contract);
            _context.SaveChanges();

            var returnContract = _mapper.Map<ContractDto>(contract);
            return Ok(returnContract);
        }

        
        [HttpPut("contracts/{id}/sign")]
        [Authorize(Policy = "TenantOnly")]
        public ActionResult SignContract(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int userId = int.Parse(userIdClaim.Value);

            var contract = _context.Contracts
                .Include(c => c.Rental)
                .ThenInclude(r => r.User)
                .FirstOrDefault(c => c.Id == id);

            if (contract == null)
                return NotFound("Contract not found.");

            if (contract.Rental.UserId != userId)
                return Forbid("You are not allowed to sign this contract.");

            contract.ContractStatus = CONTRACT_STATUS.SIGNED;

            _context.Contracts.Update(contract);
            _context.SaveChanges();

            var contractDto = _mapper.Map<ContractDto>(contract);
            return Ok(contractDto);
        }


    }
}
