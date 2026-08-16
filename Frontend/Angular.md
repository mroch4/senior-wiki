# Angular lifecycle methods

Angular lifecycle methods are hooks that Angular calls at different stages of a component's or directive's life. They let you run custom code when a component is created, updated, checked, or destroyed.

Here's the lifecycle in order:

| Lifecycle Hook            | Purpose                                       | When it is Called                                         |
| ------------------------- | --------------------------------------------- | --------------------------------------------------------- |
| `ngOnChanges()`           | Respond to input property changes             | Before `ngOnInit()` and whenever `@Input()` values change |
| `ngOnInit()`              | Initialize the component                      | Once after the first `ngOnChanges()`                      |
| `ngDoCheck()`             | Perform custom change detection               | During every change detection cycle                       |
| `ngAfterContentInit()`    | Initialize projected content (`<ng-content>`) | Once after content projection                             |
| `ngAfterContentChecked()` | Check projected content                       | After every content check                                 |
| `ngAfterViewInit()`       | Initialize component's view and child views   | Once after the view is initialized                        |
| `ngAfterViewChecked()`    | Check component's view and child views        | After every view check                                    |
| `ngOnDestroy()`           | Clean up resources                            | Just before the component is destroyed                    |

## Lifecycle Flow

```text
Constructor
 |
ngOnChanges() (if @Input exists)
 |
ngOnInit()
 |
ngDoCheck()
 |
ngAfterContentInit()
 |
ngAfterContentChecked()
 |
ngAfterViewInit()
 |
ngAfterViewChecked()
 |
(repeats during change detection)
ngOnChanges()
ngDoCheck()
ngAfterContentChecked()
ngAfterViewChecked()
 |
ngOnDestroy()
```

## Explanation of Each Hook

### 1. `constructor()`

- Called when Angular creates the component instance.
- Used for dependency injection.
- Avoid placing business logic or API calls here.

```typescript
constructor(private userService: UserService) {
  console.log('Constructor');
}
```

### 2. `ngOnChanges()`

- Triggered whenever an `@Input()` property changes.
- Receives a `SimpleChanges` object.

```typescript
@Input() name!: string;

ngOnChanges(changes: SimpleChanges) {
  console.log(changes);
}
```

Use cases:

- Reacting to input changes.
- Updating derived values.

### 3. `ngOnInit()`

- Called once after the first `ngOnChanges()`.
- Best place for initialization.

```typescript
ngOnInit() {
  this.loadUsers();
}
```

Use cases:

- API calls
- Initialize variables
- Subscribe to services

### 4. `ngDoCheck()`

- Runs during every change detection cycle.
- Used for custom change detection.

```typescript
ngDoCheck() {
  console.log('Checking...');
}
```

Use cases:

- Detect changes Angular doesn't track automatically.
- Avoid heavy computations because it runs frequently.

### 5. `ngAfterContentInit()`

- Called once after projected content (`<ng-content>`) is initialized.

```typescript
ngAfterContentInit() {
  console.log('Content initialized');
}
```

### 6. `ngAfterContentChecked()`

- Called after every check of projected content.

```typescript
ngAfterContentChecked() {
  console.log('Content checked');
}
```

### 7. `ngAfterViewInit()`

- Called once after the component's view and child views are initialized.

```typescript
@ViewChild('input') input!: ElementRef;

ngAfterViewInit() {
  this.input.nativeElement.focus();
}
```

Use cases:

- Access `@ViewChild`
- DOM manipulation
- Initialize third-party libraries

### 8. `ngAfterViewChecked()`

- Called after every check of the component's view.

```typescript
ngAfterViewChecked() {
  console.log('View checked');
}
```

Use cases:

- Rarely used.
- Avoid changing data here, as it can trigger additional change detection.

### 9. `ngOnDestroy()`

- Called just before Angular destroys the component.

```typescript
private subscription!: Subscription;

ngOnDestroy() {
  this.subscription.unsubscribe();
}
```

Use cases:

- Unsubscribe from Observables
- Remove event listeners
- Clear timers or intervals
- Clean up resources

## Example

```typescript
@Component({
  selector: "app-demo",
  template: `<h1>{{ title }}</h1>`,
})
export class DemoComponent implements OnInit, OnChanges, DoCheck, AfterContentInit, AfterContentChecked, AfterViewInit, AfterViewChecked, OnDestroy {
  @Input() title!: string;

  constructor() {
    console.log("Constructor");
  }

  ngOnChanges() {
    console.log("ngOnChanges");
  }

  ngOnInit() {
    console.log("ngOnInit");
  }

  ngDoCheck() {
    console.log("ngDoCheck");
  }

  ngAfterContentInit() {
    console.log("ngAfterContentInit");
  }

  ngAfterContentChecked() {
    console.log("ngAfterContentChecked");
  }

  ngAfterViewInit() {
    console.log("ngAfterViewInit");
  }

  ngAfterViewChecked() {
    console.log("ngAfterViewChecked");
  }

  ngOnDestroy() {
    console.log("ngOnDestroy");
  }
}
```

# Interview Tips

- **`constructor()`**: Dependency injection only.
- **`ngOnInit()`**: Initialization and API calls.
- **`ngOnChanges()`**: Respond to `@Input()` changes.
- **`ngAfterViewInit()`**: Access `@ViewChild` or manipulate the DOM.
- **`ngOnDestroy()`**: Unsubscribe from Observables and clean up resources.
- **`ngDoCheck()`**: Custom change detection; use sparingly because it runs often.

These hooks cover the complete lifecycle of an Angular component, from creation through updates to destruction.
