using HRRecruitment.Data;
using HRRecruitment.Models;
using HRRecruitment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;


namespace HRRecruitment.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly myproject.Services.IEmailService _emailService;

        public HRController(ApplicationDbContext context, IWebHostEnvironment env, myproject.Services.IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }
     
        private Employee GetCurrentHR()
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(employeeIdClaim))
                return null;

            var employeeId = int.Parse(employeeIdClaim);
            return _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == employeeId);
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var model = new HRDashboardViewModel
            {
                CurrentHR = hr,
                TotalVacancies = _context.Vacancies.Count(),
                OpenVacancies = _context.Vacancies.Count(v => v.Status == "Open"),
                ClosedVacancies = _context.Vacancies.Count(v => v.Status == "Closed"),
                SuspendedVacancies = _context.Vacancies.Count(v => v.Status == "Suspended"),
                TotalApplicants = _context.Applicants.Count(),
                ApplicantsInProcess = _context.Applicants.Count(a => a.Status == "In Process"),
                HiredApplicants = _context.Applicants.Count(a => a.Status == "Hired"),
                UpcomingInterviews = _context.Interviews.Count(i => i.Status == "Scheduled" && i.InterviewDate > today),
                TodayInterviews = _context.Interviews.Count(i => i.InterviewDate.Date == today && i.Status == "Scheduled"),
                PendingResults = _context.Interviews.Count(i => i.Status == "Completed" && i.Result == "Pending"),
                RecentVacancies = _context.Vacancies.OrderByDescending(v => v.CreatedDate).Take(5).ToList(),
                RecentApplicants = _context.Applicants.OrderByDescending(a => a.CreatedDate).Take(5).ToList(),
                TodayInterviewList = _context.Interviews
                    .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Applicant)
                    .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Vacancy)
                    .Include(i => i.Interviewer)
                    .Where(i => i.InterviewDate.Date == today && i.Status == "Scheduled")
                    .ToList()
            };

            return View(model);
        }

        // Vacancies
        public IActionResult Vacancies(string searchTerm)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var query = _context.Vacancies
                .Include(v => v.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v => v.VacancyNumber.Contains(searchTerm) || v.JobTitle.Contains(searchTerm));
            }

            var vacancies = query.OrderByDescending(v => v.CreatedDate).ToList();
            return View(vacancies);
        }

        // Create Vacancy - GET
        // GET: /HR/CreateVacancy
        public IActionResult CreateVacancy()
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            // Populate departments dropdown (if needed in the view)
            var departments = _context.Departments
                .Select(d => new { d.DepartmentId, d.DepartmentName })
                .ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            // Return an empty view model
            return View(new CreateVacancyViewModel());
        }

        // POST: /HR/CreateVacancy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVacancy(CreateVacancyViewModel model)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            // Repopulate departments dropdown in case of validation errors
            var departments = _context.Departments
                .Select(d => new { d.DepartmentId, d.DepartmentName })
                .ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            if (ModelState.IsValid)
            {
                // Map the view model to the Vacancy entity
                var vacancy = new Vacancy
                {
                    JobTitle = model.JobTitle,
                    JobDescription = model.JobDescription,
                    RequiredQualifications = model.RequiredQualifications,
                    TotalOpenings = model.TotalOpenings,
                    DepartmentId = model.DepartmentId,
                    ClosingDate = model.ClosingDate,
                    CreatedBy = hr.EmployeeId,
                    CreatedDate = DateTime.Today,
                    Status = "Open",
                    RemainingOpenings = model.TotalOpenings
                };

                // Generate VacancyNumber (already handled by your existing logic)
                var count = await _context.Vacancies.CountAsync() + 1;
                vacancy.VacancyNumber = $"V{count:D4}";

                _context.Vacancies.Add(vacancy);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Vacancy {vacancy.VacancyNumber} created successfully!";
                return RedirectToAction("Vacancies");
            }

            // If validation fails, return the same view with the model
            return View(model);
        }
        // Edit Vacancy - GET
        public IActionResult EditVacancy(int id)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var vacancy = _context.Vacancies.Find(id);
            if (vacancy == null) return NotFound();

            // Populate departments dropdown
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", vacancy.DepartmentId);


            return View(vacancy);
        }

        // Edit Vacancy - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVacancy(int id, Vacancy model)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", model.DepartmentId);

            // Use the route ID, not the model's ID (which might be 0)
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null) return NotFound();

            // Update fields
            vacancy.JobTitle = model.JobTitle;
            vacancy.JobDescription = model.JobDescription;
            vacancy.RequiredQualifications = model.RequiredQualifications;
            vacancy.DepartmentId = model.DepartmentId;
            vacancy.ClosingDate = model.ClosingDate;

            if (model.TotalOpenings > vacancy.TotalOpenings)
            {
                var additional = model.TotalOpenings - vacancy.TotalOpenings;
                vacancy.TotalOpenings = model.TotalOpenings;
                vacancy.RemainingOpenings += additional;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Vacancy updated successfully!";
            return RedirectToAction("Vacancies");
        }
        // Change Vacancy Status
        [HttpPost]
        public async Task<IActionResult> ChangeVacancyStatus(int id, string status, string reason = null)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                return Json(new { success = false, message = "Vacancy not found" });

            if (vacancy.Status == "Closed" && status != "Closed")
                return Json(new { success = false, message = "Closed vacancy cannot be reopened" });

            vacancy.Status = status;
            if (status == "Closed")
            {
                vacancy.ClosingDate = DateTime.Today;
                vacancy.ClosingReason = reason;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Vacancy {status} successfully" });
        }

        // Applicants
        public IActionResult Applicants(string searchTerm)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var query = _context.Applicants.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.ApplicantNumber.Contains(searchTerm) ||
                                        a.FirstName.Contains(searchTerm) ||
                                        a.LastName.Contains(searchTerm));
            }

            var applicants = query.OrderByDescending(a => a.CreatedDate).ToList();
            return View(applicants);
        }

        // Create Applicant - GET
        // GET: /HR/CreateApplicant
        public IActionResult CreateApplicant()
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;
            return View(new CreateApplicantViewModel());
        }

        // POST: /HR/CreateApplicant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateApplicant(CreateApplicantViewModel model)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Applicants.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                // Generate applicant number
                var count = await _context.Applicants.CountAsync() + 1;
                var applicantNumber = $"A{count:D4}";

                // Create applicant entity
                var applicant = new Applicant
                {
                    ApplicantNumber = applicantNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address ?? "",
                    HighestQualification = model.HighestQualification ?? "",
                    ExperienceYears = model.ExperienceYears ?? 0,
                    CurrentCompany = model.CurrentCompany ?? "",
                    ResumePath = "",
                    Status = "Not in Process",
                    CreatedDate = DateTime.Now,
                    CreatedBy = hr.EmployeeId,
                    Password = "" // set below
                };

                // Generate temporary password and hash it
                var tempPassword = HRRecruitment.Services.PasswordHelper.GenerateTemporaryPassword(10);
                applicant.Password = HRRecruitment.Services.PasswordHelper.Hash(tempPassword);

                // Handle resume upload (optional)
                if (model.Resume != null && model.Resume.Length > 0)
                {
                    var allowed = new[] { ".pdf", ".doc", ".docx" };
                    var ext = Path.GetExtension(model.Resume.FileName).ToLowerInvariant();
                    if (allowed.Contains(ext) && model.Resume.Length <= 10 * 1024 * 1024)
                    {
                        var resumesFolder = Path.Combine(_env.WebRootPath, "uploads", "resumes");
                        Directory.CreateDirectory(resumesFolder);
                        var fileName = $"resume_{applicantNumber}_{Guid.NewGuid():N}{ext}";
                        var filePath = Path.Combine(resumesFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Resume.CopyToAsync(stream);
                        }
                        applicant.ResumePath = $"/uploads/resumes/{fileName}";
                    }
                }

                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();

                // Email login credentials to the new applicant
                try
                {
                    string subject = "Your HR Recruitment Account Credentials";
                    string body = $@"
                        <h2>Welcome to HR Recruitment</h2>
                        <p>Dear {applicant.FirstName} {applicant.LastName},</p>
                        <p>An account has been created for you. Use the credentials below to log in:</p>
                        <ul>
                            <li><strong>Email:</strong> {applicant.Email}</li>
                            <li><strong>Temporary Password:</strong> {tempPassword}</li>
                        </ul>
                        <p>Please log in and change your password immediately from <strong>My Profile → Change Password</strong>.</p>
                        <p>Best regards,<br/>HR Recruitment Team</p>";
                    await _emailService.SendEmailAsync(applicant.Email, subject, body);
                }
                catch
                {
                    // Don't fail creation if email fails; HR can still share the password
                    TempData["ErrorMessage"] = "Applicant created, but the credentials email could not be sent. Temporary password: " + tempPassword;
                }

                TempData["SuccessMessage"] = $"Applicant {applicant.ApplicantNumber} created successfully! Login credentials have been emailed.";
                return RedirectToAction("Applicants");
            }

            return View(model);
        }

        // Edit Applicant - GET
        public IActionResult EditApplicant(int id)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var applicant = _context.Applicants.Find(id);
            if (applicant == null) return NotFound();

            return View(applicant);
        }

        // Edit Applicant - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplicant(Applicant model)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            // Remove validation errors for properties not present in the edit form
            ModelState.Remove("Password");
            ModelState.Remove("CreatedByEmployee");
            ModelState.Remove("ApplicantVacancies");

            if (ModelState.IsValid)
            {
                var applicant = await _context.Applicants.FindAsync(model.ApplicantId);
                if (applicant == null) return NotFound();

                // Check email uniqueness
                if (applicant.Email != model.Email && await _context.Applicants.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                applicant.FirstName = model.FirstName;
                applicant.LastName = model.LastName;
                applicant.Email = model.Email;
                applicant.Phone = model.Phone;
                applicant.Address = model.Address;
                applicant.HighestQualification = model.HighestQualification;
                applicant.ExperienceYears = model.ExperienceYears;
                applicant.CurrentCompany = model.CurrentCompany;

                // Status cannot be changed if already Hired or Banned
                if (applicant.Status != "Hired" && applicant.Status != "Banned")
                {
                    applicant.Status = model.Status;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Applicant updated successfully!";
                return RedirectToAction("Applicants");
            }

            return View(model);
        }

        // Attach Applicant to Vacancy - GET
        public IActionResult AttachApplicant(int id)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var applicant = _context.Applicants.Find(id);
            if (applicant == null) return NotFound();

            if (applicant.Status == "Hired" || applicant.Status == "Banned")
            {
                TempData["ErrorMessage"] = "Cannot attach Hired or Banned applicant";
                return RedirectToAction("Applicants");
            }

            // Exclude vacancies this applicant is already attached to
            var alreadyAttached = _context.ApplicantVacancies
                .Where(av => av.ApplicantId == id)
                .Select(av => av.VacancyId)
                .ToList();

            var vacancies = _context.Vacancies
                .Where(v => v.Status == "Open" && v.RemainingOpenings > 0 && !alreadyAttached.Contains(v.VacancyId))
                .Select(v => new { v.VacancyId, DisplayText = $"{v.VacancyNumber} - {v.JobTitle} ({v.RemainingOpenings} openings)" })
                .ToList();

            if (!vacancies.Any())
            {
                TempData["ErrorMessage"] = "No open vacancies available to attach (all open ones may already be linked to this applicant).";
                return RedirectToAction("Applicants");
            }

            var model = new AttachApplicantViewModel
            {
                ApplicantId = applicant.ApplicantId,
                ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                ApplicantNumber = applicant.ApplicantNumber,
                VacanciesList = new SelectList(vacancies, "VacancyId", "DisplayText")
            };

            return View(model);
        }
        // Attach Applicant to Vacancy - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttachApplicant(int applicantId, int vacancyId)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var existing = await _context.ApplicantVacancies
                .FirstOrDefaultAsync(av => av.ApplicantId == applicantId && av.VacancyId == vacancyId);

            if (existing != null)
            {
                TempData["ErrorMessage"] = "Applicant already attached to this vacancy";
                return RedirectToAction("Applicants");
            }

            var applicant = await _context.Applicants.FindAsync(applicantId);
            var vacancy = await _context.Vacancies.FindAsync(vacancyId);

            if (applicant.Status == "Hired" || applicant.Status == "Banned")
            {
                TempData["ErrorMessage"] = "Cannot attach Hired or Banned applicant";
                return RedirectToAction("Applicants");
            }

            if (vacancy.Status != "Open")
            {
                TempData["ErrorMessage"] = "Vacancy is not open";
                return RedirectToAction("Applicants");
            }

            if (vacancy.RemainingOpenings <= 0)
            {
                TempData["ErrorMessage"] = "No openings available";
                return RedirectToAction("Applicants");
            }

            var applicantVacancy = new ApplicantVacancy
            {
                ApplicantId = applicantId,
                VacancyId = vacancyId,
                AttachedDate = DateTime.Now,
                CurrentStatus = "Applied",
                AttachedBy = hr.EmployeeId
            };

            _context.ApplicantVacancies.Add(applicantVacancy);

            if (applicant.Status == "Not in Process")
            {
                applicant.Status = "In Process";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Applicant attached to vacancy successfully!";
            return RedirectToAction("Applicants");
        }

        // Vacancy Details
        public IActionResult VacancyDetails(int id)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var vacancy = _context.Vacancies
                .Include(v => v.Department)
                .Include(v => v.ApplicantVacancies)
                    .ThenInclude(av => av.Applicant)
                .FirstOrDefault(v => v.VacancyId == id);

            if (vacancy == null) return NotFound();

            return View(vacancy);
        }

        // Interviews
        public IActionResult Interviews(DateTime? fromDate, DateTime? toDate)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;

            var query = _context.Interviews
                .Include(i => i.Interviewer)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Vacancy)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.InterviewDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(i => i.InterviewDate <= toDate.Value);

            var interviews = query.OrderByDescending(i => i.InterviewDate).ToList();
            return View(interviews);
        }
        public IActionResult InterviewDetails(int id)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var interview = _context.Interviews
                .Include(i => i.Interviewer)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Vacancy)
                .FirstOrDefault(i => i.InterviewId == id);

            if (interview == null) return NotFound();

            return View(interview);
        }
     
     
        // Schedule Interview - GET
        public IActionResult ScheduleInterview(int applicantVacancyId)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var applicantVacancy = _context.ApplicantVacancies
                .Include(av => av.Applicant)
                .Include(av => av.Vacancy)
                .FirstOrDefault(av => av.ApplicantVacancyId == applicantVacancyId);

            if (applicantVacancy == null) return NotFound();

            // Use the vacancy's department, not the HR's department
            var vacancyDepartmentId = applicantVacancy.Vacancy.DepartmentId;

            var model = new ScheduleInterviewViewModel
            {
                ApplicantVacancyId = applicantVacancyId,
                ApplicantName = $"{applicantVacancy.Applicant.FirstName} {applicantVacancy.Applicant.LastName}",
                JobTitle = applicantVacancy.Vacancy.JobTitle,
                InterviewDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                InterviewMode = "Offline",
                InterviewRound = 1,
                InterviewersList = GetInterviewersSelectList(vacancyDepartmentId), // vacancy's department
                InterviewModes = GetInterviewModesSelectList()
            };

            return View(model);
        }

        // Schedule Interview - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterview(int applicantVacancyId, int interviewerId, DateTime interviewDate, TimeSpan startTime, TimeSpan endTime, string interviewMode, string meetingLink)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var applicantVacancy = await _context.ApplicantVacancies
                .Include(av => av.Applicant)
                .FirstOrDefaultAsync(av => av.ApplicantVacancyId == applicantVacancyId);

            if (applicantVacancy == null) return NotFound();

            if (interviewDate <= DateTime.Today)
            {
                TempData["ErrorMessage"] = "Interview date must be in future";
                return RedirectToAction("VacancyDetails", new { id = applicantVacancy.VacancyId });
            }

            // Check for time clash
            var timeClash = await _context.Interviews
                .AnyAsync(i => i.InterviewerId == interviewerId &&
                              i.InterviewDate == interviewDate &&
                              ((startTime >= i.StartTime && startTime < i.EndTime) ||
                               (endTime > i.StartTime && endTime <= i.EndTime)) &&
                              i.Status != "Cancelled");

            if (timeClash)
            {
                TempData["ErrorMessage"] = "Interviewer already has an interview at this time";
                return RedirectToAction("VacancyDetails", new { id = applicantVacancy.VacancyId });
            }
            var interview = new Interview
            {
                InterviewNumber = $"INT-{DateTime.Now:yyyyMMdd}-{(_context.Interviews.Count() + 1):D4}",
                ApplicantVacancyId = applicantVacancyId,
                InterviewerId = interviewerId,
                InterviewDate = interviewDate,
                StartTime = startTime,
                EndTime = endTime,
                InterviewMode = interviewMode,
                MeetingLink = meetingLink ?? "",
                Status = "Scheduled",
                Result = "Pending",
                CancellationReason = "",
                Feedback = "",   // 👈 add this line
                CreatedBy = hr.EmployeeId
            };

            _context.Interviews.Add(interview);
            applicantVacancy.CurrentStatus = "Interview Scheduled";

            await _context.SaveChangesAsync();

            // Send notification emails to interviewer + applicant
            try { _emailService.QueueInterviewNotification(interview); }
            catch { /* non-blocking */ }

            TempData["SuccessMessage"] = "Interview scheduled successfully! Notification emails have been sent.";
            return RedirectToAction("VacancyDetails", new { id = applicantVacancy.VacancyId });
        }

        // Cancel Interview
        [HttpPost]
        public async Task<IActionResult> CancelInterview(int id, string reason)
        {
            var hr = GetCurrentHR();
            if (hr == null) return Json(new { success = false, message = "Not authorized" });

            var interview = await _context.Interviews
                .Include(i => i.ApplicantVacancy)
                .FirstOrDefaultAsync(i => i.InterviewId == id);

            if (interview == null)
                return Json(new { success = false, message = "Interview not found" });

            if (interview.Status != "Scheduled")
                return Json(new { success = false, message = "Only scheduled interviews can be cancelled" });

            interview.Status = "Cancelled";
            interview.CancellationReason = reason;

            var applicantVacancy = interview.ApplicantVacancy;
            applicantVacancy.CurrentStatus = "Applied";

            await _context.SaveChangesAsync();

            try { _emailService.QueueInterviewUpdateNotification(interview, "Cancelled"); }
            catch { }

            return Json(new { success = true, message = "Interview cancelled successfully" });
        }
        // Reschedule Interview
        [HttpPost]
        public async Task<IActionResult> RescheduleInterview(int id, DateTime newDate, TimeSpan newStartTime, TimeSpan newEndTime)
        {
            var hr = GetCurrentHR();
            if (hr == null) return Json(new { success = false, message = "Not authorized" });

            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null) return Json(new { success = false, message = "Interview not found" });

            if (interview.Status != "Scheduled")
                return Json(new { success = false, message = "Only scheduled interviews can be rescheduled" });

            // Check time clash
            var timeClash = await _context.Interviews
                .AnyAsync(i => i.InterviewerId == interview.InterviewerId &&
                               i.InterviewDate == newDate &&
                               ((newStartTime >= i.StartTime && newStartTime < i.EndTime) ||
                                (newEndTime > i.StartTime && newEndTime <= i.EndTime)) &&
                               i.InterviewId != id &&
                               i.Status != "Cancelled");

            if (timeClash)
                return Json(new { success = false, message = "Interviewer already has an interview at this time" });

            interview.InterviewDate = newDate;
            interview.StartTime = newStartTime;
            interview.EndTime = newEndTime;

            await _context.SaveChangesAsync();

            try { _emailService.QueueInterviewUpdateNotification(interview, "Rescheduled"); }
            catch { }

            return Json(new { success = true, message = "Interview rescheduled successfully" });
        }
        // Reports
        public IActionResult Reports()
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            ViewBag.HR = hr;
            return View();
        }

        // Helper methods
        private SelectList GetInterviewersSelectList(int departmentId)
        {
            var interviewers = _context.Employees
                .Where(e => e.DepartmentId == departmentId && e.Role == "Interviewer" && e.IsActive)
                .Select(e => new { e.EmployeeId, FullName = $"{e.EmployeeName} ({e.EmployeeNumber})" })
                .ToList();
            return new SelectList(interviewers, "EmployeeId", "FullName");
        }

        private SelectList GetInterviewModesSelectList()
        {
            var modes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Offline", Text = "Offline (In-Person)" },
                new SelectListItem { Value = "Online", Text = "Online (Video Call)" },
                new SelectListItem { Value = "Phone", Text = "Phone Interview" }
            };
            return new SelectList(modes, "Value", "Text");
        }
        public IActionResult VacancyReport()
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var vacancies = _context.Vacancies
                .Include(v => v.Department)
                .Include(v => v.ApplicantVacancies)
                .ToList();

            var model = new VacancyReportViewModel
            {
                TotalVacancies = vacancies.Count,
                OpenVacancies = vacancies.Count(v => v.Status == "Open"),
                ClosedVacancies = vacancies.Count(v => v.Status == "Closed"),
                SuspendedVacancies = vacancies.Count(v => v.Status == "Suspended"),
                TotalOpenings = vacancies.Sum(v => v.TotalOpenings),
                FilledOpenings = vacancies.Sum(v => v.TotalOpenings - v.RemainingOpenings),
                Vacancies = vacancies
            };

            return View(model);
        }

        public IActionResult InterviewReport(DateTime? fromDate, DateTime? toDate)
        {
            var hr = GetCurrentHR();
            if (hr == null) return RedirectToAction("Login", "Account");

            var query = _context.Interviews
                .Include(i => i.Interviewer)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Vacancy)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.InterviewDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(i => i.InterviewDate <= toDate.Value);

            var interviews = query.ToList();

            var model = new InterviewReportViewModel
            {
                TotalInterviews = interviews.Count,
                ScheduledInterviews = interviews.Count(i => i.Status == "Scheduled"),
                CompletedInterviews = interviews.Count(i => i.Status == "Completed"),
                CancelledInterviews = interviews.Count(i => i.Status == "Cancelled"),
                SelectedCount = interviews.Count(i => i.Result == "Selected"),
                RejectedCount = interviews.Count(i => i.Result == "Rejected"),
                PendingResults = interviews.Count(i => i.Result == "Pending"),
                Interviews = interviews,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(model);
        }
    }
}