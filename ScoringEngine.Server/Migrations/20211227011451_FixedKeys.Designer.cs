﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScoringEngine.Server.Data;

#nullable disable

namespace ScoringEngine.Server.Migrations
{
    [DbContext(typeof(ScoringEngineDbContext))]
    [Migration("20211227011451_FixedKeys")]
    partial class FixedKeys
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ScoringEngine.Models.CompetitionSystem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<string>("ReadmeText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SystemIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("CompetitionSystems", (string)null);
                });

            modelBuilder.Entity("ScoringEngine.Models.CompletedScoringItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<DateTime>("ApplicationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<int>("CompletionStatus")
                        .HasColumnType("int");

                    b.Property<Guid>("RegisteredVirtualMachineID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ScoringItemId")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("RegisteredVirtualMachineID");

                    b.HasIndex("ScoringItemId");

                    b.ToTable("CompletedScoringItems", (string)null);
                });

            modelBuilder.Entity("ScoringEngine.Models.RegisteredVirtualMachine", b =>
                {
                    b.Property<Guid>("RegisteredVirtualMachineID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CompetitionSystemID")
                        .HasColumnType("int");

                    b.Property<bool>("IsConnectedNow")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastCheckIn")
                        .HasColumnType("datetime2");

                    b.Property<int>("SystemIdentifier")
                        .HasColumnType("int");

                    b.Property<int>("TeamID")
                        .HasColumnType("int");

                    b.HasKey("RegisteredVirtualMachineID");

                    b.HasIndex("CompetitionSystemID");

                    b.HasIndex("TeamID");

                    b.ToTable("RegisteredVirtualMachine", (string)null);
                });

            modelBuilder.Entity("ScoringEngine.Models.ScoringItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<int>("CompetitionSystemID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<int>("ScoringItemType")
                        .HasColumnType("int");

                    b.Property<string>("Script")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ScriptType")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("CompetitionSystemID");

                    b.ToTable("ScoringItems", (string)null);
                });

            modelBuilder.Entity("ScoringEngine.Models.Team", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Teams", (string)null);
                });

            modelBuilder.Entity("ScoringEngine.Models.CompletedScoringItem", b =>
                {
                    b.HasOne("ScoringEngine.Models.RegisteredVirtualMachine", "AppliedVirtualMachine")
                        .WithMany("ScoringHistory")
                        .HasForeignKey("RegisteredVirtualMachineID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScoringEngine.Models.ScoringItem", "ScoringItem")
                        .WithMany()
                        .HasForeignKey("ScoringItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppliedVirtualMachine");

                    b.Navigation("ScoringItem");
                });

            modelBuilder.Entity("ScoringEngine.Models.RegisteredVirtualMachine", b =>
                {
                    b.HasOne("ScoringEngine.Models.CompetitionSystem", "CompetitionSystem")
                        .WithMany()
                        .HasForeignKey("CompetitionSystemID");

                    b.HasOne("ScoringEngine.Models.Team", "Team")
                        .WithMany("RegisteredVirtualMachines")
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CompetitionSystem");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("ScoringEngine.Models.ScoringItem", b =>
                {
                    b.HasOne("ScoringEngine.Models.CompetitionSystem", null)
                        .WithMany("ScoringItems")
                        .HasForeignKey("CompetitionSystemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ScoringEngine.Models.CompetitionSystem", b =>
                {
                    b.Navigation("ScoringItems");
                });

            modelBuilder.Entity("ScoringEngine.Models.RegisteredVirtualMachine", b =>
                {
                    b.Navigation("ScoringHistory");
                });

            modelBuilder.Entity("ScoringEngine.Models.Team", b =>
                {
                    b.Navigation("RegisteredVirtualMachines");
                });
#pragma warning restore 612, 618
        }
    }
}
