using HRRecruitment.Data;
using HRRecruitment.Models;
using HRRecruitment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HRRecruitment.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public NewsletterController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            {
                TempData["NewsletterError"] = "Please enter a valid email address.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Check if email already exists and is active
            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == email && s.IsActive);

            if (existing != null)
            {
                TempData["NewsletterInfo"] = "You are already subscribed!";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // If the email was previously unsubscribed, we can reactivate or create new.
            var inactive = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == email && !s.IsActive);

            Subscriber subscriber;
            if (inactive != null)
            {
                // Reactivate existing record
                inactive.IsActive = true;
                inactive.UnsubscribeToken = Guid.NewGuid().ToString(); // new token
                inactive.SubscribedDate = DateTime.UtcNow;
                subscriber = inactive;
            }
            else
            {
                // New subscriber
                subscriber = new Subscriber
                {
                    Email = email,
                    SubscribedDate = DateTime.UtcNow,
                    UnsubscribeToken = Guid.NewGuid().ToString(),
                    IsActive = true
                };
                _context.Subscribers.Add(subscriber);
            }
            await _context.SaveChangesAsync();

            // Send welcome email with unsubscribe link
            try
            {
                string subject = "Welcome to HR Recruitment Newsletter!";
                string unsubscribeUrl = Url.Action("Unsubscribe", "Newsletter", new { token = subscriber.UnsubscribeToken }, Request.Scheme);

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to HR Recruitment</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f4f4; font-family: Arial, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f4f4f4'>
         <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' border='0' bgcolor='#ffffff' style='border-collapse:collapse; margin:20px auto; border-radius:8px; overflow:hidden; box-shadow:0 2px 5px rgba(0,0,0,0.1);'>
                    <!-- Header with brand color -->
                    <tr>
                        <td bgcolor='#1a2b4c' style='padding:30px 20px; text-align:center;'>
                            <div style='display:inline-block; text-align:left;'>
                                <span style='font-size:14px; color:#ccc; letter-spacing:1px; display:block;'>Online</span>
                                <span style='font-size:28px; font-weight:700; color:#fff; text-transform:uppercase;'>Recruitment</span>
                                <span style='font-size:16px; font-weight:400; color:#4a90e2; display:block;'>Process</span>
                            </div>
                            <div style='display:inline-block; margin-left:15px; background:#ff6b6b; color:#fff; font-size:12px; font-weight:bold; padding:2px 10px; border-radius:20px;'>HR</div>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding:40px 30px;'>
                            <h2 style='margin-top:0; color:#1a2b4c;'>Welcome to HR Recruitment!</h2>
                            <p style='color:#555; line-height:1.6; margin:15px 0;'>Thank you for subscribing to our newsletter.</p>
                            <p style='color:#555; line-height:1.6; margin:15px 0;'>You'll now receive updates on new job opportunities, HR tips, and company news.</p>
                            <p style='color:#555; line-height:1.6; margin:25px 0;'>If you ever wish to unsubscribe, please click the button below:</p>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                <tr>
                                    <td align='center'>
                                        <a href='{unsubscribeUrl}' style='display:inline-block; background-color:#0d6efd; color:#ffffff; text-decoration:none; padding:12px 25px; border-radius:50px; font-weight:bold;'>Unsubscribe from newsletter</a>
                                    </td>
                                </tr>
                            </table>
                            <p style='color:#555; line-height:1.6; margin:30px 0 0;'>Best regards,<br>The HR Recruitment Team</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td bgcolor='#f8f9fa' style='padding:20px; text-align:center; color:#6c757d; font-size:12px;'>
                            <p style='margin:0 0 5px;'>© HR Recruitment. All rights reserved.</p>
                            <p style='margin:0;'>You received this email because you subscribed to our newsletter.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                // Log error (optional)
                Console.WriteLine($"Welcome email failed: {ex.Message}");
            }

            TempData["NewsletterSuccess"] = "Thank you for subscribing! A welcome email has been sent to your inbox.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // GET: /Newsletter/Unsubscribe?token=...
        public async Task<IActionResult> Unsubscribe(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token");
            }

            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token && s.IsActive);

            if (subscriber == null)
            {
                TempData["NewsletterError"] = "Invalid or already unsubscribed.";
                return RedirectToAction("Index", "Home");
            }

            // Deactivate (or delete) the subscriber
            subscriber.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["UnsubscribeMessage"] = "You have successfully unsubscribed from our newsletter.";
            return RedirectToAction("UnsubscribeConfirmation", "Home");
        }
    }
}