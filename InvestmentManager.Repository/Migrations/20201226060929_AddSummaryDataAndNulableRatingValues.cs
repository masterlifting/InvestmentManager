using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace InvestmentManager.Repository.Migrations
{
    public partial class AddSummaryDataAndNulableRatingValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ReportComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientAverageValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CashFlowPositiveBalanceValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.CreateTable(
                name: "AccountSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    FreeSum = table.Column<decimal>(type: "numeric", nullable: false),
                    InvestedSum = table.Column<decimal>(type: "numeric", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSummaries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountSummaries_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComissionSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    TotalSum = table.Column<decimal>(type: "numeric", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComissionSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComissionSummaries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComissionSummaries_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    ActualLot = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySummaries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanySummaries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanySummaries_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DividendSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    TotalSum = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DividendSummaries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DividendSummaries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DividendSummaries_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    TotalSoldQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSoldCost = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPurchasedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPurchasedCost = table.Column<decimal>(type: "numeric", nullable: false),
                    AvgSoldRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AvgPurchasedRate = table.Column<decimal>(type: "numeric", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRateSummaries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeRateSummaries_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummaries_AccountId",
                table: "AccountSummaries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummaries_CurrencyId",
                table: "AccountSummaries",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComissionSummaries_AccountId",
                table: "ComissionSummaries",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComissionSummaries_CurrencyId",
                table: "ComissionSummaries",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySummaries_AccountId",
                table: "CompanySummaries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySummaries_CompanyId",
                table: "CompanySummaries",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySummaries_CurrencyId",
                table: "CompanySummaries",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DividendSummaries_AccountId",
                table: "DividendSummaries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DividendSummaries_CompanyId",
                table: "DividendSummaries",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendSummaries_CurrencyId",
                table: "DividendSummaries",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateSummaries_AccountId",
                table: "ExchangeRateSummaries",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateSummaries_CurrencyId",
                table: "ExchangeRateSummaries",
                column: "CurrencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSummaries");

            migrationBuilder.DropTable(
                name: "ComissionSummaries");

            migrationBuilder.DropTable(
                name: "CompanySummaries");

            migrationBuilder.DropTable(
                name: "DividendSummaries");

            migrationBuilder.DropTable(
                name: "ExchangeRateSummaries");

            migrationBuilder.AlterColumn<decimal>(
                name: "ReportComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientAverageValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CashFlowPositiveBalanceValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);
        }
    }
}
