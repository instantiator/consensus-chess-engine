using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    /// <inheritdoc />
    public partial class ModifyCollectionNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_commitment_participants_participant_id",
                table: "commitment");

            migrationBuilder.DropForeignKey(
                name: "fk_node_states_network_network_id",
                table: "node_states");

            migrationBuilder.DropForeignKey(
                name: "fk_post_node_states_node_state_id",
                table: "post");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_participants_participant_id",
                table: "vote");

            migrationBuilder.DropPrimaryKey(
                name: "pk_participants",
                table: "participants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_node_states",
                table: "node_states");

            migrationBuilder.RenameTable(
                name: "participants",
                newName: "participant");

            migrationBuilder.RenameTable(
                name: "node_states",
                newName: "node_state");

            migrationBuilder.RenameIndex(
                name: "ix_node_states_shortcode",
                table: "node_state",
                newName: "ix_node_state_shortcode");

            migrationBuilder.RenameIndex(
                name: "ix_node_states_network_id",
                table: "node_state",
                newName: "ix_node_state_network_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_participant",
                table: "participant",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_node_state",
                table: "node_state",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_commitment_participant_participant_id",
                table: "commitment",
                column: "participant_id",
                principalTable: "participant",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_node_state_network_network_id",
                table: "node_state",
                column: "network_id",
                principalTable: "network",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_node_state_node_state_id",
                table: "post",
                column: "node_state_id",
                principalTable: "node_state",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_vote_participant_participant_id",
                table: "vote",
                column: "participant_id",
                principalTable: "participant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_commitment_participant_participant_id",
                table: "commitment");

            migrationBuilder.DropForeignKey(
                name: "fk_node_state_network_network_id",
                table: "node_state");

            migrationBuilder.DropForeignKey(
                name: "fk_post_node_state_node_state_id",
                table: "post");

            migrationBuilder.DropForeignKey(
                name: "fk_vote_participant_participant_id",
                table: "vote");

            migrationBuilder.DropPrimaryKey(
                name: "pk_participant",
                table: "participant");

            migrationBuilder.DropPrimaryKey(
                name: "pk_node_state",
                table: "node_state");

            migrationBuilder.RenameTable(
                name: "participant",
                newName: "participants");

            migrationBuilder.RenameTable(
                name: "node_state",
                newName: "node_states");

            migrationBuilder.RenameIndex(
                name: "ix_node_state_shortcode",
                table: "node_states",
                newName: "ix_node_states_shortcode");

            migrationBuilder.RenameIndex(
                name: "ix_node_state_network_id",
                table: "node_states",
                newName: "ix_node_states_network_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_participants",
                table: "participants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_node_states",
                table: "node_states",
                column: "id");

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
                name: "fk_post_node_states_node_state_id",
                table: "post",
                column: "node_state_id",
                principalTable: "node_states",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_vote_participants_participant_id",
                table: "vote",
                column: "participant_id",
                principalTable: "participants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
