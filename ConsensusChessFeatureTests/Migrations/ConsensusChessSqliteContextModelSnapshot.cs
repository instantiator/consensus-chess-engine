﻿// <auto-generated />
using System;
using ConsensusChessFeatureTests.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ConsensusChessFeatureTests.Migrations
{
    [DbContext(typeof(ConsensusChessSqliteContext))]
    partial class ConsensusChessSqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("FEN")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("fen");

                    b.HasKey("Id")
                        .HasName("pk_board");

                    b.ToTable("board", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Commitment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("GameShortcode")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("game_shortcode");

                    b.Property<int>("GameSide")
                        .HasColumnType("INTEGER")
                        .HasColumnName("game_side");

                    b.Property<Guid?>("ParticipantId")
                        .HasColumnType("TEXT")
                        .HasColumnName("participant_id");

                    b.HasKey("Id")
                        .HasName("pk_commitment");

                    b.HasIndex("ParticipantId")
                        .HasDatabaseName("ix_commitment_participant_id");

                    b.ToTable("commitment", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("TEXT")
                        .HasColumnName("finished");

                    b.Property<TimeSpan>("MoveDuration")
                        .HasColumnType("TEXT")
                        .HasColumnName("move_duration");

                    b.Property<DateTime>("ScheduledStart")
                        .HasColumnType("TEXT")
                        .HasColumnName("scheduled_start");

                    b.Property<string>("Shortcode")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("shortcode");

                    b.Property<int>("SideRules")
                        .HasColumnType("INTEGER")
                        .HasColumnName("side_rules");

                    b.HasKey("Id")
                        .HasName("pk_games");

                    b.ToTable("games", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("Alt")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("alt");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB")
                        .HasColumnName("data");

                    b.Property<Guid?>("PostId")
                        .HasColumnType("TEXT")
                        .HasColumnName("post_id");

                    b.HasKey("Id")
                        .HasName("pk_media");

                    b.HasIndex("PostId")
                        .HasDatabaseName("ix_media_post_id");

                    b.ToTable("media", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<DateTime>("Deadline")
                        .HasColumnType("TEXT")
                        .HasColumnName("deadline");

                    b.Property<Guid>("FromId")
                        .HasColumnType("TEXT")
                        .HasColumnName("from_id");

                    b.Property<Guid?>("GameId")
                        .HasColumnType("TEXT")
                        .HasColumnName("game_id");

                    b.Property<Guid?>("SelectedVoteId")
                        .HasColumnType("TEXT")
                        .HasColumnName("selected_vote_id");

                    b.Property<Guid?>("ToId")
                        .HasColumnType("TEXT")
                        .HasColumnName("to_id");

                    b.HasKey("Id")
                        .HasName("pk_move");

                    b.HasIndex("FromId")
                        .HasDatabaseName("ix_move_from_id");

                    b.HasIndex("GameId")
                        .HasDatabaseName("ix_move_game_id");

                    b.HasIndex("SelectedVoteId")
                        .HasDatabaseName("ix_move_selected_vote_id");

                    b.HasIndex("ToId")
                        .HasDatabaseName("ix_move_to_id");

                    b.ToTable("move", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Network", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("AppKey")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("app_key");

                    b.Property<string>("AppName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("app_name");

                    b.Property<string>("AppSecret")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("app_secret");

                    b.Property<string>("AppToken")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("app_token");

                    b.Property<string>("AuthorisedAccounts")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("authorised_accounts");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<bool>("DryRuns")
                        .HasColumnType("INTEGER")
                        .HasColumnName("dry_runs");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("network_server");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_network");

                    b.ToTable("network", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.NodeState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<long>("LastNotificationId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("last_notification_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<Guid>("NetworkId")
                        .HasColumnType("TEXT")
                        .HasColumnName("network_id");

                    b.Property<string>("Shortcode")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("shortcode");

                    b.HasKey("Id")
                        .HasName("pk_node_state");

                    b.HasIndex("NetworkId")
                        .HasDatabaseName("ix_node_state_network_id");

                    b.HasIndex("Shortcode")
                        .IsUnique()
                        .HasDatabaseName("ix_node_state_shortcode");

                    b.ToTable("node_state", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Participant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("network_server");

                    b.Property<string>("NetworkUserAccount")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("network_user_account");

                    b.HasKey("Id")
                        .HasName("pk_participant");

                    b.ToTable("participant", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("AppName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("app_name");

                    b.Property<DateTime?>("Attempted")
                        .HasColumnType("TEXT")
                        .HasColumnName("attempted");

                    b.Property<Guid?>("BoardId")
                        .HasColumnType("TEXT")
                        .HasColumnName("board_id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("TEXT")
                        .HasColumnName("error_message");

                    b.Property<string>("ExceptionType")
                        .HasColumnType("TEXT")
                        .HasColumnName("exception_type");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("message");

                    b.Property<long?>("NetworkPostId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("network_post_id");

                    b.Property<long?>("NetworkReplyToId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("network_reply_to_id");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("network_server");

                    b.Property<string>("NodeShortcode")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("node_shortcode");

                    b.Property<Guid?>("NodeStateId")
                        .HasColumnType("TEXT")
                        .HasColumnName("node_state_id");

                    b.Property<bool>("Succeeded")
                        .HasColumnType("INTEGER")
                        .HasColumnName("succeeded");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_post");

                    b.HasIndex("BoardId")
                        .HasDatabaseName("ix_post_board_id");

                    b.HasIndex("NodeStateId")
                        .HasDatabaseName("ix_post_node_state_id");

                    b.ToTable("post", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.StoredString", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<Guid?>("GameId")
                        .HasColumnType("TEXT")
                        .HasColumnName("game_id");

                    b.Property<Guid?>("GameId1")
                        .HasColumnType("TEXT")
                        .HasColumnName("game_id1");

                    b.Property<Guid?>("GameId2")
                        .HasColumnType("TEXT")
                        .HasColumnName("game_id2");

                    b.Property<Guid?>("GameId3")
                        .HasColumnType("TEXT")
                        .HasColumnName("game_id3");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("pk_stored_string");

                    b.HasIndex("GameId")
                        .HasDatabaseName("ix_stored_string_game_id");

                    b.HasIndex("GameId1")
                        .HasDatabaseName("ix_stored_string_game_id1");

                    b.HasIndex("GameId2")
                        .HasDatabaseName("ix_stored_string_game_id2");

                    b.HasIndex("GameId3")
                        .HasDatabaseName("ix_stored_string_game_id3");

                    b.ToTable("stored_string", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Vote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<Guid?>("MoveId")
                        .HasColumnType("TEXT")
                        .HasColumnName("move_id");

                    b.Property<string>("MoveText")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("move_text");

                    b.Property<long>("NetworkMovePostId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("network_move_post_id");

                    b.Property<Guid>("ParticipantId")
                        .HasColumnType("TEXT")
                        .HasColumnName("participant_id");

                    b.Property<Guid?>("ValidationPostId")
                        .HasColumnType("TEXT")
                        .HasColumnName("validation_post_id");

                    b.Property<int>("ValidationState")
                        .HasColumnType("INTEGER")
                        .HasColumnName("validation_state");

                    b.HasKey("Id")
                        .HasName("pk_vote");

                    b.HasIndex("MoveId")
                        .HasDatabaseName("ix_vote_move_id");

                    b.HasIndex("ParticipantId")
                        .HasDatabaseName("ix_vote_participant_id");

                    b.HasIndex("ValidationPostId")
                        .HasDatabaseName("ix_vote_validation_post_id");

                    b.ToTable("vote", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Commitment", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Participant", null)
                        .WithMany("Commitments")
                        .HasForeignKey("ParticipantId")
                        .HasConstraintName("fk_commitment_participant_participant_id");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Media", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Post", null)
                        .WithMany("MediaPng")
                        .HasForeignKey("PostId")
                        .HasConstraintName("fk_media_post_post_id");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Board", "From")
                        .WithMany()
                        .HasForeignKey("FromId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_move_board_from_id");

                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("Moves")
                        .HasForeignKey("GameId")
                        .HasConstraintName("fk_move_games_game_id");

                    b.HasOne("ConsensusChessShared.DTO.Vote", "SelectedVote")
                        .WithMany()
                        .HasForeignKey("SelectedVoteId")
                        .HasConstraintName("fk_move_vote_selected_vote_id");

                    b.HasOne("ConsensusChessShared.DTO.Board", "To")
                        .WithMany()
                        .HasForeignKey("ToId")
                        .HasConstraintName("fk_move_board_to_id");

                    b.Navigation("From");

                    b.Navigation("SelectedVote");

                    b.Navigation("To");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.NodeState", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Network", "Network")
                        .WithMany()
                        .HasForeignKey("NetworkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_node_state_network_network_id");

                    b.Navigation("Network");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Board", null)
                        .WithMany("BoardPosts")
                        .HasForeignKey("BoardId")
                        .HasConstraintName("fk_post_board_board_id");

                    b.HasOne("ConsensusChessShared.DTO.NodeState", null)
                        .WithMany("StatePosts")
                        .HasForeignKey("NodeStateId")
                        .HasConstraintName("fk_post_node_state_node_state_id");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.StoredString", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("BlackParticipantNetworkServers")
                        .HasForeignKey("GameId")
                        .HasConstraintName("fk_stored_string_games_game_id");

                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("BlackPostingNodeShortcodes")
                        .HasForeignKey("GameId1")
                        .HasConstraintName("fk_stored_string_games_game_id1");

                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("WhiteParticipantNetworkServers")
                        .HasForeignKey("GameId2")
                        .HasConstraintName("fk_stored_string_games_game_id2");

                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("WhitePostingNodeShortcodes")
                        .HasForeignKey("GameId3")
                        .HasConstraintName("fk_stored_string_games_game_id3");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Vote", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Move", null)
                        .WithMany("Votes")
                        .HasForeignKey("MoveId")
                        .HasConstraintName("fk_vote_move_move_id");

                    b.HasOne("ConsensusChessShared.DTO.Participant", "Participant")
                        .WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_vote_participant_participant_id");

                    b.HasOne("ConsensusChessShared.DTO.Post", "ValidationPost")
                        .WithMany()
                        .HasForeignKey("ValidationPostId")
                        .HasConstraintName("fk_vote_post_validation_post_id");

                    b.Navigation("Participant");

                    b.Navigation("ValidationPost");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Navigation("BoardPosts");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Game", b =>
                {
                    b.Navigation("BlackParticipantNetworkServers");

                    b.Navigation("BlackPostingNodeShortcodes");

                    b.Navigation("Moves");

                    b.Navigation("WhiteParticipantNetworkServers");

                    b.Navigation("WhitePostingNodeShortcodes");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.Navigation("Votes");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.NodeState", b =>
                {
                    b.Navigation("StatePosts");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Participant", b =>
                {
                    b.Navigation("Commitments");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.Navigation("MediaPng");
                });
#pragma warning restore 612, 618
        }
    }
}
