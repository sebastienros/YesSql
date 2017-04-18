﻿using System;
using System.Data;
using System.Data.SqlClient;
using YesSql.Services;
using YesSql.Storage.Sql;

namespace YesSql.Provider.SqlServer
{
    public static class SqlServerDbProviderOptionsExtensions
    {
        public static IConfiguration UseSqlServer(
            this IConfiguration configuration,
            string connectionString)
        {
            return UseSqlServer(configuration, connectionString, IsolationLevel.ReadUncommitted);
        }

        public static IConfiguration UseSqlServer(
            this IConfiguration configuration,
            string connectionString,
            IsolationLevel isolationLevel)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            configuration.ConnectionFactory = new DbConnectionFactory<SqlConnection>(connectionString);
            configuration.DocumentStorageFactory = new SqlDocumentStorageFactory();
            configuration.IsolationLevel = isolationLevel;

            return configuration;
        }
    }
}
