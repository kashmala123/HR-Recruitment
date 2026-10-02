using HRRecruitment.Data;
using HRRecruitment.Models;
using HRRecruitment.Services;
using HRRecruitment.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myproject.Services;
using System.IO;
using System.Security.Claims;
using HRRecruitment.Services;
namespace HRRecruitment.Controllers
{
    public class AccountController : Controller
    {
        private readonly IActivityLogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly NumberGeneratorService _numberGenerator;
        private readonly IWebHostEnvironment _env;
        private readonly HRRecruitment.Services.IEmailService _emailService;

        public AccountController(
            ApplicationDbContext context,
            IActivityLogger logger,
            NumberGeneratorService numberGenerator,
            IWebHostEnvironment env,
            HRRecruitment.Services.IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _numberGenerator = numberGenerator;
            _env = env;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userType = User.FindFirstValue("UserType");
                if (userType == "Applicant")
                    return RedirectToAction("Index", "UserPanel");
                else
                {
                    var role = User.FindFirstValue(ClaimTypes.Role);
                    if (role == "HR")
                        return RedirectToAction("Dashboard", "HR");
                    else if (role == "Interviewer")
                        return RedirectToAction("MyInterviews", "Interviewer");
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // First try employee (HR/Interviewer)
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == model.Email);

                if (employee != null && PasswordHelper.Verify(model.Password, employee.Password))
                {
                    // Upgrade plain-text password to hash on successful login
                    if (!PasswordHelper.IsHashed(employee.Password))
                    {
                        employee.Password = PasswordHelper.Hash(model.Password);
                        await _context.SaveChangesAsync();
                    }

                    employee.LastLoginDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                        new Claim(ClaimTypes.Name, employee.EmployeeName ?? "User"),
                        new Claim(ClaimTypes.Email, employee.Email),
                        new Claim("EmployeeNumber", employee.EmployeeNumber),
                        new Claim(ClaimTypes.Role, employee.Role ?? "User"),
                        new Claim("UserType", "Employee")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        authProperties);

                    _logger.Log(employee.EmployeeId.ToString(), "Login", "Employee logged in.");

                    if (employee.Role == "HR")
                        return RedirectToAction("Dashboard", "HR");
                    else if (employee.Role == "Interviewer")
                        return RedirectToAction("MyInterviews", "Interviewer");
                    else
                        return RedirectToAction("Index", "Home");
                }

                // Then try applicant
                var applicant = await _context.Applicants
                    .FirstOrDefaultAsync(a => a.Email == model.Email);

