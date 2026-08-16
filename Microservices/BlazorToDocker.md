# Containerizing a Blazor app

Containerizing a Blazor app (especially a Blazor Server app or a hosted Blazor WebAssembly app) and deploying it to Azure App Service is a fairly common migration. Since you're already deploying with Azure DevOps YAML, the main changes are:

1. Add a `Dockerfile`.
2. Build and push the image to a container registry (Azure Container Registry or Docker Hub).
3. Configure Azure App Service to run the container instead of deploying files.

## Architecture

Current:

```
Azure DevOps
 |
dotnet publish
 |
Azure App Service
```

Containerized:

```
Azure DevOps
 |
docker build
 |
Azure Container Registry
 |
Azure App Service (Linux)
```

# Step 1. Create a Dockerfile

For a .NET 8 Blazor Server application:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MyBlazorApp.dll"]
```

Replace `MyBlazorApp.dll` with your project output.

# Step 2. Create an Azure Container Registry

```
az acr create \
    --resource-group rg-demo \
    --name mycompanyacr \
    --sku Basic
```

Example image name:

```
mycompanyacr.azurecr.io/blazorapp:1.0.0
```

# Step 3. Update Azure DevOps pipeline

Instead of:

```
dotnet restore
dotnet build
dotnet publish
AzureWebApp@1
```

you'll have:

```
dotnet restore
docker build
docker push
AzureWebAppContainer
```

Example:

```yaml
trigger:
  - main

variables:
  imageName: blazorapp
  tag: $(Build.BuildId)

steps:
  - task: Docker@2
    displayName: Build image
    inputs:
      command: build
      Dockerfile: Dockerfile
      repository: $(imageName)
      tags: |
        $(tag)

  - task: Docker@2
    displayName: Push image
    inputs:
      command: push
      repository: $(imageName)
      containerRegistry: MyACRServiceConnection
      tags: |
        $(tag)

  - task: AzureWebAppContainer@1
    inputs:
      azureSubscription: AzureConnection
      appName: my-blazor-app
      containers: mycompanyacr.azurecr.io/blazorapp:$(tag)
```

# Step 4. Configure Azure App Service

Instead of publishing files, configure:

```
App Service
    Runtime
        Docker Container
```

Point it to:

```
mycompanyacr.azurecr.io/blazorapp:latest
```

or a versioned tag.

# Step 5. App settings

Any values previously in Azure App Service configuration remain environment variables inside the container. For example:

```
ConnectionStrings__MainDb
ApiSettings__BaseUrl
Logging__LogLevel__Default
```

.NET reads these automatically.

# Step 6. Logging

Don't write logs to files inside the container.

Instead:

```csharp
builder.Logging.AddConsole();
```

Then view logs with:

```
az webapp log tail
```

or through Azure App Service Log Stream.

# Step 7. Health checks

Expose a health endpoint:

```csharp
builder.Services.AddHealthChecks();

app.MapHealthChecks("/health");
```

Configure App Service Health Check to use:

```
/health
```

# Benefits

- Same runtime everywhere (local, CI, Azure).
- No dependency on the App Service's installed .NET runtime.
- Easier rollback by switching image tags.
- Faster, more predictable deployments.
- Simpler path to Azure Container Apps or AKS if you outgrow App Service.

## Typical Azure DevOps flow

```text
Git Push
    │
    ▼
Azure DevOps
    │
    ├── dotnet restore
    ├── dotnet test
    ├── docker build
    ├── docker tag
    ├── docker push (ACR)
    ▼
Azure Container Registry
    ▼
Azure App Service (Linux)
```

# Interview Tips

If asked how you'd containerize an existing Azure App Service deployment, a strong answer is:

> "I'd create a multi-stage Dockerfile to keep the runtime image small, build and publish the Blazor app in the SDK image, then copy the published output into the ASP.NET runtime image. I'd store the image in Azure Container Registry, update the Azure DevOps YAML pipeline to build and push the image, and configure Azure App Service for Linux to pull from ACR. I'd keep configuration in App Service environment variables, expose a health endpoint, and log to stdout/stderr so Azure can collect container logs. This gives consistent deployments, versioned images, and an easier migration path to orchestrators like Azure Container Apps or AKS."
