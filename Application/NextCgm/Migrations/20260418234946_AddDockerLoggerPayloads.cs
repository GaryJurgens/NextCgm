using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class AddDockerLoggerPayloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestPayload",
                table: "DockerLogger",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsePayload",
                table: "DockerLogger",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestPayload",
                table: "DockerLogger");

            migrationBuilder.DropColumn(
                name: "ResponsePayload",
                table: "DockerLogger");
        }
    }
}
