using System;
using System.Data;

namespace YesSql.Sql.Schema
{
    public class CreateTableCommand : SchemaCommand, ICreateTableCommand
    {
        public CreateTableCommand(string name)
            : base(name, SchemaCommandType.CreateTable)
        {
        }

        public ICreateTableCommand Column(string columnName, Type dbType, Action<ICreateColumnCommand> column = null)
        {
            var command = new CreateColumnCommand(Name, columnName);
            command.WithType(dbType);

            column?.Invoke(command);

            Console.WriteLine($"{Name}: {columnName} - {dbType.Name}");

            TableCommands.Add(command);
            return this;
        }

        public ICreateTableCommand Column<T>(string columnName, Action<ICreateColumnCommand> column = null)
        {
            return Column(columnName, typeof(T), column);
        }

    }
}
