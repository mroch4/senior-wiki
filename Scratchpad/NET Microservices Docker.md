Docker is one of the most important technologies in modern .NET development because it lets you **package an application together with everything it needs to run** (runtime, libraries, configuration) into a **container**. The application then behaves the same on your laptop, a test server, or in production.

# Why Docker?

Without Docker:

```text
Developer Machine
    |
    | Works!
    |
Production Server
    |
    | Doesn't work
```

Typical reasons:

- Different .NET runtime version
- Missing libraries
- Different OS
- Configuration differences

This is the classic:

> "It works on my machine."

Docker solves this.

```text
Application
     +
.NET Runtime
     +
Dependencies
     +
Configuration
     ↓
Docker Image
```

The same image runs everywhere.

# Virtual Machine vs Docker

### Virtual Machine

```text
Hardware
    │
Host OS
    │
Hypervisor
    │
Guest OS
    │
Application
```

Each VM has its own operating system.

Advantages:

- Strong isolation

Disadvantages:

- Large (GBs)
- Slow startup
- Higher memory usage

### Docker Container

```text
Hardware
    │
Host OS
    │
Docker Engine
    │
Containers
```

Containers **share the host OS kernel**.

Advantages:

- Lightweight
- Starts in seconds
- Lower resource usage
- Easy to scale

# Docker Terminology

## Image

An **image** is a read-only template.

Think of it like a class in object-oriented programming.

Example:

```text
aspnet:8.0
```

It contains:

- .NET runtime
- Linux
- Required libraries

## Container

A **container** is a running instance of an image.

Think:

```text
Image
   ↓
Container
```

Just as:

```text
Class
   ↓
Object
```

One image can create many containers.

# Dockerfile

A **Dockerfile** contains instructions for building an image.

Example:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY . .

ENTRYPOINT ["dotnet", "ProductService.dll"]
```

Meaning:

- Start with the ASP.NET runtime image.
- Set `/app` as the working directory.
- Copy application files.
- Run `ProductService.dll`.

# Multi-Stage Builds

In .NET, the common approach is a **multi-stage build**.

```text
Stage 1
SDK Image
    │
Build Application
    │
Publish
    │
Stage 2
Runtime Image
    │
Copy Published Files
```

Why?

The .NET SDK image is large.

The runtime image is much smaller.

Example:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet publish -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "ProductService.dll"]
```

This is the standard approach for production.

# Docker Commands

Build an image:

```bash
docker build -t productservice .
```

Run a container:

```bash
docker run productservice
```

Run in the background:

```bash
docker run -d productservice
```

Map ports:

```bash
docker run -p 8080:80 productservice
```

Meaning:

```text
Browser

↓

localhost:8080

↓

Container Port 80
```

# Listing Containers

Running containers:

```bash
docker ps
```

All containers:

```bash
docker ps -a
```

# Stop a Container

```bash
docker stop <container-id>
```

# Remove a Container

```bash
docker rm <container-id>
```

# List Images

```bash
docker images
```

# Remove an Image

```bash
docker rmi image-name
```

# Volumes

Containers are **ephemeral**.

If the container is deleted:

```text
Container

↓

Database

↓

Delete Container

↓

Data Lost
```

Volumes provide persistent storage.

```text
Container

↓

Volume

↓

Disk
```

Example:

```bash
docker volume create sql-data
```

Mount it:

```bash
docker run -v sql-data:/var/opt/mssql ...
```

Now the data survives even if the container is recreated.

# Networks

Containers communicate over Docker networks.

```text
Order Service

↓

Docker Network

↓

Product Service

↓

Database
```

Instead of IP addresses, containers can use service names.

Example:

```text
Order Service

↓

http://product-service
```

# Environment Variables

Avoid hardcoding configuration.

Instead of:

```csharp
string connection =
"Server=localhost...";
```

Use:

```bash
-e ConnectionStrings__Default=...
```

ASP.NET Core automatically reads environment variables into configuration.

# Docker Compose

A microservices application often needs multiple containers:

```text
Order Service

Product Service

RabbitMQ

SQL Server

Redis
```

Starting them individually is tedious.

`docker-compose.yml` lets you define them together.

Example:

```yaml
services:
  product-api:
    build: .
    ports:
      - "5001:80"

  rabbitmq:
    image: rabbitmq:management

  sqlserver:
    image: mcr.microsoft.com/mssql/server
```

Start everything:

```bash
docker compose up
```

Stop everything:

```bash
docker compose down
```

# Docker in Microservices

```text
           API Gateway
                │
      ┌─────────┴─────────┐
      │                   │
 Product Service     Order Service
      │                   │
      └─────────┬─────────┘
                │
            RabbitMQ
                │
            SQL Server
```

Each service runs in its **own container**.

Benefits:

- Independent deployment
- Independent scaling
- Isolation
- Easier upgrades

# Docker vs Kubernetes

A common interview question.

Docker:

- Builds images
- Runs containers

Kubernetes:

- Manages containers across many machines
- Restarts failed containers
- Scales applications
- Performs rolling updates
- Load balances traffic

Think of it this way:

```text
Docker

↓

Creates Containers

Kubernetes

↓

Manages Containers
```

# Best Practices

- Use **multi-stage builds** to reduce image size.
- Keep images small by including only what's needed at runtime.
- Store secrets and configuration in environment variables or secret management systems, not in the image.
- Run a single main process per container.
- Use health checks so orchestrators can detect unhealthy containers.
- Don't store persistent data inside the container filesystem; use volumes or external storage.

# Common Interview Questions

### 1. What is Docker?

> Docker is a containerization platform that packages an application and its dependencies into a portable container, ensuring consistent execution across environments.

### 2. What is the difference between an image and a container?

- **Image**: A read-only template used to create containers.
- **Container**: A running instance of an image.

### 3. Why use Docker for microservices?

- Consistent environments
- Easy deployment
- Isolation between services
- Independent scaling
- Better resource utilization than virtual machines

### 4. What is a Docker volume?

A volume provides persistent storage outside the container, so data is preserved even if the container is removed.

### 5. What is Docker Compose?

A tool for defining and running multiple containers (such as APIs, databases, and message brokers) with a single configuration file.

### 6. What is a multi-stage build?

A Docker build technique that uses one stage to compile and publish the application and a second, smaller stage containing only the runtime and published output. This reduces the final image size and improves security.

## Interview Summary

A strong interview answer is:

> **Docker is a containerization platform that packages an application, its runtime, and dependencies into a portable container. In .NET microservices, each service is typically packaged as its own Docker image and deployed as a separate container. Docker Compose is commonly used for local development to run multiple services together, while Kubernetes is often used in production to orchestrate and scale those containers.**
