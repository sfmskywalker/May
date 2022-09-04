﻿using Elsa.Persistence.EntityFrameworkCore.Modules.Labels;

namespace Elsa.Persistence.EntityFrameworkCore.Sqlite.Modules.Labels
{
    public static class Extensions
    {
        public static EFCoreLabelPersistenceFeature UseSqlite(this EFCoreLabelPersistenceFeature feature, string connectionString = Constants.DefaultConnectionString)
        {
            feature.DbContextOptionsBuilder = (_, db) => db.UseElsaSqlite(connectionString);
            return feature;
        }
    }
}