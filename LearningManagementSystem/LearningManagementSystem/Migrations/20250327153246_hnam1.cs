using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class hnam1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 22, 32, 43, 932, DateTimeKind.Local).AddTicks(1130));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 22, 32, 43, 930, DateTimeKind.Local).AddTicks(7582));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 22, 32, 43, 931, DateTimeKind.Local).AddTicks(9036));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 22, 32, 43, 932, DateTimeKind.Local).AddTicks(599));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 22, 32, 43, 932, DateTimeKind.Local).AddTicks(1661));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKHdmW3keF+46NcYtjnM1UG+E6hzjERQ698gaBib1TvZIEf0bqUTW0fqm++Bmj3X1w==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEGgAOjxj9gvYNL26bmjCVrKFHwh+9+6rjVlQhY2UVQOZM112xDu2VLI55nF4KoPU7g==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 23, 23, 312, DateTimeKind.Local).AddTicks(3054));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 23, 23, 308, DateTimeKind.Local).AddTicks(358));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 23, 23, 312, DateTimeKind.Local).AddTicks(17));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 14, 23, 23, 312, DateTimeKind.Local).AddTicks(2313));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 14, 23, 23, 312, DateTimeKind.Local).AddTicks(3901));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEIYclZbKMR8pcuabcqqmvYko8N63G4v/8BQkYTZcNtLIy1Dh9XTEppASZo9u0jq+Kw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHNdbUqEDafOotiQt+f2itQTWQRlimIBhEhyxJ1S/p8mNj39nXbMcP/qyGNwF1fg0Q==");
        }
    }
}
