using Bizcord.MessageClient;
using Bizcord.Messaging.Api.Application;
using Bizcord.Messaging.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bizcord.Messaging.IntegrationTests;

public sealed class MessagingApiFactory(bool useRealBroker = false) : WebApplicationFactory<Program>
{
    private readonly string dataDirectory = Path.Combine(Path.GetTempPath(), "bizcord-tests-" + Guid.NewGuid().ToString("N"));
    public FakeMessageClient Messages { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<SqliteMessageRepository>();
            services.RemoveAll<IMessageRepository>();
            services.AddSingleton(new SqliteMessageRepository($"Data Source={Path.Combine(dataDirectory, "messages.db")};Pooling=False"));
            services.AddSingleton<IMessageRepository>(provider => provider.GetRequiredService<SqliteMessageRepository>());
            if (!useRealBroker)
            {
                services.RemoveAll<IMessageClient>();
                services.AddSingleton<IMessageClient>(Messages);
            }
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        // Only these exact files in this factory's freshly created temporary directory are removed.
        foreach (var name in new[] { "messages.db", "messages.db-shm", "messages.db-wal" })
            File.Delete(Path.Combine(dataDirectory, name));
        if (Directory.Exists(dataDirectory)) Directory.Delete(dataDirectory);
    }
}
