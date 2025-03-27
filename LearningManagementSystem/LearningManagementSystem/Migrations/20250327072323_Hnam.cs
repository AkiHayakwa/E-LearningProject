using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Hnam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: "comment1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 17, 4, 265, DateTimeKind.Local).AddTicks(9585));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course1",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 17, 4, 264, DateTimeKind.Local).AddTicks(2848));

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: "course2",
                column: "CreatedDate",
                value: new DateTime(2025, 3, 27, 14, 17, 4, 265, DateTimeKind.Local).AddTicks(7007));

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "EnrollmentId",
                keyValue: "enrollment1",
                column: "EnrollmentDate",
                value: new DateTime(2025, 3, 27, 14, 17, 4, 265, DateTimeKind.Local).AddTicks(8916));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "ProgressId",
                keyValue: "progress1",
                column: "CompletionDate",
                value: new DateTime(2025, 3, 27, 14, 17, 4, 266, DateTimeKind.Local).AddTicks(251));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "admin1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAECAqa887r/XNr7gXRY7SuCtMDTX6iFCnE7bb9v0FIysaPqrpa7VhP+8Rrvw2jz7CaA==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserName",
                keyValue: "student1",
                column: "Password",
                value: "AQAAAAIAAYagAAAAEMeI5DFNhiCvO3Fvu2fx5JrXn+iX1lB3NrII4rLrxOVwGjzkQ9oGastwwGHhkPBfPg==");
        }
    }
}
