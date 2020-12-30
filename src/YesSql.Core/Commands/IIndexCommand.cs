using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace YesSql.Commands
{
    public interface IIndexCommand
    {
        Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger);
        bool AddToBatch(ISqlDialect dialect, List<string> queries, DynamicParameters parameters, List<Action<DbDataReader>> actions);
        int ExecutionOrder { get; }
    }
}