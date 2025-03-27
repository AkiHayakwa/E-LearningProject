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
            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 46, 25, 554, DateTimeKind.Local).AddTicks(6195));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 46, 25, 553, DateTimeKind.Local).AddTicks(4008));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 46, 25, 554, DateTimeKind.Local).AddTicks(3331));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 21, 46, 25, 554, DateTimeKind.Local).AddTicks(5362));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 21, 46, 25, 554, DateTimeKind.Local).AddTicks(7006));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEDn7aeTBuL/SWC1KF+8wXgwoZwnuEGMRdNeonc9eQ7a8fbLYRg4RnimKdlo+KV7nxA==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHG6TIDvfe++Slj4JordVqUKeBFoosjDhLIr/E8XCm0naK93DEYDHoyUDBEQATg/Rg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 29, 3, 311, DateTimeKind.Local).AddTicks(5337));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 29, 3, 310, DateTimeKind.Local).AddTicks(1429));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 21, 29, 3, 311, DateTimeKind.Local).AddTicks(2491));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 21, 29, 3, 311, DateTimeKind.Local).AddTicks(4500));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 21, 29, 3, 311, DateTimeKind.Local).AddTicks(6210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEIx6pqZRtS/Mef0yhrcOamVgF5ZXwhggTJZC36YNV9eW6w/sx08rRVfI9KsMiyt2vg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHWvvglOkP5JEeerSOVovDol5LvSENHY8MS31qgrhrrN56b4C+yIhD36hJSNmIbXsA==");
        }
    }
}
