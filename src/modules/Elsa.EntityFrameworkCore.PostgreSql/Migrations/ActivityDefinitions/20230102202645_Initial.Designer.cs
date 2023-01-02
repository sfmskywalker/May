﻿// <auto-generated />
using System;
using Elsa.EntityFrameworkCore.Modules.ActivityDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Elsa.EntityFrameworkCore.PostgreSql.Migrations.ActivityDefinitions
{
    [DbContext(typeof(ActivityDefinitionsElsaDbContext))]
    [Migration("20230102202645_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Elsa")
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Elsa.ActivityDefinitions.Entities.ActivityDefinition", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Category")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .HasColumnType("text");

                    b.Property<string>("DefinitionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .HasColumnType("text");

                    b.Property<bool>("IsLatest")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("IsLatest")
                        .HasDatabaseName("IX_ActivityDefinition_IsLatest");

                    b.HasIndex("IsPublished")
                        .HasDatabaseName("IX_ActivityDefinition_IsPublished");

                    b.HasIndex("Type")
                        .HasDatabaseName("IX_ActivityDefinition_Type");

                    b.HasIndex("Version")
                        .HasDatabaseName("IX_ActivityDefinition_Version");

                    b.HasIndex("DefinitionId", "Version")
                        .IsUnique()
                        .HasDatabaseName("IX_ActivityDefinition_DefinitionId_Version");

                    b.ToTable("ActivityDefinitions", "Elsa");
                });
#pragma warning restore 612, 618
        }
    }
}
