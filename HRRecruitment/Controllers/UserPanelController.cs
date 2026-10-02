using HRRecruitment.Data;
using HRRecruitment.Filters;
using HRRecruitment.Models;
using HRRecruitment.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;

namespace HRRecruitment.Controllers
{
    [Authorize]
    [NoCache]
    public class UserPanelController : Controller
    {
        private readonly IActivityLogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserPanelController(ApplicationDbContext context, IActivityLogger logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        private int GetCurrentApplicantId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Dashboard
        public IActionResult Index()
        {
            var applicantId = GetCurrentApplicantId();
            var applicant = _context.Applicants.Find(applicantId);
            if (applicant == null) return NotFound();

            ViewBag.ApplicantName = applicant.FirstName + " " + applicant.LastName;
            ViewBag.ApplicantEmail = applicant.Email;
            ViewBag.ApplicantId = applicant.ApplicantId.ToString();
            ViewBag.Status = applicant.Status;

            // Get all applications
            var applications = _context.ApplicantVacancies
                .Where(av => av.ApplicantId == applicantId)
                .ToList();
            ViewBag.ApplicationsCount = applications.Count;

            // Count interviews that are scheduled (CurrentStatus == "Interview Scheduled")
            var scheduledInterviews = applications.Count(av => av.CurrentStatus == "Interview Scheduled");
            ViewBag.ScheduledInterviews = scheduledInterviews;

            // Recent activities
            var recentActivities = _context.ApplicantActivityLogs
                .Where(log => log.ApplicantId == applicantId)
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .ToList();
            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        // Profile GET
        [HttpGet]
        public IActionResult MyProfile()
        {
            var applicantId = GetCurrentApplicantId();
            var applicant = _context.Applicants.Find(applicantId);
            if (applicant == null) return NotFound();

            var model = new UserProfileVM
            {
                FullName = applicant.FirstName + " " + applicant.LastName,
                Email = applicant.Email,
                ProfilePicture = applicant.ProfilePicture
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> MyProfile(UserProfileVM model)
        {
            var applicantId = GetCurrentApplicantId();
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null) return NotFound();

            // Fallback: get file from form if model binding missed it
            var uploadedFile = model.ProfileImage;
            if ((uploadedFile == null || uploadedFile.Length == 0) && Request.Form.Files.Count > 0)
            {
                uploadedFile = Request.Form.Files["ProfileImage"] ?? Request.Form.Files.FirstOrDefault();
            }

            if (ModelState.IsValid)
            {
                // Split FullName into FirstName and LastName
                var nameParts = (model.FullName ?? "").Trim().Split(' ', 2);
                applicant.FirstName = nameParts[0];
                applicant.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                applicant.Email = model.Email;

                // Handle profile picture upload
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif", ".bmp" };
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp" };
                    var ext = Path.GetExtension(uploadedFile.FileName)?.ToLowerInvariant() ?? "";
                    var contentType = (uploadedFile.ContentType ?? "").ToLowerInvariant();

                    if (string.IsNullOrEmpty(ext) || (!allowedExt.Contains(ext) && !allowedTypes.Contains(contentType)))
                    {
                        ModelState.AddModelError("ProfileImage", "Only JPG, PNG, GIF, WEBP or BMP images are allowed.");
                        model.ProfilePicture = applicant.ProfilePicture;
                        return View(model);
                    }
                    if (uploadedFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfileImage", "Image must be less than 5 MB.");
                        model.ProfilePicture = applicant.ProfilePicture;
                        return View(model);
                    }

                    // Normalize extension
                    if (ext == ".jfif" || (string.IsNullOrEmpty(ext) && contentType.Contains("jpeg")))
                        ext = ".jpg";
                    if (string.IsNullOrEmpty(ext))
                        ext = ".jpg";

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    // Delete old picture if it was an uploaded file
                    if (!string.IsNullOrEmpty(applicant.ProfilePicture) &&
                        applicant.ProfilePicture.StartsWith("/uploads/profiles/"))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, applicant.ProfilePicture.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var fileName = $"applicant_{applicantId}_{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }
                    applicant.ProfilePicture = $"/uploads/profiles/{fileName}";
                }

                await _context.SaveChangesAsync();

                // Refresh the cookie with updated name/email/picture
                var updatedClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, applicant.ApplicantId.ToString()),
                    new Claim(ClaimTypes.Name, applicant.FirstName + " " + applicant.LastName),
                    new Claim(ClaimTypes.Email, applicant.Email),
                    new Claim("UserType", "Applicant"),
                    new Claim("ProfilePicture", applicant.ProfilePicture ?? "")
                };
                var identity = new ClaimsIdentity(updatedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("MyProfile");
            }

            model.ProfilePicture = applicant.ProfilePicture;
            return View(model);
        }

        // Change Password GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Change Password POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var applicantId = GetCurrentApplicantId();
                var applicant = _context.Applicants.Find(applicantId);

                if (applicant != null)
                {
                    if (PasswordHelper.Verify(model.CurrentPassword, applicant.Password))
                    {
                        applicant.Password = PasswordHelper.Hash(model.NewPassword);
                        _context.SaveChanges();

                        _context.ApplicantActivityLogs.Add(new ApplicantActivityLog
                        {
                            ApplicantId = applicant.ApplicantId,
                            ActionType = "PasswordChange",
                            Description = "Changed password",
                            Timestamp = DateTime.UtcNow
                        });
                        _context.SaveChanges();

                        ViewBag.SuccessMessage = "Password changed successfully!";
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "The current password you entered is incorrect.");
                    }
                }
            }
            return View(model);
        }

        // Help
        public IActionResult Help()
        {
            return View();
        }

        // My Applications (renamed from MyInterviews to avoid confusion)
        public IActionResult MyApplications(string searchString)
        {
            var applicantId = GetCurrentApplicantId();
            var applications = _context.ApplicantVacancies
                .Include(av => av.Vacancy)
                .Include(av => av.Interviews) // load interviews
                .Where(av => av.ApplicantId == applicantId)
                .ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                applications = applications.Where(s =>
                    s.ApplicantId.ToString().Contains(searchString) ||
                    s.Vacancy.VacancyNumber.Contains(searchString) ||
                    s.Vacancy.JobTitle.Contains(searchString) ||
                    s.CurrentStatus.Contains(searchString)
                ).ToList();
            }

            return View(applications);
        }

        // Vacancies list (only open vacancies)
        public IActionResult Vacancies()
        {
            var applicantId = GetCurrentApplicantId();

            // Get all open vacancies
            var vacancies = _context.Vacancies
                .Where(v => v.Status == "Open")
                .OrderByDescending(v => v.CreatedDate)
                .ToList();

            // Get IDs of vacancies the applicant has already applied to
            var appliedIds = _context.ApplicantVacancies
                .Where(av => av.ApplicantId == applicantId)
                .Select(av => av.VacancyId)
                .ToHashSet();

            ViewBag.AppliedVacancyIds = appliedIds;
            return View(vacancies);
        }

        // Apply to a vacancy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int vacancyId)  // use int, not string
        {
            var applicantId = GetCurrentApplicantId();
            var applicant = await _context.Applicants.FindAsync(applicantId);

            if (applicant.Status == "Hired" || applicant.Status == "Banned")
            {
                TempData["Error"] = "You cannot apply for more vacancies.";
                return RedirectToAction("Vacancies");
            }

            var vacancy = await _context.Vacancies.FindAsync(vacancyId);
            if (vacancy == null || vacancy.Status != "Open")
            {
                TempData["Error"] = "Vacancy not available.";
                return RedirectToAction("Vacancies");
            }

            // Check if already applied
            var alreadyApplied = await _context.ApplicantVacancies
                .AnyAsync(av => av.ApplicantId == applicantId && av.VacancyId == vacancyId);
            if (alreadyApplied)
            {
                TempData["Error"] = "You have already applied for this vacancy.";
                return RedirectToAction("Vacancies");
            }

            // Create application
            var application = new ApplicantVacancy
            {
                ApplicantId = applicantId,
                VacancyId = vacancyId,
                AttachedDate = DateTime.UtcNow,
                CurrentStatus = "Applied"
            };
            _context.ApplicantVacancies.Add(application);

            if (applicant.Status == "Not in Process")
            {
                applicant.Status = "In Process";
            }

            await _context.SaveChangesAsync();

            _context.ApplicantActivityLogs.Add(new ApplicantActivityLog
            {
                ApplicantId = applicantId,
                ActionType = "Apply",
                Description = $"Applied for {vacancy.JobTitle}",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted successfully!";
            return RedirectToAction("Vacancies");
        }
        public IActionResult VacancyDetails(int id)
        {
            var vacancy = _context.Vacancies
                .Include(v => v.Department)
                .FirstOrDefault(v => v.VacancyId == id);

            if (vacancy == null) return NotFound();

            return View(vacancy);
        }
    }
}