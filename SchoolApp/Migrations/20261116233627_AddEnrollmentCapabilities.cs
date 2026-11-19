using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Capabilities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 20, "Enroll a student in a course", "ENROLL_STUDENT" },
                    { 21, "Remove a student from a course", "UNENROLL_STUDENT" }
                });

            migrationBuilder.InsertData(
                table: "RolesCapabilities",
                columns: new[] { "CapabilitiesId", "RolesId" },
                values: new object[,]
                {
                    { 20, 1 },
                    { 20, 2 },
                    { 21, 1 },
                    { 21, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 20, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 21, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 21, 2 });

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
