using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryLists",
                columns: table => new
                {
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Iso2 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Iso3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NumericCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PhoneCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Capital = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CurrencyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrencySymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Tld = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Native = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegionId = table.Column<int>(type: "integer", nullable: true),
                    Subregion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubregionId = table.Column<int>(type: "integer", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    EmojiU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryLists", x => x.CountryListID);
                });

            migrationBuilder.CreateTable(
                name: "DockerLogger",
                columns: table => new
                {
                    DockerLoggerID = table.Column<Guid>(type: "uuid", nullable: false),
                    DockerInstanceID = table.Column<string>(type: "text", nullable: false),
                    DockerInstanceName = table.Column<string>(type: "text", nullable: false),
                    LogMessage = table.Column<string>(type: "text", nullable: false),
                    IsError = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FriendlyContanierName = table.Column<string>(type: "text", nullable: false),
                    ContainerStatus = table.Column<string>(type: "text", nullable: false),
                    ImageNameInUse = table.Column<string>(type: "text", nullable: false),
                    ExeptionMessage = table.Column<string>(type: "text", nullable: false),
                    ExposedPortLeft = table.Column<int>(type: "integer", nullable: false),
                    HostPortRight = table.Column<int>(type: "integer", nullable: false),
                    DockerStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockerLogger", x => x.DockerLoggerID);
                });

            migrationBuilder.CreateTable(
                name: "NginxSyncLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NginxSyncLogID = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    LastErrorCode = table.Column<string>(type: "text", nullable: false),
                    RawJsonSent = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxSyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProvinceStateLists",
                columns: table => new
                {
                    ProvinceStateListID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StateCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceStateLists", x => x.ProvinceStateListID);
                    table.ForeignKey(
                        name: "FK_ProvinceStateLists_CountryLists_CountryListID",
                        column: x => x.CountryListID,
                        principalTable: "CountryLists",
                        principalColumn: "CountryListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeZones",
                columns: table => new
                {
                    TimeZonesID = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GmtOffset = table.Column<int>(type: "integer", nullable: false),
                    GmtOffsetName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TzName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeZones", x => x.TimeZonesID);
                    table.ForeignKey(
                        name: "FK_TimeZones_CountryLists_CountryListID",
                        column: x => x.CountryListID,
                        principalTable: "CountryLists",
                        principalColumn: "CountryListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEntities",
                columns: table => new
                {
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    EmailUsername = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationCode = table.Column<string>(type: "text", nullable: false),
                    VerificationCodeSentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerificationCodeExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryName = table.Column<string>(type: "text", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    ProvinceStateListID = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyForNightScout = table.Column<string>(type: "text", nullable: false),
                    UserSubDomain = table.Column<string>(type: "text", nullable: false),
                    TimeZoneID = table.Column<Guid>(type: "uuid", nullable: false),
                    DockerStatus = table.Column<string>(type: "text", nullable: false),
                    DockerName = table.Column<string>(type: "text", nullable: false),
                    TimeZonesID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntities", x => x.UserEntityID);
                    table.ForeignKey(
                        name: "FK_UserEntities_CountryLists_CountryListID",
                        column: x => x.CountryListID,
                        principalTable: "CountryLists",
                        principalColumn: "CountryListID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEntities_ProvinceStateLists_ProvinceStateListID",
                        column: x => x.ProvinceStateListID,
                        principalTable: "ProvinceStateLists",
                        principalColumn: "ProvinceStateListID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEntities_TimeZones_TimeZonesID",
                        column: x => x.TimeZonesID,
                        principalTable: "TimeZones",
                        principalColumn: "TimeZonesID");
                });

            migrationBuilder.CreateTable(
                name: "DockerContainers",
                columns: table => new
                {
                    DockerContainersID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseImageContaierName = table.Column<string>(type: "text", nullable: false),
                    InstanceUserNameIdentifer = table.Column<string>(type: "text", nullable: false),
                    InstanceID = table.Column<string>(type: "text", nullable: false),
                    ExposedPortLeft = table.Column<int>(type: "integer", nullable: false),
                    PortRight = table.Column<int>(type: "integer", nullable: false),
                    DockerStatus = table.Column<string>(type: "text", nullable: false),
                    DataBaseConnectionString = table.Column<string>(type: "text", nullable: false),
                    EnvironmentVariables = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    ImageNameInUse = table.Column<string>(type: "text", nullable: false),
                    HostPortRight = table.Column<int>(type: "integer", nullable: false),
                    DockerStatusExeption = table.Column<string>(type: "text", nullable: false),
                    ContainerIpAddress = table.Column<string>(type: "text", nullable: false),
                    AppUniqueName = table.Column<string>(type: "text", nullable: false),
                    AppType = table.Column<string>(type: "text", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    DockerLable = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DockerContainers", x => x.DockerContainersID);
                    table.ForeignKey(
                        name: "FK_DockerContainers_UserEntities_UserEntityID",
                        column: x => x.UserEntityID,
                        principalTable: "UserEntities",
                        principalColumn: "UserEntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NginxContainers",
                columns: table => new
                {
                    NginxContainerID = table.Column<Guid>(type: "uuid", nullable: false),
                    DockerContainerID = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerDockerContainersID = table.Column<Guid>(type: "uuid", nullable: false),
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    PathPrefix = table.Column<string>(type: "text", nullable: false),
                    ExternalPort = table.Column<int>(type: "integer", nullable: false),
                    InternalAddress = table.Column<string>(type: "text", nullable: false),
                    InternalPort = table.Column<int>(type: "integer", nullable: false),
                    EnableWebSockets = table.Column<bool>(type: "boolean", nullable: false),
                    ClientMaxBodySizeMb = table.Column<int>(type: "integer", nullable: false),
                    SslCertName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxContainers", x => x.NginxContainerID);
                    table.ForeignKey(
                        name: "FK_NginxContainers_DockerContainers_ContainerDockerContainersID",
                        column: x => x.ContainerDockerContainersID,
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
                    ContainerDockerContainersID = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalHost = table.Column<string>(type: "text", nullable: false),
                    ListenPort = table.Column<int>(type: "integer", nullable: false),
                    TargetContainerPort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NginxRoutingRules", x => x.NginxRoutingRuleID);
                    table.ForeignKey(
                        name: "FK_NginxRoutingRules_DockerContainers_ContainerDockerContainer~",
                        column: x => x.ContainerDockerContainersID,
                        principalTable: "DockerContainers",
                        principalColumn: "DockerContainersID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DockerContainers_UserEntityID",
                table: "DockerContainers",
                column: "UserEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_NginxContainers_ContainerDockerContainersID",
                table: "NginxContainers",
                column: "ContainerDockerContainersID");

            migrationBuilder.CreateIndex(
                name: "IX_NginxRoutingRules_ContainerDockerContainersID",
                table: "NginxRoutingRules",
                column: "ContainerDockerContainersID");

            migrationBuilder.CreateIndex(
                name: "IX_ProvinceStateLists_CountryListID",
                table: "ProvinceStateLists",
                column: "CountryListID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeZones_CountryListID",
                table: "TimeZones",
                column: "CountryListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_ApiKeyForNightScout",
                table: "UserEntities",
                column: "ApiKeyForNightScout",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_CountryListID",
                table: "UserEntities",
                column: "CountryListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_ProvinceStateListID",
                table: "UserEntities",
                column: "ProvinceStateListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_TimeZonesID",
                table: "UserEntities",
                column: "TimeZonesID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_UserSubDomain",
                table: "UserEntities",
                column: "UserSubDomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DockerLogger");

            migrationBuilder.DropTable(
                name: "NginxContainers");

            migrationBuilder.DropTable(
                name: "NginxRoutingRules");

            migrationBuilder.DropTable(
                name: "NginxSyncLogs");

            migrationBuilder.DropTable(
                name: "DockerContainers");

            migrationBuilder.DropTable(
                name: "UserEntities");

            migrationBuilder.DropTable(
                name: "ProvinceStateLists");

            migrationBuilder.DropTable(
                name: "TimeZones");

            migrationBuilder.DropTable(
                name: "CountryLists");
        }
    }
}
