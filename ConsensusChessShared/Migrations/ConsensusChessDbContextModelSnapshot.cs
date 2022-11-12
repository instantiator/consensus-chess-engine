﻿// <auto-generated />
using System;
using System.Collections.Generic;
using ConsensusChessShared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConsensusChessShared.Migrations
{
    [DbContext(typeof(ConsensusChessDbContext))]
    partial class ConsensusChessDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("ActiveSide")
                        .HasColumnType("integer")
                        .HasColumnName("active_side");

                    b.Property<string>("CastlingAvailability_FEN")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("castling_availability_fen");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("EnPassantTargetSquare_FEN")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("en_passant_target_square_fen");

                    b.Property<int>("FullMoveNumber")
                        .HasColumnType("integer")
                        .HasColumnName("full_move_number");

                    b.Property<int>("HalfMoveClock")
                        .HasColumnType("integer")
                        .HasColumnName("half_move_clock");

                    b.Property<string>("Pieces_FEN")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pieces_fen");

                    b.HasKey("Id")
                        .HasName("pk_board");

                    b.ToTable("board", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Commitment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("GameShortcode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("game_shortcode");

                    b.Property<int>("GameSide")
                        .HasColumnType("integer")
                        .HasColumnName("game_side");

                    b.Property<Guid?>("ParticipantId")
                        .HasColumnType("uuid")
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
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<List<string>>("BlackNetworks")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("black_networks");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("finished");

                    b.Property<TimeSpan>("MoveDuration")
                        .HasColumnType("interval")
                        .HasColumnName("move_duration");

                    b.Property<DateTime>("ScheduledStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_start");

                    b.Property<int>("SideRules")
                        .HasColumnType("integer")
                        .HasColumnName("side_rules");

                    b.Property<List<string>>("WhiteNetworks")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("white_networks");

                    b.HasKey("Id")
                        .HasName("pk_games");

                    b.ToTable("games", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Alt")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("alt");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("data");

                    b.Property<Guid?>("PostId")
                        .HasColumnType("uuid")
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
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<DateTime>("Deadline")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deadline");

                    b.Property<Guid>("FromId")
                        .HasColumnType("uuid")
                        .HasColumnName("from_id");

                    b.Property<Guid?>("GameId")
                        .HasColumnType("uuid")
                        .HasColumnName("game_id");

                    b.Property<Guid?>("SelectedVoteId")
                        .HasColumnType("uuid")
                        .HasColumnName("selected_vote_id");

                    b.Property<int>("SideToPlay")
                        .HasColumnType("integer")
                        .HasColumnName("side_to_play");

                    b.Property<Guid?>("ToId")
                        .HasColumnType("uuid")
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

            modelBuilder.Entity("ConsensusChessShared.DTO.NodeState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<long>("LastNotificationId")
                        .HasColumnType("bigint")
                        .HasColumnName("last_notification_id");

                    b.Property<long>("LastReplyId")
                        .HasColumnType("bigint")
                        .HasColumnName("last_reply_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Shortcode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("shortcode");

                    b.HasKey("Id")
                        .HasName("pk_node_states");

                    b.HasIndex("Shortcode")
                        .IsUnique()
                        .HasDatabaseName("ix_node_states_shortcode");

                    b.ToTable("node_states", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Participant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("network_server");

                    b.Property<string>("NetworkUserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("network_user_id");

                    b.HasKey("Id")
                        .HasName("pk_participant");

                    b.ToTable("participant", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("AppName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("app_name");

                    b.Property<DateTime?>("Attempted")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("attempted");

                    b.Property<Guid?>("BoardId")
                        .HasColumnType("uuid")
                        .HasColumnName("board_id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text")
                        .HasColumnName("error_message");

                    b.Property<string>("ExceptionType")
                        .HasColumnType("text")
                        .HasColumnName("exception_type");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("network_server");

                    b.Property<string>("NodeShortcode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("node_shortcode");

                    b.Property<Guid?>("NodeStateId")
                        .HasColumnType("uuid")
                        .HasColumnName("node_state_id");

                    b.Property<long?>("ReplyTo")
                        .HasColumnType("bigint")
                        .HasColumnName("reply_to");

                    b.Property<bool>("Succeeded")
                        .HasColumnType("boolean")
                        .HasColumnName("succeeded");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_post");

                    b.HasIndex("BoardId")
                        .HasDatabaseName("ix_post_board_id");

                    b.HasIndex("NodeStateId")
                        .HasDatabaseName("ix_post_node_state_id");

                    b.ToTable("post", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Vote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<Guid?>("MoveId")
                        .HasColumnType("uuid")
                        .HasColumnName("move_id");

                    b.Property<string>("MoveText")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("move_text");

                    b.Property<Guid>("ParticipantId")
                        .HasColumnType("uuid")
                        .HasColumnName("participant_id");

                    b.Property<Guid>("ValidationId")
                        .HasColumnType("uuid")
                        .HasColumnName("validation_id");

                    b.HasKey("Id")
                        .HasName("pk_vote");

                    b.HasIndex("MoveId")
                        .HasDatabaseName("ix_vote_move_id");

                    b.HasIndex("ParticipantId")
                        .HasDatabaseName("ix_vote_participant_id");

                    b.HasIndex("ValidationId")
                        .HasDatabaseName("ix_vote_validation_id");

                    b.ToTable("vote", (string)null);
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.VoteValidation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("note");

                    b.Property<bool>("ValidationState")
                        .HasColumnType("boolean")
                        .HasColumnName("validation_state");

                    b.Property<Guid>("VoteValidationPostId")
                        .HasColumnType("uuid")
                        .HasColumnName("vote_validation_post_id");

                    b.HasKey("Id")
                        .HasName("pk_vote_validation");

                    b.HasIndex("VoteValidationPostId")
                        .HasDatabaseName("ix_vote_validation_vote_validation_post_id");

                    b.ToTable("vote_validation", (string)null);
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

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Board", null)
                        .WithMany("BoardPosts")
                        .HasForeignKey("BoardId")
                        .HasConstraintName("fk_post_board_board_id");

                    b.HasOne("ConsensusChessShared.DTO.NodeState", null)
                        .WithMany("StatePosts")
                        .HasForeignKey("NodeStateId")
                        .HasConstraintName("fk_post_node_states_node_state_id");
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

                    b.HasOne("ConsensusChessShared.DTO.VoteValidation", "Validation")
                        .WithMany()
                        .HasForeignKey("ValidationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_vote_vote_validation_validation_id");

                    b.Navigation("Participant");

                    b.Navigation("Validation");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.VoteValidation", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Post", "VoteValidationPost")
                        .WithMany()
                        .HasForeignKey("VoteValidationPostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_vote_validation_post_vote_validation_post_id");

                    b.Navigation("VoteValidationPost");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Navigation("BoardPosts");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Game", b =>
                {
                    b.Navigation("Moves");
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
