using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class DistinguishPostingNodesFromParticipantNetworks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_commitment_participant_participant_id",
                table: "commitment");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_participant_participant_id",
                table: "vote");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_vote_validation_validation_id",
                table: "vote");

            migrationBuilder.DropTable(
                name: "vote_validation");

            migrationBuilder.DropPrimaryKey(
                name: "pk_participant",
                table: "participant");

            migrationBuilder.DropColumn(
                name: "side_to_play",
                table: "move");

            migrationBuilder.DropColumn(
                name: "active_side",
                table: "board");

            migrationBuilder.DropColumn(
                name: "castling_availability_fen",
                table: "board");

            migrationBuilder.DropColumn(
                name: "en_passant_target_square_fen",
                table: "board");

            migrationBuilder.DropColumn(
                name: "full_move_number",
                table: "board");

            migrationBuilder.DropColumn(
                name: "half_move_clock",
                table: "board");

            migrationBuilder.RenameTable(
                name: "participant",
                newName: "participants");

            migrationBuilder.RenameColumn(
                name: "validation_id",
                table: "vote",
                newName: "validation_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_vote_validation_id",
                table: "vote",
                newName: "ix_vote_validation_post_id");

            migrationBuilder.RenameColumn(
                name: "reply_to",
                table: "post",
                newName: "network_reply_to_id");

            migrationBuilder.RenameColumn(
                name: "white_networks",
                table: "games",
                newName: "white_posting_node_shortcodes");

            migrationBuilder.RenameColumn(
                name: "black_networks",
                table: "games",
                newName: "white_participant_network_servers");

            migrationBuilder.RenameColumn(
                name: "pieces_fen",
                table: "board",
                newName: "fen");

            migrationBuilder.RenameColumn(
                name: "network_user_id",
                table: "participants",
                newName: "network_user_account");

            migrationBuilder.AddColumn<long>(
                name: "network_move_post_id",
                table: "vote",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "validation_state",
                table: "vote",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "network_post_id",
                table: "post",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "network_id",
                table: "node_states",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "shortcode",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_participants",
                table: "participants",
                column: "id");

            migrationBuilder.CreateTable(
                name: "network",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    networkserver = table.Column<string>(name: "network_server", type: "text", nullable: false),
                    appkey = table.Column<string>(name: "app_key", type: "text", nullable: false),
                    appsecret = table.Column<string>(name: "app_secret", type: "text", nullable: false),
                    apptoken = table.Column<string>(name: "app_token", type: "text", nullable: false),
                    appname = table.Column<string>(name: "app_name", type: "text", nullable: false),
                    authorisedaccounts = table.Column<string>(name: "authorised_accounts", type: "text", nullable: false),
                    dryruns = table.Column<bool>(name: "dry_runs", type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_node_states_network_id",
                table: "node_states",
                column: "network_id");

            migrationBuilder.AddForeignKey(
                name: "fk_commitment_participants_participant_id",
                table: "commitment",
                column: "participant_id",
                principalTable: "participants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_node_states_network_network_id",
                table: "node_states",
                column: "network_id",
                principalTable: "network",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vote_participants_participant_id",
                table: "vote",
                column: "participant_id",
                principalTable: "participants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vote_post_validation_post_id",
                table: "vote",
                column: "validation_post_id",
                principalTable: "post",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_commitment_participants_participant_id",
                table: "commitment");

            migrationBuilder.DropForeignKey(
                name: "fk_node_states_network_network_id",
                table: "node_states");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_participants_participant_id",
                table: "vote");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_post_validation_post_id",
                table: "vote");

            migrationBuilder.DropTable(
                name: "network");

            migrationBuilder.DropIndex(
                name: "ix_node_states_network_id",
                table: "node_states");

            migrationBuilder.DropPrimaryKey(
                name: "pk_participants",
                table: "participants");

            migrationBuilder.DropColumn(
                name: "network_move_post_id",
                table: "vote");

            migrationBuilder.DropColumn(
                name: "validation_state",
                table: "vote");

            migrationBuilder.DropColumn(
                name: "network_post_id",
                table: "post");

            migrationBuilder.DropColumn(
                name: "network_id",
                table: "node_states");

            migrationBuilder.DropColumn(
                name: "black_participant_network_servers",
                table: "games");

            migrationBuilder.DropColumn(
                name: "black_posting_node_shortcodes",
                table: "games");

            migrationBuilder.DropColumn(
                name: "description",
                table: "games");

            migrationBuilder.DropColumn(
                name: "shortcode",
                table: "games");

            migrationBuilder.RenameTable(
                name: "participants",
                newName: "participant");

            migrationBuilder.RenameColumn(
                name: "validation_post_id",
                table: "vote",
                newName: "validation_id");

            migrationBuilder.RenameIndex(
                name: "ix_vote_validation_post_id",
                table: "vote",
                newName: "ix_vote_validation_id");

            migrationBuilder.RenameColumn(
                name: "network_reply_to_id",
                table: "post",
                newName: "reply_to");

            migrationBuilder.RenameColumn(
                name: "white_posting_node_shortcodes",
                table: "games",
                newName: "white_networks");

            migrationBuilder.RenameColumn(
                name: "white_participant_network_servers",
                table: "games",
                newName: "black_networks");

            migrationBuilder.RenameColumn(
                name: "fen",
                table: "board",
                newName: "pieces_fen");

            migrationBuilder.RenameColumn(
                name: "network_user_account",
                table: "participant",
                newName: "network_user_id");

            migrationBuilder.AddColumn<int>(
                name: "side_to_play",
                table: "move",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "active_side",
                table: "board",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "castling_availability_fen",
                table: "board",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "en_passant_target_square_fen",
                table: "board",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "full_move_number",
                table: "board",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "half_move_clock",
                table: "board",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_participant",
                table: "participant",
                column: "id");

            migrationBuilder.CreateTable(
                name: "vote_validation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    votevalidationpostid = table.Column<Guid>(name: "vote_validation_post_id", type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    validationstate = table.Column<bool>(name: "validation_state", type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "ix_vote_validation_vote_validation_post_id",
                table: "vote_validation",
                column: "vote_validation_post_id");

            migrationBuilder.AddForeignKey(
                name: "fk_commitment_participant_participant_id",
                table: "commitment",
                column: "participant_id",
                principalTable: "participant",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_vote_participant_participant_id",
                table: "vote",
                column: "participant_id",
                principalTable: "participant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vote_vote_validation_validation_id",
                table: "vote",
                column: "validation_id",
                principalTable: "vote_validation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
