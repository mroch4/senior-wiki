# Realistic .NET API + Docker + Kubernetes example

## Table of content

1. [Dockerfile](#1-dockerfile)
2. [ConfigMap](#2-configmap)
3. [Secret](#3-secret)
4. [Deployment](#4-deployment)
5. [Service](#5-service)
6. [Ingress](#6-ingress)
7. [Put everything together](#7-put-everything-together)
8. [One important distinction](#one-important-distinction)

## 1. Dockerfile

This builds your .NET API into a container image:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderApi.dll"]
```

You then build the image:

```bash
docker build -t myregistry/order-api:1.0 .
```

And push it to a container registry:

```bash
docker push myregistry/order-api:1.0
```

## 2. ConfigMap

For **non-sensitive** configuration:

```yaml
apiVersion: v1
kind: ConfigMap

metadata:
  name: order-api-config

data:
  ASPNETCORE_ENVIRONMENT: "Production"
  PaymentServiceUrl: "http://payment-service"
```

Think of it as:

```
ConfigMap
 ↓
Application configuration
```

Don't put passwords here.

## 3. Secret

For **sensitive** values.

```yaml
apiVersion: v1
kind: Secret

metadata:
  name: order-api-secret

type: Opaque

stringData:
  ConnectionStrings__DefaultConnection: "Server=sql;Database=Orders;User Id=app;Password=MyPassword;"
```

In a real Azure production setup, you'd generally want a stronger secret-management approach such as **Azure Key Vault + a Kubernetes integration**, rather than committing credentials directly into YAML.

## 4. Deployment

This is where Kubernetes says:

> "I want 3 instances of my Order API running."

```yaml
apiVersion: apps/v1
kind: Deployment

metadata:
  name: order-api

spec:
  replicas: 3

  selector:
    matchLabels:
      app: order-api

  template:
    metadata:
      labels:
        app: order-api

    spec:
      containers:
        - name: order-api

          image: myregistry/order-api:1.0

          ports:
            - containerPort: 8080

          envFrom:
            - configMapRef:
                name: order-api-config

            - secretRef:
                name: order-api-secret

          resources:
            requests:
              cpu: "100m"
              memory: "128Mi"

            limits:
              cpu: "500m"
              memory: "512Mi"
```

This produces:

```
            Deployment
                │
            replicas: 3
                │
   ┌────────────┼────────────┐
   ▼            ▼            ▼
 Pod 1        Pod 2        Pod 3
   │            │            │
Order API    Order API    Order API
```

If Pod 2 crashes:

```
Pod 2 💥
 ↓
Kubernetes detects failure
 ↓
Creates replacement Pod
 ↓
3 Pods again
```

## 5. Service

Pods are temporary. Their IP addresses can change.

The **Service** gives them a stable network address.

```yaml
apiVersion: v1
kind: Service

metadata:
  name: order-api

spec:
  selector:
    app: order-api

  ports:
    - port: 80
      targetPort: 8080

  type: ClusterIP
```

Now another Kubernetes service can call: `http://order-api`. The Service distributes traffic:

```
          order-api
           Service
              │
  ┌───────────┼───────────┐
  ▼           ▼           ▼
Pod 1       Pod 2       Pod 3
```

## 6. Ingress

Now suppose you want users on the internet to access your API.

The Ingress can route: `https://api.mycompany.com/orders` to the `order-api` Service.

Example:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress

metadata:
  name: order-api-ingress

spec:
  ingressClassName: nginx

  rules:
    - host: api.mycompany.com

      http:
        paths:
          - path: /orders
            pathType: Prefix

            backend:
              service:
                name: order-api
                port:
                  number: 80
```

The flow becomes:

```text
           Internet
              │
     api.mycompany.com/orders
              │
           Ingress
              │
          order-api
           Service
              │
  ┌───────────┼───────────┐
  ▼           ▼           ▼
Pod 1       Pod 2       Pod 3
```

## 7. Put everything together

You can have a directory like:

```
kubernetes/
│
├── configmap.yaml
├── secret.yaml
├── deployment.yaml
├── service.yaml
└── ingress.yaml
```

Then:

```bash
kubectl apply -f kubernetes/
```

Kubernetes creates everything.

## One important distinction

**Dockerfile =/= Kubernetes YAML**

The Dockerfile answers: `How do I package my application?`

Kubernetes YAML answers: `How should this application run in my cluster?`

So your deployment pipeline typically looks like:

```
.NET source code
 |
Dockerfile
 |
Docker Image
 |
Container Registry
 |
Kubernetes Deployment
 |
Pods
 |
Kubernetes Service
 |
Ingress
 |
Users
```
