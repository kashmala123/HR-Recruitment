using HRRecruitment.Data;
using HRRecruitment.Models;
using HRRecruitment.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; // 👈 for RelationalEventId
using myproject.Services; // for NumberGeneratorService, etc.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Email settings and service (your original, for contact form)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<HRRecruitment.Services.IEmailService, HRRecruitment.Services.EmailService>();

// Add the teammate's email service (for HR/Interviewer dashboards)
builder.Services.AddScoped<myproject.Services.IEmailService, myproject.Services.EmailService>();

// Database context (combined) – only once!
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .ConfigureWarnings(warnings =>
               warnings.Ignore(RelationalEventId.PendingModelChangesWarning))); // ignore warning

// Other services
builder.Services.AddScoped<NumberGeneratorService>();
builder.Services.AddHttpContextAccessor();

// User theme services
builder.Services.AddScoped<IActivityLogger, ActivityLogger>();

// Authentication (cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Session (optional)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Show custom 404 page for missing routes / resources
app.UseStatusCodePagesWithReExecute("/Home/PageNotFound");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Seed default data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Seed departments
    if (!context.Departments.Any())
    {
        context.Departments.AddRange(
            new Department { DepartmentName = "Human Resources", DepartmentCode = "HRD", IsActive = true },
            new Department { DepartmentName = "Information Technology", DepartmentCode = "IT", IsActive = true },
            new Department { DepartmentName = "Finance", DepartmentCode = "FIN", IsActive = true }
        );
        context.SaveChanges();
    }

    // Seed a default HR employee
    if (!context.Employees.Any(e => e.Role == "HR"))
    {
        context.Employees.Add(new Employee
        {
            EmployeeNumber = "HR001",
            EmployeeName = "HR Manager",
            Email = "hr@abccompany.com",
            Password = PasswordHelper.Hash("Admin@123"),
            DepartmentId = 1, // HRD
            Role = "HR",
            IsActive = true
        });
        context.SaveChanges();
    }

    // Seed a default interviewer
    if (!context.Employees.Any(e => e.Role == "Interviewer"))
    {
        context.Employees.Add(new Employee
        {
            EmployeeNumber = "IT001",
            EmployeeName = "Senior Interviewer",
            Email = "interviewer@abccompany.com",
            Password = PasswordHelper.Hash("Interviewer@123"),
            DepartmentId = 2, // IT
            Role = "Interviewer",
            IsActive = true
        });
        context.SaveChanges();
    }

    // Seed a test applicant
    if (!context.Applicants.Any())
    {
        context.Applicants.Add(new Applicant
        {
            FirstName = "Test",
            LastName = "Applicant",
            ApplicantNumber = "A0001",
            Email = "test@example.com",
            Password = PasswordHelper.Hash("123"),
            Phone = "1234567890",
            Address = "",
            CurrentCompany = "",
            HighestQualification = "",
            ExperienceYears = 0,
            ResumePath = "",
            Status = "Not in Process",
            CreatedDate = DateTime.Now
        });
        context.SaveChanges();
    }

    // Seed sample vacancies
    if (!context.Vacancies.Any())
    {
        context.Vacancies.AddRange(
            new Vacancy
            {
                VacancyNumber = "V001",
                JobTitle = "Senior .NET Developer",
                JobDescription = "We are looking for an experienced .NET developer to join our team. Requirements: 5+ years C#, ASP.NET Core, SQL Server.",
                RequiredQualifications = "5+ years C#, ASP.NET Core, SQL Server, Entity Framework",  // 👈 added
                Status = "Open",
                TotalOpenings = 2,
                RemainingOpenings = 2,
                DepartmentId = 2, // IT
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                ClosingDate = DateTime.UtcNow.AddDays(20),
                CreatedBy = 1
            },
            new Vacancy
            {
                VacancyNumber = "V002",
                JobTitle = "HR Specialist",
                JobDescription = "Manage recruitment processes, employee relations, and HR policies. Ideal for candidates with HR background.",
                RequiredQualifications = "HR degree, 2+ years experience",  // 👈 added
                Status = "Open",
                TotalOpenings = 1,
                RemainingOpenings = 1,
                DepartmentId = 1, // HRD
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                ClosingDate = DateTime.UtcNow.AddDays(15),
                CreatedBy = 1
            },
            new Vacancy
            {
                VacancyNumber = "V003",
                JobTitle = "Frontend Developer (React)",
                JobDescription = "Join our product team to build modern web applications. Skills: React, JavaScript, HTML/CSS.",
                RequiredQualifications = "React, JavaScript, HTML/CSS, 3+ years",  // 👈 added
                Status = "Open",
                TotalOpenings = 3,
                RemainingOpenings = 3,
                DepartmentId = 2, // IT
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                ClosingDate = DateTime.UtcNow.AddDays(30),
                CreatedBy = 1
            }
        );
        context.SaveChanges();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();