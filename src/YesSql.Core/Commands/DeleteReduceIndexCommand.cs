using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Threading.Tasks;
using YesSql.Collections;
using YesSql.Indexes;

namespace YesSql.Commands
{
    public sealed class DeleteReduceIndexCommand : IndexCommand
    {
        public DeleteReduceIndexCommand(IIndex index, string tablePrefix) : base(index, tablePrefix)
        {
        }

        public override int ExecutionOrder { get; } = 1;

        public override async Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger)
        {
            var indexTypeName = Index.GetType().Name;
            var indexTableName = CollectionHelper.Current.GetPrefixedName(indexTypeName);
            var documentTable = CollectionHelper.Current.GetPrefixedName(Store.DocumentTable);
            var bridgeTableName = _tablePrefix + indexTableName + "_" + documentTable;
            var bridgeSql = "delete from " + dialect.QuoteForTableName(bridgeTableName) +" where " + dialect.QuoteForColumnName(indexTypeName + "Id") + " = @Id";
            logger.LogTrace(bridgeSql);
            await connection.ExecuteAsync(bridgeSql, new { Id = Index.Id }, transaction);
            var command = "delete from " + _tablePrefix + indexTableName + " where " + dialect.QuoteForColumnName("Id") + " = @Id";
            logger.LogTrace(command);
            await connection.ExecuteAsync(command, new { Id = Index.Id }, transaction);
        }
    }
}
