using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRRecruitment.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 30, 13, 41, 49, 441, DateTimeKind.Local).AddTicks(3905));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 30, 13, 41, 49, 444, DateTimeKind.Local).AddTicks(4208));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 30, 13, 41, 49, 444, DateTimeKind.Local).AddTicks(4248));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 30, 13, 41, 49, 446, DateTimeKind.Local).AddTicks(3437));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 30, 13, 41, 49, 447, DateTimeKind.Local).AddTicks(1206));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 7, 49, 5, 12, DateTimeKind.Local).AddTicks(4158));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 7, 49, 5, 15, DateTimeKind.Local).AddTicks(1042));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 7, 49, 5, 15, DateTimeKind.Local).AddTicks(1073));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 7, 49, 5, 16, DateTimeKind.Local).AddTicks(6810));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 7, 49, 5, 17, DateTimeKind.Local).AddTicks(4154));
        }
    }
}
