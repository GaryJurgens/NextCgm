using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class FixNginxForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NginxContainers_DockerContainers_ContainerDockerContainersID",
                table: "NginxContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_NginxRoutingRules_DockerContainers_ContainerDockerContainer~",
                table: "NginxRoutingRules");

            migrationBuilder.DropIndex(
                name: "IX_NginxRoutingRules_ContainerDockerContainersID",
                table: "NginxRoutingRules");

            migrationBuilder.DropIndex(
                name: "IX_NginxContainers_ContainerDockerContainersID",
                table: "NginxContainers");

            migrationBuilder.DropColumn(
                name: "ContainerDockerContainersID",
                table: "NginxRoutingRules");

            migrationBuilder.DropColumn(
                name: "ContainerDockerContainersID",
                table: "NginxContainers");

            migrationBuilder.CreateIndex(
                name: "IX_NginxRoutingRules_DockerContainerID",
                table: "NginxRoutingRules",
                column: "DockerContainerID");

            migrationBuilder.CreateIndex(
                name: "IX_NginxContainers_DockerContainerID",
                table: "NginxContainers",
                column: "DockerContainerID");

            migrationBuilder.AddForeignKey(
                name: "FK_NginxContainers_DockerContainers_DockerContainerID",
                table: "NginxContainers",
                column: "DockerContainerID",
                principalTable: "DockerContainers",
                principalColumn: "DockerContainersID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NginxRoutingRules_DockerContainers_DockerContainerID",
                table: "NginxRoutingRules",
                column: "DockerContainerID",
                principalTable: "DockerContainers",
                principalColumn: "DockerContainersID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NginxContainers_DockerContainers_DockerContainerID",
                table: "NginxContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_NginxRoutingRules_DockerContainers_DockerContainerID",
                table: "NginxRoutingRules");

            migrationBuilder.DropIndex(
                name: "IX_NginxRoutingRules_DockerContainerID",
                table: "NginxRoutingRules");

            migrationBuilder.DropIndex(
                name: "IX_NginxContainers_DockerContainerID",
                table: "NginxContainers");

            migrationBuilder.AddColumn<Guid>(
                name: "ContainerDockerContainersID",
                table: "NginxRoutingRules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ContainerDockerContainersID",
                table: "NginxContainers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_NginxRoutingRules_ContainerDockerContainersID",
                table: "NginxRoutingRules",
                column: "ContainerDockerContainersID");

            migrationBuilder.CreateIndex(
                name: "IX_NginxContainers_ContainerDockerContainersID",
                table: "NginxContainers",
                column: "ContainerDockerContainersID");

            migrationBuilder.AddForeignKey(
                name: "FK_NginxContainers_DockerContainers_ContainerDockerContainersID",
                table: "NginxContainers",
                column: "ContainerDockerContainersID",
                principalTable: "DockerContainers",
                principalColumn: "DockerContainersID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NginxRoutingRules_DockerContainers_ContainerDockerContainer~",
                table: "NginxRoutingRules",
                column: "ContainerDockerContainersID",
                principalTable: "DockerContainers",
                principalColumn: "DockerContainersID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
