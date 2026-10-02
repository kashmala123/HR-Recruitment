using HRRecruitment.Models;
using Microsoft.EntityFrameworkCore;


namespace HRRecruitment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables from both projects
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<ApplicantVacancy> ApplicantVacancies { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<EmailNotification> EmailNotifications { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<ApplicantActivityLog> ApplicantActivityLogs { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Department ==========
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);
                entity.Property(e => e.DepartmentName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DepartmentCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.DepartmentCode).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // ========== Employee ==========
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                entity.Property(e => e.EmployeeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ProfilePicture).HasDefaultValue("/images/default-avatar.png");

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== Vacancy ==========
            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.HasKey(e => e.VacancyId);
                entity.Property(e => e.VacancyNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.VacancyNumber).IsUnique();
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasDefaultValue("Open");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ClosingReason).IsRequired(false);

                entity.HasOne(v => v.Department)
                    .WithMany(d => d.Vacancies)
                    .HasForeignKey(v => v.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.CreatedByEmployee)
                    .WithMany(e => e.CreatedVacancies)
                    .HasForeignKey(v => v.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== Applicant ==========
            modelBuilder.Entity<Applicant>(entity =>
            {
                entity.HasKey(e => e.ApplicantId);
                entity.Property(e => e.ApplicantNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ApplicantNumber).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ExperienceYears).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasDefaultValue("Not in Process");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(a => a.CreatedByEmployee)
                    .WithMany()
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========== ApplicantVacancy ==========
            modelBuilder.Entity<ApplicantVacancy>(entity =>
            {
                entity.HasKey(e => e.ApplicantVacancyId);
                entity.Property(e => e.CurrentStatus).HasDefaultValue("Applied");
                entity.Property(e => e.AttachedDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(av => av.Applicant)
                    .WithMany(a => a.ApplicantVacancies)
                    .HasForeignKey(av => av.ApplicantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(av => av.Vacancy)
                    .WithMany(v => v.ApplicantVacancies)
                    .HasForeignKey(av => av.VacancyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(av => av.AttachedByEmployee)
                    .WithMany()
                    .HasForeignKey(av => av.AttachedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(av => new { av.ApplicantId, av.VacancyId }).IsUnique();
            });

            // ========== Interview ==========
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasKey(e => e.InterviewId);
                entity.Property(e => e.InterviewNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.InterviewNumber).IsUnique();
                entity.Property(e => e.InterviewMode).HasDefaultValue("Offline");
                entity.Property(e => e.Status).HasDefaultValue("Scheduled");
                entity.Property(e => e.Result).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(i => i.ApplicantVacancy)
                    .WithMany(av => av.Interviews)
                    .HasForeignKey(i => i.ApplicantVacancyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Interviewer)
                    .WithMany(e => e.Interviews)
                    .HasForeignKey(i => i.InterviewerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== EmailNotification ==========
            modelBuilder.Entity<EmailNotification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // ========== Subscriber ==========
            modelBuilder.Entity<Subscriber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.SubscribedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UnsubscribeToken).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // ========== ApplicantActivityLog ==========
            modelBuilder.Entity<ApplicantActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApplicantId).IsRequired();
                entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");
            });

            // ========== UserActivityLog ==========
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");
            });

            // ========== Seed Data ==========
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, DepartmentName = "Human Resources", DepartmentCode = "HRD", IsActive = true },
                new Department { DepartmentId = 2, DepartmentName = "Information Technology", DepartmentCode = "IT", IsActive = true },
                new Department { DepartmentId = 3, DepartmentName = "Finance", DepartmentCode = "FIN", IsActive = true }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 1,
                    EmployeeNumber = "HR001",
                    EmployeeName = "HR Manager",
                    Email = "hr@abccompany.com",
                    Password = "Admin@123",
                    DepartmentId = 1,
                    Role = "HR",
                    IsActive = true
                },
                new Employee
                {
                    EmployeeId = 2,
                    EmployeeNumber = "IT001",
                    EmployeeName = "Senior Interviewer",
                    Email = "interviewer@abccompany.com",
                    Password = "Interviewer@123",
                    DepartmentId = 2,
                    Role = "Interviewer",
                    IsActive = true
                }
            );
        }
    }
}