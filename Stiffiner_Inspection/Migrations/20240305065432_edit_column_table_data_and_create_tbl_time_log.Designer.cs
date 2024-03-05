﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stiffiner_Inspection.Contexts;

#nullable disable

namespace Stiffiner_Inspection.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240305065432_edit_column_table_data_and_create_tbl_time_log")]
    partial class edit_column_table_data_and_create_tbl_time_log
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<string>("ErrorCode")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("error_code");

                    b.Property<string>("ErrorDetection")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("error_detection");

                    b.Property<string>("Model")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("model");

                    b.Property<int?>("No")
                        .HasColumnType("int")
                        .HasColumnName("no");

                    b.Property<string>("Result")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("result");

                    b.Property<string>("Side")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("side");

                    b.Property<DateTime?>("Time")
                        .HasMaxLength(255)
                        .HasColumnType("datetime2")
                        .HasColumnName("time");

                    b.Property<int?>("Tray")
                        .HasColumnType("int")
                        .HasColumnName("tray");

                    b.HasKey("Id");

                    b.ToTable("data");
                });
#pragma warning restore 612, 618
        }
    }
}
