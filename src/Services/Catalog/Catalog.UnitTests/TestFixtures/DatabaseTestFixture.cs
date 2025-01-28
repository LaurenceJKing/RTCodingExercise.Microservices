using Catalog.API.Data;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace Catalog.UnitTests.TestFixtures
{
    public sealed class DatabaseTestFixture : IAsyncDisposable
    {
        public DatabaseTestFixture()
        {
            Container.StartAsync().GetAwaiter().GetResult();
            ConnectionString = Container.GetConnectionString();
            Environment.SetEnvironmentVariable("ConnectionString", ConnectionString);
            Database = new ApplicationDbContextFactory().CreateDbContext(Array.Empty<string>());
            
            Database.Database.Migrate();
        }
        private MsSqlContainer Container { get; } = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .Build();
        public string ConnectionString { get; }

        public ApplicationDbContext Database { get; }

        public async Task Reset()
        {
            await Database.Database.EnsureDeletedAsync();
            await Database.Database.EnsureCreatedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            Database.Database.EnsureDeleted();
            await Container.StopAsync();
            await Container.DisposeAsync();
        }
    }
}
