using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    public partial class PostReplies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReplyTo",
                table: "Post",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyTo",
                table: "Post");
        }
    }
}
