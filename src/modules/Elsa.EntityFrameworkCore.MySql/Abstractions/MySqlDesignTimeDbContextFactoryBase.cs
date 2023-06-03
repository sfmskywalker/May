﻿using Elsa.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Elsa.EntityFrameworkCore.MySql.Abstractions;

public abstract class MySqlDesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext> where TDbContext : DbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Any() ? args[0] : "Data Source=local";

        var builder = new DbContextOptionsBuilder<TDbContext>();
        builder.UseElsaMySql(connectionString);

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), builder.Options)!;
    }
}