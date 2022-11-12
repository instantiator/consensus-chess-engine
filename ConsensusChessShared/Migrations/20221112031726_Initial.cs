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
                name: "board",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    piecesfen = table.Column<string>(name: "pieces_fen", type: "text", nullable: false),
                    activeside = table.Column<int>(name: "active_side", type: "integer", nullable: false),
                    castlingavailabilityfen = table.Column<string>(name: "castling_availability_fen", type: "text", nullable: false),
                    enpassanttargetsquarefen = table.Column<string>(name: "en_passant_target_square_fen", type: "text", nullable: false),
                    halfmoveclock = table.Column<int>(name: "half_move_clock", type: "integer", nullable: false),
                    fullmovenumber = table.Column<int>(name: "full_move_number", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduledstart = table.Column<DateTime>(name: "scheduled_start", type: "timestamp with time zone", nullable: false),
                    finished = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    moveduration = table.Column<TimeSpan>(name: "move_duration", type: "interval", nullable: false),
                    siderules = table.Column<int>(name: "side_rules", type: "integer", nullable: false),
                    blacknetworks = table.Column<List<string>>(name: "black_networks", type: "text[]", nullable: false),
                    whitenetworks = table.Column<List<string>>(name: "white_networks", type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "node_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    shortcode = table.Column<string>(type: "text", nullable: false),
                    lastnotificationid = table.Column<long>(name: "last_notification_id", type: "bigint", nullable: false),
                    lastreplyid = table.Column<long>(name: "last_reply_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_node_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "participant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    networkuserid = table.Column<string>(name: "network_user_id", type: "text", nullable: false),
                    networkserver = table.Column<string>(name: "network_server", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_participant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    networkserver = table.Column<string>(name: "network_server", type: "text", nullable: false),
                    appname = table.Column<string>(name: "app_name", type: "text", nullable: false),
                    nodeshortcode = table.Column<string>(name: "node_shortcode", type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    replyto = table.Column<long>(name: "reply_to", type: "bigint", nullable: true),
                    attempted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    errormessage = table.Column<string>(name: "error_message", type: "text", nullable: true),
                    exceptiontype = table.Column<string>(name: "exception_type", type: "text", nullable: true),
                    boardid = table.Column<Guid>(name: "board_id", type: "uuid", nullable: true),
                    nodestateid = table.Column<Guid>(name: "node_state_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_board_board_id",
                        column: x => x.boardid,
                        principalTable: "board",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_post_node_states_node_state_id",
                        column: x => x.nodestateid,
                        principalTable: "node_states",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "commitment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gameshortcode = table.Column<string>(name: "game_shortcode", type: "text", nullable: false),
                    gameside = table.Column<int>(name: "game_side", type: "integer", nullable: false),
                    participantid = table.Column<Guid>(name: "participant_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commitment", x => x.id);
                    table.ForeignKey(
                        name: "fk_commitment_participant_participant_id",
                        column: x => x.participantid,
                        principalTable: "participant",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    alt = table.Column<string>(type: "text", nullable: false),
                    postid = table.Column<Guid>(name: "post_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_post_post_id",
                        column: x => x.postid,
                        principalTable: "post",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vote_validation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    validationstate = table.Column<bool>(name: "validation_state", type: "boolean", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    votevalidationpostid = table.Column<Guid>(name: "vote_validation_post_id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vote_validation", x => x.id);
                    table.ForeignKey(
                        name: "fk_vote_validation_post_vote_validation_post_id",
                        column: x => x.votevalidationpostid,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "move",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fromid = table.Column<Guid>(name: "from_id", type: "uuid", nullable: false),
                    toid = table.Column<Guid>(name: "to_id", type: "uuid", nullable: true),
                    selectedvoteid = table.Column<Guid>(name: "selected_vote_id", type: "uuid", nullable: true),
                    sidetoplay = table.Column<int>(name: "side_to_play", type: "integer", nullable: false),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gameid = table.Column<Guid>(name: "game_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_move", x => x.id);
                    table.ForeignKey(
                        name: "fk_move_board_from_id",
                        column: x => x.fromid,
                        principalTable: "board",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_move_board_to_id",
                        column: x => x.toid,
                        principalTable: "board",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_move_games_game_id",
                        column: x => x.gameid,
                        principalTable: "games",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    movetext = table.Column<string>(name: "move_text", type: "text", nullable: false),
                    participantid = table.Column<Guid>(name: "participant_id", type: "uuid", nullable: false),
                    validationid = table.Column<Guid>(name: "validation_id", type: "uuid", nullable: false),
                    moveid = table.Column<Guid>(name: "move_id", type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vote", x => x.id);
                    table.ForeignKey(
                        name: "fk_vote_move_move_id",
                        column: x => x.moveid,
                        principalTable: "move",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_vote_participant_participant_id",
                        column: x => x.participantid,
                        principalTable: "participant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vote_vote_validation_validation_id",
                        column: x => x.validationid,
                        principalTable: "vote_validation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_commitment_participant_id",
                table: "commitment",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_post_id",
                table: "media",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_move_from_id",
                table: "move",
                column: "from_id");

            migrationBuilder.CreateIndex(
                name: "ix_move_game_id",
                table: "move",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_move_selected_vote_id",
                table: "move",
                column: "selected_vote_id");

            migrationBuilder.CreateIndex(
                name: "ix_move_to_id",
                table: "move",
                column: "to_id");

            migrationBuilder.CreateIndex(
                name: "ix_node_states_shortcode",
                table: "node_states",
                column: "shortcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_board_id",
                table: "post",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_node_state_id",
                table: "post",
                column: "node_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_move_id",
                table: "vote",
                column: "move_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_participant_id",
                table: "vote",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_validation_id",
                table: "vote",
                column: "validation_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_validation_vote_validation_post_id",
                table: "vote_validation",
                column: "vote_validation_post_id");

            migrationBuilder.AddForeignKey(
                name: "fk_move_vote_selected_vote_id",
                table: "move",
                column: "selected_vote_id",
                principalTable: "vote",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vote_participant_participant_id",
                table: "vote");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_validation_post_vote_validation_post_id",
                table: "vote_validation");

            migrationBuilder.DropForeignKey(
                name: "fk_move_board_from_id",
                table: "move");

            migrationBuilder.DropForeignKey(
                name: "fk_move_board_to_id",
                table: "move");

            migrationBuilder.DropForeignKey(
                name: "fk_move_games_game_id",
                table: "move");

            migrationBuilder.DropForeignKey(
                name: "fk_move_vote_selected_vote_id",
                table: "move");

            migrationBuilder.DropTable(
                name: "commitment");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "participant");

            migrationBuilder.DropTable(
                name: "post");

            migrationBuilder.DropTable(
                name: "node_states");

            migrationBuilder.DropTable(
                name: "board");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "vote");

            migrationBuilder.DropTable(
                name: "move");

            migrationBuilder.DropTable(
                name: "vote_validation");
        }
    }
}
