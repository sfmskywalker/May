using System;
using System.Threading.Tasks;
using Elsa.Abstractions.Multitenancy;
using Elsa.Attributes;
using Elsa.WorkflowSettings.Persistence.EntityFramework.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.WorkflowSettings.Persistence.EntityFramework.Sqlite
{
    [Feature("WorkflowSettings:EntityFrameworkCore:Sqlite")]
    public class Startup : EntityFrameworkWorkflowSettingsStartupBase
    {  
        protected override string ProviderName => "Sqlite";
        protected override string GetDefaultConnectionString() => "Data Source=elsa.sqlite.db;Cache=Shared;";
        protected override void Configure(DbContextOptionsBuilder options, string connectionString) => options.UseWorkflowSettingsSqlite(connectionString);
        protected override async Task Configure(DbContextOptionsBuilder options, IServiceProvider serviceProvider)
        {
            var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            var tenant = await tenantProvider.GetCurrentTenantAsync();
            var connectionString = tenant.GetDatabaseConnectionString();

            options.UseWorkflowSettingsSqlite(connectionString);
        }
    }
}