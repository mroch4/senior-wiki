# Testcontainers

## Table of content

1. [What are Testcontainers?](#what-are-testcontainers)
2. [Why use Testcontainers?](#why-use-testcontainers)
3. [Simple example](#simple-example)
4. [Typical ASP.NET Core integration test](#typical-aspnet-core-integration-test)
5. [Common examples](#common-examples)
6. [Why not always use Testcontainers?](#why-not-always-use-testcontainers)
7. [Testcontainers vs mocks](#testcontainers-vs-mocks)
8. [Interview Tips](#interview-tips)

## What are Testcontainers?

> **Testcontainers** lets you run real dependencies—such as databases, Redis, RabbitMQ, or other services—in **Docker containers during integration tests**.

Instead of mocking everything:

```
Your application
 |
Mock database ❌
```

you can test against the real infrastructure:

```
Test
 |
ASP.NET Core application
 |
Testcontainer
 |
PostgreSQL / SQL Server / Redis / RabbitMQ
```

## Why use Testcontainers?

Without Testcontainers, integration tests might require:

- A database installed locally
- A shared test database
- Manual cleanup
- Different configuration on every developer's machine

With Testcontainers:

```
Test starts
 |
Docker container starts
 |
PostgreSQL is available
 |
Tests run
 |
Container is destroyed
```

Each test environment can be isolated and reproducible.

## Simple example

For example, using PostgreSQL:

```csharp
public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_Save_User()
    {
        var connectionString = _postgres.GetConnectionString();

        // Use the real PostgreSQL database here
    }
}
```

The test starts a **real PostgreSQL instance in Docker**.

## Typical ASP.NET Core integration test

A common pattern is:

```
WebApplicationFactory
 ├── Replace connection string
Testcontainer
 |
Real database
```

For example:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
            };

            config.AddInMemoryCollection(settings);
        });
    }
}
```

Then your test can use `HttpClient`:

```csharp
public class UserApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

Now you're testing:

- ASP.NET Core middleware
- Routing
- Controllers/endpoints
- Dependency injection
- EF Core
- Real database communication

This is much closer to production than mocking the database.

## Common examples

Testcontainers can run:

- PostgreSQL
- SQL Server
- MySQL
- MongoDB
- Redis
- RabbitMQ
- Kafka
- Elasticsearch
- LocalStack for AWS services

## Why not always use Testcontainers?

Because they are **not a replacement for unit tests**.

A good testing strategy is:

- **Unit tests** - (bottom) lots of them, very fast, mocks/fakes where appropriate.
- **Integration tests** - (middle) fewer, test real components together with Testcontainers.
- **End-to-end tests** - (top) even fewer, test important user flows.

The key idea is: **use Testcontainers when the interaction with the real infrastructure is what you actually want to verify.**

> **"I use mocks for unit tests where I want to isolate business logic. For integration tests, Testcontainers are useful because they allow me to test against real infrastructure, such as PostgreSQL, Redis, or RabbitMQ, in an isolated Docker environment. This gives higher confidence that the application works correctly with the actual dependency while keeping the test environment reproducible and disposable."**

## Testcontainers vs mocks

| Mocks                               | Testcontainers                              |
| ----------------------------------- | ------------------------------------------- |
| Fast                                | Slower                                      |
| No Docker required                  | Requires Docker                             |
| Tests isolated code                 | Tests real integration                      |
| Can miss SQL/configuration problems | Catches infrastructure integration problems |
| Best for unit tests                 | Best for integration tests                  |

# Interview Tips

> It's a tool commonly used for integration tests and running dependencies in containers. They allow us to run real dependencies, such as a database or message broker, in isolated Docker containers. This lets us test how our application integrates with those real components instead of relying only on mocks.

> Testcontainers is a library that manages Docker containers for automated tests. In .NET, I can spin up real dependencies such as PostgreSQL, Redis, or RabbitMQ for integration tests, configure my application to connect to them, run the tests, and dispose of the environment afterward. This makes integration tests more realistic and reproducible than relying entirely on mocks or a shared test environment.

A likely follow-up interview question would be: **"How would you use Testcontainers together with `WebApplicationFactory` and EF Core?"**
