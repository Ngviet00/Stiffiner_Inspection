﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stiffiner_Inspection.Contexts;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Stiffiner_Inspection.Models.Entity.Data", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Camera")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("camera");

                    b.Property<int?>("ClientId")
                        .HasColumnType("int")
                        .HasColumnName("client_id");

                    b.Property<int?>("ErrorCode")
                        .HasColumnType("int")
                        .HasColumnName("error_code");

                    b.Property<int?>("Flag")
                        .HasColumnType("int")
                        .HasColumnName("flag");

                    b.Property<string>("Image")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("image");

                    b.Property<int?>("Index")
                        .HasColumnType("int")
                        .HasColumnName("index");

                    b.Property<string>("Model")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("model");

                    b.Property<int?>("Result")
                        .HasColumnType("int")
                        .HasColumnName("result");

                    b.Property<int?>("Result1")
                        .HasColumnType("int")
                        .HasColumnName("result_1");

                    b.Property<string>("Side")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("side");

                    b.Property<DateTime?>("Time")
                        .HasColumnType("datetime2")
                        .HasColumnName("time");

                    b.Property<DateTime?>("TimeEnd")
                        .HasColumnType("datetime2")
                        .HasColumnName("time_end");

                    b.Property<DateTime?>("TimeStart")
                        .HasColumnType("datetime2")
                        .HasColumnName("time_start");

                    b.Property<string>("Tray")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("tray");

                    b.HasKey("Id");

                    b.ToTable("data");
                });

            modelBuilder.Entity("Stiffiner_Inspection.Models.Entity.ErrorCode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("ErrorContent")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("error_content");

                    b.HasKey("Id");

                    b.ToTable("error_code");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            ErrorContent = "black dot"
                        },
                        new
                        {
                            Id = 2L,
                            ErrorContent = "dirty"
                        },
                        new
                        {
                            Id = 3L,
                            ErrorContent = "glue"
                        },
                        new
                        {
                            Id = 4L,
                            ErrorContent = "ng sus position"
                        },
                        new
                        {
                            Id = 5L,
                            ErrorContent = "ng hole"
                        },
                        new
                        {
                            Id = 6L,
                            ErrorContent = "ng tape position"
                        },
                        new
                        {
                            Id = 7L,
                            ErrorContent = "scratch"
                        },
                        new
                        {
                            Id = 8L,
                            ErrorContent = "sus black dot"
                        },
                        new
                        {
                            Id = 9L,
                            ErrorContent = "white dot"
                        },
                        new
                        {
                            Id = 10L,
                            ErrorContent = "white line particle"
                        },
                        new
                        {
                            Id = 11L,
                            ErrorContent = "dent-tray1"
                        },
                        new
                        {
                            Id = 12L,
                            ErrorContent = "dent-tray2"
                        },
                        new
                        {
                            Id = 13L,
                            ErrorContent = "deform"
                        },
                        new
                        {
                            Id = 14L,
                            ErrorContent = "importinted"
                        },
                        new
                        {
                            Id = 15L,
                            ErrorContent = "curl tape"
                        },
                        new
                        {
                            Id = 16L,
                            ErrorContent = "curl sus"
                        },
                        new
                        {
                            Id = 17L,
                            ErrorContent = "ng tape"
                        });
                });

            modelBuilder.Entity("Stiffiner_Inspection.Models.Entity.StatusCAM", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.ToTable("status_cam");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Status = 0
                        },
                        new
                        {
                            Id = 2,
                            Status = 0
                        },
                        new
                        {
                            Id = 3,
                            Status = 0
                        },
                        new
                        {
                            Id = 4,
                            Status = 0
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
