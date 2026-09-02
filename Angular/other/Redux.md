# Redux

NgRx is a reactive state management framework for Angular applications inspired by the Redux pattern.

```
Component
   │ dispatch(Action)
   ▼
 Action
   ▼
 Reducer ──► Store
   │           │ select
   │           ▼
   └─────── Component
```

## Example

### State

```typescript
export interface CounterState {
  count: number;
}
```

### Action

`createAction("[Entity] Action", props)`

```typescript
export const increment = createAction("[Counter] Increment");
export const decrement = createAction("[Counter] Decrement");
```

### Reducer

`createReducer`

- No Switch Statements: Replaces traditional switch and case blocks with a clean **builder callback function** (builder.addCase).
- Immutability Made Easy: Safely write code that appears to directly mutate the state (e.g., state.value += 1), and Immer automatically turns it into safe, immutable updates.

```typescript
export const initialState: CounterState = {
  count: 0,
};

export const counterReducer = createReducer(
  initialState,

  on(increment, (state) => ({
    ...state,
    count: state.count + 1,
  })),

  on(decrement, (state) => ({
    ...state,
    count: state.count - 1,
  }))
);
```

### Component

```typescript
export class CounterComponent {
  count$ = this.store.select((state) => state.counter.count);

  constructor(private store: Store) {}

  increment() {
    this.store.dispatch(increment());
  }
}
```

## Najważniejsze pojęcia

| Element  | Odpowiedzialność                          | Remarks                                    |
| -------- | ----------------------------------------- | ------------------------------------------ |
| Store    | przechowuje **globalny** state            |                                            |
| Action   | opisuje, co się wydarzyło                 | komunikat _co się wydarzyło_               |
| Reducer  | zmienia **state** na podstawie **Action** | **synchroniczna**, **czysta** zmiana state |
| Selector | odczytuje fragment **state**              | odczyt state                               |
| Effect   | obsługuje operacje asynchroniczne/API     | API, HTTP, WebSocket, inne side effects    |

Complete flow:

```
Component
   │ dispatch(loadShips())
   ▼
 Action
   ▼
 Effect ──────► API
   │ shipsLoaded(...)
   ▼
 Reducer
   ▼
 Store
   ▼
 Selector
   ▼
 Component
```

---

# Complex example

Imagine your Angular application has several pieces of global state: **user, ships, and notifications**.

## Define the overall state

```typescript
export interface AppState {
  user: UserState;
  ships: ShipsState;
  notifications: NotificationsState;
}

export interface UserState {
  id: string;
  name: string;
  isLoggedIn: boolean;
}

export interface ShipsState {
  ships: Ship[];
  loading: boolean;
  error: string | null;
}

export interface NotificationsState {
  notifications: Notification[];
  unreadCount: number;
}
```

So the Redux/NgRx store looks conceptually like:

```
Store
├── user
│   ├── id
│   ├── name
│   └── isLoggedIn
├── ships
│   ├── ships[]
│   ├── loading
│   └── error
└── notifications
    ├── notifications[]
    └── unreadCount
```

## Actions

Actions describe what happened:

```typescript
export const login = createAction("[User] Login", props<{ id: string; name: string }>());

export const loadShips = createAction("[Ships] Load Ships");

export const shipsLoaded = createAction("[Ships] Ships Loaded", props<{ ships: Ship[] }>());

export const addNotification = createAction("[Notifications] Add Notification", props<{ notification: Notification }>());
```

## Separate reducers

Usually you don't create one giant reducer. Each state member gets its own reducer.

### User

```typescript
export const userReducer = createReducer(
  initialUserState,

  on(login, (state, action) => ({
    ...state,
    id: action.id,
    name: action.name,
    isLoggedIn: true,
  }))
);
```

### Ships

```typescript
export const shipsReducer = createReducer(
  initialShipsState,

  on(loadShips, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(shipsLoaded, (state, action) => ({
    ...state,
    ships: action.ships,
    loading: false,
  }))
);
```

### Notifications

```typescript
export const notificationsReducer = createReducer(
  initialNotificationsState,

  on(addNotification, (state, action) => ({
    ...state,
    notifications: [...state.notifications, action.notification],
    unreadCount: state.unreadCount + 1,
  }))
);
```

### Register them in the Store

```typescript
provideStore({
  user: userReducer,
  ships: shipsReducer,
  notifications: notificationsReducer,
});
```

Now NgRx combines them into:

```
AppState
├── userReducer ──────────► user
├── shipsReducer ─────────► ship
└── notificationsReducer ─► notifications
```

### Select individual state members

