using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    public partial class Shortcodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NodeName",
                table: "NodeStates",
                newName: "Shortcode");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NodeStates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NodeStates_Shortcode",
                table: "NodeStates",
                column: "Shortcode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NodeStates_Shortcode",
                table: "NodeStates");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "NodeStates");

            migrationBuilder.RenameColumn(
                name: "Shortcode",
                table: "NodeStates",
                newName: "NodeName");
        }
    }
}
