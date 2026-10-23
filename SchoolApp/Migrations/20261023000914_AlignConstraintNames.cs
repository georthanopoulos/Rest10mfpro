using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolApp.Migrations
{
    /// <inheritdoc />
    public partial class AlignConstraintNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "AM",
                table: "Students",
                newName: "Am");

            migrationBuilder.RenameIndex(
                name: "UQ_Students_AM",
                table: "Students",
                newName: "UQ_Students_Am");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_TeacherId",
                table: "Courses",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_TeacherId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "Am",
                table: "Students",
                newName: "AM");

            migrationBuilder.RenameIndex(
                name: "UQ_Students_Am",
                table: "Students",
                newName: "UQ_Students_AM");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers",
                table: "Courses",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