For example, a component might need the logged-in user and ships:

```typescript
user$ = this.store.select((state) => state.user);
ships$ = this.store.select((state) => state.ships.ships);
loading$ = this.store.select((state) => state.ships.loading);
```

Or, preferably, define **selectors**:

```typescript
export const selectUser = (state: AppState) => state.user;

export const selectShips = (state: AppState) => state.ships.ships;

export const selectShipsLoading = (state: AppState) => state.ships.loading;
```

Then:

```typescript
user$ = this.store.select(selectUser);
ships$ = this.store.select(selectShips);
```

### Important concept

The key thing to understand is that **there is one global Store, but it can contain many state slices**:

```
                 ┌───────────────┐
                 │     Store     │
                 └───────┬───────┘
                         │
          ┌──────────────┼──────────────┐
          ▼              ▼              ▼
       UserState     ShipsState    NotificationState
          │              │              │
       reducer        reducer        reducer
          │              │              │
       selectors      selectors      selectors
          │              │              │
          └──────────────┼──────────────┘
                         ▼
                     Components
```

This is particularly useful in your **multiple Angular applications / backend events** scenario: for example, a WebSocket/SignalR event can trigger an NgRx **Action**, the reducer updates the relevant state slice, and Angular components react through **selectors**.

---

# Immutability

Conceptually:

```typescript
newState = reducer(previousState, action);
```

For example:

```typescript
on(increment, (state) => ({
  ...state,
  count: state.count + 1,
}));
```

Here:

```
previous state: count: 5
 │ increment action
reducer
 │
new state: count: 6
```

## Important: don't mutate the previous state

❌ Don't do:

```typescript
state.count++;
return state;
```

Instead:

```typescript
return {
  ...state,
  count: state.count + 1,
};
```

You're creating a **new state object**.

For nested state:

```typescript
return {
  ...state,
  user: {
    ...state.user,
    name: "John",
  },
};
```

So the rule to remember for an interview is:

> A reducer is a **pure** function that takes the current state and an action, and returns the next state **without mutating** the current state.

Also, the reducer **doesn't normally call APIs, databases, HTTP, etc.** That's what **Effects** are for.

---

# Effect

Sure. The important thing is that an **Effect sits between the Action and the resulting Action** and is used for side effects such as HTTP calls.

## Component dispatches an action

```typescript
loadShips() {
  this.store.dispatch(loadShips());
}
```

Action:

```typescript
export const loadShips = createAction("[Ships Page] Load Ships");
```

## Effect listens for that action

`createEffect`

```typescript
loadShips$ = createEffect(() =>
  this.actions$.pipe(
    ofType(loadShips),

    switchMap(() =>
      this.shipService.getShips().pipe(
        map((ships) => shipsLoaded({ ships })),

        catchError((error) =>
          of(
            loadShipsFailed({
              error: error.message,
            })
          )
        )
      )
    )
  )
);
```

The flow is:

```text
Component
    │ dispatch(loadShips())
    ▼
Action
    ▼
Effect
    │ HTTP request
    ▼
ShipService ──────► Backend
    │ ships
    ▼
Effect
    │ dispatch
    ▼
shipsLoaded({ ships })
    ▼
Reducer
    ▼
New State
    ▼
Observable
    ▼
Component
```

## Reducer handles the result

```typescript
export const shipsReducer = createReducer(
  initialState,

  on(loadShips, (state) => ({
    ...state,
    loading: true,
  })),

  on(shipsLoaded, (state, { ships }) => ({
    ...state,
    ships,
    loading: false,
  })),

  on(loadShipsFailed, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  }))
);
```

Notice the separation:

```text
Action
   │
   ├──────────────► Reducer
   │                  │
   │                  └── update state
   │
   └──────────────► Effect
                      │
                      └── HTTP/API
                           │
                           ▼
                     New Action
                           │
                           ▼
                        Reducer
```

## Why not put HTTP inside the reducer?

Because a reducer should be:

```typescript
(state, action) => newState;
```

**pure and synchronous**.

You don't want:

```typescript
on(loadShips, state => {
  this.http.get(...); // ❌
  return state;
});
```

Instead:

```
Reducer = "How does state change?"
Effect  = "What external operation needs to happen?"
```

## And this connects directly to RxJS

The Effect itself is essentially an **RxJS pipeline**:

```typescript
this.actions$.pipe(
  ofType(loadShips),
  switchMap(() => this.shipService.getShips()),
  map((ships) => shipsLoaded({ ships }))
);
```

So you can think of the architecture as:

**NgRx manages state; RxJS manages asynchronous streams; Effects connect the two.**
