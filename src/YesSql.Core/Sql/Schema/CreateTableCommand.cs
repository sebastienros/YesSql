﻿using System;
using System.Data;

namespace YesSql.Sql.Schema
{
    public class CreateTableCommand : SchemaCommand
    {
        public CreateTableCommand(string name)
            : base(name, SchemaCommandType.CreateTable)
        {
        }

        public CreateTableCommand Column(string columnName, DbType dbType, Action<CreateColumnCommand> column = null)
        {
            var command = new CreateColumnCommand(Name, columnName);
            command.WithType(dbType);

            if (column != null)
            {
                column(command);
            }
            TableCommands.Add(command);
            return this;
        }

        public CreateTableCommand Column<T>(string columnName, Action<CreateColumnCommand> column = null)
        {
            var dbType = SchemaUtils.ToDbType(typeof(T));
            return Column(columnName, dbType, column);
        }

    }
}
