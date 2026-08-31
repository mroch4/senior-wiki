Absolutely. If the interview asks you to **review or implement a PR in a Blazor app**, they may deliberately include code that _works_ but has Blazor-specific problems. Here are the gotchas I'd actively look for.

### 🔥 High-value Blazor PR gotchas

| Gotcha                                             | What to look for                                                                          | Why it's a problem                                |
| -------------------------------------------------- | ----------------------------------------------------------------------------------------- | ------------------------------------------------- |
| **`IDisposable` / subscriptions**                  | `Timer`, event handlers, `NavigationManager.LocationChanged`, `EditContext` subscriptions | Memory leaks / callbacks after component is gone  |
| **Async lifecycle**                                | `async void`, especially outside event handlers                                           | Exceptions can't be properly awaited/handled      |
| **`OnInitializedAsync` vs `OnParametersSetAsync`** | Loading data only in initialization when parameters can change                            | Component may display stale data                  |
| **Calling JS too early**                           | JS interop in `OnInitialized` / `OnParametersSet`                                         | DOM may not exist yet                             |
| **`OnAfterRenderAsync` loop**                      | Calling `StateHasChanged()` without `firstRender`/guard                                   | Infinite render loop                              |
| **`@key` missing**                                 | Rendering lists of stateful components without `@key`                                     | Blazor may reuse component instances incorrectly  |
| **Mutating parameters**                            | Child component modifies `[Parameter]` directly                                           | Breaks parent/child ownership model               |
| **`StateHasChanged()` abuse**                      | Explicit calls everywhere                                                                 | Usually unnecessary; can cause extra renders      |
| **Blocking async**                                 | `.Result`, `.Wait()`                                                                      | Deadlocks / thread starvation                     |
| **Long-running work**                              | HTTP/database work directly in rendering/lifecycle without cancellation                   | Work can continue after navigation/disposal       |
| **Cascading values**                               | Large/ frequently changing objects cascaded globally                                      | Causes unnecessary component rerenders            |
| **EventCallback**                                  | `Action`/`EventHandler` instead of `EventCallback` for component events                   | Loses Blazor's event/rendering semantics          |
| **`@bind` assumptions**                            | Incorrect `Value`/`ValueChanged` implementation                                           | Two-way binding silently doesn't behave correctly |
| **Forms validation**                               | Missing `EditForm`, `EditContext`, validation, or incorrect model lifetime                | Validation/state bugs                             |
| **`@onclick` async**                               | Fire-and-forget task                                                                      | Exceptions and UI state can be lost               |
| **Authentication state**                           | Reading auth state once in initialization                                                 | User/auth state can change                        |
| **Prerendering**                                   | Browser-only APIs / JS / localStorage during prerender                                    | Code executes where browser APIs aren't available |
| **DI lifetime**                                    | Injecting scoped services into inappropriate long-lived components                        | Especially important in Blazor Server             |
| **Server circuit**                                 | Assuming each component request behaves like a normal HTTP request                        | Blazor Server has a persistent circuit            |
| **Large component**                                | One component doing UI + API + business logic + state management                          | Poor maintainability/testability                  |

---

## 1. `OnInitializedAsync` vs `OnParametersSetAsync`

This is one I'd **definitely** check.

Bad:

```csharp
[Parameter]
public int ProductId { get; set; }

protected override async Task OnInitializedAsync()
{
    Product = await ProductService.GetProduct(ProductId);
}
```

If the same component instance receives a different `ProductId`, `OnInitializedAsync()` **doesn't run again**.

Better:

```csharp
protected override async Task OnParametersSetAsync()
{
    Product = await ProductService.GetProduct(ProductId);
}
```

Interview explanation:

> "`OnInitializedAsync` is for initialization of the component instance. If the component's parameters can change while the instance remains alive, parameter-dependent loading belongs in `OnParametersSetAsync`."

---

## 2. `@key` — classic PR trap

Suppose you have:

```razor
@foreach (var item in Items)
{
    <TodoItem Item="item" />
}
```

Then the list changes.

Blazor uses its rendering/diffing algorithm and can **reuse component instances**.

