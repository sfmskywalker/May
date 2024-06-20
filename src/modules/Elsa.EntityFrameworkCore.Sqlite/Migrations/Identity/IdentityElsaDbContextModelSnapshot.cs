﻿// <auto-generated />
using Elsa.EntityFrameworkCore.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Elsa.EntityFrameworkCore.Sqlite.Migrations.Identity
{
    [DbContext(typeof(IdentityElsaDbContext))]
    partial class IdentityElsaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.20");

            modelBuilder.Entity("Elsa.Identity.Entities.Application", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedApiKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedApiKeySalt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedClientSecret")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedClientSecretSalt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Roles");

                    b.Property<string>("TenantId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique()
                        .HasDatabaseName("IX_Application_ClientId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_Application_Name");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_Application_TenantId");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("Elsa.Identity.Entities.Role", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Permissions")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Permissions");

                    b.Property<string>("TenantId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_Role_Name");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_Role_TenantId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Elsa.Identity.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HashedPasswordSalt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Roles");

                    b.Property<string>("TenantId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_User_Name");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_User_TenantId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
