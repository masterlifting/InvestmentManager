using Microsoft.EntityFrameworkCore.Migrations;

namespace InvestmentManager.Repository.Migrations
{
    public partial class ChangeCompanyAndAccountSummaryType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRateSummaries_AccountId",
                table: "ExchangeRateSummaries");

            migrationBuilder.DropIndex(
                name: "IX_DividendSummaries_CompanyId",
                table: "DividendSummaries");

            migrationBuilder.DropIndex(
                name: "IX_CompanySummaries_CompanyId",
                table: "CompanySummaries");

            migrationBuilder.DropIndex(
                name: "IX_ComissionSummaries_AccountId",
                table: "ComissionSummaries");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateSummaries_AccountId",
                table: "ExchangeRateSummaries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DividendSummaries_CompanyId",
                table: "DividendSummaries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySummaries_CompanyId",
                table: "CompanySummaries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComissionSummaries_AccountId",
                table: "ComissionSummaries",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRateSummaries_AccountId",
                table: "ExchangeRateSummaries");

            migrationBuilder.DropIndex(
                name: "IX_DividendSummaries_CompanyId",
                table: "DividendSummaries");

            migrationBuilder.DropIndex(
                name: "IX_CompanySummaries_CompanyId",
                table: "CompanySummaries");

            migrationBuilder.DropIndex(
                name: "IX_ComissionSummaries_AccountId",
                table: "ComissionSummaries");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateSummaries_AccountId",
                table: "ExchangeRateSummaries",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendSummaries_CompanyId",
                table: "DividendSummaries",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySummaries_CompanyId",
                table: "CompanySummaries",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComissionSummaries_AccountId",
                table: "ComissionSummaries",
                column: "AccountId",
                unique: true);
        }
    }
}
