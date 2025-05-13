using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs;
using PropertyRentalManagementSystem.Enums.TicketEnum;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.Requests;
using PropertyRentalManagementSystem.SMTP;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceTicketController : ControllerBase
    {
        public readonly DataContext _context;

        public readonly IMapper _mapper;
        private readonly SMTPService _smtpService;

        public MaintenanceTicketController(DataContext context, IMapper mapper, SMTPService smtpService)
        {
            _context = context;
            _mapper = mapper;
            _smtpService = smtpService;
        }


        [HttpGet("all")]
        public ActionResult GetAllProperties()
        {
            var properties = _context.Properties.ToList();
            return Ok(properties);
        }

        [HttpGet("maintenance-property/{propertyId}")]
        public ActionResult GetMaintenance(int propertyId)
        {
            var maintenances = _context.MaintenanceTickets
                .Where(m => m.PropertyId == propertyId)
                .ToList();

            if (!maintenances.Any())
                return NotFound("No tickets found for this property.");

            var returnInfo = _mapper.Map<List<MaintenanceDTO>>(maintenances);
            return Ok(returnInfo);
        }



        [HttpPost("maintenance")]
        [Authorize(Policy = "TenantOnly")]
        public ActionResult AddTicket(AddTicket requests)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var property = _context.Properties
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == requests.PropertyId);

            if (property == null)
                return NotFound($"Property with ID {requests.PropertyId} not found.");

            var ticket = _mapper.Map<MaintenanceTicket>(requests);
            _context.MaintenanceTickets.Add(ticket);
            _context.SaveChanges();

            string email = property.User.Email;
            string subject = "New Maintenance Ticket Submitted";
            string body = $"Dear {property.User.LastName},<br><br>" +
                          $"A new maintenance ticket has been submitted for your property: <b>{property.Title}</b>.<br>" +
                          $"Issue: <b>{ticket.ProblemDescription}</b><br><br>Please review it in the system.<br><br>Thank you.";

            _smtpService.SendEmail(email, subject, body);

            return Ok(ticket);
        }



        [HttpPut("maintenance/{id}/update")]
        [Authorize(Policy = "TenantOnly")]
        public ActionResult UpdateTicket(int id, AddTicket requests)
        {
            var ticket = _context.MaintenanceTickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound($"Ticket with ID {id} not found.");

            var property = _context.Properties.Find(requests.PropertyId);
            if (property == null)
                return NotFound($"Property with ID {requests.PropertyId} not found.");

            _mapper.Map(requests, ticket); 
            _context.SaveChanges();

            var owner = _context.Users.FirstOrDefault(u => u.Id == property.UserId);
            if (owner != null)
            {
                string subject = "Maintenance Ticket Updated";
                string body = $"A maintenance ticket for your property (ID: {property.Id}) has been updated by the tenant.";
                _smtpService.SendEmail(owner.Email, subject, body);
            }

            var returnInfo = _mapper.Map<MaintenanceDTO>(ticket);
            return Ok(returnInfo);

        }


        [HttpPut("maintenance/{id}/resolve")]
        [Authorize(Policy = "OwnerOnly")]
        public ActionResult ResolveTicket(int id)
        {
            var ticket = _context.MaintenanceTickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound($"Ticket with ID {id} not found.");

            if (ticket.TicketStatus == TICKET_STATUS.CLOSED)
                return BadRequest("Ticket is already resolved.");

            ticket.TicketStatus = TICKET_STATUS.CLOSED;
            ticket.UpdatedOn = DateTime.UtcNow;
            _context.SaveChanges();

            var tenant = _context.Users.FirstOrDefault(u => u.Id == ticket.RentalId);
            if (tenant != null)
            {
                string subject = "Your Maintenance Ticket Has Been Resolved";
                string body = $"Your maintenance ticket (ID: {ticket.Id}) has been marked as resolved.";
                _smtpService.SendEmail(tenant.Email, subject, body);
            }

            var ticketDTO = _mapper.Map<MaintenanceDTO>(ticket);
            return Ok(ticketDTO);
        }

    }
}
