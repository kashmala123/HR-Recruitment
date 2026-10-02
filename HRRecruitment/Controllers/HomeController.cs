using HRRecruitment.Models;
using HRRecruitment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HRRecruitment.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public HomeController(IEmailService emailService, IOptions<EmailSettings> emailSettings)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string body = $@"
                <h2>New Contact Message</h2>
                <p><strong>Name:</strong> {model.Name}</p>
                <p><strong>Email:</strong> {model.Email}</p>
                <p><strong>Subject:</strong> {model.Subject}</p>
                <p><strong>Message:</strong><br/>{model.Message}</p>
            ";

                    // Admin inbox = FromEmail from configuration (no hard-coded address)
                    var adminEmail = string.IsNullOrWhiteSpace(_emailSettings.FromEmail)
                        ? _emailSettings.SmtpUsername
                        : _emailSettings.FromEmail;

                    await _emailService.SendEmailAsync(
                        adminEmail,
                        $"New Contact: {model.Subject}",
                        body
                    );

                    TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Sorry, an error occurred while sending your message. Please try again later.";
                }
                return RedirectToAction("Contact");
            }
            return View(model);
        }
        public IActionResult Index() { return View(); }
        public IActionResult About() { return View(); }
        public IActionResult Services() { return View(); }
        public IActionResult Team() { return View(); }
        public IActionResult Testimonial() { return View(); }
        public IActionResult Contact() { return View(); }
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
        public IActionResult UnsubscribeConfirmation()
        {
            return View();
        }
    }
}
