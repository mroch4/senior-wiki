# Algorithms

| Topic          | What you should remember                   |
| -------------- | ------------------------------------------ |
| Binary search  | Left / middle / right                      |
| Duplicates     | `HashSet<T>`                               |
| FizzBuzz       | `%` modulo                                 |
| Frequencies    | `Dictionary<char, int>`                    |
| Merge arrays   | Two pointers                               |
| Palindrome     | Two pointers                               |
| Parentheses    | `Stack<char>`                              |
| Reverse string | Loop backwards / `Array.Reverse`           |
| Two Sum        | `Dictionary`                               |
| OOP            | Inheritance + polymorphism + encapsulation |

# BankAccount hierarchy

Here's a simple example that demonstrates several important OOP concepts.

### Base class

```csharp
class BankAccount
{
    public string Owner { get; }
    public decimal Balance { get; protected set; }

    public BankAccount(string owner, decimal initialBalance)
    {
        Owner = owner;
        Balance = initialBalance;
    }

    public virtual void Withdraw(decimal amount)
    {
        if (amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
            return;
        }

        Balance -= amount;
    }

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }
}
```

Now we can inherit from it:

```csharp
class SavingsAccount : BankAccount
{
    public SavingsAccount(string owner, decimal initialBalance)
        : base(owner, initialBalance)
    {
    }

    public override void Withdraw(decimal amount)
    {
        if (Balance - amount < 100)
        {
            Console.WriteLine("Savings account must keep £100 minimum");
            return;
        }

        Balance -= amount;
    }
}
```

And another type:

```csharp
class CurrentAccount : BankAccount
{
    public CurrentAccount(string owner, decimal initialBalance)
        : base(owner, initialBalance)
    {
    }

    public override void Withdraw(decimal amount)
    {
        Balance -= amount;
    }
}
```

Usage:

```csharp
BankAccount account1 = new SavingsAccount("John", 500);

BankAccount account2 = new CurrentAccount("Jane", 500);

account1.Withdraw(450);
account2.Withdraw(450);

Console.WriteLine(account1.Balance);
Console.WriteLine(account2.Balance);
```

The important OOP concepts here are:

**Inheritance**

```csharp
SavingsAccount : BankAccount
```

`SavingsAccount` inherits from `BankAccount`.

**Encapsulation**

```csharp
public decimal Balance { get; protected set; }
```

Code outside the class can read the balance, but can't directly change it.

**Polymorphism**

```csharp
BankAccount account1 = new SavingsAccount(...);
```

Even though the variable is `BankAccount`, C# calls the overridden `Withdraw()` from `SavingsAccount`.

**Abstraction**

The base `BankAccount` provides common functionality, while specific account types can implement their own behavior.
