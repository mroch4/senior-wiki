# Strategy Pattern (Behavioral)

> Encapsulate interchangeable algorithms - define a family of algorithms, put each algorithm behind a common interface, and **switch between them without changing the client**.

## Simple .NET example

Suppose your application supports different payment methods.

## Strategy interface

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}
```

## Concrete strategies

```csharp
public class CreditCardStrategy : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paid {amount} using credit card");
    }
}
```

```csharp
public class PayPalStrategy : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paid {amount} using PayPal");
    }
}
```

```csharp
public class BankTransferStrategy : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paid {amount} using bank transfer");
    }
}
```

## Context

The class that uses the strategy:

```csharp
public class PaymentService
{
    private readonly IPaymentStrategy _strategy;

    public PaymentService(IPaymentStrategy strategy)
    {
        _strategy = strategy;
    }

    public void ProcessPayment(decimal amount)
    {
        _strategy.Pay(amount);
    }
}
```

## Usage

```csharp
var payment = new PaymentService(new CreditCardStrategy());

payment.ProcessPayment(100);
```

Switch strategy:

```csharp
var payment = new PaymentService(new PayPalStrategy());

payment.ProcessPayment(100);
```

The structure is:

```
       PaymentService
             ↓
      IPaymentStrategy
      ↙      ↓      ↘
CreditCard  PayPal  BankTransfer
```

The `PaymentService` doesn't care **how** payment is processed.

## Why use Strategy?

Without Strategy, you might end up with:

```csharp
if (paymentType == "CreditCard")
{
    // ...
}
else if (paymentType == "PayPal")
{
    // ...
}
else if (paymentType == "BankTransfer")
{
    // ...
}
```

As the number of algorithms grows, this becomes difficult to maintain.

Strategy moves each algorithm into its own class.

## Strategy with .NET DI

This is where Strategy becomes especially useful in ASP.NET Core.

You can register:

```csharp
services.AddScoped<CreditCardStrategy>();
services.AddScoped<PayPalStrategy>();
services.AddScoped<BankTransferStrategy>();
```

Then select the appropriate strategy based on the request/payment type.
