using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractoBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalNotesAndVisitChaining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextVisitDate",
                table: "MedicalRecords",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "MedicalRecords",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PlannedProcedures",
                table: "MedicalRecords",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentAppointmentId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ParentAppointmentId",
                table: "Appointments",
                column: "ParentAppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Appointments_ParentAppointmentId",
                table: "Appointments",
                column: "ParentAppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Appointments_ParentAppointmentId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ParentAppointmentId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NextVisitDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "PlannedProcedures",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "ParentAppointmentId",
                table: "Appointments");
        }
    }
}
