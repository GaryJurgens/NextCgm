using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class databseCloulfair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SslCertKey",
                table: "NginxContainers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudflareRecordId",
                table: "DockerContainers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserContainerDatabases",
                columns: table => new
                {
                    UserContainerDatabaseID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClusterName = table.Column<string>(type: "text", nullable: false),
                    DatabaseName = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContainerDatabases", x => x.UserContainerDatabaseID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserContainerDatabases");

            migrationBuilder.DropColumn(
                name: "SslCertKey",
                table: "NginxContainers");

            migrationBuilder.DropColumn(
                name: "CloudflareRecordId",
                table: "DockerContainers");
        }
    }
}
