using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractoBackend.Migrations
{
    /// <inheritdoc />
    public partial class DoctorOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityProofPath",
                table: "DoctorProfiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationCouncil",
                table: "DoctorProfiles",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "DoctorProfiles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RegistrationYear",
                table: "DoctorProfiles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityProofPath",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "RegistrationCouncil",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "RegistrationYear",
                table: "DoctorProfiles");
        }
    }
}
