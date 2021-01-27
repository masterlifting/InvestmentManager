using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InvestmentManager.Repository.Migrations
{
    public partial class RefactoringPriceService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "ProviderUri",
                table: "Exchanges");

            migrationBuilder.CreateTable(
                name: "Weekends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ExchangeWeekend = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExchangeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weekends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Weekends_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalTable: "Exchanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Weekends_ExchangeId",
                table: "Weekends",
                column: "ExchangeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weekends");

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "Exchanges",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderUri",
                table: "Exchanges",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
