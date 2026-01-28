using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentADad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServiceIds = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActiveBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_listings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_CustomerId",
                table: "job_listings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_Status",
                table: "job_listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_UpdatedUtc",
                table: "job_listings",
                column: "UpdatedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_listings");
        }
    }
}
