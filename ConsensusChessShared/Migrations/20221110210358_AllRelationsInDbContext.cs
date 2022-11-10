using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    public partial class AllRelationsInDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commitment_Games_GameId",
                table: "Commitment");

            migrationBuilder.DropForeignKey(
                name: "FK_Commitment_Participant_ParticipantId",
                table: "Commitment");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Post_PostId",
                table: "Media");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Participant_Network_NetworkId",
                table: "Participant");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReport_Board_BoardId",
                table: "PostReport");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReport_NodeStates_NodeStateId",
                table: "PostReport");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReport_Post_PostId",
                table: "PostReport");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Move_MoveId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Participant_ParticipantId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_VoteValidation_ValidationId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteValidation_PostReport_VoteValidationPostId",
                table: "VoteValidation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VoteValidation",
                table: "VoteValidation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReport",
                table: "PostReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Post",
                table: "Post");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participant",
                table: "Participant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Network",
                table: "Network");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Move",
                table: "Move");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Media",
                table: "Media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Commitment",
                table: "Commitment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Board",
                table: "Board");

            migrationBuilder.RenameTable(
                name: "VoteValidation",
                newName: "VoteValidations");

            migrationBuilder.RenameTable(
                name: "Vote",
                newName: "Votes");

            migrationBuilder.RenameTable(
                name: "PostReport",
                newName: "PostReports");

            migrationBuilder.RenameTable(
                name: "Post",
                newName: "Posts");

            migrationBuilder.RenameTable(
                name: "Participant",
                newName: "Participants");

            migrationBuilder.RenameTable(
                name: "Network",
                newName: "Networks");

            migrationBuilder.RenameTable(
                name: "Move",
                newName: "Moves");

            migrationBuilder.RenameTable(
                name: "Media",
                newName: "Medias");

            migrationBuilder.RenameTable(
                name: "Commitment",
                newName: "Commitments");

            migrationBuilder.RenameTable(
                name: "Board",
                newName: "Boards");

            migrationBuilder.RenameIndex(
                name: "IX_VoteValidation_VoteValidationPostId",
                table: "VoteValidations",
                newName: "IX_VoteValidations_VoteValidationPostId");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_ValidationId",
                table: "Votes",
                newName: "IX_Votes_ValidationId");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_ParticipantId",
                table: "Votes",
                newName: "IX_Votes_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_MoveId",
                table: "Votes",
                newName: "IX_Votes_MoveId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReport_PostId",
                table: "PostReports",
                newName: "IX_PostReports_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReport_NodeStateId",
                table: "PostReports",
                newName: "IX_PostReports_NodeStateId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReport_BoardId",
                table: "PostReports",
                newName: "IX_PostReports_BoardId");

            migrationBuilder.RenameIndex(
                name: "IX_Participant_NetworkId",
                table: "Participants",
                newName: "IX_Participants_NetworkId");

            migrationBuilder.RenameIndex(
                name: "IX_Move_ToId",
                table: "Moves",
                newName: "IX_Moves_ToId");

            migrationBuilder.RenameIndex(
                name: "IX_Move_SelectedVoteId",
                table: "Moves",
                newName: "IX_Moves_SelectedVoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Move_GameId",
                table: "Moves",
                newName: "IX_Moves_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Move_FromId",
                table: "Moves",
                newName: "IX_Moves_FromId");

            migrationBuilder.RenameIndex(
                name: "IX_Media_PostId",
                table: "Medias",
                newName: "IX_Medias_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Commitment_ParticipantId",
                table: "Commitments",
                newName: "IX_Commitments_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Commitment_GameId",
                table: "Commitments",
                newName: "IX_Commitments_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VoteValidations",
                table: "VoteValidations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReports",
                table: "PostReports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participants",
                table: "Participants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Networks",
                table: "Networks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Moves",
                table: "Moves",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medias",
                table: "Medias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Commitments",
                table: "Commitments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boards",
                table: "Boards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Commitments_Games_GameId",
                table: "Commitments",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Commitments_Participants_ParticipantId",
                table: "Commitments",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Posts_PostId",
                table: "Medias",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Moves_Boards_FromId",
                table: "Moves",
                column: "FromId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Moves_Boards_ToId",
                table: "Moves",
                column: "ToId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Moves_Games_GameId",
                table: "Moves",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Moves_Votes_SelectedVoteId",
                table: "Moves",
                column: "SelectedVoteId",
                principalTable: "Votes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Networks_NetworkId",
                table: "Participants",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReports_Boards_BoardId",
                table: "PostReports",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReports_NodeStates_NodeStateId",
                table: "PostReports",
                column: "NodeStateId",
                principalTable: "NodeStates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReports_Posts_PostId",
                table: "PostReports",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Moves_MoveId",
                table: "Votes",
                column: "MoveId",
                principalTable: "Moves",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Participants_ParticipantId",
                table: "Votes",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_VoteValidations_ValidationId",
                table: "Votes",
                column: "ValidationId",
                principalTable: "VoteValidations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteValidations_PostReports_VoteValidationPostId",
                table: "VoteValidations",
                column: "VoteValidationPostId",
                principalTable: "PostReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commitments_Games_GameId",
                table: "Commitments");

            migrationBuilder.DropForeignKey(
                name: "FK_Commitments_Participants_ParticipantId",
                table: "Commitments");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Posts_PostId",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Moves_Boards_FromId",
                table: "Moves");

            migrationBuilder.DropForeignKey(
                name: "FK_Moves_Boards_ToId",
                table: "Moves");

            migrationBuilder.DropForeignKey(
                name: "FK_Moves_Games_GameId",
                table: "Moves");

            migrationBuilder.DropForeignKey(
                name: "FK_Moves_Votes_SelectedVoteId",
                table: "Moves");

            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Networks_NetworkId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReports_Boards_BoardId",
                table: "PostReports");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReports_NodeStates_NodeStateId",
                table: "PostReports");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReports_Posts_PostId",
                table: "PostReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Moves_MoveId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Participants_ParticipantId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_VoteValidations_ValidationId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteValidations_PostReports_VoteValidationPostId",
                table: "VoteValidations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VoteValidations",
                table: "VoteValidations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReports",
                table: "PostReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participants",
                table: "Participants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Networks",
                table: "Networks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Moves",
                table: "Moves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Medias",
                table: "Medias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Commitments",
                table: "Commitments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Boards",
                table: "Boards");

            migrationBuilder.RenameTable(
                name: "VoteValidations",
                newName: "VoteValidation");

            migrationBuilder.RenameTable(
                name: "Votes",
                newName: "Vote");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Post");

            migrationBuilder.RenameTable(
                name: "PostReports",
                newName: "PostReport");

            migrationBuilder.RenameTable(
                name: "Participants",
                newName: "Participant");

            migrationBuilder.RenameTable(
                name: "Networks",
                newName: "Network");

            migrationBuilder.RenameTable(
                name: "Moves",
                newName: "Move");

            migrationBuilder.RenameTable(
                name: "Medias",
                newName: "Media");

            migrationBuilder.RenameTable(
                name: "Commitments",
                newName: "Commitment");

            migrationBuilder.RenameTable(
                name: "Boards",
                newName: "Board");

            migrationBuilder.RenameIndex(
                name: "IX_VoteValidations_VoteValidationPostId",
                table: "VoteValidation",
                newName: "IX_VoteValidation_VoteValidationPostId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_ValidationId",
                table: "Vote",
                newName: "IX_Vote_ValidationId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_ParticipantId",
                table: "Vote",
                newName: "IX_Vote_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_MoveId",
                table: "Vote",
                newName: "IX_Vote_MoveId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReports_PostId",
                table: "PostReport",
                newName: "IX_PostReport_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReports_NodeStateId",
                table: "PostReport",
                newName: "IX_PostReport_NodeStateId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReports_BoardId",
                table: "PostReport",
                newName: "IX_PostReport_BoardId");

            migrationBuilder.RenameIndex(
                name: "IX_Participants_NetworkId",
                table: "Participant",
                newName: "IX_Participant_NetworkId");

            migrationBuilder.RenameIndex(
                name: "IX_Moves_ToId",
                table: "Move",
                newName: "IX_Move_ToId");

            migrationBuilder.RenameIndex(
                name: "IX_Moves_SelectedVoteId",
                table: "Move",
                newName: "IX_Move_SelectedVoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Moves_GameId",
                table: "Move",
                newName: "IX_Move_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Moves_FromId",
                table: "Move",
                newName: "IX_Move_FromId");

            migrationBuilder.RenameIndex(
                name: "IX_Medias_PostId",
                table: "Media",
                newName: "IX_Media_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Commitments_ParticipantId",
                table: "Commitment",
                newName: "IX_Commitment_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Commitments_GameId",
                table: "Commitment",
                newName: "IX_Commitment_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VoteValidation",
                table: "VoteValidation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Post",
                table: "Post",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReport",
                table: "PostReport",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participant",
                table: "Participant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Network",
                table: "Network",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Move",
                table: "Move",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Media",
                table: "Media",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Commitment",
                table: "Commitment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Board",
                table: "Board",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Commitment_Games_GameId",
                table: "Commitment",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Commitment_Participant_ParticipantId",
                table: "Commitment",
                column: "ParticipantId",
                principalTable: "Participant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Post_PostId",
                table: "Media",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Move_Board_FromId",
                table: "Move",
                column: "FromId",
                principalTable: "Board",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Move_Board_ToId",
                table: "Move",
                column: "ToId",
                principalTable: "Board",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Move_Games_GameId",
                table: "Move",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Move_Vote_SelectedVoteId",
                table: "Move",
                column: "SelectedVoteId",
                principalTable: "Vote",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Participant_Network_NetworkId",
                table: "Participant",
                column: "NetworkId",
                principalTable: "Network",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReport_Board_BoardId",
                table: "PostReport",
                column: "BoardId",
                principalTable: "Board",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReport_NodeStates_NodeStateId",
                table: "PostReport",
                column: "NodeStateId",
                principalTable: "NodeStates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReport_Post_PostId",
                table: "PostReport",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Move_MoveId",
                table: "Vote",
                column: "MoveId",
                principalTable: "Move",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Participant_ParticipantId",
                table: "Vote",
                column: "ParticipantId",
                principalTable: "Participant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_VoteValidation_ValidationId",
                table: "Vote",
                column: "ValidationId",
                principalTable: "VoteValidation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteValidation_PostReport_VoteValidationPostId",
                table: "VoteValidation",
                column: "VoteValidationPostId",
                principalTable: "PostReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
