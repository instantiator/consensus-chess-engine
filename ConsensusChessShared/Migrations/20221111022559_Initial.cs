using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Board",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PiecesFEN = table.Column<string>(name: "Pieces_FEN", type: "text", nullable: false),
                    ActiveSide = table.Column<int>(type: "integer", nullable: false),
                    CastlingAvailabilityFEN = table.Column<string>(name: "CastlingAvailability_FEN", type: "text", nullable: false),
                    EnPassantTargetSquareFEN = table.Column<string>(name: "EnPassantTargetSquare_FEN", type: "text", nullable: false),
                    HalfMoveClock = table.Column<int>(type: "integer", nullable: false),
                    FullMoveNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Finished = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MoveDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SideRules = table.Column<int>(type: "integer", nullable: false),
                    BlackNetworks = table.Column<List<string>>(type: "text[]", nullable: false),
                    WhiteNetworks = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NodeStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Shortcode = table.Column<string>(type: "text", nullable: false),
                    LastNotificationId = table.Column<long>(type: "bigint", nullable: false),
                    LastReplyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NetworkUserId = table.Column<string>(type: "text", nullable: false),
                    NetworkServer = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    NetworkServer = table.Column<string>(type: "text", nullable: false),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    NodeShortcode = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ReplyTo = table.Column<long>(type: "bigint", nullable: true),
                    Attempted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "text", nullable: true),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodeStateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_Board_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Board",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_NodeStates_NodeStateId",
                        column: x => x.NodeStateId,
                        principalTable: "NodeStates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Commitment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GameShortcode = table.Column<string>(type: "text", nullable: false),
                    GameSide = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commitment_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Alt = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Post_PostId",
                        column: x => x.PostId,
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoteValidation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidationState = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    VoteValidationPostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteValidation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteValidation_Post_VoteValidationPostId",
                        column: x => x.VoteValidationPostId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Move",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedVoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SideToPlay = table.Column<int>(type: "integer", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Move", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Move_Board_FromId",
                        column: x => x.FromId,
                        principalTable: "Board",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Move_Board_ToId",
                        column: x => x.ToId,
                        principalTable: "Board",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Move_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MoveText = table.Column<string>(type: "text", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MoveId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vote_Move_MoveId",
                        column: x => x.MoveId,
                        principalTable: "Move",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vote_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vote_VoteValidation_ValidationId",
                        column: x => x.ValidationId,
                        principalTable: "VoteValidation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_ParticipantId",
                table: "Commitment",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_PostId",
                table: "Media",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Move_FromId",
                table: "Move",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_Move_GameId",
                table: "Move",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Move_SelectedVoteId",
                table: "Move",
                column: "SelectedVoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Move_ToId",
                table: "Move",
                column: "ToId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeStates_Shortcode",
                table: "NodeStates",
                column: "Shortcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_BoardId",
                table: "Post",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_NodeStateId",
                table: "Post",
                column: "NodeStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_MoveId",
                table: "Vote",
                column: "MoveId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_ParticipantId",
                table: "Vote",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_ValidationId",
                table: "Vote",
                column: "ValidationId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteValidation_VoteValidationPostId",
                table: "VoteValidation",
                column: "VoteValidationPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Move_Vote_SelectedVoteId",
                table: "Move",
                column: "SelectedVoteId",
                principalTable: "Vote",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Participant_ParticipantId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteValidation_Post_VoteValidationPostId",
                table: "VoteValidation");

            migrationBuilder.DropForeignKey(
                name: "FK_Move_Board_FromId",
                table: "Move");

            migrationBuilder.DropForeignKey(
                name: "FK_Move_Board_ToId",
                table: "Move");

            migrationBuilder.DropForeignKey(
                name: "FK_Move_Games_GameId",
                table: "Move");

            migrationBuilder.DropForeignKey(
                name: "FK_Move_Vote_SelectedVoteId",
                table: "Move");

            migrationBuilder.DropTable(
                name: "Commitment");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Post");

            migrationBuilder.DropTable(
                name: "NodeStates");

            migrationBuilder.DropTable(
                name: "Board");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Vote");

            migrationBuilder.DropTable(
                name: "Move");

            migrationBuilder.DropTable(
                name: "VoteValidation");
        }
    }
}
