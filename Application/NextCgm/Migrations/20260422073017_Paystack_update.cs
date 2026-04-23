using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class Paystack_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturesHtml",
                table: "SubscriptionPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaystackPlanCode",
                table: "SubscriptionPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FailedChargeAttempts",
                table: "BillingSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "GracePeriodEndDate",
                table: "BillingSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryDate",
                table: "BillingSubscriptions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturesHtml",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PaystackPlanCode",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "FailedChargeAttempts",
                table: "BillingSubscriptions");

            migrationBuilder.DropColumn(
                name: "GracePeriodEndDate",
                table: "BillingSubscriptions");

            migrationBuilder.DropColumn(
                name: "NextRetryDate",
                table: "BillingSubscriptions");
        }
    }
}
