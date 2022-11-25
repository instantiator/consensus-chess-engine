using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class SocialUsernameModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "shortcode",
                table: "social_username",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shortcode",
                table: "social_username");
        }
    }
}
