namespace RecipeManagement.IntegrationTests;

using RecipeManagement.Extensions.Services;
using MassTransit.Testing;
using MassTransit;
using RecipeManagement.Domain.Recipes.Features;
using RecipeManagement.Databases;
using RecipeManagement;
using RecipeManagement.Services;
using RecipeManagement.Resources;
using RecipeManagement.Services;
using HeimGuard;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Respawn.Graph;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;
using FluentAssertions;
using FluentAssertions.Extensions;

[SetUpFixture]
public class TestFixture
{
    private readonly TestcontainerDatabase _dbContainer = dbSetup();

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await _dbContainer.StartAsync();
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", _dbContainer.ConnectionString);
        
        await RunMigration();
    }

    private static async Task RunMigration()
    {
        var options = new DbContextOptionsBuilder<RecipesDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"))
            .Options;
        var context = new RecipesDbContext(options, null, null, null);
        await context?.Database?.MigrateAsync();
    }

    private static TestcontainerDatabase dbSetup()
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

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _dbContainer.DisposeAsync();
        
        // MassTransit Teardown -- Do Not Delete Comment
        // TODO: Add MassTransit Teardown
        // await _harness.Stop();
    }
}


