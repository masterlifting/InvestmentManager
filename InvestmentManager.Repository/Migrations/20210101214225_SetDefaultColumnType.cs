using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace InvestmentManager.Repository.Migrations
{
    public partial class SetDefaultColumnType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxyAddresses");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMin",
                table: "SellRecommendations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMid",
                table: "SellRecommendations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMax",
                table: "SellRecommendations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "LotMin",
                table: "SellRecommendations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "LotMid",
                table: "SellRecommendations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "LotMax",
                table: "SellRecommendations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Result",
                table: "Ratings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ReportComparisonValue",
                table: "Ratings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceComparisonValue",
                table: "Ratings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientComparisonValue",
                table: "Ratings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientAverageValue",
                table: "Ratings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CashFlowPositiveBalanceValue",
                table: "Ratings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "BuyRecommendations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMin",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMid",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMax",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LotMin",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "LotMid",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "LotMax",
                table: "SellRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Result",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ReportComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientComparisonValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CoefficientAverageValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CashFlowPositiveBalanceValue",
                table: "Ratings",
                type: "Decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "BuyRecommendations",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateTable(
                name: "ProxyAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Host = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Scheme = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyAddresses", x => x.Id);
                });
        }
    }
}
