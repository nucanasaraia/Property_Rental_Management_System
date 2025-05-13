using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.DTOs.PaymentDTOs;
using PropertyRentalManagementSystem.Enums;
using PropertyRentalManagementSystem.Enums.PaymentEnum;
using PropertyRentalManagementSystem.Enums.RentalEnum;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.SMTP;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyRentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        public readonly DataContext _context;
        public readonly IMapper _mapper;
        private readonly SMTPService _smtpService;

        public PaymentsController(DataContext context, IMapper mapper, SMTPService smtpService)
        {
            _context = context;
            _mapper = mapper;
            _smtpService = smtpService;
        }


        [HttpGet("payments/{rentalId}")]
        public ActionResult GetPayments(int rentalId)
        {
            var payment = _context.Payments.FirstOrDefault(x => x.RentalId == rentalId);
            if (payment == null)
            {
                return NotFound("Contract not found for the given rental ID.");
            }

            var returnInfo = _mapper.Map<PaymentsDto>(payment);
            return Ok(returnInfo);
        }

        [HttpPost("add-payment/{rentalId}")]
        [Authorize(Policy = "TenantOnly")]
        public ActionResult AddPayment(int rentalId, PaymentsDto paymentDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int userId = int.Parse(userIdClaim.Value);

            var rental = _context.Rentals
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == rentalId);

            if (rental == null)
                return NotFound("Rental not found.");

            Console.WriteLine($"Token UserId: {userId}");
            Console.WriteLine($"Rental.UserId: {rental.UserId}");

            if (rental.UserId != userId)
                return Forbid("You can only make payments for your own rentals.");

            var payment = new Payment
            {
                RentalId = rentalId,
                Amount = paymentDto.Amount,
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = PAYMENT_STATUS.COMPLETED,
                PaymentType = paymentDto.PaymentType
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            var paymentResult = _mapper.Map<PaymentsDto>(payment);
            return Ok(paymentResult);
        }

        [HttpGet("upcoming-payments")]
        public ActionResult<IEnumerable<Payment>> GetUpcomingPayments()
        {
            var today = DateTime.UtcNow;

            var upcomingPayments = _context.Payments
                .Include(p => p.Rental)
                .Where(p => p.NextDuoDate > today && p.PaymentStatus == PAYMENT_STATUS.PENDING)
                .ToList();

            return Ok(upcomingPayments);
        }


        [HttpPost("send-payment-reminders")]
        [Authorize(Policy = "AdminOnly")] 
        public IActionResult SendPaymentReminders()
        {
            var today = DateTime.UtcNow;
            var upcomingThreshold = today.AddDays(3);

            var upcomingPayments = _context.Payments
                .Include(p => p.Rental).ThenInclude(r => r.User)
                .Where(p => p.NextDuoDate <= upcomingThreshold &&
                            p.NextDuoDate > today &&
                            p.PaymentStatus == PAYMENT_STATUS.PENDING)
                .ToList();

            var overduePayments = _context.Payments
                .Include(p => p.Rental).ThenInclude(r => r.User)
                .Where(p => p.NextDuoDate < today &&
                            p.PaymentStatus == PAYMENT_STATUS.PENDING)
                .ToList();

            foreach (var payment in upcomingPayments)
            {
                var email = payment.Rental.User.Email;
                var subject = "Upcoming Payment Reminder";
                var body = $"Dear {payment.Rental.User.LastName},<br><br>" +
                           $"This is a reminder that your rent payment of <b>${payment.Amount}</b> is due on <b>{payment.NextDuoDate.ToShortDateString()}</b>.<br><br>Please ensure timely payment to avoid penalties.<br><br>Thank you.";
                _smtpService.SendEmail(email, subject, body);
            }

            foreach (var payment in overduePayments)
            {
                var email = payment.Rental.User.Email;
                var subject = "Overdue Payment Notice";
                var body = $"Dear {payment.Rental.User.LastName},<br><br>" +
                           $"Your rent payment of <b>${payment.Amount}</b> was due on <b>{payment.NextDuoDate.ToShortDateString()}</b> and is now overdue.<br><br>Please make the payment as soon as possible.<br><br>Thank you.";
                _smtpService.SendEmail(email, subject, body);
            }

            return Ok(new
            {
                UpcomingNotified = upcomingPayments.Count,
                OverdueNotified = overduePayments.Count
            });
        }

    }
}
