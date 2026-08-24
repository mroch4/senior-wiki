# Practical project to exercise Microsevices

## eComm system:

```
Client (Blazor WASM/Angular/React)
 |
API Gateway
 ├── Product API -> Product DB
 ├── Order API -> Order DB
 ├── Inventory API -> Inventory DB
 ├── Payment API -> Payment DB
 ├── Shipping API -> Shipping DB
 ├── Notification API -> Notification DB
```

## Technologies

.NET 10
Minimal APIs (no Controllers)
Polly for resilience
Rabbit MQ
SQL Database
MediatR
CQRS
Outbox Pattern
Clean architecture
Docker

## Questions

> Provide a .NET project structure

> Which service should communicate over HTTP and which should use message broker

> What types of actions
