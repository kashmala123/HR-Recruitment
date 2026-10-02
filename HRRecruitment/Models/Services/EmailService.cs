using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using HRRecruitment.Data;
using HRRecruitment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace myproject.Services
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(
            ApplicationDbContext context,
            ILogger<EmailService> logger,
            IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _context.EmailNotifications.Add(new EmailNotification
                {
                    RecipientEmail = to,
                    RecipientType = "System",
                    Subject = subject,
                    Body = body.Length > 2000 ? body.Substring(0, 2000) : body,
                    Status = "Sent",
                    SentDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                try
                {
                    _context.EmailNotifications.Add(new EmailNotification
                    {
                        RecipientEmail = to,
                        RecipientType = "System",
                        Subject = subject,
                        Body = body.Length > 2000 ? body.Substring(0, 2000) : body,
                        Status = "Failed",
                        ErrorMessage = ex.Message,
                        CreatedDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
                catch { /* ignore logging failure */ }
                throw;
            }
        }

        public void QueueInterviewNotification(Interview interview)
        {
            _ = SendInterviewEmailsAsync(interview, "Scheduled");
        }

        public void QueueInterviewUpdateNotification(Interview interview, string changeType)
        {
            _ = SendInterviewEmailsAsync(interview, changeType);
        }

        private async Task SendInterviewEmailsAsync(Interview interview, string changeType)
        {
            try
            {
                // Load related data
                var full = await _context.Interviews
                    .Include(i => i.Interviewer)
                    .Include(i => i.ApplicantVacancy)
                        .ThenInclude(av => av.Applicant)
                    .Include(i => i.ApplicantVacancy)
                        .ThenInclude(av => av.Vacancy)
                    .FirstOrDefaultAsync(i => i.InterviewId == interview.InterviewId);

                if (full == null) return;

                var applicant = full.ApplicantVacancy?.Applicant;
                var vacancy = full.ApplicantVacancy?.Vacancy;
                var interviewer = full.Interviewer;

                string dateStr = full.InterviewDate.ToString("dddd, dd MMM yyyy");
                string timeStr = $"{full.StartTime:hh\\:mm} - {full.EndTime:hh\\:mm}";
                string mode = full.InterviewMode ?? "N/A";
                string link = string.IsNullOrEmpty(full.MeetingLink) ? "N/A" : full.MeetingLink;
                string vacancyTitle = vacancy?.JobTitle ?? "Position";

                // Email to Interviewer
                if (interviewer != null && !string.IsNullOrEmpty(interviewer.Email))
                {
                    string subject = $"Interview {changeType}: {vacancyTitle} ({full.InterviewNumber})";
                    string body = $@"
                        <h2>Interview {changeType}</h2>
                        <p>Dear {interviewer.EmployeeName},</p>
                        <p>An interview has been <strong>{changeType.ToLower()}</strong>.</p>
                        <ul>
                            <li><strong>Interview #:</strong> {full.InterviewNumber}</li>
                            <li><strong>Applicant:</strong> {applicant?.FullName}</li>
                            <li><strong>Position:</strong> {vacancyTitle}</li>
                            <li><strong>Date:</strong> {dateStr}</li>
                            <li><strong>Time:</strong> {timeStr}</li>
                            <li><strong>Mode:</strong> {mode}</li>
                            <li><strong>Meeting Link:</strong> {link}</li>
                        </ul>
                        <p>Please log in to the Interviewer panel for more details.</p>
                        <p>Best regards,<br/>HR Recruitment Team</p>";
                    try { await SendEmailAsync(interviewer.Email, subject, body); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed interviewer email"); }
                }

                // Email to Applicant
                if (applicant != null && !string.IsNullOrEmpty(applicant.Email))
                {
                    string subject = $"Your Interview has been {changeType}: {vacancyTitle}";
                    string body = $@"
                        <h2>Interview {changeType}</h2>
                        <p>Dear {applicant.FullName},</p>
                        <p>Your interview has been <strong>{changeType.ToLower()}</strong>.</p>
                        <ul>
                            <li><strong>Position:</strong> {vacancyTitle}</li>
                            <li><strong>Date:</strong> {dateStr}</li>
                            <li><strong>Time:</strong> {timeStr}</li>
                            <li><strong>Mode:</strong> {mode}</li>
                            <li><strong>Meeting Link:</strong> {link}</li>
                        </ul>
                        <p>Please log in to your Applicant panel for more details.</p>
                        <p>Best regards,<br/>HR Recruitment Team</p>";
                    try { await SendEmailAsync(applicant.Email, subject, body); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed applicant email"); }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending interview notification emails");
            }
        }
    }
}
