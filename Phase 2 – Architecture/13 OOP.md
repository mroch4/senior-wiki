# OOP

## Table of content

## What is OOP?

> OOP is a programming paradigm where we structure software around objects that combine data (state) and behavior (methods).

In .NET, classes are the main mechanism for defining objects.

The **four main OOP principles** are:

1. **Encapsulation** — hide internal state and control how it is accessed.
2. **Abstraction** — expose what an object does while hiding implementation details.
3. **Inheritance** — create a new class based on an existing class.
4. **Polymorphism** — allow the same interface/base type to have different implementations.

## Encapsulation

Encapsulation means **keeping an object's internal state protected and controlling how it can be changed**.

```csharp
public class BankAccount
{
    private decimal _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException();

        _balance += amount;
    }

    public decimal GetBalance()
    {
        return _balance;
    }
}
```

The caller can't directly do:

```csharp
account._balance = -1000;
```

because `_balance` is private.

Instead, the class controls changes through `Deposit()`.

### Interview point

Encapsulation isn't simply **"make fields private."**

The important part is that the class **protects its invariants and controls access to its state**.

## Abstraction

Abstraction means **exposing the essential behavior while hiding implementation details**.

For example:

```csharp
public interface IPaymentService
{
    Task PayAsync(decimal amount);
}
```

The consumer only needs to know:

> "I can call `PayAsync`."

It doesn't need to know whether payment is implemented using Stripe, PayPal, a bank API, etc.

```csharp
public class StripePaymentService : IPaymentService
{
    public Task PayAsync(decimal amount)
    {
        // Stripe implementation
    }
}
```

### Encapsulation vs abstraction

This is a common interview question.

**Encapsulation** → protects and controls internal state/implementation.

**Abstraction** → hides unnecessary complexity and exposes a simpler contract.

---

## Inheritance

Inheritance allows one class to derive from another.

```csharp
public class Animal
{
    public void Eat()
    {
        Console.WriteLine("Eating");
    }
}

public class Dog : Animal
{
    public void Bark()
    {
        Console.WriteLine("Barking");
    }
}
```

`Dog` inherits `Eat()` from `Animal`.

In .NET:

```csharp
Dog dog = new Dog();

dog.Eat();
dog.Bark();
```

### Important Senior-level point

Inheritance should **not** automatically be your first choice for code reuse.

Often:

> **Composition over inheritance**

is preferable.

Instead of:

```text
Car → Vehicle → Machine
```

you might compose a `Car` from smaller components such as an engine, transmission, etc.

Inheritance creates tighter coupling between the parent and child classes.

## Polymorphism

Polymorphism means **the same contract can have different implementations**.

For example:

```csharp
public abstract class Payment
{
    public abstract void Process();
}

public class CreditCardPayment : Payment
{
    public override void Process()
    {
        Console.WriteLine("Processing credit card");
    }
}

public class PayPalPayment : Payment
{
    public override void Process()
    {
        Console.WriteLine("Processing PayPal");
    }
}
```

Now:

```csharp
Payment payment = new CreditCardPayment();

payment.Process();
```

The variable is typed as `Payment`, but the **runtime implementation** is `CreditCardPayment.Process()`.

That's runtime polymorphism.

## Interface vs abstract class

Another very common .NET interview question.

### Interface

```csharp
public interface INotification
{
    void Send();
}
```

It primarily defines a **contract**.

A class can implement multiple interfaces:

```csharp
class EmailNotification : INotification, IDisposable
{
}
```

### Abstract class

```csharp
public abstract class Notification
{
    public string Recipient { get; set; }

    public abstract void Send();
}
```

An abstract class can provide:

- state
- constructors
- implemented methods
- abstract methods
- protected members

But C# supports only **single class inheritance**.

### Simple interview answer

> "I'd use an interface when I want to define a capability or contract and allow unrelated classes to implement it. I'd use an abstract class when there is a genuine shared base concept with common state or behavior."

# Interview Tips

Don't just memorize **encapsulation, abstraction, inheritance, polymorphism**. For a senior interview, always connect them to **coupling, maintainability, extensibility, and real-world design decisions**.

Are the four OOP principles enough to create good software?"

> "No. OOP principles help structure code, but they don't automatically produce good design. I'd also consider SOLID principles, composition over inheritance, dependency inversion, separation of concerns, cohesion, coupling, and appropriate use of design patterns."
