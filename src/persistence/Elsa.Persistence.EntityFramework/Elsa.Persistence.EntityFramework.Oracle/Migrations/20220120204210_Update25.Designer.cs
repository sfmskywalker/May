﻿// <auto-generated />
using System;
using Elsa.Persistence.EntityFramework.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;

namespace Elsa.Persistence.EntityFramework.Oracle.Migrations
{
    [DbContext(typeof(ElsaContext))]
    [Migration("20220120204210_Update25")]
    partial class Update25
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Elsa")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Elsa.Models.Bookmark", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityType")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("NCLOB");

                    b.Property<string>("ModelType")
                        .IsRequired()
                        .HasColumnType("NCLOB");

                    b.Property<string>("TenantId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("WorkflowInstanceId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId")
                        .HasDatabaseName("IX_Bookmark_ActivityId");

                    b.HasIndex("ActivityType")
                        .HasDatabaseName("IX_Bookmark_ActivityType");

                    b.HasIndex("CorrelationId")
                        .HasDatabaseName("IX_Bookmark_CorrelationId");

                    b.HasIndex("Hash")
                        .HasDatabaseName("IX_Bookmark_Hash");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_Bookmark_TenantId");

                    b.HasIndex("WorkflowInstanceId")
                        .HasDatabaseName("IX_Bookmark_WorkflowInstanceId");

                    b.HasIndex("ActivityType", "TenantId", "Hash")
                        .HasDatabaseName("IX_Bookmark_ActivityType_TenantId_Hash");

                    b.HasIndex("Hash", "CorrelationId", "TenantId")
                        .HasDatabaseName("IX_Bookmark_Hash_CorrelationId_TenantId");

                    b.ToTable("Bookmarks");
                });

            modelBuilder.Entity("Elsa.Models.Trigger", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityType")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("NCLOB");

                    b.Property<string>("ModelType")
                        .IsRequired()
                        .HasColumnType("NCLOB");

                    b.Property<string>("TenantId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("WorkflowDefinitionId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId")
                        .HasDatabaseName("IX_Trigger_ActivityId");

                    b.HasIndex("ActivityType")
                        .HasDatabaseName("IX_Trigger_ActivityType");

                    b.HasIndex("Hash")
                        .HasDatabaseName("IX_Trigger_Hash");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_Trigger_TenantId");

                    b.HasIndex("WorkflowDefinitionId")
                        .HasDatabaseName("IX_Trigger_WorkflowDefinitionId");

                    b.HasIndex("Hash", "TenantId")
                        .HasDatabaseName("IX_Trigger_Hash_TenantId");

                    b.HasIndex("ActivityType", "TenantId", "Hash")
                        .HasDatabaseName("IX_Trigger_ActivityType_TenantId_Hash");

                    b.ToTable("Triggers");
                });

            modelBuilder.Entity("Elsa.Models.WorkflowDefinition", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Data")
                        .HasColumnType("NCLOB");

                    b.Property<string>("DefinitionId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<bool>("DeleteCompletedInstances")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("Description")
                        .HasColumnType("NCLOB");

                    b.Property<string>("DisplayName")
                        .HasColumnType("NCLOB");

                    b.Property<bool>("IsLatest")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("NUMBER(1)");

                    b.Property<bool>("IsSingleton")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<int>("PersistenceBehavior")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Tag")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("TenantId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<int>("Version")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("Id");

                    b.HasIndex("IsLatest")
                        .HasDatabaseName("IX_WorkflowDefinition_IsLatest");

                    b.HasIndex("IsPublished")
                        .HasDatabaseName("IX_WorkflowDefinition_IsPublished");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_WorkflowDefinition_Name");

                    b.HasIndex("Tag")
                        .HasDatabaseName("IX_WorkflowDefinition_Tag");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_WorkflowDefinition_TenantId");

                    b.HasIndex("Version")
                        .HasDatabaseName("IX_WorkflowDefinition_Version");

                    b.HasIndex("DefinitionId", "Version")
                        .IsUnique()
                        .HasDatabaseName("IX_WorkflowDefinition_DefinitionId_VersionId");

                    b.ToTable("WorkflowDefinitions");
                });

            modelBuilder.Entity("Elsa.Models.WorkflowExecutionLogRecord", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ActivityType")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("Data")
                        .HasColumnType("NCLOB");

                    b.Property<string>("EventName")
                        .HasColumnType("NCLOB");

                    b.Property<string>("Message")
                        .HasColumnType("NCLOB");

                    b.Property<string>("Source")
                        .HasColumnType("NCLOB");

                    b.Property<string>("TenantId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("WorkflowInstanceId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.HasKey("Id");

                    b.HasIndex("ActivityId")
                        .HasDatabaseName("IX_WorkflowExecutionLogRecord_ActivityId");

                    b.HasIndex("ActivityType")
                        .HasDatabaseName("IX_WorkflowExecutionLogRecord_ActivityType");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_WorkflowExecutionLogRecord_TenantId");

                    b.HasIndex("Timestamp")
                        .HasDatabaseName("IX_WorkflowExecutionLogRecord_Timestamp");

                    b.HasIndex("WorkflowInstanceId")
                        .HasDatabaseName("IX_WorkflowExecutionLogRecord_WorkflowInstanceId");

                    b.ToTable("WorkflowExecutionLogRecords");
                });

            modelBuilder.Entity("Elsa.Models.WorkflowInstance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<DateTimeOffset?>("CancelledAt")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("ContextId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("ContextType")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("Data")
                        .HasColumnType("NCLOB");

                    b.Property<string>("DefinitionId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("DefinitionVersionId")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<DateTimeOffset?>("FaultedAt")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<DateTimeOffset?>("FinishedAt")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("LastExecutedActivityId")
                        .HasColumnType("NCLOB");

                    b.Property<DateTimeOffset?>("LastExecutedAt")
                        .HasColumnType("TIMESTAMP(7) WITH TIME ZONE");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<string>("TenantId")
                        .HasColumnType("NVARCHAR2(450)");

                    b.Property<int>("Version")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int>("WorkflowStatus")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("Id");

                    b.HasIndex("ContextId")
                        .HasDatabaseName("IX_WorkflowInstance_ContextId");

                    b.HasIndex("ContextType")
                        .HasDatabaseName("IX_WorkflowInstance_ContextType");

                    b.HasIndex("CorrelationId")
                        .HasDatabaseName("IX_WorkflowInstance_CorrelationId");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("IX_WorkflowInstance_CreatedAt");

                    b.HasIndex("DefinitionId")
                        .HasDatabaseName("IX_WorkflowInstance_DefinitionId");

                    b.HasIndex("DefinitionVersionId")
                        .HasDatabaseName("IX_WorkflowInstance_DefinitionVersionId");

                    b.HasIndex("FaultedAt")
                        .HasDatabaseName("IX_WorkflowInstance_FaultedAt");

                    b.HasIndex("FinishedAt")
                        .HasDatabaseName("IX_WorkflowInstance_FinishedAt");

                    b.HasIndex("LastExecutedAt")
                        .HasDatabaseName("IX_WorkflowInstance_LastExecutedAt");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_WorkflowInstance_Name");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("IX_WorkflowInstance_TenantId");

                    b.HasIndex("WorkflowStatus")
                        .HasDatabaseName("IX_WorkflowInstance_WorkflowStatus");

                    b.HasIndex("WorkflowStatus", "DefinitionId")
                        .HasDatabaseName("IX_WorkflowInstance_WorkflowStatus_DefinitionId");

                    b.HasIndex("WorkflowStatus", "DefinitionId", "Version")
                        .HasDatabaseName("IX_WorkflowInstance_WorkflowStatus_DefinitionId_Version");

                    b.ToTable("WorkflowInstances");
                });
#pragma warning restore 612, 618
        }
    }
}
