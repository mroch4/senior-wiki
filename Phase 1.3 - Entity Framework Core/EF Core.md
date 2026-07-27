# EF Core

## Table of content

1. [What is ORM?](#what-is-orm)
2. [What is EF Core?](#what-is-ef-core)
   - [Example](#example)
   - [Advantages](#advantages)
   - [Disadvantages](#disadvantages)
3. [When to avoid the Repository Pattern](#when-to-avoid-the-repository-pattern)
   - [You're using EF Core directly in a simple application](#youre-using-ef-core-directly-in-a-simple-application)
   - [Your repository becomes a pass-through](#your-repository-becomes-a-pass-through)
   - [You need EF Core features](#you-need-ef-core-features)
   - [You have many complex queries](#you-have-many-complex-queries)
4. [When a Repository Pattern _is_ useful](#when-a-repository-pattern-is-useful)
5. [General guideline](#general-guideline)
6. [Interview Tips](#interview-tips)

## What is ORM?

> **ORM (Object-Relational Mapping)** is a technique that maps database tables to objects in your application. Instead of writing SQL for every operation, you work with C# objects, and the ORM generates the SQL behind the scenes.

## What is EF Core?

> **Entity Framework Core (EF Core)** is Microsoft's official ORM for .NET. It lets you:

- Query data using LINQ
- Insert, update, and delete records with C# objects
- Track entity changes automatically
- Manage database schema using migrations
- Work with databases such as SQL Server, PostgreSQL, MySQL, SQLite, and others. ([Microsoft Learn][1])

### Example

**Entity**

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**DbContext**

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
```

**Insert**

```csharp
using var context = new AppDbContext(options);

context.Products.Add(new Product
{
    Name = "Laptop",
    Price = 1200
});

context.SaveChanges();
```

**Query**

```csharp
var expensiveProducts = context.Products
    .Where(p => p.Price > 1000)
    .ToList();
```

### Advantages

- Less boilerplate SQL code
- Strong typing with IntelliSense
- Easier maintenance
- Database migrations
- Cross-platform support

### Disadvantages

- Slightly slower than hand-written SQL for some complex queries.
- You still need to understand SQL and database design for good performance. ([Microsoft Learn][1])

## When to avoid the Repository Pattern

### You're using EF Core directly in a simple application

For CRUD-heavy applications, adding another repository layer often just wraps EF Core methods.

Instead of:

```csharp
productRepository.GetById(id);
productRepository.Add(product);
```

You can simply use:

```csharp
var product = await context.Products.FindAsync(id);
context.Products.Add(product);
await context.SaveChangesAsync();
```

then the repository adds little value here.

### Your repository becomes a pass-through

If your repository methods only call EF Core methods, such as:

```csharp
public Task<Product> GetById(int id)
{
    return _context.Products.FindAsync(id).AsTask();
}
```

or

```csharp
public void Add(Product product)
{
    _context.Products.Add(product);
}
```

then the repository is just duplicating EF Core's API.

### You need EF Core features

Repositories often hide useful EF Core capabilities like:

- `Include()`
- `AsNoTracking()`
- `ExecuteUpdate()`
- `ExecuteDelete()`
- `FromSql()`
- LINQ composition
- Projections (`Select`)
- Split queries

You may end up exposing EF Core concepts anyway, defeating the abstraction.

### You have many complex queries

Instead of creating methods like:

```
GetProductsByCategory()
GetProductsByPrice()
GetProductsByPriceAndCategory()
GetProductsByCategoryOrdered()
```

Alternatives include:

- Specification Pattern
- Query Objects
- CQRS (separate query handlers)

## When a Repository Pattern _is_ useful

It can still make sense when:

- You need a persistence abstraction in a domain-driven design (DDD) application.
- You combine multiple data sources (e.g., SQL Server, Redis, external APIs).
- You want to encapsulate complex persistence logic.
- You have aggregate roots with rich domain behavior.
- You need a consistent data access API across multiple storage technologies.

## General guideline

For most modern ASP.NET Core applications using EF Core:

- **Simple CRUD application:** Use `DbContext` directly.
- **Complex business/domain logic:** Consider repositories (often per aggregate), possibly with the Specification Pattern.
- **Read-heavy applications:** Consider CQRS and query objects instead of large repositories.

This approach is also aligned with guidance from many experienced .NET developers: don't add a repository layer unless it solves a concrete problem that `DbContext` doesn't already solve.

# Interview Tips

If you're preparing for .NET interviews, EF Core topics commonly include:

- `DbContext` and `DbSet`
- Code First vs Database First
- Migrations
- LINQ
- Relationships (One-to-One, One-to-Many, Many-to-Many)
- Loading strategies (Eager, Lazy, Explicit)
- Change Tracking
- Transactions
- Repository Pattern (when appropriate)

These are the core concepts most employers expect for EF Core development.

The **Repository Pattern** is not always a good fit when using **Entity Framework Core** because `DbContext` already acts as a **Unit of Work**, and `DbSet<TEntity>` already behaves much like a **Repository**.

[1]: https://learn.microsoft.com/en-us/ef/core/?utm_source=chatgpt.com "Overview of Entity Framework Core - EF Core | Microsoft Learn"
