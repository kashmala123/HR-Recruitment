using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRRecruitment.Migrations
{
    /// <inheritdoc />
    public partial class InitialCombined : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicantActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "EmailNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubscribedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UnsubscribeToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "/images/default-avatar.png")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResumePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighestQualification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExperienceYears = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrentCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Not in Process"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                    table.ForeignKey(
                        name: "FK_Applicants_Employees_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vacancies",
                columns: table => new
                {
                    VacancyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VacancyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalOpenings = table.Column<int>(type: "int", nullable: false),
                    RemainingOpenings = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosingReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAutoClosed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacancies", x => x.VacancyId);
                    table.ForeignKey(
                        name: "FK_Vacancies_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacancies_Employees_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantVacancies",
                columns: table => new
                {
                    ApplicantVacancyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<int>(type: "int", nullable: false),
                    VacancyId = table.Column<int>(type: "int", nullable: false),
                    AttachedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CurrentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Applied"),
                    AttachedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantVacancies", x => x.ApplicantVacancyId);
                    table.ForeignKey(
                        name: "FK_ApplicantVacancies_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicantVacancies_Employees_AttachedBy",
                        column: x => x.AttachedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApplicantVacancies_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "VacancyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicantVacancyId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    InterviewMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Offline"),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InterviewRound = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Scheduled"),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.InterviewId);
                    table.ForeignKey(
                        name: "FK_Interviews_ApplicantVacancies_ApplicantVacancyId",
                        column: x => x.ApplicantVacancyId,
                        principalTable: "ApplicantVacancies",
                        principalColumn: "ApplicantVacancyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interviews_Employees_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_Interviews_Employees_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentId", "CreatedDate", "DepartmentCode", "DepartmentName", "IsActive" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 28, 5, 45, 2, 438, DateTimeKind.Local).AddTicks(3498), "HRD", "Human Resources", true },
                    { 2, new DateTime(2026, 3, 28, 5, 45, 2, 442, DateTimeKind.Local).AddTicks(8169), "IT", "Information Technology", true },
                    { 3, new DateTime(2026, 3, 28, 5, 45, 2, 442, DateTimeKind.Local).AddTicks(8205), "FIN", "Finance", true }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "CreatedDate", "DepartmentId", "Email", "EmployeeName", "EmployeeNumber", "IsActive", "LastLoginDate", "Password", "ProfilePicture", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 28, 5, 45, 2, 444, DateTimeKind.Local).AddTicks(5145), 1, "hr@abccompany.com", "HR Manager", "HR001", true, null, "Admin@123", "/images/default-avatar.png", "HR" },
                    { 2, new DateTime(2026, 3, 28, 5, 45, 2, 445, DateTimeKind.Local).AddTicks(2931), 2, "interviewer@abccompany.com", "Senior Interviewer", "IT001", true, null, "Interviewer@123", "/images/default-avatar.png", "Interviewer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_ApplicantNumber",
                table: "Applicants",
                column: "ApplicantNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_CreatedBy",
                table: "Applicants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Email",
                table: "Applicants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantVacancies_ApplicantId_VacancyId",
                table: "ApplicantVacancies",
                columns: new[] { "ApplicantId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantVacancies_AttachedBy",
                table: "ApplicantVacancies",
                column: "AttachedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantVacancies_VacancyId",
                table: "ApplicantVacancies",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ApplicantVacancyId",
                table: "Interviews",
                column: "ApplicantVacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_CreatedBy",
                table: "Interviews",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InterviewerId",
                table: "Interviews",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InterviewNumber",
                table: "Interviews",
                column: "InterviewNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_CreatedBy",
                table: "Vacancies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_DepartmentId",
                table: "Vacancies",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_VacancyNumber",
                table: "Vacancies",
                column: "VacancyNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantActivityLogs");

            migrationBuilder.DropTable(
                name: "EmailNotifications");

            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "Subscribers");

            migrationBuilder.DropTable(
                name: "UserActivityLogs");

            migrationBuilder.DropTable(
                name: "ApplicantVacancies");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropTable(
                name: "Vacancies");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
