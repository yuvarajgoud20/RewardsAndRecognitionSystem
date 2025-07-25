using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardsAndRecognitionRepository.Migrations
{
  
    public partial class SoftDelete : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Categories",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Nominations",
            nullable: false,
            defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Nominations");
        }
    }
}
