using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardsAndRecognitionRepository.Migrations
{
    /// <inheritdoc />
    public partial class CategoryToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Categories");
        }
    }
}
