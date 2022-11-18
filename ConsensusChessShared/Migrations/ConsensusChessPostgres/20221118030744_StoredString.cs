using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations.ConsensusChessPostgres
{
    /// <inheritdoc />
    public partial class StoredString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "black_participant_network_servers",
                table: "games");

            migrationBuilder.DropColumn(
                name: "black_posting_node_shortcodes",
                table: "games");

            migrationBuilder.DropColumn(
                name: "white_participant_network_servers",
                table: "games");

            migrationBuilder.DropColumn(
                name: "white_posting_node_shortcodes",
                table: "games");

            migrationBuilder.CreateTable(
                name: "stored_string",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    gameid = table.Column<Guid>(name: "game_id", type: "uuid", nullable: true),
                    gameid1 = table.Column<Guid>(name: "game_id1", type: "uuid", nullable: true),
                    gameid2 = table.Column<Guid>(name: "game_id2", type: "uuid", nullable: true),
                    gameid3 = table.Column<Guid>(name: "game_id3", type: "uuid", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stored_string");

            migrationBuilder.AddColumn<List<string>>(
                name: "black_participant_network_servers",
                table: "games",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "black_posting_node_shortcodes",
                table: "games",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "white_participant_network_servers",
                table: "games",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "white_posting_node_shortcodes",
                table: "games",
                type: "text[]",
                nullable: false);
        }
    }
}
