using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudflareLogger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudflareLoggers",
                columns: table => new
                {
                    CloudflareLoggerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Subdomain = table.Column<string>(type: "text", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: false),
                    ResponsePayload = table.Column<string>(type: "text", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudflareLoggers", x => x.CloudflareLoggerID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudflareLoggers");
        }
    }
}
