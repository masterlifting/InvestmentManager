using Microsoft.EntityFrameworkCore.Migrations;

namespace InvestmentManager.Repository.Migrations
{
    public partial class ChangeCompanySummareActualLotType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ActualLot",
                table: "CompanySummaries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ActualLot",
                table: "CompanySummaries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
