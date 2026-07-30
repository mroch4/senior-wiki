# Value/Reference types

# Memory Management (Stack, Heap, CLR, GC)

## Table of content

# Question 1

> **What is the difference between the Stack and the Heap?**

Don't give me textbook definitions.

Imagine you're explaining it to a junior developer.

I'd like you to cover:

- What is stored on the stack?
- What is stored on the heap?
- Why is the stack so fast?
- Why is the heap slower?
- How does this relate to value types and reference types?

Take your time.

## Before you answer, here's one thing that confuses many developers

Consider:

```csharp
class Person
{
    public string Name { get; set; }
}

void Foo()
{
    int x = 10;
    Person p = new Person();
}
```

Many people say:

> "`int` is on the stack and `Person` is on the heap."

That answer is **incomplete**.

The more accurate description is:

```
Stack

x = 10

p ───────────────┐
                 │
                 ▼

Heap

Person object
Name reference
```

The **reference** `p` is on the stack (because it's a local variable), while the actual `Person` object lives on the heap.

That distinction is one of the first things interviewers listen for.

### A small hint

When answering, don't think in terms of:

> "Value types = stack"

because that's not always true.

For example:

```csharp
class Order
{
    public int Id;
}
```

Where does `Id` live?

We'll get into that after your answer—that question catches a surprising number of experienced developers.
