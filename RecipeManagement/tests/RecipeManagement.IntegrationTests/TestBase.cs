namespace RecipeManagement.IntegrationTests;

using System.Security.Claims;
using NUnit.Framework;
using System.Threading.Tasks;
using AutoBogus;
using Databases;
using Domain.Recipes.Features;
using Extensions.Services;
using FluentAssertions;
using FluentAssertions.Extensions;
using HeimGuard;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Resources;
using Respawn;
using Services;
using static TestFixture;

[Parallelizable]
public class TestBase
{
    private static IServiceScopeFactory _scopeFactory;
    private static ServiceProvider _provider;
    private static InMemoryTestHarness _harness;
    
    [SetUp]
    public async Task TestSetUp()
    {
        // var userPolicyHandler = GetService<IHeimGuardClient>();
        // Mock.Get(userPolicyHandler)
        //     .Setup(x => x.HasPermissionAsync(It.IsAny<string>()))
        //     .ReturnsAsync(true);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            EnvironmentName = Consts.Testing.IntegrationTestingEnvName,
        });
        builder.Configuration.AddEnvironmentVariables();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();
        services.ReplaceServiceWithSingletonMock<IHeimGuardClient>();

        // MassTransit Harness Setup -- Do Not Delete Comment
        services.AddMassTransitInMemoryTestHarness(cfg =>
        {
            cfg.AddMassTransitTestHarness(harness => 
            {
                // Consumer Registration -- Do Not Delete Comment

                harness.AddConsumer<AddToBook>();
            });
        });

        _provider = services.BuildServiceProvider();
        _scopeFactory = _provider.GetService<IServiceScopeFactory>();

        // MassTransit Start Setup -- Do Not Delete Comment
        // _harness = _provider.GetRequiredService<InMemoryTestHarness>();
        // await _harness.Start();

        SetupDateAssertions();
        
        AutoFaker.Configure(builder =>
        {
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        });
    }

    public static TScopedService GetService<TScopedService>()
    {
        var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetService<TScopedService>();
        return service;
    }

    public static void SetUserRole(string role, string sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, sub)
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public static void SetUserRoles(string[] roles, string sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(ClaimTypes.Name, sub));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }
    
    public static void SetMachineRole(string role, string clientId = null)
    {
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim("client_role", role),
            new Claim("client_id", clientId)
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public static void SetMachineRoles(string[] roles, string clientId = null)
    {
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {
            claims.Add(new Claim("client_role", role));
        }
        claims.Add(new Claim("client_id", clientId));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<RecipesDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<RecipesDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();

        try
        {
            //await dbContext.BeginTransactionAsync();

            await action(scope.ServiceProvider);

            //await dbContext.CommitTransactionAsync();
        }
        catch (Exception)
        {
            //dbContext.RollbackTransaction();
            throw;
        }
    }

    public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipesDbContext>();

        try
        {
            //await dbContext.BeginTransactionAsync();

            var result = await action(scope.ServiceProvider);

            //await dbContext.CommitTransactionAsync();

            return result;
        }
        catch (Exception)
        {
            //dbContext.RollbackTransaction();
            throw;
        }
    }

    public static Task ExecuteDbContextAsync(Func<RecipesDbContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>()));

    public static Task ExecuteDbContextAsync(Func<RecipesDbContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>()).AsTask());

    public static Task ExecuteDbContextAsync(Func<RecipesDbContext, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>(), sp.GetService<IMediator>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<RecipesDbContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<RecipesDbContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>()).AsTask());

    public static Task<T> ExecuteDbContextAsync<T>(Func<RecipesDbContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<RecipesDbContext>(), sp.GetService<IMediator>()));

    public static Task<int> InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }
            return db.SaveChangesAsync();
        });
    }

    // MassTransit Methods -- Do Not Delete Comment
    /// <summary>
    /// Publishes a message to the bus, and waits for the specified response.
    /// </summary>
    /// <param name="message">The message that should be published.</param>
    /// <typeparam name="TMessage">The message that should be published.</typeparam>
    public static async Task PublishMessage<TMessage>(object message)
        where TMessage : class
    {
        await _harness.Bus.Publish<TMessage>(message);
    }
    
    /// <summary>
    /// Confirm that a message has been published for this harness.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be published.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been published.</returns>
    public static async Task<bool> IsPublished<TMessage>()
        where TMessage : class
    {
        return await _harness.Published.Any<TMessage>();
    }
    
    /// <summary>
    /// Confirm that a message has been consumed for this harness.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be consumed.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been consumed.</returns>
    public static async Task<bool> IsConsumed<TMessage>()
        where TMessage : class
    {
        return await _harness.Consumed.Any<TMessage>();
    }
    
    /// <summary>
    /// The desired consumer consumed the message.
    /// </summary>
    /// <typeparam name="TMessage">The message that should be consumed.</typeparam>
    /// <typeparam name="TConsumedBy">The consumer of the message.</typeparam>
    /// <returns>A boolean of true if a message of the given type has been consumed by the given consumer.</returns>
    public static async Task<bool> IsConsumed<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = _provider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();
        return await consumerHarness.Consumed.Any<TMessage>();
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
}

public static class ServiceCollectionServiceExtensions
{
    public static IServiceCollection ReplaceServiceWithSingletonMock<TService>(this IServiceCollection services)
        where TService : class
    {
        services.RemoveAll(typeof(TService));
        services.AddSingleton(_ => Mock.Of<TService>());
        return services;
    }
}