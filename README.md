# HR Recruitment System

A full-stack **HR Recruitment Management System** built with **ASP.NET Core MVC and .NET 10** for managing job vacancies, applicants, interviews, recruitment workflows, and role-based operations.

🔗 **Live Demo:** https://hr-recruitment.runasp.net/
💻 **Source Code:** https://github.com/kashmala123/HR-Recruitment

---

## 📌 Overview

The **HR Recruitment System** is a role-based web application designed to digitize and simplify the recruitment process.

The system provides separate experiences for **HR personnel, interviewers, and applicants**, allowing each role to perform tasks relevant to their responsibilities.

The application includes vacancy management, candidate applications, interview scheduling, interview results, notifications, profile management, and email-based communication.

---

## ✨ Features

### 🌐 Public Website

* Home page
* About section
* Services
* Team section
* Contact form
* Newsletter subscription
* Public job vacancy browsing

### 👤 Applicant Panel

* Applicant registration and authentication
* Browse available vacancies
* Apply for jobs
* Track application status
* Manage profile
* Profile picture upload
* Notifications
* Change password
* Forgot-password workflow

### 👩‍💼 HR Panel

* HR dashboard
* Create and manage job vacancies
* View applicants
* Attach applicants to vacancies
* Manage recruitment applications
* Schedule interviews
* Reschedule interviews
* Cancel interviews
* Track recruitment activity
* View recruitment reports

### 🎤 Interviewer Panel

* View assigned interviews
* Review interview information
* Submit interview results
* Provide candidate feedback
* Track completed interviews

### 📧 Email & Notifications

* Contact form email notifications
* Newsletter functionality
* Interview scheduling notifications
* Interview result notifications
* Password recovery emails
* Application-related notifications

---

## 🔐 Authentication & Security

The application implements role-based access control using ASP.NET Core authentication and authorization.

Key security-related features include:

* Cookie-based authentication
* Role-based authorization
* Password hashing
* Protected role-specific dashboards
* Forgot-password workflow
* Change-password functionality
* User-specific access control
* Secure production configuration

> Production credentials, database passwords, SMTP credentials, and other sensitive configuration values are intentionally excluded from this repository.

---

## 👥 Roles & Permissions

| Action                     |  HR | Interviewer | Applicant |
| -------------------------- | :-: | :---------: | :-------: |
| Manage vacancies           |  ✓  |             |           |
| Manage applicants          |  ✓  |             |           |
| Attach applicants to jobs  |  ✓  |             |           |
| Schedule interviews        |  ✓  |             |           |
| Reschedule interviews      |  ✓  |             |           |
| Cancel interviews          |  ✓  |             |           |
| Submit interview results   |     |      ✓      |           |
| Provide interview feedback |     |      ✓      |           |
| View vacancies             |     |             |     ✓     |
| Apply for jobs             |     |             |     ✓     |
| Track applications         |     |             |     ✓     |
| Manage profile             |  ✓  |      ✓      |     ✓     |
| Change password            |  ✓  |      ✓      |     ✓     |

---

## 🛠️ Technology Stack

### Backend

* **C#**
* **ASP.NET Core MVC**
* **.NET 10**
* **Entity Framework Core**
* **SQL Server**
* **ASP.NET Core Authentication & Authorization**

### Frontend

* **HTML5**
* **CSS3**
* **JavaScript**
* **Bootstrap 5**
* **Font Awesome**
* **Razor Views**

### Development & Deployment

* **Git**
* **GitHub**
* **Visual Studio / VS Code**
* **MonsterASP**
* **Let's Encrypt HTTPS**

---

## 🏗️ Application Architecture

The project follows the **ASP.NET Core MVC architecture**, separating application responsibilities into controllers, models, views, view models, and supporting services.

```text
HRRecruitment/
│
├── Controllers/
│   ├── Account
│   ├── Home
│   ├── HR
│   ├── Interviewer
│   ├── UserPanel
│   └── Newsletter
│
├── Models/
│   ├── Entities
│   ├── Email Services
│   ├── Password Services
│   └── Activity Logging
│
├── ViewModels/
│
├── Views/
│   ├── Shared
│   ├── Account
│   ├── HR
│   ├── Interviewer
│   └── UserPanel
│
├── Migrations/
│
├── wwwroot/
│   ├── CSS
│   ├── JavaScript
│   ├── Images
│   └── Uploads
│
├── appsettings.json
├── Program.cs
└── HRRecruitment.csproj
```

