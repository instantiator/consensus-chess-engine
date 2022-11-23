using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessFeatureTests.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "board",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    fen = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    shortcode = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    scheduledstart = table.Column<DateTime>(name: "scheduled_start", type: "TEXT", nullable: false),
                    finished = table.Column<DateTime>(type: "TEXT", nullable: true),
                    moveduration = table.Column<TimeSpan>(name: "move_duration", type: "TEXT", nullable: false),
                    siderules = table.Column<int>(name: "side_rules", type: "INTEGER", nullable: false),
                    state = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "network",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    networkserver = table.Column<string>(name: "network_server", type: "TEXT", nullable: false),
                    appkey = table.Column<string>(name: "app_key", type: "TEXT", nullable: false),
                    appsecret = table.Column<string>(name: "app_secret", type: "TEXT", nullable: false),
                    apptoken = table.Column<string>(name: "app_token", type: "TEXT", nullable: false),
                    appname = table.Column<string>(name: "app_name", type: "TEXT", nullable: false),
                    authorisedaccounts = table.Column<string>(name: "authorised_accounts", type: "TEXT", nullable: false),
                    dryruns = table.Column<bool>(name: "dry_runs", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "social_username",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    networktype = table.Column<int>(name: "network_type", type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    server = table.Column<string>(type: "TEXT", nullable: false),
                    displayname = table.Column<string>(name: "display_name", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_social_username", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "move",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    fromid = table.Column<Guid>(name: "from_id", type: "TEXT", nullable: false),
                    toid = table.Column<Guid>(name: "to_id", type: "TEXT", nullable: true),
                    selectedsan = table.Column<string>(name: "selected_san", type: "TEXT", nullable: true),
                    deadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    gameid = table.Column<Guid>(name: "game_id", type: "TEXT", nullable: true)
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
                name: "stored_string",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    value = table.Column<string>(type: "TEXT", nullable: true),
                    gameid = table.Column<Guid>(name: "game_id", type: "TEXT", nullable: true),
                    gameid1 = table.Column<Guid>(name: "game_id1", type: "TEXT", nullable: true),
                    gameid2 = table.Column<Guid>(name: "game_id2", type: "TEXT", nullable: true),
                    gameid3 = table.Column<Guid>(name: "game_id3", type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stored_string", x => x.id);
                    table.ForeignKey(
                        name: "fk_stored_string_games_game_id",
                        column: x => x.gameid,
                        principalTable: "games",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stored_string_games_game_id1",
                        column: x => x.gameid1,
                        principalTable: "games",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stored_string_games_game_id2",
                        column: x => x.gameid2,
                        principalTable: "games",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stored_string_games_game_id3",
                        column: x => x.gameid3,
                        principalTable: "games",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "node_state",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    shortcode = table.Column<string>(type: "TEXT", nullable: false),
                    lastnotificationid = table.Column<long>(name: "last_notification_id", type: "INTEGER", nullable: false),
                    networkid = table.Column<Guid>(name: "network_id", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_node_state", x => x.id);
                    table.ForeignKey(
                        name: "fk_node_state_network_network_id",
                        column: x => x.networkid,
                        principalTable: "network",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    usernameid = table.Column<Guid>(name: "username_id", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_participant", x => x.id);
                    table.ForeignKey(
                        name: "fk_participant_social_username_username_id",
                        column: x => x.usernameid,
                        principalTable: "social_username",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    networkserver = table.Column<string>(name: "network_server", type: "TEXT", nullable: false),
                    appname = table.Column<string>(name: "app_name", type: "TEXT", nullable: false),
                    nodeshortcode = table.Column<string>(name: "node_shortcode", type: "TEXT", nullable: false),
                    message = table.Column<string>(type: "TEXT", nullable: false),
                    networkpostid = table.Column<long>(name: "network_post_id", type: "INTEGER", nullable: true),
                    networkreplytoid = table.Column<long>(name: "network_reply_to_id", type: "INTEGER", nullable: true),
                    attempted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    succeeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    errormessage = table.Column<string>(name: "error_message", type: "TEXT", nullable: true),
                    exceptiontype = table.Column<string>(name: "exception_type", type: "TEXT", nullable: true),
                    boardid = table.Column<Guid>(name: "board_id", type: "TEXT", nullable: true),
                    gameid = table.Column<Guid>(name: "game_id", type: "TEXT", nullable: true),
                    nodestateid = table.Column<Guid>(name: "node_state_id", type: "TEXT", nullable: true)
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
                        name: "fk_post_games_game_id",
                        column: x => x.gameid,
                        principalTable: "games",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_post_node_state_node_state_id",
                        column: x => x.nodestateid,
                        principalTable: "node_state",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "commitment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    gameshortcode = table.Column<string>(name: "game_shortcode", type: "TEXT", nullable: false),
                    gameside = table.Column<int>(name: "game_side", type: "INTEGER", nullable: false),
                    participantid = table.Column<Guid>(name: "participant_id", type: "TEXT", nullable: true)
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
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    data = table.Column<byte[]>(type: "BLOB", nullable: false),
                    alt = table.Column<string>(type: "TEXT", nullable: false),
                    postid = table.Column<Guid>(name: "post_id", type: "TEXT", nullable: true)
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
                name: "vote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    networkmovepostid = table.Column<long>(name: "network_move_post_id", type: "INTEGER", nullable: false),
                    movetext = table.Column<string>(name: "move_text", type: "TEXT", nullable: false),
                    participantid = table.Column<Guid>(name: "participant_id", type: "TEXT", nullable: false),
                    validationstate = table.Column<int>(name: "validation_state", type: "INTEGER", nullable: false),
                    validationpostid = table.Column<Guid>(name: "validation_post_id", type: "TEXT", nullable: true),
                    moveid = table.Column<Guid>(name: "move_id", type: "TEXT", nullable: true)
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
                        name: "fk_vote_post_validation_post_id",
                        column: x => x.validationpostid,
                        principalTable: "post",
                        principalColumn: "id");
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
                name: "ix_move_to_id",
                table: "move",
                column: "to_id");

            migrationBuilder.CreateIndex(
                name: "ix_node_state_network_id",
                table: "node_state",
                column: "network_id");

            migrationBuilder.CreateIndex(
                name: "ix_node_state_shortcode",
                table: "node_state",
                column: "shortcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_participant_username_id",
                table: "participant",
                column: "username_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_board_id",
                table: "post",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_game_id",
                table: "post",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_node_state_id",
                table: "post",
                column: "node_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_stored_string_game_id",
                table: "stored_string",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_stored_string_game_id1",
                table: "stored_string",
                column: "game_id1");

            migrationBuilder.CreateIndex(
                name: "ix_stored_string_game_id2",
                table: "stored_string",
                column: "game_id2");

            migrationBuilder.CreateIndex(
                name: "ix_stored_string_game_id3",
                table: "stored_string",
                column: "game_id3");

            migrationBuilder.CreateIndex(
                name: "ix_vote_move_id",
                table: "vote",
                column: "move_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_participant_id",
                table: "vote",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_validation_post_id",
                table: "vote",
                column: "validation_post_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commitment");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "stored_string");

            migrationBuilder.DropTable(
                name: "vote");

            migrationBuilder.DropTable(
                name: "move");

            migrationBuilder.DropTable(
                name: "participant");

            migrationBuilder.DropTable(
                name: "post");

            migrationBuilder.DropTable(
                name: "social_username");

            migrationBuilder.DropTable(
                name: "board");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "node_state");

            migrationBuilder.DropTable(
                name: "network");
        }
    }
}
