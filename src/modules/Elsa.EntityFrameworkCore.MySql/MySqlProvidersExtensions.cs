﻿using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Elsa.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides extensions to configure EF Core to use MySQL.
/// </summary>
public static class MySqlProvidersExtensions
{
    private static Assembly Assembly => typeof(MySqlProvidersExtensions).Assembly;
    
    public static TFeature UseMySql<TFeature, TDbContext>(this PersistenceFeatureBase<TFeature, TDbContext> feature, 
        string connectionString, 
        ElsaDbContextOptions? options = null, 
        Action<MySqlDbContextOptionsBuilder>? configure = null) 
        where TDbContext : ElsaDbContextBase
        where TFeature : PersistenceFeatureBase<TFeature, TDbContext>
    {
        return feature.UseMySql(Assembly, connectionString, options, configure);
    }
    
    public static TFeature UseMySql<TFeature, TDbContext>(this PersistenceFeatureBase<TFeature, TDbContext> feature, 
        Assembly migrationsAssembly, 
        string connectionString, 
        ElsaDbContextOptions? options = null,
        Action<MySqlDbContextOptionsBuilder>? configure = null
        ) 
        where TDbContext : ElsaDbContextBase
        where TFeature : PersistenceFeatureBase<TFeature, TDbContext>
    {
        feature.DbContextOptionsBuilder = (_, db) => db.UseElsaMySql(Assembly, connectionString, options, configure: configure);
        return (TFeature)feature;
    }
}