                if (applicant != null && PasswordHelper.Verify(model.Password, applicant.Password))
                {
                    if (!PasswordHelper.IsHashed(applicant.Password))
                    {
                        applicant.Password = PasswordHelper.Hash(model.Password);
                        await _context.SaveChangesAsync();
                    }

                    // Log activity
                    _context.ApplicantActivityLogs.Add(new ApplicantActivityLog
                    {
                        ApplicantId = applicant.ApplicantId,
                        ActionType = "Login",
                        Description = "User logged in",
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, applicant.ApplicantId.ToString()),
                        new Claim(ClaimTypes.Name, applicant.FirstName + " " + applicant.LastName),
                        new Claim(ClaimTypes.Email, applicant.Email),
                        new Claim("UserType", "Applicant"),
                        new Claim("ProfilePicture", applicant.ProfilePicture ?? "")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        authProperties);

                    return RedirectToAction("Index", "UserPanel");
                }

                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ApplicantRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existing = await _context.Applicants.FirstOrDefaultAsync(a => a.Email == model.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var employeeExists = await _context.Employees.AnyAsync(e => e.Email == model.Email);
                if (employeeExists)
                {
                    ModelState.AddModelError("Email", "Email already registered as employee.");
                    return View(model);
                }

                // Split FullName into first and last
                var nameParts = model.FullName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Generate a unique applicant number
                var applicantNumber = _numberGenerator.GenerateApplicantNumber();

                var applicant = new Applicant
                {
                    ApplicantNumber = applicantNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = model.Email,
                    Password = PasswordHelper.Hash(model.Password),
                    Phone = "Not provided",
                    Address = "",
                    HighestQualification = "",
                    CurrentCompany = "",
                    ResumePath = "",
                    ExperienceYears = 0,
                    Status = "Not in Process",
                    CreatedDate = DateTime.Now
                };

                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();

                // Log registration
                _context.ApplicantActivityLogs.Add(new ApplicantActivityLog
                {
                    ApplicantId = applicant.ApplicantId,
                    ActionType = "Register",
                    Description = "New account created",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // Do NOT auto-login — send user to login page first
                TempData["SuccessMessage"] = "Registration successful! Please log in with your email and password.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Change password for employees (optional)
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(employeeIdClaim))
                    return RedirectToAction("Login");

                var employeeId = int.Parse(employeeIdClaim);
                var employee = await _context.Employees.FindAsync(employeeId);

                if (employee != null && PasswordHelper.Verify(model.CurrentPassword, employee.Password))
                {
                    employee.Password = PasswordHelper.Hash(model.NewPassword);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("ChangePassword");
                }
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// JSON notifications for the top-bar bell icon (all dashboards).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Json(new { items = Array.Empty<object>(), count = 0 });

            var userType = User.FindFirstValue("UserType");
            var list = new List<object>();

            if (userType == "Applicant")
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int applicantId))
                {
                    var logs = await _context.ApplicantActivityLogs
                        .Where(l => l.ApplicantId == applicantId)
                        .OrderByDescending(l => l.Timestamp)
                        .Take(8)
                        .ToListAsync();

                    foreach (var log in logs)
                    {
                        list.Add(new
                        {
                            title = log.ActionType ?? "Update",
                            message = log.Description ?? "",
                            time = log.Timestamp.ToLocalTime().ToString("dd MMM, HH:mm"),
                            icon = "fa-info-circle"
                        });
                    }

                    // Also surface interview status from applications
                    var apps = await _context.ApplicantVacancies
                        .Where(av => av.ApplicantId == applicantId)
                        .OrderByDescending(av => av.AttachedDate)
                        .Take(5)
                        .ToListAsync();
                    foreach (var av in apps.Where(a => a.CurrentStatus != null && a.CurrentStatus != "Applied"))
                    {
                        list.Add(new
                        {
                            title = "Application Update",
                            message = $"Status: {av.CurrentStatus}",
                            time = av.AttachedDate.ToLocalTime().ToString("dd MMM, HH:mm"),
                            icon = "fa-briefcase"
                        });
                    }
                }
            }
            else
            {
                // HR / Interviewer – recent interviews involving them
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int empId))
                {
                    var role = User.FindFirstValue(ClaimTypes.Role);
                    IQueryable<Interview> q = _context.Interviews
                        .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Applicant)
                        .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Vacancy);

                    if (role == "Interviewer")
                        q = q.Where(i => i.InterviewerId == empId);
                    else
                        q = q.Where(i => i.CreatedBy == empId || true); // HR sees recent

                    var interviews = await q.OrderByDescending(i => i.CreatedDate).Take(8).ToListAsync();
                    foreach (var i in interviews)
                    {
                        var name = i.ApplicantVacancy?.Applicant?.FullName ?? "Applicant";
                        var job = i.ApplicantVacancy?.Vacancy?.JobTitle ?? "Position";
                        list.Add(new
                        {
                            title = $"Interview {i.Status}",
                            message = $"{name} – {job} on {i.InterviewDate:dd MMM}",
                            time = i.CreatedDate.ToLocalTime().ToString("dd MMM, HH:mm"),
                            icon = i.Status == "Scheduled" ? "fa-calendar-check" : "fa-clipboard-check"
                        });
                    }
                }
            }

            var items = list.Take(10).ToList();
            return Json(new { items, count = items.Count });
        }

        // ========== FORGOT / RESET PASSWORD ==========
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Please enter your email address.";
                return View();
            }

            // Always show success message (don't reveal if email exists)
            TempData["SuccessMessage"] = "If an account exists with that email, a temporary password has been sent.";

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            var applicant = employee == null
                ? await _context.Applicants.FirstOrDefaultAsync(a => a.Email == email)
                : null;

            if (employee == null && applicant == null)
                return View();

            var tempPassword = PasswordHelper.GenerateTemporaryPassword(10);
            var hashed = PasswordHelper.Hash(tempPassword);

            if (employee != null)
                employee.Password = hashed;
            else
                applicant.Password = hashed;

            await _context.SaveChangesAsync();

            try
            {
                string name = employee?.EmployeeName ?? (applicant.FirstName + " " + applicant.LastName);
                string body = $@"
                    <h2>Password Reset</h2>
                    <p>Dear {name},</p>
                    <p>Your password has been reset. Use the temporary password below to log in:</p>
                    <p style='font-size:18px;'><strong>{tempPassword}</strong></p>
                    <p>Please change your password immediately after logging in.</p>
                    <p>Best regards,<br/>HR Recruitment Team</p>";
                await _emailService.SendEmailAsync(email, "Password Reset - HR Recruitment", body);
            }
            catch
            {
                TempData["ErrorMessage"] = "Could not send the email. Please contact the administrator.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(employeeIdClaim))
                return RedirectToAction("Login");

            var employeeId = int.Parse(employeeIdClaim);
            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null)
                return NotFound();

            // Use different views based on role
            if (employee.Role == "Interviewer")
                return View("InterviewerProfile", employee);

            return View("Profile", employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profileImage)
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(employeeIdClaim))
                return RedirectToAction("Login");

            var employeeId = int.Parse(employeeIdClaim);
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound();

            // Fallback if model binding missed the file
            if ((profileImage == null || profileImage.Length == 0) && Request.Form.Files.Count > 0)
                profileImage = Request.Form.Files["profileImage"] ?? Request.Form.Files.FirstOrDefault();

            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image to upload.";
                return RedirectToAction("Profile");
            }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif", ".bmp" };
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp" };
            var ext = Path.GetExtension(profileImage.FileName)?.ToLowerInvariant() ?? "";
            var contentType = (profileImage.ContentType ?? "").ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || (!allowedExt.Contains(ext) && !allowedTypes.Contains(contentType)))
            {
                TempData["ErrorMessage"] = "Only JPG, PNG, GIF, WEBP or BMP images are allowed.";
                return RedirectToAction("Profile");
            }
            if (profileImage.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Image must be less than 5 MB.";
                return RedirectToAction("Profile");
            }

            if (ext == ".jfif" || (string.IsNullOrEmpty(ext) && contentType.Contains("jpeg")))
                ext = ".jpg";
            if (string.IsNullOrEmpty(ext))
                ext = ".jpg";

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(employee.ProfilePicture) &&
                employee.ProfilePicture.StartsWith("/uploads/profiles/"))
            {
                var oldPath = Path.Combine(_env.WebRootPath, employee.ProfilePicture.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"employee_{employeeId}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }
            employee.ProfilePicture = $"/uploads/profiles/{fileName}";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile picture updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}