﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using YesSql.Indexes;

namespace YesSql
{
    public interface IStore : IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="ISession"/> to communicate with the <see cref="IStore"/> with
        /// the specified <see cref="IsolationLevel"/>.
        /// </summary>
        ISession CreateSession(IsolationLevel isolationLevel);

        /// <summary>
        /// Registers the specified <see cref="IIndexProvider"/> instance.
        /// </summary>
        /// <param name="indexProviders">The <see cref="IIndexProvider"/> to register.</param>
        /// <returns>The current <see cref="IStore"/>.</returns>
        IStore RegisterIndexes(params IIndexProvider[] indexProviders);

        IConfiguration Configuration { get; set; }
        Task InitializeAsync();
        Task InitializeCollectionAsync(string collection);
        IIdAccessor<int> GetIdAccessor(Type tContainer, string name);
        int GetNextId(ISession session, string collection);
        IEnumerable<IndexDescriptor> Describe(Type target);
    }

    public static class IStoreExtensions
    {
        /// <summary>
        /// Creates a new <see cref="ISession"/> to communicate with the <see cref="IStore"/> with
        /// the default <see cref="IsolationLevel"/>.
        /// </summary>
        public static ISession CreateSession(this IStore store)
        {
            return store.CreateSession(store.Configuration.IsolationLevel);
        }
    }
}
