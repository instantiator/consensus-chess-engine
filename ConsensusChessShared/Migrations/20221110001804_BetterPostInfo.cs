using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    public partial class BetterPostInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NetworkName",
                table: "Post",
                newName: "NodeShortcode");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Network",
                newName: "AppName");

            migrationBuilder.AddColumn<string>(
                name: "AppName",
                table: "Post",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NetworkServer",
                table: "Post",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppName",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "NetworkServer",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "NodeShortcode",
                table: "Post",
                newName: "NetworkName");

            migrationBuilder.RenameColumn(
                name: "AppName",
                table: "Network",
                newName: "Name");
        }
    }
}
