# C# Interview Series — Part 1: The 12 Questions That Decide the First 15 Minutes

[source](https://medium.com/@thecurlybrace/c-interview-series-part-1-the-12-questions-that-decide-the-first-15-minutes-4f784c33a576)

The first 15 minutes of any C# interview are basically the same 10–15 questions.

Different companies, different panels, different years — same questions. They’re filters. If you fumble these, the interviewer mentally checks out before you even reach the “tell me about your project” part.

This is Part 1 of a series where I’m covering the C# interview pipeline end to end, broken down so you can read each part in 5 minutes during your commute. No fluff, no 3,000-word essays, no “in this article we will discuss” filler.

Let’s get into the 12 questions that decide the first 15 minutes.

## 1. `is` vs `as` — what's the difference?

- `is` → checks the type, returns true/false.
- `as` → tries to cast, returns the object or null.

```csharp
if (obj is string) // just checking
    Console.WriteLine("yes");
```

```csharp
string s = obj as string; // try to cast; null if fails
```

**Why interviewers ask:** They want to know if you understand safe casting vs throwing exceptions. The trap they love: _'Can I write obj as int?'_ - No. `as` only works with reference types or nullable types, because the failure case returns `null`.

---

## 2. What does the `using` keyword do?

Two different things with the same keyword:

- using directive — imports a namespace:

```csharp
using System;
```

- using statement — auto-disposes an IDisposable:

```csharp
using var conn = new SqlConnection(connStr);
// Dispose() called automatically when scope ends
```

**What to actually say:** _'Either it imports a namespace, or it ensures an IDisposable gets disposed without writing try-finally.'_ Drops mic. Move on.

---

## 3. `throw` vs `throw ex` — which one and why?

Always `throw`.
Never `throw ex`.

```csharp
catch (Exception ex)
{
    LogError(ex);
    throw;      // ✅ keeps original stack trace
    // throw ex;   ❌ resets the stack trace — you lose where it came from
}
```

**Why this matters:** This is the single question where a wrong answer instantly signals _'this person hasn’t debugged a real production bug.'_ I’ve literally watched interviewers cross candidates off the list over this one.

---

## 4. `typeof` vs `GetType()`

- `typeof(Person)` → works on the type itself. Compile-time.
- `person.GetType()` → works on an instance. Runtime.

```csharp
  Animal a = new Dog();
  typeof(Animal); // Animal
  a.GetType();    // Dog ✅ actual runtime type
```

**Why interviewers ask:** They’re testing whether you understand the difference between static (compile-time) and dynamic (runtime) type info. This question is also a springboard into reflection - be ready for that follow-up.

---

## 5. `string` vs `StringBuilder` — when to use what?

- `string` is immutable. Every change creates a new string.
- `StringBuilder` is mutable. Modifies in place.

```csharp
// BAD - creates ~1000 string objects, all garbage
string result = "";
for (int i = 0; i < 1000; i++) result += i;
```

```csharp
// GOOD - one buffer
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++) sb.Append(i);
```

**Rule of thumb:** Concatenating in a loop or building large strings? `StringBuilder`. A handful of concatenations? Plain `string` is fine and more readable.

---

## 6. Why does `string` behave like a value type if it's a reference type?

Because **it’s immutable**. You can’t change a string in place — every “modification” returns a new one. Two variables can point to the same string, but you can never accidentally mutate one through the other.

That’s also why == on strings compares _values_, not references. C# overloads it specifically because the value-type-like behavior is what developers actually expect.

**Why interviewers ask:** It’s a deceptively simple question that exposes whether you really understand value vs reference semantics or just memorized the definitions.

## 7. What’s `dynamic` and when would you use it?

`dynamic` tells the compiler: _'Skip type checking for this variable. Trust me, I'll handle it at runtime.'_

```csharp
dynamic x = "hello";
x.SomeMethodThatDoesntExist(); // compiles fine; explodes at runtime
```

**Real-world use cases:** COM interop (Excel automation), calling into IronPython, working with JSON where you don’t want to create DTOs, or `ExpandoObject`.

Honest take: 99% of the time, don’t use it. It’s a footgun. But know it exists for the 1% case.

---

## 8. `double` vs `decimal` — which one for what?

- `double` → fast, approximate (binary floating point). Scientific, graphics, ML.
- `decimal` → slower, exact (base-10). **Money. Always.**

```csharp
double  d = 0.1 + 0.2; // 0.30000000000000004 😱
decimal m = 0.1m + 0.2m; // 0.3 ✅
```

**Why interviewers ask:** Quick filter for backend developers. If you say _'use double for money'_, you’ve just told them you’ve never built a billing system.

---

## 9. What’s the `checked` keyword for?

It forces arithmetic overflow to throw, instead of silently wrapping around.

```csharp
checked {
  int x = int.MaxValue + 1; // throws OverflowException
}
unchecked {
  int x = int.MaxValue + 1; // silently wraps to int.MinValue
}
```

**When you’d actually use it:** Financial calculations, anywhere a wrong number is worse than a crash. Default in C# is `unchecked`, which surprises people.

---

## 10. What are expression-bodied members?

A shorthand for methods, properties, and constructors that fit in a single expression.

Before:

```csharp
public string FullName {
    get { return FirstName + " " + LastName; }
}
```

After:

```csharp
public string FullName => FirstName + " " + LastName;
```

Works on methods too:

```csharp
public int Square(int x) => x * x;
```

**Why it matters in interviews:** They want to see you write modern, idiomatic C# — not 2010-era code. Sprinkling these into your live coding demo signals _'this person keeps up.'_

---

## 11. What is pattern matching?

A way to check a value’s shape and extract data in one step.

Type pattern:

```csharp
if (obj is string s)
    Console.WriteLine(s.Length); // 's' is already typed, no extra cast
```

Switch expression with patterns (C# 8+):

```csharp
var area = shape switch {
    Circle c => Math.PI _ c.Radius _ c.Radius,
    Square sq => sq.Side * sq.Side,
    null => 0,
    _ => throw new ArgumentException()
};
```

**Why interviewers love this:** It’s a quick way to see if you write modern C# or you’re stuck in the C# 5 era. Show it off.

---

### 12. What are nullable reference types?

A C# 8+ feature that makes the compiler track which references _can_ be null.

```
#nullable enable
```

```csharp
string name = null;      // ⚠️ compiler warning
string? nickname = null; // ✅ '?' explicitly says nullable
```

Once you enable it, the compiler warns you every time you might dereference a null. It’s the single biggest bug-reduction feature added to C# in the last decade.

**What to say in the interview:** _'It’s how C# finally caught up with Kotlin and TypeScript on null safety.'_ Bonus points for knowing it’s opt-in per project via <Nullable>enable</Nullable> in the csproj.

# The actual secret to surviving the first 15 minutes

It’s not memorization. It’s three things:

1. Lead with the one-liner. Don’t ramble. Give the crisp answer in 1–2 sentences, then offer to go deeper if they want. Interviewers love candidates who respect their time.
2. Always pair the “what” with the “why.” “throw preserves the stack trace" is better than "use throw not throw ex" — the first one shows understanding.
3. Admit gaps honestly. “I haven’t used dynamic in production, but my understanding is…” beats confident BS every single time.

Get the first 15 minutes right, and the rest of the interview shifts from interrogation to conversation. That’s where you actually win the offer.
