using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessFeatureTests.Migrations
{
    /// <inheritdoc />
    public partial class ExpectedAccountName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "expected_account_name",
                table: "network",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expected_account_name",
                table: "network");
        }
    }
}
