There are **23 GoF (Gang of Four) design patterns**, divided into 3 categories.

### 1. Creational — 5

Concerned with **object creation**.

| #   | Pattern                                         | Main idea                                    |
| --- | ----------------------------------------------- | -------------------------------------------- |
| 1   | **Abstract Factory**                            | Create families of related objects           |
| 2   | [Builder](/Patterns/Builder.md)                 | Build complex objects step by step           |
| 3   | [Factory Method](/Patterns/Factory%20method.md) | Let subclasses decide which object to create |
| 4   | Prototype                                       | Create objects by cloning existing objects   |
| 5   | [Singleton](/Patterns/Singleton.md)             | Ensure only one instance exists              |

### 2. Structural — 7

Concerned with **how classes and objects are composed**.

| #   | Pattern                             | Main idea                                             |
| --- | ----------------------------------- | ----------------------------------------------------- |
| 6   | **Adapter**                         | Make incompatible interfaces work together            |
| 7   | Bridge                              | Separate abstraction from implementation              |
| 8   | Composite                           | Treat individual objects and groups uniformly         |
| 9   | [Decorator](/Patterns/Decorator.md) | Add behavior by wrapping an object                    |
| 10  | [Facade](/Patterns/Facade.md)       | Provide a simple interface to a complex subsystem     |
| 11  | Flyweight                           | Share objects to reduce memory usage                  |
| 12  | **Proxy**                           | Provide a substitute/control access to another object |

### 3. Behavioral — 11

Concerned with **communication and responsibility between objects**.

| #   | Pattern                                                             | Main idea                                                  |
| --- | ------------------------------------------------------------------- | ---------------------------------------------------------- |
| 13  | [Chain of Responsibility](/Patterns/Chain%20of%20Responsibility.md) | Pass a request through a chain of handlers                 |
| 14  | [Command](/Patterns/Command.md)                                     | Encapsulate a request as an object                         |
| 15  | Interpreter                                                         | Define and evaluate a language/grammar                     |
| 16  | Iterator                                                            | Traverse a collection without exposing its internals       |
| 17  | [Mediator](/Patterns/Mediator.md)                                   | Centralize communication between objects                   |
| 18  | Memento                                                             | Capture and restore an object's state                      |
| 19  | [Observer](/Patterns/Observer.md)                                   | Notify dependent objects when state changes                |
| 20  | **State**                                                           | Change behavior when internal state changes                |
| 21  | [Strategy](/Patterns/Strategy.md)                                   | Encapsulate interchangeable algorithms                     |
| 22  | **Template Method**                                                 | Define algorithm structure while allowing steps to vary    |
| 23  | Visitor                                                             | Add operations to object structures without modifying them |

### 4. Architectural

| #   | Pattern                                                                                  | Main idea |
| --- | ---------------------------------------------------------------------------------------- | --------- |
|     | [CQRS](/Phase%202%20–%20Architecture/09%20CQRS.md)                                       |           |
|     | [Dependency Injection](/Phase%201.2%20-%20ASP.NET%20Core/02%20Dependency%20Injection.md) |           |
|     | [Outbox](Outbox%20Pattern.md)                                                            |           |
|     | [Repository](/Phase%202%20–%20Architecture/12%20Repository%20Pattern.md)                 |           |
|     | [Saga](/Patterns/Saga%20Pattern.md)                                                      |           |
|     | [Specification](/Phase%202%20–%20Architecture/14%20Specification%20Pattern.md)           |           |
|     | [Unit of Work](/Phase%202%20–%20Architecture/13%20Unit%20of%20Work.md)                   |           |

A useful interview distinction is:

> **GoF = exactly 23 classic patterns. Architectural are commonly called patterns, but they are not part of the original 23 GoF patterns.**
