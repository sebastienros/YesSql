﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using YesSql.Collections;
using YesSql.Services;
using YesSql.Sql.Schema;

namespace YesSql.Sql
{
    public class SchemaBuilder
    {
        private ICommandInterpreter _builder;
        private string _tablePrefix;
        private ISqlDialect _dialect;
        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; private set; }
        public bool ThrowOnError { get; set; } = true;

        public SchemaBuilder(ISession session)
        {
            Transaction = session.Demand();
            Connection = Transaction.Connection;
            _builder = SchemaBuilderFactory.For(Connection);
            _dialect = SqlDialectFactory.For(Connection);
            _tablePrefix = session.Store.Configuration.TablePrefix;
        }

        public SchemaBuilder(IDbConnection connection, IDbTransaction transaction, string tablePrefix)
        {
            _builder = SchemaBuilderFactory.For(connection);
            _dialect = SqlDialectFactory.For(connection);
            _tablePrefix = tablePrefix;
            Connection = connection;
            Transaction = transaction;
        }

        private void Execute(IEnumerable<string> statements)
        {
            foreach (var statement in statements)
            {
                Connection.Execute(statement, null, Transaction);
            }
        }

        private string FormatTable(string table)
        {
            return _tablePrefix + table;
        }

        public SchemaBuilder CreateMapIndexTable(string name, Action<CreateTableCommand> table)
        {
            try
            {
                var createTable = new CreateTableCommand(FormatTable(name));
                var collection = CollectionHelper.Current;
                var documentTable = collection.GetPrefixedName(Store.DocumentTable);

                createTable
                    .Column<int>("Id", column => column.PrimaryKey().Identity().NotNull())
                    .Column<int>("DocumentId");

                table(createTable);
                Execute(_builder.CreateSql(createTable));

                CreateForeignKey("FK_" + name, name, new[] { "DocumentId" }, documentTable, new[] { "Id" });
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder CreateReduceIndexTable(string name, Action<CreateTableCommand> table)
        {
            try
            {
                var createTable = new CreateTableCommand(FormatTable(name));
                var collection = CollectionHelper.Current;
                var documentTable = collection.GetPrefixedName(Store.DocumentTable);

                createTable
                    .Column<int>("Id", column => column.Identity().NotNull())
                    ;

                table(createTable);
                Execute(_builder.CreateSql(createTable));

                var bridgeTableName = name + "_" + documentTable;

                CreateTable(bridgeTableName, bridge => bridge
                    .Column<int>(name + "Id", column => column.NotNull())
                    .Column<int>("DocumentId", column => column.NotNull())
                );

                CreateForeignKey("FK_" + bridgeTableName + "_Id", bridgeTableName, new[] { name + "Id" }, name, new[] { "Id" });
                CreateForeignKey("FK_" + bridgeTableName + "_DocumentId", bridgeTableName, new[] { "DocumentId" }, documentTable, new[] { "Id" });
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder DropReduceIndexTable(string name)
        {
            try
            {
                var collection = CollectionHelper.Current;
                var documentTable = collection.GetPrefixedName(Store.DocumentTable);

                var bridgeTableName = name + "_" + documentTable;

                if (String.IsNullOrEmpty(_dialect.CascadeConstraintsString))
                {
                    DropForeignKey(bridgeTableName, "FK_" + bridgeTableName + "_Id");
                    DropForeignKey(bridgeTableName, "FK_" + bridgeTableName + "_DocumentId");
                }

                DropTable(bridgeTableName);
                DropTable(name);
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder DropMapIndexTable(string name)
        {
            try
            {
                if (String.IsNullOrEmpty(_dialect.CascadeConstraintsString))
                {
                    DropForeignKey(name, "FK_" + name);
                }

                DropTable(name);
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder CreateTable(string name, Action<CreateTableCommand> table)
        {
            try
            {
                var createTable = new CreateTableCommand(FormatTable(name));
                table(createTable);
                Execute(_builder.CreateSql(createTable));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder AlterTable(string name, Action<AlterTableCommand> table)
        {
            try
            {
                var alterTable = new AlterTableCommand(FormatTable(name));
                table(alterTable);
                Execute(_builder.CreateSql(alterTable));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder DropTable(string name)
        {
            try
            {
                var deleteTable = new DropTableCommand(FormatTable(name));
                Execute(_builder.CreateSql(deleteTable));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        {
            try
            {
                var command = new CreateForeignKeyCommand(FormatTable(name), FormatTable(srcTable), srcColumns, FormatTable(destTable), destColumns);
                Execute(_builder.CreateSql(command));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destTable, string[] destColumns)
        {
            try
            {
                var command = new CreateForeignKeyCommand(FormatTable(name), FormatTable(srcTable), srcColumns, FormatTable(destTable), destColumns);
                Execute(_builder.CreateSql(command));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }
            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns)
        {
            try
            {
                var command = new CreateForeignKeyCommand(FormatTable(name), FormatTable(srcTable), srcColumns, FormatTable(destTable), destColumns);
                Execute(_builder.CreateSql(command));

            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns)
        {
            try
            {
                var command = new CreateForeignKeyCommand(FormatTable(name), FormatTable(srcTable), srcColumns, FormatTable(destTable), destColumns);
                Execute(_builder.CreateSql(command));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }

        public SchemaBuilder DropForeignKey(string srcTable, string name)
        {
            try
            {
                var command = new DropForeignKeyCommand(FormatTable(srcTable), name);
                Execute(_builder.CreateSql(command));
            }
            catch
            {
                if (ThrowOnError)
                {
                    throw;
                }
            }

            return this;
        }
    }
}
