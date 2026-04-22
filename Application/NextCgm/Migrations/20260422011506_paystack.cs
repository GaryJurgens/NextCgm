using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextCgm.Migrations
{
    /// <inheritdoc />
    public partial class paystack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingSubscriptions",
                columns: table => new
                {
                    BillingSubscriptionEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaystackSubscriptionCode = table.Column<string>(type: "text", nullable: false),
                    PaystackCustomerCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingSubscriptions", x => x.BillingSubscriptionEntityID);
                    table.ForeignKey(
                        name: "FK_BillingSubscriptions_UserEntities_UserEntityID",
                        column: x => x.UserEntityID,
                        principalTable: "UserEntities",
                        principalColumn: "UserEntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    PaymentMethodID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorizationCode = table.Column<string>(type: "text", nullable: false),
                    CardType = table.Column<string>(type: "text", nullable: false),
                    Last4 = table.Column<string>(type: "text", nullable: false),
                    ExpMonth = table.Column<string>(type: "text", nullable: false),
                    ExpYear = table.Column<string>(type: "text", nullable: false),
                    Bank = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Reusable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.PaymentMethodID);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_UserEntities_UserEntityID",
                        column: x => x.UserEntityID,
                        principalTable: "UserEntities",
                        principalColumn: "UserEntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    SubscriptionPlanID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PriceZAR = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceUSD = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BillingCycleMonths = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.SubscriptionPlanID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PaymentTransactionID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityID = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingSubscriptionEntityID = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PaystackReference = table.Column<string>(type: "text", nullable: false),
                    PaystackAuthorizationCode = table.Column<string>(type: "text", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawResponse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PaymentTransactionID);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_BillingSubscriptions_BillingSubscriptio~",
                        column: x => x.BillingSubscriptionEntityID,
                        principalTable: "BillingSubscriptions",
                        principalColumn: "BillingSubscriptionEntityID");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_UserEntities_UserEntityID",
                        column: x => x.UserEntityID,
                        principalTable: "UserEntities",
                        principalColumn: "UserEntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingSubscriptions_UserEntityID",
                table: "BillingSubscriptions",
                column: "UserEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserEntityID",
                table: "PaymentMethods",
                column: "UserEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BillingSubscriptionEntityID",
                table: "PaymentTransactions",
                column: "BillingSubscriptionEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_UserEntityID",
                table: "PaymentTransactions",
                column: "UserEntityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "BillingSubscriptions");
        }
    }
}
