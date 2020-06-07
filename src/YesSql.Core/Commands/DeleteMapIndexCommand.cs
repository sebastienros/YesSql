using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Collections;

namespace YesSql.Commands
{
    public sealed class DeleteMapIndexCommand : IIndexCommand
    {
        public IEnumerable<int> DocumentIds { get; }
        public Type IndexType { get; }
        private readonly string _tablePrefix;

        public int ExecutionOrder { get; } = 1;

        public DeleteMapIndexCommand(Type indexType, IEnumerable<int> documentIds, string tablePrefix, ISqlDialect dialect)
        {
            IndexType = indexType;
            DocumentIds = documentIds;
            _tablePrefix = tablePrefix;
        }

        public Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger )
        {
            var indexTableName = _tablePrefix + CollectionHelper.Current.GetPrefixedName(IndexType.Name);
            var command = "delete from " + dialect.QuoteForTableName(indexTableName) + " where " + dialect.QuoteForColumnName("DocumentId") + " = @Id";
            logger.LogTrace(command);
            return connection.ExecuteAsync(command, DocumentIds.Select(x => new { Id = x }), transaction);
        }
    }
}
