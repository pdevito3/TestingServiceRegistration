namespace RecipeManagement.IntegrationTests;

using Databases;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Extensions.Services;
using FluentAssertions;
using FluentAssertions.Extensions;
using HeimGuard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Resources;

[SetUpFixture]
public class TestFixture
{
    public static IServiceScopeFactory BaseScopeFactory;
    private readonly TestcontainerDatabase _dbContainer = DbSetup();
    private readonly RmqConfig _rmqContainer = RmqSetup();

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await _dbContainer.StartAsync();
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", _dbContainer.ConnectionString);
        await RunMigration();

        await _rmqContainer.Container.StartAsync();
        Environment.SetEnvironmentVariable("RMQ_PORT", _rmqContainer.Port.ToString());
        Environment.SetEnvironmentVariable("RMQ_HOST", "localhost");
        Environment.SetEnvironmentVariable("RMQ_VIRTUAL_HOST", "/");
        Environment.SetEnvironmentVariable("RMQ_USERNAME", "guest");
        Environment.SetEnvironmentVariable("RMQ_PASSWORD", "guest");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName
        });
        builder.Configuration.AddEnvironmentVariables();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();
        services.ReplaceServiceWithSingletonMock<IHeimGuardClient>();

        var provider = services.BuildServiceProvider();
        BaseScopeFactory = provider.GetService<IServiceScopeFactory>();
        SetupDateAssertions();
    }

    private static async Task RunMigration()
    {
        var options = new DbContextOptionsBuilder<RecipesDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"))
            .Options;
        var context = new RecipesDbContext(options, null, null, null);
        await context?.Database?.MigrateAsync();
    }

    private static TestcontainerDatabase DbSetup()
    {
        return new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "db",
                Username = "postgres",
                Password = "postgres"
            })
            .WithName($"IntegrationTesting_RecipeManagement_{Guid.NewGuid()}")
            .WithImage("postgres:latest")
            .Build();
    }

    private static RmqConfig RmqSetup()
    {
        // var freePort = DockerUtilities.GetFreePort();
        var freePort = 7741;
        return new RmqConfig
        {
            Container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("masstransit/rabbitmq")
                .WithPortBinding(freePort, 4566)
                .WithName($"IntegrationTesting_CD-RMQ_{Guid.NewGuid()}")
                .Build(),
            Port = freePort
        };
    }

    private static void SetupDateAssertions()
    {
        // close to equivalency required to reconcile precision differences between EF and Postgres
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            options.Using<DateTime>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTime>();
            options.Using<DateTimeOffset>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTimeOffset>();

            return options;
        });
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _dbContainer.DisposeAsync();

        // MassTransit Teardown -- Do Not Delete Comment
        // TODO: Add MassTransit Teardown
        // await _harness.Stop();
    }

    private class RmqConfig
    {
        public TestcontainersContainer Container { get; set; }
        public int Port { get; set; }
    }
}