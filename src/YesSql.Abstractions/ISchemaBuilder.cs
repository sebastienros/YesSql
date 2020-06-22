using System;
using System.Data.Common;
using YesSql.Sql.Schema;

namespace YesSql.Sql
{
    public interface ISchemaBuilder
    {
        string TablePrefix { get; }
        ISqlDialect Dialect { get; }
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }

        ISchemaBuilder AlterTable(string name, Action<IAlterTableCommand> table);
        ISchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns);
        ISchemaBuilder CreateForeignKey(string name, string srcModule, string srcTable, string[] srcColumns, string destTable, string[] destColumns);
        ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destModule, string destTable, string[] destColumns);
        ISchemaBuilder CreateForeignKey(string name, string srcTable, string[] srcColumns, string destTable, string[] destColumns);
        ISchemaBuilder CreateMapIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection);
        ISchemaBuilder CreateReduceIndexTable(Type indexType, Action<ICreateTableCommand> table, string collection);
        ISchemaBuilder CreateTable(string name, Action<ICreateTableCommand> table);
        ISchemaBuilder DropForeignKey(string srcTable, string name);
        ISchemaBuilder DropMapIndexTable(Type indexType, string collection = null);
        ISchemaBuilder DropReduceIndexTable(Type indexType, string collection = null);
        ISchemaBuilder DropTable(string name);
    }
}