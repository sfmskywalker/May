﻿// <auto-generated />
using Elsa.Persistence.EntityFrameworkCore.Modules.WorkflowSink;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Elsa.Persistence.EntityFrameworkCore.Sqlite.Migrations
{
    [DbContext(typeof(WorkflowSinkElsaDbContext))]
    partial class WorkflowSinkElsaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.12");

            modelBuilder.Entity("Elsa.Workflows.Sink.Models.WorkflowSink", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CancelledAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedAt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<string>("FaultedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("FinishedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastExecutedAt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasDatabaseName("IX_WorkflowSink_Id");

                    b.ToTable("WorkflowSink");
                });
#pragma warning restore 612, 618
        }
    }
}
