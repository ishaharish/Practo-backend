using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbhaId",
                table: "Appointments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Appointments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPatientEmail",
                table: "Appointments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPatientSms",
                table: "Appointments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlannedProcedures",
                table: "Appointments",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QueueStatus",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TokenNumber",
                table: "Appointments",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CalendarCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CategoryId",
                table: "Appointments",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_CalendarCategories_CategoryId",
                table: "Appointments",
                column: "CategoryId",
                principalTable: "CalendarCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_CalendarCategories_CategoryId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "CalendarCategories");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CategoryId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AbhaId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NotifyPatientEmail",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NotifyPatientSms",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PlannedProcedures",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "QueueStatus",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TokenNumber",
                table: "Appointments");
        }
    }
}
