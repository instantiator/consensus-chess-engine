using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessFeatureTests.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyNetworkForMastonet2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app_key",
                table: "network");

            migrationBuilder.DropColumn(
                name: "app_secret",
                table: "network");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "app_key",
                table: "network",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "app_secret",
                table: "network",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
