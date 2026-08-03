# Repository Pattern

## Table of Content

1. [What is Repository Pattern?](#what-is-repository-pattern)
   - [Why use a Repository?](#why-use-a-repository)
2. [Basic Example](#basic-example)
3. [Benefits](#benefits)
   - [Separation of concerns](#separation-of-concerns)
   - [Easier testing](#easier-testing)
   - [Centralized queries](#centralized-queries)
   - [Hide database technology](#hide-database-technology)
4. [Generic Repository](#generic-repository)
   - [Why Generic Repository is controversial](#why-generic-repository-is-controversial)
5. [Microsoft's recommendation](#microsofts-recommendation)
6. [When a Repository **does** make sense](#when-a-repository-does-make-sense)
7. [Repository + Unit of Work](#repository--unit-of-work)
8. [Repository in Clean Architecture](#repository-in-clean-architecture)
9. [Interview Tips](#interview-tips)
   - [Cheat Sheet](#cheat-sheet)
   - [Should I always create a Generic Repository?](#should-i-always-create-a-generic-repository)
   - [When should I create repositories with EF Core?](#when-should-i-create-repositories-with-ef-core)

## What is Repository Pattern?

> The **Repository Pattern** is a design pattern that provides an abstraction layer between your application and the data access logic. It hides the details of how data is stored (SQL Server, PostgreSQL, MongoDB, etc.) and exposes a collection-like interface for working with entities.

**Think of it as a librarian.** You ask the librarian for a book—you don't walk into the archives yourself. The repository is the librarian between your application and the database.

### Why use a Repository?

Without a repository:

```
Controller
 |
DbContext
 |
Database
```

The controller knows about EF Core.

With a repository:

```
Controller
 |
Repository
 |
DbContext
 |
Database
```

The controller only knows about the repository. It simply:

- Separates business logic from data access.
- Centralizes complex queries.
- Improves testability through abstractions.
- Reduces coupling to persistence details.

## Basic Example

Suppose we have a `Product`:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

EF Core context:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
}
```

Repository interface:

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task SaveChangesAsync();
}
```

Implementation:

```csharp
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(int id) => _context.Products.FindAsync(id).AsTask();

    public Task<List<Product>> GetAllAsync() => _context.Products.ToListAsync();

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
```

Usage:

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product?> Get(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

## Benefits

### 1. Separation of concerns

Business logic doesn't know how data is stored:

```
Service
 |
Repository
 |
EF Core
```

Instead of:

```
Service
 |
DbContext
```

### 2. Easier testing

Mock the repository:

```csharp
var repo = new Mock<IProductRepository>();

repo.Setup(x => x.GetByIdAsync(1))
    .ReturnsAsync(new Product { Id = 1 });
```

### 3. Centralized queries

Instead of repeating:

```csharp
_context.Products.Where(x => x.Price > 100)
```

everywhere, put it in one place.

```csharp
Task<List<Product>> GetPremiumProducts();
```

### 4. Hide database technology

Today: `SQL Server`
Tomorrow:`PostgreSQL`

Only repository changes.

## Generic Repository

Many beginners create this:

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Remove(T entity);
}
```

Implementation:

```csharp
public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly AppDbContext Context;

    public Repository(AppDbContext context)
    {
        Context = context;
    }

    public Task<T?> GetByIdAsync(int id) => Context.Set<T>().FindAsync(id).AsTask();

    public Task<List<T>> GetAllAsync() => Context.Set<T>().ToListAsync();

    public Task AddAsync(T entity) => Context.Set<T>().AddAsync(entity).AsTask();

    public void Remove(T entity) => Context.Set<T>().Remove(entity);
}
```

### Why Generic Repository is controversial

Many senior .NET developers discourage using a generic repository with EF Core because **`DbContext` already implements the Repository and Unit of Work patterns**.

`DbSet<T>` already behaves like a repository:

```csharp
_context.Products.Add(product);

_context.Products.Remove(product);

await _context.Products.FindAsync(id);

await _context.Products.ToListAsync();
```

Notice how similar it is to:

```csharp
_repository.Add(product);

_repository.Remove(product);

_repository.GetById(id);

_repository.GetAll();
```

The generic repository often just wraps EF Core without adding value.

## Microsoft's recommendation

Modern ASP.NET Core applications commonly inject `DbContext` directly into services:

```csharp
public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> Get(int id)
    {
        return await _context.Products.FindAsync(id);
    }
}
```

## When a Repository **does** make sense

A repository is valuable when it encapsulates **meaningful domain-specific data access** rather than mirroring `DbSet`.

```csharp
public interface IOrderRepository
{
    Task<Order?> GetOrderWithItemsAsync(Guid id);
    Task<List<Order>> GetPendingOrdersAsync();
    Task<List<Order>> GetOrdersForCustomerAsync(Guid customerId);
    Task AddAsync(Order order);
}
```

Implementation:

```csharp
public async Task<Order?> GetOrderWithItemsAsync(Guid id)
{
    return await _context.Orders
        .Include(o => o.Items)
        .Include(o => o.Customer)
        .FirstOrDefaultAsync(o => o.Id == id);
}
```

This hides complex queries and keeps them reusable.

## Repository + Unit of Work

Traditionally:

```
Service
 |
Repository
 |
Unit of Work
 |
Database
```

In EF Core:

```
DbSet<T>   = Repository

DbContext  = Unit of Work
```

`DbContext` tracks changes across multiple entities and commits them together when you call:

```csharp
await _context.SaveChangesAsync();
```

## Repository in Clean Architecture

```
Presentation
 |
Application
 |
IRepository
 |
Infrastructure
 |
EF Core Repository
 |
Database
```

The **Application** layer depends only on the `IRepository` abstraction. The **Infrastructure** layer contains the EF Core implementation, keeping the application independent of the persistence technology.

# Interview Tips

## Cheat Sheet

| Concept             | Summary                                                           |
| ------------------- | ----------------------------------------------------------------- |
| Repository Pattern  | Abstraction over data access                                      |
| Goal                | Decouple business logic from persistence                          |
| EF Core `DbSet<T>`  | Already behaves as a repository                                   |
| EF Core `DbContext` | Implements Unit of Work                                           |
| Generic Repository  | Often unnecessary with EF Core                                    |
| Best Practice       | Inject `DbContext` directly unless a repository adds domain value |
| Good Repository     | Contains meaningful, reusable queries and persistence logic       |
| Avoid               | Creating repositories that simply duplicate `DbSet<T>` methods    |

## Should I always create a Generic Repository?

Usually no. If it simply wraps `DbSet<T>` with CRUD methods, it adds little value and can hide useful EF Core features.

## When should I create repositories with EF Core?

When they encapsulate business-specific queries, aggregate loading, or persistence rules—not just CRUD operations.

For modern .NET applications (especially with ASP.NET Core, Clean Architecture, and EF Core), a common approach is:

- **Use `DbContext` directly** in simpler applications.
- **Create domain-specific repositories** only when they encapsulate complex queries or business-specific persistence behavior.
- **Avoid generic CRUD repositories** that merely duplicate EF Core's existing API.
