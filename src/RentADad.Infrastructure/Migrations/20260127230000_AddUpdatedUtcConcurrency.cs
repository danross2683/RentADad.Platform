using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentADad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedUtcConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "jobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "providers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "providers");
        }
    }
}
