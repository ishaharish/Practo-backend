using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractoBackend.Migrations
{
    /// <inheritdoc />
    public partial class ExpandedLabTestBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "LabTestBookings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "LabTestBookings",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HouseOrFlat",
                table: "LabTestBookings",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                table: "LabTestBookings",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "LabTestBookings",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "LabTestBookings",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TimeSlot",
                table: "LabTestBookings",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "HouseOrFlat",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "Landmark",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "LabTestBookings");

            migrationBuilder.DropColumn(
                name: "TimeSlot",
                table: "LabTestBookings");
        }
    }
}
