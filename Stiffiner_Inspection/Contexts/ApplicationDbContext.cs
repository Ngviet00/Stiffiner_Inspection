﻿using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Models.Entity;

namespace Stiffiner_Inspection.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Data> Data { get; set; }
        public DbSet<ErrorCode> ErrorCodes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Error> Errors { get; set; }
        public DbSet<Target> Targets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorCode>().HasData(
                new ErrorCode { Id = 1, ErrorContent = "black dot" },
                new ErrorCode { Id = 2, ErrorContent = "dirty" },
                new ErrorCode { Id = 3, ErrorContent = "glue" },
                new ErrorCode { Id = 4, ErrorContent = "ng sus position" },
                new ErrorCode { Id = 5, ErrorContent = "ng hole" },
                new ErrorCode { Id = 6, ErrorContent = "ng tape position" },
                new ErrorCode { Id = 7, ErrorContent = "scratch" },
                new ErrorCode { Id = 8, ErrorContent = "sus black dot" },
                new ErrorCode { Id = 9, ErrorContent = "white dot" },
                new ErrorCode { Id = 10, ErrorContent = "white line particle" },
                new ErrorCode { Id = 11, ErrorContent = "dent-tray1" },
                new ErrorCode { Id = 12, ErrorContent = "dent-tray2" },
                new ErrorCode { Id = 13, ErrorContent = "deform" },
                new ErrorCode { Id = 14, ErrorContent = "importinted" },
                new ErrorCode { Id = 15, ErrorContent = "curl tape" },
                new ErrorCode { Id = 16, ErrorContent = "curl sus" },
                new ErrorCode { Id = 17, ErrorContent = "ng tape" }
                );

            modelBuilder.Entity<Error>().HasIndex(x => x.DataId);
            modelBuilder.Entity<Image>().HasIndex(x => x.DataId);

            modelBuilder.Entity<Error>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Errors)
                .HasForeignKey(child => child.DataId);

            modelBuilder.Entity<Image>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Images)
                .HasForeignKey(child => child.DataId);
        }
    }
}
