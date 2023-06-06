using FluentMigrator;

namespace Elsa.Dapper.Migrations.Management;

/// <inheritdoc />
[Migration(10001, "Elsa:Management:Initial")]
public class Initial : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        IfDatabase("SqlServer", "Oracle", "MySql", "Postgres")
            .Create
            .Table("WorkflowDefinitions")
            .WithColumn("Id").AsString().PrimaryKey()
            .WithColumn("DefinitionId").AsString().NotNullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("ProviderName").AsString().Nullable()
            .WithColumn("MaterializerName").AsString().NotNullable()
            .WithColumn("MaterializerContext").AsString().Nullable()
            .WithColumn("Props").AsString().NotNullable()
            .WithColumn("UsableAsActivity").AsBoolean().Nullable()
            .WithColumn("StringData").AsString().Nullable()
            .WithColumn("BinaryData").AsBinary().Nullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("Version").AsInt32().NotNullable()
            .WithColumn("IsLatest").AsBoolean().NotNullable()
            .WithColumn("IsPublished").AsBoolean().NotNullable();

        IfDatabase("Sqlite")
            .Create
            .Table("WorkflowDefinitions")
            .WithColumn("Id").AsString().PrimaryKey()
            .WithColumn("DefinitionId").AsString().NotNullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("ProviderName").AsString().Nullable()
            .WithColumn("MaterializerName").AsString().NotNullable()
            .WithColumn("MaterializerContext").AsString().Nullable()
            .WithColumn("Props").AsString().NotNullable()
            .WithColumn("UsableAsActivity").AsBoolean().Nullable()
            .WithColumn("StringData").AsString().Nullable()
            .WithColumn("BinaryData").AsBinary().Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable()
            .WithColumn("Version").AsInt32().NotNullable()
            .WithColumn("IsLatest").AsBoolean().NotNullable()
            .WithColumn("IsPublished").AsBoolean().NotNullable();

        IfDatabase("SqlServer", "Oracle", "MySql", "Postgres")
            .Create
            .Table("WorkflowInstances")
            .WithColumn("Id").AsString().PrimaryKey()
            .WithColumn("DefinitionId").AsString().NotNullable()
            .WithColumn("DefinitionVersionId").AsString().NotNullable()
            .WithColumn("Version").AsInt32().NotNullable()
            .WithColumn("WorkflowState").AsString().NotNullable()
            .WithColumn("Status").AsString().NotNullable()
            .WithColumn("SubStatus").AsString().NotNullable()
            .WithColumn("CorrelationId").AsString().Nullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("LastExecutedAt").AsDateTimeOffset().Nullable()
            .WithColumn("FinishedAt").AsDateTimeOffset().Nullable()
            .WithColumn("CancelledAt").AsDateTimeOffset().Nullable()
            .WithColumn("FaultedAt").AsDateTimeOffset().Nullable();

        IfDatabase("Sqlite")
            .Create
            .Table("WorkflowInstances")
            .WithColumn("Id").AsString().PrimaryKey()
            .WithColumn("DefinitionId").AsString().NotNullable()
            .WithColumn("DefinitionVersionId").AsString().NotNullable()
            .WithColumn("Version").AsInt32().NotNullable()
            .WithColumn("WorkflowState").AsString().NotNullable()
            .WithColumn("Status").AsString().NotNullable()
            .WithColumn("SubStatus").AsString().NotNullable()
            .WithColumn("CorrelationId").AsString().Nullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable()
            .WithColumn("LastExecutedAt").AsDateTime2().Nullable()
            .WithColumn("FinishedAt").AsDateTime2().Nullable()
            .WithColumn("CancelledAt").AsDateTime2().Nullable()
            .WithColumn("FaultedAt").AsDateTime2().Nullable();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Table("WorkflowDefinitions");
        Delete.Table("WorkflowInstances");
    }
}