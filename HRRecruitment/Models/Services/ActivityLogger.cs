using System;
using HRRecruitment.Data;
using HRRecruitment.Models;

namespace HRRecruitment.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(string employeeId, string actionType, string description)
        {
            var log = new UserActivityLog
            {
                EmployeeId = employeeId,
                ActionType = actionType,
                Description = description,
                Timestamp = DateTime.Now
            };

            _context.UserActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}