For stateful child components, I'd consider:

```razor
@foreach (var item in Items)
{
    <TodoItem @key="item.Id" Item="item" />
}
```

This tells Blazor which rendered element/component corresponds to which logical item.

A good interview phrase:

> "When rendering a collection of stateful components, I'd check whether `@key` is required to preserve component identity when the collection changes."

---

## 3. JS interop in the wrong lifecycle method

Bad:

```csharp
protected override async Task OnInitializedAsync()
{
    await JS.InvokeVoidAsync("initializeWidget");
}
```

The DOM element may not exist yet.

Usually:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JS.InvokeVoidAsync("initializeWidget");
    }
}
```

And if the JS library attaches handlers/resources, check whether they are cleaned up when the component is disposed.

---

## 4. `OnAfterRenderAsync` infinite loop

This is a great PR-review test:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    await LoadSomething();

    StateHasChanged();
}
```

🚨 Potential render loop.

Render → `OnAfterRenderAsync` → `StateHasChanged()` → render → `OnAfterRenderAsync` → ...

Usually:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await LoadSomething();
        StateHasChanged();
    }
}
```

Though even this should be used deliberately.

---

## 5. Event subscriptions and disposal

Look for:

```csharp
Navigation.LocationChanged += OnLocationChanged;
```

or:

```csharp
SomeService.Changed += OnChanged;
```

If the component subscribes, it often needs to unsubscribe.

```csharp
public void Dispose()
{
    Navigation.LocationChanged -= OnLocationChanged;
}
```

This is especially important for long-lived services/events.

Also look for:

```csharp
Timer timer = new();
```

without disposal.

---

## 6. `async void`

Bad:

```csharp
private async void Save()
{
    await service.SaveAsync();
}
```

Prefer:

```csharp
private async Task Save()
{
    await service.SaveAsync();
}
```

Then:

```razor
<button @onclick="Save">Save</button>
```

Blazor event handlers can work with `Task`, allowing the framework to await the operation and handle rendering appropriately.

**Exception:** event-handler APIs that inherently require `void` can legitimately use `async void`, but that's uncommon in normal Blazor component code.

---

## 7. `StateHasChanged()` everywhere

You'll often see junior code like:

```csharp
private async Task Save()
{
    IsSaving = true;
    StateHasChanged();

    await Service.SaveAsync();

    IsSaving = false;
    StateHasChanged();
}
```

The first `StateHasChanged()` is often unnecessary because an async Blazor event handler triggers rendering.

I'd ask:

> "Is this explicit `StateHasChanged()` actually necessary?"

You shouldn't blindly remove every call, though. It can be necessary when state changes **outside Blazor's normal event/lifecycle flow**, such as external callbacks.

---

## 8. Child modifying `[Parameter]`

Bad:

```csharp
[Parameter]
public User User { get; set; }

private void ChangeName()
{
    User.Name = "Bob";
}
```

The child is effectively modifying state owned by the parent.

Better patterns include:

```csharp
[Parameter]
public EventCallback<User> UserChanged { get; set; }
```

or passing the required operation/callback.

The conceptual rule:

> **Parameters flow down; events/state changes flow up.**

---

## 9. `EventCallback` vs `Action`

Suppose:

```csharp
[Parameter]
public Action OnSave { get; set; }
```

I'd question it.

Usually:

```csharp
[Parameter]
public EventCallback OnSave { get; set; }
```

And:

```razor
<button @onclick="OnSave">
    Save
</button>
```

`EventCallback` integrates with Blazor's event/rendering model and supports asynchronous handlers.

---

## 10. Cancellation when navigating away

Imagine:

```csharp
protected override async Task OnInitializedAsync()
{
    Items = await Service.GetItemsAsync();
}
```

For simple short requests this may be fine.

But for long-running work, search-as-you-type, streaming, etc., look for cancellation.

For example:

```csharp
private CancellationTokenSource _cts = new();

protected override async Task OnInitializedAsync()
{
    Items = await Service.GetItemsAsync(_cts.Token);
}

