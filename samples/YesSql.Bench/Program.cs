using System.Threading.Tasks;
using Xunit;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.SqlServer;
using YesSql.Sql;

namespace Bench
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var store = new Store(
                new Configuration()
                    .UseSqlServer(@"Data Source =.; Initial Catalog = yessql; Integrated Security = True")
                    .SetTablePrefix("Bench")
                );

            await store.InitializeAsync();

            using (var connection = store.Configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    var builder = new SchemaBuilder(store, transaction);

                    builder.CreateMapIndexTable(nameof(UserByName), c => c
                        .Column<string>("Name")
                        .Column<bool>("Adult")
                        .Column<int>("Age")
                    );

                    transaction.Commit();
                }
            }

            store.RegisterIndexes<UserIndexProvider>();

            using (var session = store.CreateSession())
            {
                var user = await session.Query<User>().FirstOrDefaultAsync();
                Assert.Null(user);

                var bill = new User
                {
                    Name = "Bill",
                    Adult = true,
                    Age = 1
                };


                session.Save(bill);

            }

            using (var session = store.CreateSession())
            {
                var user = await session.Query<User, UserByName>().Where(x => x.Adult == true).FirstOrDefaultAsync();
                Assert.NotNull(user);

                user = await session.Query<User, UserByName>().Where(x => x.Age == 1).FirstOrDefaultAsync();
                Assert.NotNull(user);

                user = await session.Query<User, UserByName>().Where(x => x.Age == 1 && x.Adult).FirstOrDefaultAsync();
                Assert.NotNull(user);

                user = await session.Query<User, UserByName>().Where(x => x.Name.StartsWith("B")).FirstOrDefaultAsync();
                Assert.NotNull(user);
            }
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Adult { get; set; }
        public int Age { get; set; }
    }

    public class UserByName : MapIndex
    {
        public string Name { get; set; }
        public bool Adult { get; set; }
        public int Age { get; set; }
    }

    public class UserIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByName>()
                .Map(user => new UserByName { Name = user.Name, Adult = user.Adult, Age = user.Age });
        }
    }

}
