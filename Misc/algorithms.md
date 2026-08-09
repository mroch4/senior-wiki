# ALGORITHMS

## 1. Reverse a string

```csharp
string ReverseString(string input)
{
    char[] chars = input.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
}

// Example
Console.WriteLine(ReverseString("hello"));
// olleh
```

A common alternative using a loop:

```csharp
string ReverseString(string input)
{
    string result = "";

    for (int i = input.Length - 1; i >= 0; i--)
    {
        result += input[i];
    }

    return result;
}
```

---

## 2. Check if a string is a palindrome

A palindrome reads the same forwards and backwards.

```csharp
bool IsPalindrome(string input)
{
    int left = 0;
    int right = input.Length - 1;

    while (left < right)
    {
        if (input[left] != input[right])
            return false;

        left++;
        right--;
    }

    return true;
}

// Examples
Console.WriteLine(IsPalindrome("level")); // True
Console.WriteLine(IsPalindrome("hello")); // False
```

**Key idea:** compare the first and last characters, then move towards the middle.

---

## 3. Two Sum

Given an array and a target, find two numbers that add up to the target.

```csharp
int[] TwoSum(int[] numbers, int target)
{
    var dictionary = new Dictionary<int, int>();

    for (int i = 0; i < numbers.Length; i++)
    {
        int needed = target - numbers[i];

        if (dictionary.ContainsKey(needed))
        {
            return new int[] { dictionary[needed], i };
        }

        dictionary[numbers[i]] = i;
    }

    return Array.Empty<int>();
}

// Example
int[] result = TwoSum(new int[] { 2, 7, 11, 15 }, 9);

// result = [0, 1]
```

Why?

```text
2 + 7 = 9
```

The dictionary remembers numbers we've already seen.

---

## 4. Count character frequencies

This is very similar to the task you were working on earlier.

```csharp
Dictionary<char, int> CountCharacters(string input)
{
    var dictionary = new Dictionary<char, int>();

    foreach (char letter in input)
    {
        if (dictionary.ContainsKey(letter))
        {
            dictionary[letter]++;
        }
        else
        {
            dictionary[letter] = 1;
        }
    }

    return dictionary;
}
```

Example:

```csharp
var result = CountCharacters("hello");
```

Result:

```text
h -> 1
e -> 1
l -> 2
o -> 1
```

Notice the important syntax:

```csharp
dictionary[letter]++;
```

not:

```csharp
dictionary[letter].value++
```

---

## 5. Find duplicates using `HashSet`

```csharp
List<int> FindDuplicates(int[] numbers)
{
    var seen = new HashSet<int>();
    var duplicates = new List<int>();

    foreach (int number in numbers)
    {
        if (!seen.Add(number))
        {
            duplicates.Add(number);
        }
    }

    return duplicates;
}
```

Example:

```csharp
var result = FindDuplicates(
    new int[] { 1, 2, 3, 2, 4, 5, 3 }
);
```

Result:

```text
2, 3
```

The clever part is:

```csharp
seen.Add(number)
```

`HashSet.Add()` returns:

- `true` → item was not already there
- `false` → item already existed

So:

```csharp
if (!seen.Add(number))
```

means **"if we've already seen this number..."**

---

## 6. Merge two sorted arrays

For example:

```text
[1, 3, 5]
[2, 4, 6]

Result:
[1, 2, 3, 4, 5, 6]
```

```csharp
int[] MergeSortedArrays(int[] first, int[] second)
{
    var result = new List<int>();

    int i = 0;
    int j = 0;

    while (i < first.Length && j < second.Length)
    {
        if (first[i] < second[j])
        {
            result.Add(first[i]);
            i++;
        }
        else
        {
            result.Add(second[j]);
            j++;
        }
    }

    while (i < first.Length)
    {
        result.Add(first[i]);
        i++;
    }

    while (j < second.Length)
    {
        result.Add(second[j]);
        j++;
    }

    return result.ToArray();
}
```

The important concept is using **two pointers**:

```text
first:  [1, 3, 5]
         ↑
         i

second: [2, 4, 6]
         ↑
         j
```

Compare the two values and take the smaller one.

---

## 7. Binary search

Binary search requires a **sorted array**.

```csharp
int BinarySearch(int[] numbers, int target)
{
    int left = 0;
    int right = numbers.Length - 1;

    while (left <= right)
    {
        int middle = left + (right - left) / 2;

        if (numbers[middle] == target)
            return middle;

        if (numbers[middle] < target)
            left = middle + 1;
        else
            right = middle - 1;
    }

    return -1;
}
```

Example:

```csharp
int[] numbers = { 1, 3, 5, 7, 9 };

Console.WriteLine(BinarySearch(numbers, 7));
```

Output:

```text
3
```

Because:

```text
Index:  0  1  2  3  4
Value:  1  3  5  7  9
                  ↑
```

Instead of checking every number, binary search repeatedly cuts the search area in half.

---

## 8. FizzBuzz

Classic interview question.

Print:

- `"Fizz"` for multiples of 3
- `"Buzz"` for multiples of 5
- `"FizzBuzz"` for multiples of both
- Otherwise print the number

```csharp
void FizzBuzz(int n)
{
    for (int i = 1; i <= n; i++)
    {
        if (i % 3 == 0 && i % 5 == 0)
        {
            Console.WriteLine("FizzBuzz");
        }
        else if (i % 3 == 0)
        {
            Console.WriteLine("Fizz");
        }
        else if (i % 5 == 0)
        {
            Console.WriteLine("Buzz");
        }
        else
        {
            Console.WriteLine(i);
        }
    }
}
```

Example:

```csharp
FizzBuzz(15);
```

Output:

```text
1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz
```

**Important:** check `FizzBuzz` first, otherwise 15 would match the `3` condition before reaching the `5` condition.

---

## 9. Validate parentheses using `Stack<char>`

For example:

```text
"()[]{}"       -> true
"([{}])"       -> true
"([)]"         -> false
```

```csharp
bool IsValid(string input)
{
    var stack = new Stack<char>();

    foreach (char c in input)
    {
        if (c == '(' || c == '[' || c == '{')
        {
            stack.Push(c);
        }
        else
        {
            if (stack.Count == 0)
                return false;

            char opening = stack.Pop();

            if (c == ')' && opening != '(')
                return false;

            if (c == ']' && opening != '[')
                return false;

            if (c == '}' && opening != '{')
                return false;
        }
    }

    return stack.Count == 0;
}
```

The key concept is **LIFO**:

> Last In, First Out.

For:

```text
([{}])
```

the stack behaves roughly like:

```text
(
(
[
(
[
{
```

Then the closing brackets must match the most recently opened bracket.

---

## 10. OOP — BankAccount hierarchy

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

---

## What I'd focus on for your assessment

If this is a **.NET/C# technical assessment**, I'd make sure you're comfortable writing these without looking them up:

| Topic          | What you should remember                   |
| -------------- | ------------------------------------------ |
| Reverse string | Loop backwards / `Array.Reverse`           |
| Palindrome     | Two pointers                               |
| Two Sum        | `Dictionary`                               |
| Frequencies    | `Dictionary<char, int>`                    |
| Duplicates     | `HashSet<T>`                               |
| Merge arrays   | Two pointers                               |
| Binary search  | Left / middle / right                      |
| FizzBuzz       | `%` modulo                                 |
| Parentheses    | `Stack<char>`                              |
| OOP            | Inheritance + polymorphism + encapsulation |

The **highest-value ones to practise by hand** are probably **Dictionary, HashSet, Stack, two pointers, and binary search**. Those are the patterns that let you solve many variations rather than memorizing individual solutions.

---
