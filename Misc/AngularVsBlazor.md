Angular and Blazor have similar component lifecycles, but the hook names and some behaviors differ.

| Angular                   | Blazor                                          | Purpose                                             |
| ------------------------- | ----------------------------------------------- | --------------------------------------------------- |
| `constructor()`           | Constructor                                     | Create the component and inject dependencies        |
| `ngOnChanges()`           | `OnParametersSet()` / `OnParametersSetAsync()`  | React to parameter (`@Input`/`[Parameter]`) changes |
| `ngOnInit()`              | `OnInitialized()` / `OnInitializedAsync()`      | Initialize the component once                       |
| `ngDoCheck()`             | No direct equivalent                            | Custom change detection                             |
| `ngAfterContentInit()`    | No direct equivalent                            | Content projection initialization                   |
| `ngAfterContentChecked()` | No direct equivalent                            | Content projection checks                           |
| `ngAfterViewInit()`       | `OnAfterRender(bool firstRender)`               | Access rendered DOM or component references         |
| `ngAfterViewChecked()`    | `OnAfterRender()` (after every render)          | Run logic after each render                         |
| `ngOnDestroy()`           | `Dispose()` / `IAsyncDisposable.DisposeAsync()` | Cleanup resources                                   |

## Lifecycle Comparison

```text
Angular                          Blazor
--------                         --------
constructor()                    Constructor
      │                                │
ngOnChanges()                    Set parameters
      │                                │
ngOnInit()                       OnInitialized()
      │                                │
Render                           Render
      │                                │
ngAfterViewInit()                OnAfterRender(firstRender)
      │                                │
Updates                          Parameter changes
      │                                │
ngOnChanges()                    OnParametersSet()
ngDoCheck()                      (No equivalent)
Render                           Render
ngAfterViewChecked()             OnAfterRender()
      │                                │
ngOnDestroy()                    Dispose()
```

## Hook-by-hook Comparison

### 1. Initialization

Angular:

```typescript
ngOnInit() {
  this.loadUsers();
}
```

Blazor:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadUsers();
}
```

These are the primary places for initial data loading.

### 2. Parent Parameter Changes

Angular:

```typescript
@Input() userId!: number;

ngOnChanges(changes: SimpleChanges) {
    this.loadUser(this.userId);
}
```

Blazor:

```csharp
[Parameter]
public int UserId { get; set; }

protected override async Task OnParametersSetAsync()
{
    await LoadUser(UserId);
}
```

**Equivalent:**

- Angular `@Input()` ↔ Blazor `[Parameter]`
- `ngOnChanges()` ↔ `OnParametersSet()`

### 3. After Rendering

Angular:

```typescript
@ViewChild('textbox') textbox!: ElementRef;

ngAfterViewInit() {
    this.textbox.nativeElement.focus();
}
```

Blazor:

```csharp
private ElementReference textbox;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await textbox.FocusAsync();
    }
}
```

Both are used because the DOM isn't available until after the first render.

### 4. Cleanup

Angular:

```typescript
private sub!: Subscription;

ngOnDestroy() {
    this.sub.unsubscribe();
}
```

Blazor:

```csharp
public void Dispose()
{
    timer?.Dispose();
}
```

Both are used to release resources and prevent leaks.

## Major Differences

### 1. Change Detection

**Angular**

- Uses a change detection mechanism (often driven by Zone.js or signals in newer Angular patterns).
- `ngDoCheck()` allows custom change detection.

**Blazor**

- Components re-render when:

  - Parameters change
  - Events occur
  - `StateHasChanged()` is called

- No equivalent to `ngDoCheck()`.

### 2. DOM Access

**Angular**

- `@ViewChild`
- `ElementRef`

**Blazor**

- `ElementReference`
- JavaScript interop for many DOM operations

### 3. Async Lifecycle

Angular lifecycle hooks themselves are synchronous, although they can start asynchronous work:

```typescript
async ngOnInit() {
    await this.loadUsers();
}
```

Blazor provides built-in asynchronous lifecycle methods:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadUsers();
}
```

This makes asynchronous initialization a first-class pattern in Blazor.

## Mapping for a Blazor Developer

| If you know Blazor...             | In Angular use...                                                                    |
| --------------------------------- | ------------------------------------------------------------------------------------ |
| `OnInitializedAsync()`            | `ngOnInit()`                                                                         |
| `OnParametersSetAsync()`          | `ngOnChanges()`                                                                      |
| `OnAfterRenderAsync(firstRender)` | `ngAfterViewInit()`                                                                  |
| `OnAfterRenderAsync()`            | `ngAfterViewChecked()`                                                               |
| `Dispose()`                       | `ngOnDestroy()`                                                                      |
| `[Parameter]`                     | `@Input()`                                                                           |
| `StateHasChanged()`               | Angular's automatic change detection (or signals/manual detection in advanced cases) |

### Key takeaway

For a Blazor developer, the closest mental model is:

- **`OnInitialized`** → **`ngOnInit`** (one-time setup)
- **`OnParametersSet`** → **`ngOnChanges`** (react to parent input changes)
- **`OnAfterRender`** → **`ngAfterViewInit`** / **`ngAfterViewChecked`** (interact with the rendered view)
- **`Dispose`** → **`ngOnDestroy`** (cleanup)

The biggest conceptual difference is that Angular has a more explicit component lifecycle with hooks for change detection and content projection, while Blazor's lifecycle is centered around parameter updates, rendering, and disposal.
