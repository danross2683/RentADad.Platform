using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentADad.Infrastructure.Migrations;

public partial class AddJobBookingIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_jobs_CustomerId",
            table: "jobs",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_jobs_Status",
            table: "jobs",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_JobId",
            table: "bookings",
            column: "JobId");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_ProviderId",
            table: "bookings",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_bookings_Status",
            table: "bookings",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_jobs_CustomerId",
            table: "jobs");

        migrationBuilder.DropIndex(
            name: "IX_jobs_Status",
            table: "jobs");

        migrationBuilder.DropIndex(
            name: "IX_bookings_JobId",
            table: "bookings");

        migrationBuilder.DropIndex(
            name: "IX_bookings_ProviderId",
            table: "bookings");

        migrationBuilder.DropIndex(
            name: "IX_bookings_Status",
            table: "bookings");
    }
}