public void Dispose()
{
    _cts.Cancel();
    _cts.Dispose();
}
```

The exact implementation depends on the architecture, but the PR-review question is:

> "What happens to this asynchronous operation if the component is disposed?"

---

# 11. Blazor Server: don't treat a circuit like an HTTP request

A particularly good **senior-level** gotcha.

In Blazor Server, a user's interaction occurs over a persistent circuit.

So be careful with things like:

```csharp
private UserState _state;
```

and injected services whose lifetime you don't understand.

Also be suspicious of:

```csharp
Singleton
```

services containing **per-user mutable state**.

A singleton can accidentally share state between users.

Interview answer:

> "I need to understand whether this service contains application-wide state or circuit/user-specific state before choosing its DI lifetime."

---

# 12. Prerendering

If the application uses prerendering/interactive SSR, this can bite you:

```csharp
await JS.InvokeVoidAsync("localStorage.getItem", "token");
```

during initialization.

There may not yet be an interactive browser context.

Similarly:

```csharp
window.location
document.querySelector(...)
localStorage
sessionStorage
```

are browser-only concerns.

I'd specifically ask:

> "Is this component prerendered? If so, is this browser-dependent code running only once interactivity is established?"

---

# 13. Forms — `EditForm` and model lifetime

Look for:

```razor
<EditForm Model="@model">
```

and whether `model` is recreated unexpectedly.

Also check:

```razor
<DataAnnotationsValidator />
<ValidationSummary />
```

if data-annotation validation is expected.

A common mistake is assuming validation happens merely because an `EditForm` exists.

---

# 14. `@bind` implementation

If the PR creates a reusable component:

```razor
<MyInput @bind-Value="Name" />
```

look carefully at the component implementation.

For two-way binding:

```csharp
[Parameter]
public string Value { get; set; }

[Parameter]
public EventCallback<string> ValueChanged { get; set; }
```

and when changing:

```csharp
await ValueChanged.InvokeAsync(newValue);
```

A mismatch between `Value`, `ValueChanged`, and the actual event can create very subtle binding bugs.

---

# 15. Don't put business logic into the component

This:

```csharp
private async Task Save()
{
    if (Order.Total > 1000)
    {
        Order.Discount = 0.15m;
    }

    if (Order.Customer.Country == "PL")
    {
        // ...
    }

    await Http.PostAsJsonAsync(...);
}
```

is a PR smell.

I'd prefer the component to orchestrate UI concerns:

```csharp
private async Task Save()
{
    await OrderService.SaveAsync(Order);
}
```

and have business rules in an appropriate application/domain service.

---

# What I'd say while doing the PR

Don't just silently identify issues. **Think aloud**, because interviewers want to see your review process.

A strong sequence is:

> "First I'll understand the component's lifecycle and ownership of state."

Then:

1. **Parameters**

   - Can they change?
   - Is initialization in the right lifecycle method?

2. **Rendering**

   - Is there unnecessary rendering?
   - Are collections using `@key` where appropriate?

3. **Async**

   - Any `async void`?
   - Any `.Result`/`.Wait()`?
   - What happens if the component is disposed?

4. **Events**

   - Are subscriptions removed?
   - Are child events using `EventCallback`?

5. **JS**

   - Is JS invoked at the correct lifecycle stage?
   - Is JS/browser functionality compatible with prerendering?

6. **State**

   - Who owns this state?
   - Is the child mutating parent-owned state?
   - Could a service accidentally share state between users?

7. **Resources**

   - Timers?
   - Streams?
   - Event handlers?
   - `CancellationTokenSource`?
   - `IJSObjectReference`?

8. **Architecture**

   - Is business logic leaking into the component?
   - Is the component making HTTP calls directly when an application/service layer exists?

### ⭐ The 5 I'd memorize for an interview

If you only have time to remember five:

**`@key` → lifecycle (`OnInitialized` vs `OnParametersSet`) → disposal/subscriptions → `OnAfterRender`/JS → async/cancellation.**

Those are particularly good because they distinguish someone who knows **Blazor's component model** from someone who simply knows C# and Razor.
