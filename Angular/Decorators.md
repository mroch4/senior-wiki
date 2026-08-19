# Decorators

## Table of content

1. [What is Decorator?](#what-is-decorator)
2. [`@Component`](#1-component)
3. [`@Injectable`](#2-injectable)
4. [`@Directive`](#3-directive)
5. [`@Pipe`](#4-pipe)
6. [`@Input`](#5-input)
7. [`@Output`](#6-output)
8. [`@ViewChild`](#7-viewchild)
9. [`@ViewChildren`](#8-viewchildren)
10. [`@ContentChild`](#9-contentchild)
11. [Decorator categories](#decorator-categories)
12. [Interview Tips](#interview-tips)

## What is Decorator?

> Decorators are special TypeScript annotations that tell Angular how a class, property, method, or parameter should be treated.

### 1. `@Component`

Defines an Angular component.

```typescript
@Component({
  selector: "app-user",
  templateUrl: "./user.component.html",
  styleUrls: ["./user.component.css"],
})
export class UserComponent {}
```

It tells Angular:

- This class is a component
- What HTML selector represents it
- Which template to use
- Which styles belong to it

### 2. `@Injectable`

Marks a class as available for **dependency injection**.

```typescript
@Injectable({
  providedIn: "root",
})
export class UserService {}
```

Then:

```typescript
constructor(private userService: UserService) {}
```

`providedIn: 'root'` means Angular creates a singleton-like service in the application's root injector.

### 3. `@Directive`

Defines a custom directive.

```typescript
@Directive({
  selector: "[appHighlight]",
})
export class HighlightDirective {}
```

Used like:

```html
<p appHighlight>Hello</p>
```

A directive modifies or adds behavior to an existing element, whereas a component has its own template.

### 4. `@Pipe`

Defines a custom pipe.

```typescript
@Pipe({
  name: "capitalize",
})
export class CapitalizePipe implements PipeTransform {
  transform(value: string): string {
    return value.toUpperCase();
  }
}
```

Usage:

```html
{{ name | capitalize }}
```

### 5. `@Input`

Defines data flowing **from parent → child**.

```typescript
@Component({
  selector: "app-user",
})
export class UserComponent {
  @Input() userName!: string;
}
```

Parent:

```html
<app-user [userName]="name"></app-user>
```

### 6. `@Output`

Defines an event flowing **from child → parent**.

```typescript
@Output() userSelected = new EventEmitter<number>();
```

Child:

```typescript
this.userSelected.emit(123);
```

Parent:

```html
<app-user (userSelected)="onUserSelected($event)"> </app-user>
```

So the classic interview answer is:

**`@Input` = parent sends data down**
**`@Output` = child sends events up**

### 7. `@ViewChild`

Gets a reference to an element, component, or directive in the component's view.

```typescript
@ViewChild(UserComponent)
userComponent!: UserComponent;
```

Then:

```typescript
this.userComponent.loadUser();
```

It can also reference a DOM element:

```typescript
@ViewChild('input')
input!: ElementRef;
```

```html
<input #input />
```

### 8. `@ViewChildren`

Gets multiple items from the component's view.

```typescript
@ViewChildren(UserComponent)
users!: QueryList<UserComponent>;
```

Useful when there are multiple instances of a component/directive.

### 9. `@ContentChild`

Gets something projected into the component using `<ng-content>`.

```html
<app-panel>
  <p #message>Hello</p>
</app-panel>
```

Inside `PanelComponent`:

```typescript
@ContentChild('message')
message!: ElementRef;
```

The distinction is important:

**`@ViewChild` → something in my component's own template**

**`@ContentChild` → something projected into my component**

### Decorator categories

| Decorator          | Purpose                           |
| ------------------ | --------------------------------- |
| `@Component`       | Defines a component               |
| `@Directive`       | Defines a directive               |
| `@Pipe`            | Defines a pipe                    |
| `@Injectable`      | Enables DI for a class            |
| `@Input`           | Parent → child data               |
| `@Output`          | Child → parent event              |
| `@ViewChild`       | Access one item in component view |
| `@ViewChildren`    | Access multiple view items        |
| `@ContentChild`    | Access one projected item         |
| `@ContentChildren` | Access multiple projected items   |

# Interview Tips

Angular decorators are fundamentally **metadata**. Angular uses that metadata to understand how a class participates in the framework.

For example:

```typescript
@Component(...)
export class MyComponent {}
```

isn't just a TypeScript class anymore from Angular's perspective—the decorator provides the metadata Angular needs to create and manage it.

Also, in **modern Angular**, some older decorator-based APIs have newer alternatives. For example, Angular supports signal-based `input()` and `output()` APIs, so you may encounter:

```typescript
name = input<string>();
selected = output<number>();
```

instead of:

```typescript
@Input() name!: string;
@Output() selected = new EventEmitter<number>();
```

For an interview, I'd know **both**, because many existing Angular applications still use the decorator approach.
