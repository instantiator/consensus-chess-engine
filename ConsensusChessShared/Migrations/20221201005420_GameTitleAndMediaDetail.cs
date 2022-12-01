using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class GameTitleAndMediaDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "node_shortcode",
                table: "post",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "network_server",
                table: "post",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "post",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "app_name",
                table: "post",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "filename",
                table: "media",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "preview_url",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "social_id",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "filename",
                table: "media");

            migrationBuilder.DropColumn(
                name: "preview_url",
                table: "media");

            migrationBuilder.DropColumn(
                name: "social_id",
                table: "media");

            migrationBuilder.DropColumn(
                name: "title",
                table: "games");

            migrationBuilder.AlterColumn<string>(
                name: "node_shortcode",
                table: "post",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "network_server",
                table: "post",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "post",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "app_name",
                table: "post",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
