using HRRecruitment.Models;
using System.Threading.Tasks;

namespace myproject.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        void QueueInterviewNotification(Interview interview);
        void QueueInterviewUpdateNotification(Interview interview, string changeType);
    }
}