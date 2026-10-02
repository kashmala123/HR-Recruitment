# HR Recruitment System

ASP.NET Core MVC web application for managing job vacancies, applicants, and interviews with three role-based dashboards.

## Features

- **Public site** – Home, About, Services, Team, Contact, Newsletter
- **Applicant panel** – Register, apply to vacancies, track applications, profile picture, notifications
- **HR panel** – Vacancies, applicants, attach to jobs, schedule/cancel/reschedule interviews, reports
- **Interviewer panel** – View assigned interviews, submit results & feedback
- Email notifications (Contact, Newsletter, interview schedule/result) via Gmail SMTP
- Password hashing, forgot-password flow, profile pictures

## Tech stack

- ASP.NET Core MVC
- Entity Framework Core + SQL Server / LocalDB
- Cookie authentication & role-based authorization
- Bootstrap 5, Font Awesome

## Demo logins

| Role        | Email                         | Password         |
|-------------|-------------------------------|------------------|
| HR          | `hr@abccompany.com`           | `Admin@123`      |
| Interviewer | `interviewer@abccompany.com`  | `Interviewer@123`|
| Applicant   | `test@example.com`            | `123`            |

> Demo accounts only. Change passwords before any real use.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or the version targeted by the project)
- SQL Server LocalDB or SQL Server Express
- Visual Studio 2022 / VS Code / Rider (optional)

## Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/HR-Recruitment.git
   cd HR-Recruitment
   ```

2. **Configure email (optional but needed for Contact / Newsletter / interview emails)**  
   Edit `HRRecruitment/appsettings.json` (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):
   ```json
   "EmailSettings": {
     "SmtpUsername": "your-gmail@gmail.com",
     "SmtpPassword": "your-16-char-app-password",
     "FromEmail": "your-gmail@gmail.com",
     "FromName": "HR Recruitment"
   }
   ```
   Create a Gmail [App Password](https://myaccount.google.com/apppasswords) (2-Step Verification required).

3. **Connection string**  
   Default uses LocalDB:
   ```
   Server=(localdb)\MSSQLLocalDB;Database=HRRecruitment;Trusted_Connection=True;TrustServerCertificate=True
   ```
   Change in `appsettings.json` if you use another SQL Server instance.

4. **Run**
   ```bash
   cd HRRecruitment
   dotnet restore
   dotnet run
   ```
   Or open `HRRecruitment.slnx` / the `.csproj` in Visual Studio and press **F5**.

   On first run, EF migrations run automatically and seed HR, Interviewer, and a test Applicant.

## Project structure

```
HRRecruitment/
├── Controllers/     # Account, Home, HR, Interviewer, UserPanel, Newsletter
├── Models/          # Entities, services (email, password, activity log)
├── Views/           # Razor views + shared layouts
├── ViewModels/
├── Migrations/
└── wwwroot/         # CSS, JS, images, uploads
```

## Roles overview

| Action                         | HR | Interviewer | Applicant |
|--------------------------------|:--:|:-----------:|:---------:|
| Manage vacancies               | ✓  |             |           |
| Manage applicants / attach     | ✓  |             |           |
| Schedule interviews            | ✓  |             |           |
| Submit interview result        |    | ✓           |           |
| View vacancies & apply         |    |             | ✓         |
| Profile / change password      | ✓  | ✓           | ✓         |

## License

This project is provided for educational / portfolio use.  
Add a license file (e.g. MIT) if you want others to reuse the code formally.
