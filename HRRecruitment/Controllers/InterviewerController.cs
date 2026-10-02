using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRRecruitment.Data;
using HRRecruitment.Models;
using System.Security.Claims;

namespace HRRecruitment.Controllers  // ✅ Fixed namespace
{
    [Authorize(Roles = "Interviewer")]
    public class InterviewerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly myproject.Services.IEmailService _emailService;

        public InterviewerController(ApplicationDbContext context, myproject.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private Employee GetCurrentInterviewer()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return null;
            return _context.Employees
                .FirstOrDefault(e => e.Email == email && e.Role == "Interviewer");
        }

        public IActionResult Dashboard()
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            ViewBag.Interviewer = interviewer;

            var interviews = _context.Interviews
                .Where(i => i.InterviewerId == interviewer.EmployeeId && i.Status == "Scheduled")
                .Count();

            ViewBag.TotalInterviews = interviews;

            return View();
        }


        public IActionResult MyInterviews(string searchString)
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            var interviews = _context.Interviews
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Vacancy)
                .Where(i => i.InterviewerId == interviewer.EmployeeId && i.Status == "Scheduled")
                .OrderBy(i => i.InterviewDate)
                .ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                interviews = interviews.Where(i =>
                    i.ApplicantVacancy.Applicant.FirstName.Contains(searchString) ||
                    i.ApplicantVacancy.Vacancy.JobTitle.Contains(searchString)).ToList();
            }

            return View(interviews);
        }
        public IActionResult CompletedInterviews()
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            var interviews = _context.Interviews
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Vacancy)
                .Where(i => i.InterviewerId == interviewer.EmployeeId && i.Status == "Completed")
                .OrderByDescending(i => i.CompletedDate)
                .ToList();

            return View(interviews);
        }
        public IActionResult InterviewDetails(int id)
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            var interview = _context.Interviews
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy).ThenInclude(av => av.Vacancy).ThenInclude(v => v.Department)
                .FirstOrDefault(i => i.InterviewId == id && i.InterviewerId == interviewer.EmployeeId);

            if (interview == null) return NotFound();
            ViewBag.Interviewer = interviewer;
            return View(interview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitResult(int id, string result, string feedback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(result) ||
                    (result != "Selected" && result != "Rejected"))
                {
                    TempData["ErrorMessage"] = "Please select Select or Reject.";
                    return RedirectToAction("InterviewDetails", new { id });
                }

                feedback ??= "";

                var interview = await _context.Interviews
                    .Include(i => i.ApplicantVacancy)
                        .ThenInclude(av => av.Applicant)
                    .Include(i => i.ApplicantVacancy)
                        .ThenInclude(av => av.Vacancy)
                    .FirstOrDefaultAsync(i => i.InterviewId == id);

                if (interview == null)
                {
                    TempData["ErrorMessage"] = "Interview not found.";
                    return RedirectToAction("MyInterviews");
                }

                if (interview.Status != "Scheduled")
                {
                    TempData["ErrorMessage"] = "Cannot submit result for this interview (already completed or cancelled).";
                    return RedirectToAction("MyInterviews");
                }

                interview.Status = "Completed";
                interview.Result = result;
                interview.Feedback = feedback;
                interview.CancellationReason = interview.CancellationReason ?? "";
                interview.MeetingLink = interview.MeetingLink ?? "";
                interview.CompletedDate = DateTime.Now;

                var applicantVacancy = interview.ApplicantVacancy;
                if (applicantVacancy != null)
                {
                    applicantVacancy.CurrentStatus = result == "Selected" ? "Selected" : "Rejected";

                    var applicant = applicantVacancy.Applicant;
                    if (applicant != null)
                    {
                        if (result == "Selected")
                        {
                            applicant.Status = "Hired";
                            var vacancy = applicantVacancy.Vacancy;
                            if (vacancy != null && vacancy.RemainingOpenings > 0)
                            {
                                vacancy.RemainingOpenings--;
                                if (vacancy.RemainingOpenings == 0)
                                    vacancy.Status = "Closed";
                            }
                        }
                        else
                        {
                            // Only mark applicant Rejected if they have no other active applications
                            var hasOtherActive = await _context.ApplicantVacancies
                                .AnyAsync(av => av.ApplicantId == applicant.ApplicantId &&
                                               av.ApplicantVacancyId != applicantVacancy.ApplicantVacancyId &&
                                               av.CurrentStatus != "Rejected" &&
                                               av.CurrentStatus != "Selected");
                            if (!hasOtherActive)
                                applicant.Status = "Rejected";
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Notify applicant (non-blocking)
                try
                {
                    var applicant = interview.ApplicantVacancy?.Applicant;
                    var vacancyTitle = interview.ApplicantVacancy?.Vacancy?.JobTitle ?? "the position";
                    if (applicant != null && !string.IsNullOrEmpty(applicant.Email))
                    {
                        string subject = $"Interview Result: {vacancyTitle}";
                        string body = $@"
                            <h2>Interview Result</h2>
                            <p>Dear {applicant.FullName},</p>
                            <p>Your interview result for <strong>{vacancyTitle}</strong> is: <strong>{result}</strong>.</p>
                            {(string.IsNullOrEmpty(feedback) ? "" : $"<p><strong>Feedback:</strong> {System.Net.WebUtility.HtmlEncode(feedback)}</p>")}
                            <p>Please log in to your Applicant panel for more details.</p>
                            <p>Best regards,<br/>HR Recruitment Team</p>";
                        await _emailService.SendEmailAsync(applicant.Email, subject, body);
                    }
                }
                catch { /* non-blocking */ }

                TempData["SuccessMessage"] = "Result submitted successfully!";
                return RedirectToAction("MyInterviews");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = "Could not save result: " + inner;
                return RedirectToAction("MyInterviews");
            }
        }
        public IActionResult Vacancies(string searchTerm)
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            ViewBag.Interviewer = interviewer;

            // ✅ Fix: Use correct property names
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

        public IActionResult Applicants(string searchTerm)
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            ViewBag.Interviewer = interviewer;

            // ✅ Fix: Use correct property names
            var query = _context.Applicants.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.ApplicantNumber.Contains(searchTerm) ||
                                        a.FirstName.Contains(searchTerm) ||
                                        a.LastName.Contains(searchTerm) ||
                                        a.Email.Contains(searchTerm));
            }

            var applicants = query.OrderByDescending(a => a.CreatedDate).ToList();
            return View(applicants);
        }

        public IActionResult VacancyDetails(int id)  // ✅ Fix: int id, not string
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            ViewBag.Interviewer = interviewer;

            var vacancy = _context.Vacancies
                .Include(v => v.Department)
                .Include(v => v.ApplicantVacancies)
                    .ThenInclude(av => av.Applicant)
                .FirstOrDefault(v => v.VacancyId == id);

            if (vacancy == null) return NotFound();

            return View(vacancy);
        }

        public IActionResult ApplicantDetails(int id)  // ✅ Fix: int id, not string
        {
            var interviewer = GetCurrentInterviewer();
            if (interviewer == null) return RedirectToAction("Login", "Account");

            ViewBag.Interviewer = interviewer;

            var applicant = _context.Applicants
                .Include(a => a.ApplicantVacancies)
                    .ThenInclude(av => av.Vacancy)
                .FirstOrDefault(a => a.ApplicantId == id);

            if (applicant == null) return NotFound();

            return View(applicant);
        }

        // ✅ Helper method to check time clash
        public IActionResult CheckTimeClash(int interviewerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeInterviewId = null)
        {
            var query = _context.Interviews
                .Where(i => i.InterviewerId == interviewerId &&
                           i.InterviewDate == date &&
                           i.Status != "Cancelled");

            if (excludeInterviewId.HasValue)
            {
                query = query.Where(i => i.InterviewId != excludeInterviewId.Value);
            }

            var clash = query.Any(i => (startTime >= i.StartTime && startTime < i.EndTime) ||
                                      (endTime > i.StartTime && endTime <= i.EndTime) ||
                                      (startTime <= i.StartTime && endTime >= i.EndTime));

            return Json(new { hasClash = clash });
        }
    }
}