---

## 🔄 Recruitment Workflow

```text
HR Creates Vacancy
        ↓
Applicant Browses Vacancies
        ↓
Applicant Submits Application
        ↓
HR Reviews Application
        ↓
Applicant Attached to Vacancy
        ↓
Interview Scheduled
        ↓
Interviewer Conducts Interview
        ↓
Interview Result & Feedback Submitted
        ↓
HR Tracks Recruitment Outcome
```

---

## 🗄️ Database

The application uses **Microsoft SQL Server** with **Entity Framework Core**.

The database manages information related to:

* Users and roles
* Job vacancies
* Applicants
* Applications
* Interviews
* Interview results
* Notifications
* Newsletter subscriptions
* Activity records

Entity Framework Core migrations are included in the project for database schema management.

---

## 📸 Screenshots

Screenshots of the deployed application will be added here.

### 🏠 Public Website

*Add screenshot here*

### 🔐 Authentication

*Add screenshot here*

### 📊 HR Dashboard

*Add screenshot here*

### 👤 Applicant Dashboard

*Add screenshot here*

### 🎤 Interviewer Dashboard

*Add screenshot here*

### 💼 Job Vacancy / Application

*Add screenshot here*

---

## 🚀 Run the Project Locally

### Prerequisites

Make sure you have:

* **.NET 10 SDK**
* **SQL Server LocalDB, SQL Server Express, or SQL Server**
* Visual Studio 2022, VS Code, or JetBrains Rider

Check your .NET version:

```bash
dotnet --version
```

---

### 1. Clone the repository

```bash
git clone https://github.com/kashmala123/HR-Recruitment.git
cd HR-Recruitment
```

---

### 2. Restore dependencies

```bash
dotnet restore
```

---

### 3. Configure the database

The development configuration uses SQL Server LocalDB.

Example:

```text
Server=(localdb)\MSSQLLocalDB;
Database=HRRecruitment;
Trusted_Connection=True;
TrustServerCertificate=True;
```

Update the connection string in `appsettings.json` if you are using a different SQL Server instance.

---

### 4. Configure email

Email functionality uses Gmail SMTP.

For local development, configure your own SMTP credentials using **User Secrets or another secure configuration method**.

Example structure:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "HR Recruitment"
  }
}
```

**Never commit real email passwords, app passwords, API keys, or other secrets to GitHub.**

---

### 5. Apply database migrations

```bash
dotnet ef database update
```

---

### 6. Run the application

```bash
dotnet run
```

Or open the project in Visual Studio and run it using the development profile.

---

## ☁️ Deployment

The application is deployed on **MonsterASP** using:

* ASP.NET Core / .NET 10
* Microsoft SQL Server
* Production configuration
* Let's Encrypt SSL certificate
* HTTPS
* Automatic HTTPS certificate renewal
* HTTP → HTTPS redirection

### Live Application

🌐 **https://hr-recruitment.runasp.net/**

---

## 🎯 What I Practiced & Demonstrated

This project provided hands-on experience with:

* ASP.NET Core MVC
* C# backend development
* Entity Framework Core
* SQL Server
* Authentication and authorization
* Role-based application design
* CRUD operations
* Form handling and validation
* File uploads
* Email integration
* Password recovery
* Database migrations
* MVC architecture
* Git and GitHub
* Production deployment
* HTTPS configuration
* Responsive web development

---

## 📂 Repository Structure

The main application is located inside:

```text
HR-Recruitment/
└── HRRecruitment/
```

The `HRRecruitment` project contains the ASP.NET Core MVC application, views, controllers, models, migrations, static assets, and configuration.

---

## 🔒 Security Notes

This repository does **not** contain production credentials.

Sensitive configuration such as:

* Database passwords
* SMTP passwords
* Gmail App Passwords
* API keys
* Production secrets

should be supplied through secure configuration or environment-specific settings.

For production deployment, sensitive files should also be protected from being overwritten by automated deployments.

---

## 📜 License

This project was developed as a **portfolio and educational project**.

If you plan to reuse or distribute the source code, consider adding an appropriate open-source license such as MIT.

---

## 👩‍💻 Developer

**Kashmala Khan**

Full-Stack Developer focused on **C#, ASP.NET Core, SQL Server, and modern web application development**.

---

⭐ **Explore the source code:**
https://github.com/kashmala123/HR-Recruitment

🌐 **Try the live application:**
https://hr-recruitment.runasp.net/
