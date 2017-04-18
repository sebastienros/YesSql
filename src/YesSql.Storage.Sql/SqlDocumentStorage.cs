﻿using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Collections;
using YesSql.Services;
using YesSql.Sql;
using YesSql.Storage;

namespace YesSql.Storage.Sql
{
    public class SqlDocumentStorage : IDocumentStorage
    {
        private readonly SqlDocumentStorageFactory _factory;
        private readonly static JsonSerializerSettings _jsonSettings;
        private readonly ISession _session;

        static SqlDocumentStorage()
        {
            _jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        }

        public SqlDocumentStorage(ISession session, SqlDocumentStorageFactory factory)
        {
            _session = session;
            _factory = factory;
        }

        public async Task CreateAsync(params IIdentityEntity[] documents)
        {
            var contentTable = CollectionHelper.Current.GetPrefixedName("Content");

            var tx = _session.Demand();

            foreach (var document in documents)
            {
                var content = JsonConvert.SerializeObject(document.Entity, _jsonSettings);

                var dialect = SqlDialectFactory.For(tx.Connection);
                var insertCmd = "insert into " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " (" + dialect.QuoteForColumnName("Id") + ", " + dialect.QuoteForColumnName("Content") + ") values (@Id, @Content);";
                await tx.Connection.ExecuteScalarAsync<int>(insertCmd, new { Id = document.Id, Content = content }, tx);
            }
        }

        public async Task UpdateAsync(params IIdentityEntity[] documents)
        {
            var contentTable = CollectionHelper.Current.GetPrefixedName("Content");
            var tx = _session.Demand();

            foreach (var document in documents)
            {
                var content = JsonConvert.SerializeObject(document.Entity, _jsonSettings);

                var dialect = SqlDialectFactory.For(tx.Connection);
                var updateCmd = "update " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " set " + dialect.QuoteForColumnName("Content") + " = @Content where " + dialect.QuoteForColumnName("Id") + " = @Id;";
                await tx.Connection.ExecuteScalarAsync<int>(updateCmd, new { Id = document.Id, Content = content }, tx);
            }
        }

        public async Task DeleteAsync(params IIdentityEntity[] documents)
        {
            var contentTable = CollectionHelper.Current.GetPrefixedName("Content");
            var tx = _session.Demand();

            foreach (var documentsPage in documents.PagesOf(128))
            {
                var dialect = SqlDialectFactory.For(tx.Connection);
                var deleteCmd = "delete from " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " where " + dialect.QuoteForColumnName("Id") + dialect.InOperator("@Id") + ";";
                await tx.Connection.ExecuteScalarAsync<int>(deleteCmd, new { Id = documentsPage.Select(x => x.Id).ToArray() }, tx);
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(params int[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("id");
            }

            var contentTable = CollectionHelper.Current.GetPrefixedName("Content");
            var result = new T[ids.Length];

            // Create an index to lookup the position of a specific document id
            var orderedLookup = new Dictionary<int, int>();
            for (var i = 0; i < ids.Length; i++)
            {
                orderedLookup[ids[i]] = i;
            }

            var tx = _session.Demand();

            var dialect = SqlDialectFactory.For(tx.Connection);
            var selectCmd = "select " + dialect.QuoteForColumnName("Id") + ", " + dialect.QuoteForColumnName("Content") + " from " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " where " + dialect.QuoteForColumnName("Id") + dialect.InOperator("@Id") + ";";

            foreach (var idPages in ids.PagesOf(128))
            {
                var entities = await tx.Connection.QueryAsync<IdString>(selectCmd, new { Id = idPages.ToArray() }, tx);

                foreach (var entity in entities)
                {
                    var index = orderedLookup[entity.Id];
                    if (typeof(T) == typeof(object))
                    {
                        result[index] = JsonConvert.DeserializeObject<dynamic>(entity.Content, _jsonSettings);
                    }
                    else
                    {
                        result[index] = JsonConvert.DeserializeObject<T>(entity.Content, _jsonSettings);
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<object>> GetAsync(params IIdentityEntity[] documents)
        {
            var result = new object[documents.Length];

            // Create an index to lookup the position of a specific document id
            var orderedLookup = new Dictionary<int, int>();
            for (var i = 0; i < documents.Length; i++)
            {
                orderedLookup[documents[i].Id] = i;
            }

            var contentTable = CollectionHelper.Current.GetPrefixedName("Content");
            var tx = _session.Demand();

            var typeGroups = documents.GroupBy(x => x.EntityType);

            // In case identities are from different types, group queries by type
            foreach (var typeGroup in typeGroups)
            {
                // Limit the IN clause to 128 items at a time
                foreach (var documentsPage in typeGroup.PagesOf(128))
                {
                    var ids = documentsPage.Select(x => x.Id).ToArray();
                    var dialect = SqlDialectFactory.For(tx.Connection);
                    IEnumerable<IdString> entities;
                    if (ids.Length == 1)
                    {
                        var single = "select " + dialect.QuoteForColumnName("Id") + ", " + dialect.QuoteForColumnName("Content") + " from " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " where " + dialect.QuoteForColumnName("Id") + " = @Id;";
                        entities = await tx.Connection.QueryAsync<IdString>(single, new { Id = ids[0] }, tx);
                    }
                    else
                    {
                        var single = "select " + dialect.QuoteForColumnName("Id") + ", " + dialect.QuoteForColumnName("Content") + " from " + dialect.QuoteForTableName(_factory.TablePrefix + contentTable) + " where " + dialect.QuoteForColumnName("Id") + dialect.InOperator("@Id");
                        entities = await tx.Connection.QueryAsync<IdString>(single, new { Id = ids }, tx);
                    }                        
                    
                    foreach (var entity in entities)
                    {
                        var index = orderedLookup[entity.Id];
                        result[index] = JsonConvert.DeserializeObject(entity.Content, typeGroup.Key, _jsonSettings);
                    }
                }
            }

            return result;
        }

        private struct IdString
        {
#pragma warning disable 0649
            public int Id;
            public string Content;
#pragma warning restore 0649
        }
    }
}
