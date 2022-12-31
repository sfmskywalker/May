﻿using Elsa.EntityFrameworkCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Elsa.EntityFrameworkCore.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseElsaSqlServer(this DbContextOptionsBuilder builder, string connectionString, Action<SqlServerDbContextOptionsBuilder>? configure = default) =>
        builder.UseSqlServer(connectionString, db =>
        {
            db
                .MigrationsAssembly(typeof(DbContextOptionsBuilderExtensions).Assembly.GetName().Name)
                .MigrationsHistoryTable(ElsaDbContextBase.MigrationsHistoryTable, ElsaDbContextBase.ElsaSchema);
            
            configure?.Invoke(db);
        });
}