using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class Data_Countries_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_TimeZones_TimeZonesID",
                table: "UserEntities");

            migrationBuilder.DropTable(
                name: "TimeZones");

            migrationBuilder.RenameColumn(
                name: "TimeZonesID",
                table: "UserEntities",
                newName: "TimeZoneDataID");

            migrationBuilder.RenameIndex(
                name: "IX_UserEntities_TimeZonesID",
                table: "UserEntities",
                newName: "IX_UserEntities_TimeZoneDataID");

            migrationBuilder.CreateTable(
                name: "TimeZoneData",
                columns: table => new
                {
                    TimeZoneDataID = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GmtOffset = table.Column<int>(type: "integer", nullable: false),
                    GmtOffsetName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TzName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeZoneData", x => x.TimeZoneDataID);
                    table.ForeignKey(
                        name: "FK_TimeZoneData_CountryLists_CountryListID",
                        column: x => x.CountryListID,
                        principalTable: "CountryLists",
                        principalColumn: "CountryListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeZoneData_CountryListID",
                table: "TimeZoneData",
                column: "CountryListID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_TimeZoneData_TimeZoneDataID",
                table: "UserEntities",
                column: "TimeZoneDataID",
                principalTable: "TimeZoneData",
                principalColumn: "TimeZoneDataID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_TimeZoneData_TimeZoneDataID",
                table: "UserEntities");

            migrationBuilder.DropTable(
                name: "TimeZoneData");

            migrationBuilder.RenameColumn(
                name: "TimeZoneDataID",
                table: "UserEntities",
                newName: "TimeZonesID");

            migrationBuilder.RenameIndex(
                name: "IX_UserEntities_TimeZoneDataID",
                table: "UserEntities",
                newName: "IX_UserEntities_TimeZonesID");

            migrationBuilder.CreateTable(
                name: "TimeZones",
                columns: table => new
                {
                    TimeZonesID = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryListID = table.Column<Guid>(type: "uuid", nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GmtOffset = table.Column<int>(type: "integer", nullable: false),
                    GmtOffsetName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TzName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ZoneName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TimeZones_CountryListID",
                table: "TimeZones",
                column: "CountryListID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_TimeZones_TimeZonesID",
                table: "UserEntities",
                column: "TimeZonesID",
                principalTable: "TimeZones",
                principalColumn: "TimeZonesID");
        }
    }
}
