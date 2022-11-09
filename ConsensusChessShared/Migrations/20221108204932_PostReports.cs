using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    public partial class PostReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_Board_BoardId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Network_NetworkId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteValidation_Post_PostId",
                table: "VoteValidation");

            migrationBuilder.DropIndex(
                name: "IX_Post_BoardId",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Post_NetworkId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "NetworkId",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "VoteValidation",
                newName: "VoteValidationPostId");

            migrationBuilder.RenameIndex(
                name: "IX_VoteValidation_PostId",
                table: "VoteValidation",
                newName: "IX_VoteValidation_VoteValidationPostId");

            migrationBuilder.AddColumn<string>(
                name: "NetworkName",
                table: "Post",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PostReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "text", nullable: true),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodeStateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostReport_Board_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Board",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostReport_NodeStates_NodeStateId",
                        column: x => x.NodeStateId,
                        principalTable: "NodeStates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostReport_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostReport_BoardId",
                table: "PostReport",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_PostReport_NodeStateId",
                table: "PostReport",
                column: "NodeStateId");

            migrationBuilder.CreateIndex(
                name: "IX_PostReport_PostId",
                table: "PostReport",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteValidation_PostReport_VoteValidationPostId",
                table: "VoteValidation",
                column: "VoteValidationPostId",
                principalTable: "PostReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteValidation_PostReport_VoteValidationPostId",
                table: "VoteValidation");

            migrationBuilder.DropTable(
                name: "PostReport");

            migrationBuilder.DropColumn(
                name: "NetworkName",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "VoteValidationPostId",
                table: "VoteValidation",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "IX_VoteValidation_VoteValidationPostId",
                table: "VoteValidation",
                newName: "IX_VoteValidation_PostId");

            migrationBuilder.AddColumn<Guid>(
                name: "BoardId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkId",
                table: "Post",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Post_BoardId",
                table: "Post",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_NetworkId",
                table: "Post",
                column: "NetworkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Board_BoardId",
                table: "Post",
                column: "BoardId",
                principalTable: "Board",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Network_NetworkId",
                table: "Post",
                column: "NetworkId",
                principalTable: "Network",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteValidation_Post_PostId",
                table: "VoteValidation",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
