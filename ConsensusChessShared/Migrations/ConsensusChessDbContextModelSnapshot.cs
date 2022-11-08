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
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ActiveSide")
                        .HasColumnType("integer");

                    b.Property<string>("CastlingAvailability_FEN")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EnPassantTargetSquare_FEN")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("FullMoveNumber")
                        .HasColumnType("integer");

                    b.Property<int>("HalfMoveClock")
                        .HasColumnType("integer");

                    b.Property<string>("Pieces_FEN")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Board");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Commitment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("GameId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ParticipantId")
                        .HasColumnType("uuid");

                    b.Property<int>("Side")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("ParticipantId");

                    b.ToTable("Commitment");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<List<string>>("BlackNetworks")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("MoveDuration")
                        .HasColumnType("interval");

                    b.Property<DateTime>("ScheduledStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SideRules")
                        .HasColumnType("integer");

                    b.Property<List<string>>("WhiteNetworks")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Alt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<Guid?>("PostId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Deadline")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("FromId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("GameId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SelectedVoteId")
                        .HasColumnType("uuid");

                    b.Property<int>("SideToPlay")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ToId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("FromId");

                    b.HasIndex("GameId");

                    b.HasIndex("SelectedVoteId");

                    b.HasIndex("ToId");

                    b.ToTable("Move");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Network", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AppKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AppSecret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AppToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AuthorisedAccounts")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NetworkServer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Network");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.NodeState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("LastMentionId")
                        .HasColumnType("bigint");

                    b.Property<long>("LastReplyId")
                        .HasColumnType("bigint");

                    b.Property<string>("NodeName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("NodeStates");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Participant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("NetworkId")
                        .HasColumnType("uuid");

                    b.Property<string>("NetworkUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("NetworkId");

                    b.ToTable("Participant");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BoardId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("NetworkId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BoardId");

                    b.HasIndex("NetworkId");

                    b.ToTable("Post");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Vote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("MoveId")
                        .HasColumnType("uuid");

                    b.Property<string>("MoveText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ParticipantId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ValidationId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MoveId");

                    b.HasIndex("ParticipantId");

                    b.HasIndex("ValidationId");

                    b.ToTable("Vote");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.VoteValidation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<bool>("ValidationState")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("VoteValidation");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Commitment", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ConsensusChessShared.DTO.Participant", null)
                        .WithMany("Commitments")
                        .HasForeignKey("ParticipantId");

                    b.Navigation("Game");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Media", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Post", null)
                        .WithMany("MediaPng")
                        .HasForeignKey("PostId");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Board", "From")
                        .WithMany()
                        .HasForeignKey("FromId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ConsensusChessShared.DTO.Game", null)
                        .WithMany("Moves")
                        .HasForeignKey("GameId");

                    b.HasOne("ConsensusChessShared.DTO.Vote", "SelectedVote")
                        .WithMany()
                        .HasForeignKey("SelectedVoteId");

                    b.HasOne("ConsensusChessShared.DTO.Board", "To")
                        .WithMany()
                        .HasForeignKey("ToId");

                    b.Navigation("From");

                    b.Navigation("SelectedVote");

                    b.Navigation("To");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Participant", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Network", "Network")
                        .WithMany()
                        .HasForeignKey("NetworkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Network");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Post", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Board", null)
                        .WithMany("Posts")
                        .HasForeignKey("BoardId");

                    b.HasOne("ConsensusChessShared.DTO.Network", "Network")
                        .WithMany()
                        .HasForeignKey("NetworkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Network");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Vote", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Move", null)
                        .WithMany("Votes")
                        .HasForeignKey("MoveId");

                    b.HasOne("ConsensusChessShared.DTO.Participant", "Participant")
                        .WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ConsensusChessShared.DTO.VoteValidation", "Validation")
                        .WithMany()
                        .HasForeignKey("ValidationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Participant");

                    b.Navigation("Validation");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.VoteValidation", b =>
                {
                    b.HasOne("ConsensusChessShared.DTO.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Board", b =>
                {
                    b.Navigation("Posts");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Game", b =>
                {
                    b.Navigation("Moves");
                });

            modelBuilder.Entity("ConsensusChessShared.DTO.Move", b =>
                {
                    b.Navigation("Votes");
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
