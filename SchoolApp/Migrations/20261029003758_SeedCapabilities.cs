using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Capabilities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Create a new teacher", "INSERT_TEACHER" },
                    { 2, "View teacher list and details", "VIEW_TEACHERS" },
                    { 3, "View teacher", "VIEW_TEACHER" },
                    { 4, "Modify existing teacher", "EDIT_TEACHER" },
                    { 5, "Remove a teacher", "DELETE_TEACHER" },
                    { 6, "View only own teacher details", "VIEW_ONLY_TEACHER" },
                    { 7, "Create a new student", "INSERT_STUDENT" },
                    { 8, "View student list and details", "VIEW_STUDENTS" },
                    { 9, "View student", "VIEW_STUDENT" },
                    { 10, "Modify existing student", "EDIT_STUDENT" },
                    { 11, "Remove a student", "DELETE_STUDENT" },
                    { 12, "View only own student details", "VIEW_ONLY_STUDENT" },
                    { 13, "Create a new course", "INSERT_COURSE" },
                    { 14, "View course list and details", "VIEW_COURSES" },
                    { 15, "View course", "VIEW_COURSE" },
                    { 16, "Modify existing course", "EDIT_COURSE" },
                    { 17, "Remove a course", "DELETE_COURSE" },
                    { 18, "View the list of all users (registry)", "VIEW_USERS" }
                });

            migrationBuilder.InsertData(
                table: "RolesCapabilities",
                columns: new[] { "CapabilitiesId", "RolesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 1 },
                    { 3, 2 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 6, 3 },
                    { 7, 1 },
                    { 7, 2 },
                    { 8, 1 },
                    { 8, 2 },
                    { 9, 1 },
                    { 9, 2 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 12, 4 },
                    { 13, 1 },
                    { 14, 1 },
                    { 14, 2 },
                    { 15, 1 },
                    { 15, 2 },
                    { 16, 1 },
                    { 17, 1 },
                    { 18, 1 },
                    { 18, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 7, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 8, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 9, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 9, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 10, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 11, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 12, 4 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 14, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 14, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 15, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 15, 2 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 16, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 17, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 18, 1 });

            migrationBuilder.DeleteData(
                table: "RolesCapabilities",
                keyColumns: new[] { "CapabilitiesId", "RolesId" },
                keyValues: new object[] { 18, 2 });

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Capabilities",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}
