using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InvestmentManager.Repository.Migrations
{
    public partial class AddDateSplitField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateSplit",
                table: "Companies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateSplit",
                table: "Companies");
        }
    }
}
