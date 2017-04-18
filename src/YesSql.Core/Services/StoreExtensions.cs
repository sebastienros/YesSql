﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YesSql.Collections;
using YesSql.Indexes;

namespace YesSql
{
    public static class StoreExtensions
    {
        public static IStore RegisterIndexes<T>(this IStore store) where T : IIndexProvider
        {
            return store.RegisterIndexes(typeof(T));
        }

        public static IStore RegisterIndexes(this IStore store, Type type)
        {
            var index = Activator.CreateInstance(type) as IIndexProvider;
            if (index != null)
            {
                store.RegisterIndexes(index);
            }

            return store;
        }

        public static IStore RegisterIndexes(this IStore store, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                store.RegisterIndexes(type);
            }

            return store;
        }

        public static IStore RegisterIndexes(this IStore store, Assembly assembly)
        {
            var exportedTypes = assembly.GetExportedTypes();
            var indexes = exportedTypes.Where(x => typeof(IIndexProvider).IsAssignableFrom(x));
            return store.RegisterIndexes(indexes);
        }

    }
}
