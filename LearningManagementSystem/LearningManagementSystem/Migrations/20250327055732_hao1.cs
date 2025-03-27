using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class hao1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Courses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 12, 57, 30, 916, DateTimeKind.Local).AddTicks(3184));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                columns: new[] { "CreatedDate", "ImageUrl" },
                values: new object[] { new DateTime(2025, 3, 27, 12, 57, 30, 915, DateTimeKind.Local).AddTicks(832), "/images/course1.jpg" });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                columns: new[] { "CreatedDate", "ImageUrl" },
                values: new object[] { new DateTime(2025, 3, 27, 12, 57, 30, 916, DateTimeKind.Local).AddTicks(147), "/images/course2.jpg" });

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 12, 57, 30, 916, DateTimeKind.Local).AddTicks(2365));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 12, 57, 30, 916, DateTimeKind.Local).AddTicks(4053));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEElzZekPDj1SyBD4gcKVM6fdwz+u8sT7Xy6EA4KSyzpK/Us+934It3+XSPqy0H8t8Q==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEOKfdGC28e5T8DcUn91D05+HbDWtWhl8guaWS918h+KLMT5vUPMJBm6m8Oj8IrgYDQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 26, 23, 22, 38, 98, DateTimeKind.Local).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 26, 23, 22, 38, 96, DateTimeKind.Local).AddTicks(8938));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 26, 23, 22, 38, 97, DateTimeKind.Local).AddTicks(8172));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 26, 23, 22, 38, 98, DateTimeKind.Local).AddTicks(723));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 26, 23, 22, 38, 98, DateTimeKind.Local).AddTicks(2500));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAELOj0BL4JpXWImXEToM1Z6cphg76qxPajJj75MalKGawFwfmu7CXOiemH7mP1u7Y+w==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEOEhjYNYbwrYMSqZXw8KNqfP/zWRd1zx7aCISzh5ChfWFat2WGwGaM/8T7aw6QFJzQ==");
        }
    }
}
