using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRRecruitment.Migrations
{
    /// <inheritdoc />
    public partial class dd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 5, 45, 2, 438, DateTimeKind.Local).AddTicks(3498));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 5, 45, 2, 442, DateTimeKind.Local).AddTicks(8169));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 5, 45, 2, 442, DateTimeKind.Local).AddTicks(8205));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 5, 45, 2, 444, DateTimeKind.Local).AddTicks(5145));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 3, 28, 5, 45, 2, 445, DateTimeKind.Local).AddTicks(2931));
        }
    }
}
