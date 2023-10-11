using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace YesSql.Provider.MySql
{
    public class MySqlCommandInterpreter : BaseCommandInterpreter
    {
        public MySqlCommandInterpreter(IConfiguration configuration) : base(configuration)
        {
        }

        public override void Run(StringBuilder builder, IAlterColumnCommand command)
        {
            builder.AppendFormat("alter table {0} modify column {1} ",
                            _dialect.QuoteForTableName(command.Name, _configuration.Schema),
                            _dialect.QuoteForColumnName(command.ColumnName));
            var initLength = builder.Length;

            var dbType = _dialect.ToDbType(command.DbType);

            // type
            if (dbType != DbType.Object)
            {
                builder.Append(_dialect.GetTypeName(dbType, command.Length, command.Precision, command.Scale));
            }
            else
            {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0)
                {
                    throw new Exception("Error while executing data migration: you need to specify the field's type in order to change its properties");
                }
            }

            // [default value]
            var builder2 = new StringBuilder();

            builder2.AppendFormat("alter table {0} alter column {1} ",
                            _dialect.QuoteForTableName(command.Name, _configuration.Schema),
                            _dialect.QuoteForColumnName(command.ColumnName));
            var initLength2 = builder2.Length;

            if (command.Default != null)
            {
                builder2.Append(" set default ").Append(_dialect.GetSqlValue(command.Default)).Append(" ");
            }

            // result
            var result = new List<string>();

            if (builder.Length > initLength)
            {
                result.Add(builder.ToString());
            }

            if (builder2.Length > initLength2)
            {
                result.Add(builder2.ToString());
            }
        }

        public override void Run(StringBuilder builder, IAddIndexCommand command)
        {
            builder.AppendFormat("create index {1} on {0} ({2}) ",
                _dialect.QuoteForTableName(command.Name, _configuration.Schema),
                _dialect.QuoteForColumnName(command.IndexName),
                string.Join(", ", command.ColumnNames.Select(x => GetColumName(x)).ToArray())
                );
        }

        private static readonly Regex ColumnNamePattern = new(@"([a-zA-Z_][a-zA-Z0-9_]+)\s*(\(\s*(\d+)\s*\))?");

        private string GetColumName(string name)
        {
            var result = ColumnNamePattern.Match(name);
            if (result.Success && result.Groups[1].Success)
            {
                var final = _dialect.QuoteForColumnName(result.Groups[1].Value);

                if (result.Groups.Count == 4 && result.Groups[3].Success)
                {
                    final += $"({int.Parse(result.Groups[3].Value)})";
                }

                return final;
            }

            return _dialect.QuoteForColumnName(name);
        }
    }
}
