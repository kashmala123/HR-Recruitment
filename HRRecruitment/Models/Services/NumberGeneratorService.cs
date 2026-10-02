using HRRecruitment.Data;
using System;
using System.Linq;

namespace myproject.Services
{
    public class NumberGeneratorService
    {
        private readonly ApplicationDbContext _context;

        public NumberGeneratorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GenerateVacancyNumber()
        {
            var count = _context.Vacancies.Count() + 1;
            if (count > 5000) throw new Exception("Vacancy limit reached");
            return $"V{count:D4}";
        }

        public string GenerateApplicantNumber()
        {
            var count = _context.Applicants.Count() + 1;
            if (count > 5000) throw new Exception("Applicant limit reached");
            return $"A{count:D4}";
        }

        public string GenerateInterviewNumber()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var count = _context.Interviews.Count(i => i.CreatedDate.Date == DateTime.Today) + 1;
            return $"INT-{today}-{count:D4}";
        }
    }
}