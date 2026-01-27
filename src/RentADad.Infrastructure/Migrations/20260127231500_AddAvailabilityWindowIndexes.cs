using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentADad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityWindowIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_provider_availability_provider_window",
                table: "provider_availability",
                columns: new[] { "ProviderId", "StartUtc", "EndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_provider_window",
                table: "bookings",
                columns: new[] { "ProviderId", "StartUtc", "EndUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_provider_availability_provider_window",
                table: "provider_availability");

            migrationBuilder.DropIndex(
                name: "IX_bookings_provider_window",
                table: "bookings");
        }
    }
}
