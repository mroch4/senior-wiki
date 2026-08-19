# Pipes

## Table of content

1. [What is Pipes?](#what-is-pipes)
2. [Built-in pipes](#1-built-in-pipes)
3. [Pipes can be chained](#2-pipes-can-be-chained)
4. [Custom pipes](#3-custom-pipes)
5. [Pure vs impure pipes — important interview topic](#4-pure-vs-impure-pipes-—-important-interview-topic)
6. [`async` pipe](#5-async-pipe)
7. [Pipes vs methods](#6-pipes-vs-methods)
8. [Pipes vs services](#7-pipes-vs-services)
9. [Interview Tips](#interview-tips)

## What is Pipes?

> In Angular, **pipes** are a way to transform data in the template without changing the underlying value.

### 1. Built-in pipes

Common Angular pipes:

| Pipe        | Purpose                         | Example                                |
| ----------- | ------------------------------- | -------------------------------------- |
| `date`      | Format dates                    | `{{ orderDate \| date:'dd/MM/yyyy' }}` |
| `currency`  | Format money                    | `{{ price \| currency:'USD' }}`        |
| `number`    | Format numbers                  | `{{ amount \| number:'1.2-2' }}`       |
| `percent`   | Format percentages              | `{{ rate \| percent }}`                |
| `uppercase` | Uppercase text                  | `{{ name \| uppercase }}`              |
| `lowercase` | Lowercase text                  | `{{ name \| lowercase }}`              |
| `json`      | Display object as JSON          | `{{ user \| json }}`                   |
| `async`     | Subscribe to Observable/Promise | `{{ users$ \| async }}`                |

Example:

```html
<p>{{ customer.name | uppercase }}</p>
<p>{{ customer.balance | currency:'EUR' }}</p>
<p>{{ customer.createdAt | date:'medium' }}</p>
```

### 2. Pipes can be chained

You can apply multiple transformations:

```html
{{ customer.name | lowercase | titlecase }}
```

The output of one pipe becomes the input of the next.

### 3. Custom pipes

You can create your own pipe when you need reusable presentation logic.

```typescript
import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: "initials",
  standalone: true,
})
export class InitialsPipe implements PipeTransform {
  transform(name: string): string {
    return name
      .split(" ")
      .map((x) => x[0])
      .join("");
  }
}
```

Then:

```html
{{ customer.name | initials }}
```

For `"John Smith"`, the result is:

```text
JS
```

### 4. Pure vs impure pipes — important interview topic

By default, Angular pipes are **pure**:

```typescript
@Pipe({
  name: 'myPipe',
  pure: true
})
```

A pure pipe runs only when Angular detects a **change in the input value/reference**.

For example:

```typescript
users.push(newUser);
```

doesn't change the `users` array reference, whereas:

```typescript
users = [...users, newUser];
```

creates a new reference.

An **impure pipe**:

```typescript
@Pipe({
  name: 'myPipe',
  pure: false
})
```

can run during every change-detection cycle.

**Senior-level advice:** avoid impure pipes unless you have a strong reason. They can execute very frequently and hurt performance.

### 5. `async` pipe

This is particularly important in Angular applications using RxJS.

Instead of manually subscribing:

```typescript
users: User[] = [];

ngOnInit() {
  this.userService.getUsers()
    .subscribe(users => this.users = users);
}
```

you can expose the Observable:

```typescript
users$ = this.userService.getUsers();
```

and use:

```html
<div *ngFor="let user of users$ | async">{{ user.name }}</div>
```

The `async` pipe handles subscription/unsubscription for you.

### 6. Pipes vs methods

You might see:

```html
{{ getFullName(user) }}
```

versus:

```html
{{ user | fullName }}
```

A pipe is generally preferable for **reusable presentation transformations**, particularly when it can remain pure.

A method can potentially be called repeatedly during change detection, whereas Angular can optimize pure pipes because their inputs haven't changed.

### 7. Pipes vs services

A useful interview distinction:

- **Pipe** → presentation transformation
- **Service** → application/business logic
- **Component** → coordinates UI and state
- **Directive** → changes DOM behavior/appearance

For example, formatting `"John Smith"` as `"JS"` is a reasonable pipe.

Calling an API to determine whether John is eligible for a loan is **not** pipe logic—that belongs in a service/business layer.

# Interview Tips

If asked **"What are Angular pipes?"**, a strong concise answer is:

> "Angular pipes transform data for presentation in templates without modifying the underlying data. Angular provides built-in pipes such as date, currency, number, async and json, and we can create custom pipes with `PipeTransform`. Pipes are pure by default, meaning Angular can avoid re-running them when their inputs haven't changed. Impure pipes run during change detection and should be used carefully. The async pipe is especially useful with RxJS because it manages subscriptions and cleanup automatically."
