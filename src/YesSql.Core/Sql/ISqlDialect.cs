﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using YesSql.Sql.Providers.MySql;
using YesSql.Sql.Providers.PostgreSql;
using YesSql.Sql.Providers.Sqlite;
using YesSql.Sql.Providers.SqlServer;

namespace YesSql.Sql
{
    public interface ISqlDialect
    {
        string Name { get; }
        string CascadeConstraintsString { get; }
        string CreateTableString { get; }
        string PrimaryKeyString { get; }
        string NullColumnString { get; }
        bool SupportsUnique { get; }
        bool HasDataTypeInIdentityColumn { get; }
        bool SupportsIdentityColumns { get; }
        string IdentityColumnString { get; }
        string IdentitySelectString { get; }
        string GetTypeName(DbType dbType, int? length, byte precision, byte scale);
        string GetSqlValue(object value);
        string QuoteForTableName(string v);
        string GetDropTableString(string name);
        string QuoteForColumnName(string columnName);
        string InOperator(string values);
        string GetDropForeignKeyConstraintString(string name);
        string GetAddForeignKeyConstraintString(string name, string[] srcColumns, string destTable, string[] destColumns, bool primaryKey);
        string DefaultValuesInsert { get; }
        void Page(SqlBuilder sqlBuilder, int offset, int limit);
        ISqlBuilder CreateBuilder(string tablePrefix);
    }

    public class SqlDialectFactory
    {
        public static Dictionary<string, ISqlDialect> SqlDialects { get; } = new Dictionary<string, ISqlDialect>
        {
            {"sqliteconnection", new SqliteDialect()},
            {"sqlconnection", new SqlServerDialect()},
            {"mysqlconnection", new MySqlDialect()},
            {"npgsqlconnection", new PostgreSqlDialect()}
        };

        public static void RegisterSqlDialect(string connectionName, ISqlDialect sqlTypeAdapter)
        {
            SqlDialects[connectionName] = sqlTypeAdapter;
        }

        public static ISqlDialect For(IDbConnection connection)
        {
            string connectionName = connection.GetType().Name.ToLower();

            if (!SqlDialects.TryGetValue(connectionName, out ISqlDialect dialect))
            {
                throw new ArgumentException("Unknown connection name: " + connectionName);
            }

            return dialect;
        }
    }
}
