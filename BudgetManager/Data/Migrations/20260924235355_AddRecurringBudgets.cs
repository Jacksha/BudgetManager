using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManager.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Budgets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Budgets");
        }
    }
}
