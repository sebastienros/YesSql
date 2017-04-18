﻿using System;
using Microsoft.Data.Sqlite;
using System.Data;
using YesSql.Services;
using YesSql.Samples.Hi.Indexes;
using YesSql.Samples.Hi.Models;
using YesSql.Storage.InMemory;
using YesSql.Sql;

namespace YesSql.Samples.Hi
{
    internal class Program
    {
        private static void Main()
        {
            var configuration = new Configuration
            {
                ConnectionFactory = new DbConnectionFactory<SqliteConnection>(@"Data Source=:memory:", true),
                DocumentStorageFactory = new InMemoryDocumentStorageFactory(),
                IsolationLevel = IsolationLevel.Serializable
            };

            var store = new Store(configuration);

            store.InitializeAsync().Wait();

            using (var session = store.CreateSession())
            {
                new SchemaBuilder(session).CreateMapIndexTable(nameof(BlogPostByAuthor), table => table
                        .Column<string>("Author")
                    )
                    .CreateReduceIndexTable(nameof(BlogPostByDay), table => table
                        .Column<int>("Count")
                        .Column<int>("Day")
                    );
            };

            // register available indexes
            store.RegisterIndexes<BlogPostIndexProvider>();

            // creating a blog post
            var post = new BlogPost
            {
                Title = "Hello YesSql",
                Author = "Bill",
                Content = "Hello",
                PublishedUtc = DateTime.UtcNow,
                Tags = new[] { "Hello", "YesSql" }
            };

            // saving the post to the database
            using (var session = store.CreateSession())
            {
                session.Save(post);
            }

            // loading a single blog post
            using (var session = store.CreateSession())
            {
                var p = session.QueryAsync().For<BlogPost>().FirstOrDefault().Result;
                Console.WriteLine(p.Title); // > Hello YesSql
            }

            // loading blog posts by author
            using (var session = store.CreateSession())
            {
                var ps = session.QueryAsync<BlogPost, BlogPostByAuthor>().Where(x => x.Author.StartsWith("B")).List().Result;

                foreach (var p in ps)
                {
                    Console.WriteLine(p.Author); // > Bill
                }
            }

            // loading blog posts by day of publication
            using (var session = store.CreateSession())
            {
                var ps = session.QueryAsync<BlogPost, BlogPostByDay>(x => x.Day == DateTime.UtcNow.ToString("yyyyMMdd")).List().Result;

                foreach (var p in ps)
                {
                    Console.WriteLine(p.PublishedUtc); // > [Now]
                }
            }

            // counting blog posts by day
            using (var session = store.CreateSession())
            {
                var days = session.QueryIndexAsync<BlogPostByDay>().List().Result;

                foreach (var day in days)
                {
                    Console.WriteLine(day.Day + ": " + day.Count); // > [Today]: 1
                }
            }
        }
    }
}