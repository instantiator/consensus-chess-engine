using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class MoveSAN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "move_san",
                table: "vote",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "move_san",
                table: "vote");
        }
    }
}
