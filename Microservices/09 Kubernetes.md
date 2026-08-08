# Kubernetes

## Table of content

1. [What is Kubernetes?](#what-is-kubernetes)
2. [Why Kubernetes exists](#why-kubernetes-exists)
3. [Main responsibilities](#main-responsibilities)
   - [Deploy applications](#1-deploy-applications)
   - [Self-healing](#2-self-healing)
   - [Scaling](#3-scaling)
   - [Load balancing](#4-load-balancing)
   - [Service discovery](#5-service-discovery)
   - [Rolling deployments](#6-rolling-deployments)
   - [Rollbacks](#7-rollbacks)
   - [Configuration management](#8-configuration-management)
   - [Scheduling](#9-scheduling)
4. [Kubernetes architecture](#kubernetes-architecture)
   - [Control Plane](#control-plane)
   - [Worker Nodes](#worker-nodes)
5. [Important Kubernetes objects](#important-kubernetes-objects)
   - [Pod](#pod)
   - [Deployment](#deployment)
   - [Service](#service)
   - [Ingress](#ingress)
6. [Example in a .NET microservices application](#example-in-a-net-microservices-application)
7. [Kubernetes vs Docker](#kubernetes-vs-docker)
8. [When do you need Kubernetes?](#when-do-you-need-kubernetes)
9. [Interview Tips](#interview-tips)

## What is Kubernetes?

> Kubernetes (often abbreviated as **K8s**) is a **container orchestration platform**. Its job is to **deploy, manage, scale, and heal containerized applications automatically**.

If Docker packages your application into a container, Kubernetes manages **hundreds or thousands of those containers** running across many servers.

## Why Kubernetes exists

Imagine you have a microservices application with:

- User Service
- Product Service
- Order Service
- Payment Service
- Notification Service

Each service runs in multiple Docker containers.

❌ Without Kubernetes, you would have to manually:

- Start containers
- Restart crashed containers
- Scale during high traffic
- Route requests
- Roll out new versions
- Roll back failed deployments

Kubernetes automates all of this.

## Main responsibilities

### 1. Deploy applications

You describe the desired state:

> "Run 5 instances of the Order API."

Kubernetes makes sure exactly 5 are always running.

Desired State: Order API has 5 replicas - Kubernetes creates 5 Pods.

### 2. Self-healing

If a container crashes:

```
Pod 3 crashes
 |
Kubernetes detects it
 |
Starts a replacement automatically
```

No manual intervention is needed.

### 3. Scaling

If traffic increases:

```
3 Order API Pods
 |
High load
 |
Scale to 10 Pods
```

Scaling can be:

- Manual
- Automatic based on CPU or memory
- Automatic based on custom metrics (requests/sec, queue length, etc.)

### 4. Load balancing

Suppose there are 5 Order API Pods.

```
Client
 |
Service
 |
Pod 1
Pod 2
Pod 3
Pod 4
Pod 5
```

Kubernetes distributes requests across healthy Pods.

### 5. Service discovery

Instead of calling an IP address:

```
http://10.5.4.8:5000
```

Applications call:

```
http://order-service
```

Kubernetes resolves the correct Pods automatically.

This is especially useful because Pods come and go frequently.

### 6. Rolling deployments

Suppose you're deploying version 2.

Without Kubernetes:

```
Stop v1
 |
Start v2
 |
Downtime
```

With Kubernetes:

```
v1 v1 v1
 |
v1 v1 v2
 |
v1 v2 v2
 |
v2 v2 v2
```

Users experience little or no downtime.

### 7. Rollbacks

If version 2 has a bug:

```
Deploy v2
 |
Errors increase
 |
kubectl rollout undo
 |
Back to v1
```

### 8. Configuration management

Instead of hardcoding:

```json
ConnectionString=...
```

Kubernetes provides:

- ConfigMaps (non-sensitive configuration)
- Secrets (passwords, API keys, certificates)

Applications receive these as environment variables or mounted files.

### 9. Scheduling

Suppose you have 20 servers.

```
Server A
Server B
Server C
...
```

Kubernetes decides:

- which server should run each Pod
- how to spread workloads
- whether the server has enough CPU/RAM
- whether Pods should avoid or prefer certain nodes

## Kubernetes architecture

```
                Control Plane
            --------------------
            API Server
            Scheduler
            Controller Manager
            etcd
            --------------------
                    │
      ------------------------------
      │             │              │

   Worker        Worker        Worker
    Node          Node          Node

   Pod A         Pod C          Pod F
   Pod B         Pod D
                 Pod E
```

### Control Plane

The "brain" of the cluster.

Responsible for:

- Scheduling
- Monitoring
- Desired state
- API
- Cluster management

### Worker Nodes

Machines that actually run your application containers.

---

## Important Kubernetes objects

### Pod

Smallest deployable unit.

```
Pod

└── Container
```

Usually one application container per Pod.

### Deployment

Says:

> "Keep 5 Pods running."

If one dies, Kubernetes creates another.

### Service

Provides a stable network endpoint.

```
Clients
 |
Service
 |
Pods
```

Pods change. Service stays the same.

### Ingress

Handles incoming HTTP/HTTPS traffic.

```
Internet
 |
Ingress
 |
Orders API

Payments API

Users API
```

## Example in a .NET microservices application

```
Order API

Deployment
Replica Count = 3
 |
Pod 1
Pod 2
Pod 3
 |
Service
 |
Ingress
 |
Internet
```

### If Pod 2 crashes:

```
Pod 2 dies
 |
Kubernetes notices
 |
Creates new Pod 2
```

### If Black Friday arrives:

```
CPU > 80%
 |
Autoscaler
 |
3 Pods
 |
12 Pods
```

### When traffic drops:

```
12 Pods
 |
Scale back
 |
3 Pods
```

## Kubernetes vs Docker

| Docker                  | Kubernetes                                       |
| ----------------------- | ------------------------------------------------ |
| Builds container images | Orchestrates containers                          |
| Runs containers         | Manages many containers across multiple machines |
| Single host             | Cluster of hosts                                 |
| Manual scaling          | Automatic scaling                                |
| No self-healing         | Self-healing                                     |
| Limited networking      | Built-in service discovery and load balancing    |

A common analogy is:

- **Docker** is like shipping containers.
- **Kubernetes** is the port authority that decides where containers go, monitors them, replaces damaged ones, and routes traffic efficiently.

## When do you need Kubernetes?

Kubernetes is a good fit when you have:

- Many microservices.
- High availability requirements.
- Automatic scaling needs.
- Frequent deployments.
- Multiple servers or cloud nodes.
- Teams practicing CI/CD and DevOps.

For a small application with one or two services, Kubernetes is often unnecessary complexity. A single VM, Docker Compose, or a managed container service may be simpler.

# Interview Tips

- A concise definition that interviewers like is: **"Kubernetes is a container orchestration platform that maintains the desired state of containerized applications."**
- Be ready to explain the relationship between **Docker → Kubernetes → Cloud**: Docker creates containers, Kubernetes orchestrates them, and cloud providers (such as managed Kubernetes services) supply the infrastructure.
- Know the core Kubernetes objects: **Pod, Deployment, Service, ConfigMap, Secret, Ingress**, and be able to describe each in one sentence.
- A common follow-up is: **"What happens when a Pod crashes?"** Explain that Kubernetes detects the failure, creates a replacement Pod, and the Service continues routing traffic to healthy Pods.

Why would you use Kubernetes instead of just Docker?

> Docker packages and runs containers, but it doesn't manage them at scale. Kubernetes orchestrates containers across multiple machines by handling scheduling, self-healing, load balancing, service discovery, rolling deployments, rollbacks, and automatic scaling, making it well suited for production environments running distributed applications.
