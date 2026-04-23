using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class Traefik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudflareLoggers");

            migrationBuilder.DropTable(
                name: "NginxContainers");

            migrationBuilder.DropTable(
                name: "NginxRoutingRules");

            migrationBuilder.DropTable(
                name: "NginxSyncLogs");

            migrationBuilder.DropColumn(
                name: "CloudflareRecordId",
                table: "DockerContainers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudflareRecordId",
                table: "DockerContainers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CloudflareLoggers",
                columns: table => new
                {
                    CloudflareLoggerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: false),
                    ResponsePayload = table.Column<string>(type: "text", nullable: false),
                    Subdomain = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudflareLoggers", x => x.CloudflareLoggerID);
                });

            migrationBuilder.CreateTable(
                name: "NginxContainers",
                columns: table => new
                {
                    NginxContainerID = table.Column<Guid>(type: "uuid", nullable: false),
                    DockerContainerID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientMaxBodySizeMb = table.Column<int>(type: "integer", nullable: false),
                    EnableWebSockets = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalPort = table.Column<int>(type: "integer", nullable: false),
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    InternalAddress = table.Column<string>(type: "text", nullable: false),
                    InternalPort = table.Column<int>(type: "integer", nullable: false),
                    PathPrefix = table.Column<string>(type: "text", nullable: false),
                    SslCertKey = table.Column<string>(type: "text", nullable: false),
                    SslCertName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxContainers", x => x.NginxContainerID);
                    table.ForeignKey(
                        name: "FK_NginxContainers_DockerContainers_DockerContainerID",
                        column: x => x.DockerContainerID,
                        principalTable: "DockerContainers",
                        principalColumn: "DockerContainersID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NginxRoutingRules",
                columns: table => new
                {
                    NginxRoutingRuleID = table.Column<Guid>(type: "uuid", nullable: false),
                    DockerContainerID = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalHost = table.Column<string>(type: "text", nullable: false),
                    ListenPort = table.Column<int>(type: "integer", nullable: false),
                    TargetContainerPort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxRoutingRules", x => x.NginxRoutingRuleID);
                    table.ForeignKey(
                        name: "FK_NginxRoutingRules_DockerContainers_DockerContainerID",
                        column: x => x.DockerContainerID,
                        principalTable: "DockerContainers",
                        principalColumn: "DockerContainersID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NginxSyncLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContainerEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastErrorCode = table.Column<string>(type: "text", nullable: false),
                    NginxSyncLogID = table.Column<Guid>(type: "uuid", nullable: false),
                    RawJsonSent = table.Column<string>(type: "text", nullable: false),
                    SyncTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxSyncLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NginxContainers_DockerContainerID",
                table: "NginxContainers",
                column: "DockerContainerID");

            migrationBuilder.CreateIndex(
                name: "IX_NginxRoutingRules_DockerContainerID",
                table: "NginxRoutingRules",
                column: "DockerContainerID");
        }
    }
}
