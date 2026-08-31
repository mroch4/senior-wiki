# Redux Hooks — React

Redux hooks are the modern way for React components to interact with the Redux store, primarily through **React-Redux**.

The two most important hooks are:

- `useSelector()` — **read data** from the Redux store
- `useDispatch()` — **dispatch actions** to update the store

### 1. `useSelector`

Reads a value from the Redux state.

```tsx
const user = useSelector((state: RootState) => state.user);
```

Example:

```tsx
function UserProfile() {
  const user = useSelector((state: RootState) => state.user);

  return <div>{user.name}</div>;
}
```

When the selected state changes, the component is re-rendered.

You can select a specific property:

```tsx
const username = useSelector((state: RootState) => state.user.name);
```

**Interview point:** `useSelector` subscribes the component to the Redux store and performs a reference comparison to determine whether the selected value changed.

### 2. `useDispatch`

Returns the Redux `dispatch` function.

```tsx
const dispatch = useDispatch();
```

Then dispatch an action:

```tsx
dispatch(login(user));
```

Example:

```tsx
function LoginButton() {
  const dispatch = useDispatch();

  const handleLogin = () => {
    dispatch(
      login({
        id: 123,
        name: "John",
      })
    );
  };

  return <button onClick={handleLogin}>Login</button>;
}
```

The action is sent to the Redux store, where reducers process it.

### 3. Typical flow

```text
React Component
      │
      │ dispatch(action)
      ▼
   Redux Store
      │
      ▼
    Reducer
      │
      ▼
 New State
      │
      │ useSelector()
      ▼
React Component re-renders
```

For example:

```tsx
const count = useSelector((state: RootState) => state.counter.value);

const dispatch = useDispatch();

return (
  <>
    <span>{count}</span>

    <button onClick={() => dispatch(increment())}>+</button>
  </>
);
```

### 4. `useStore`

Less commonly used:

```tsx
const store = useStore();
```

It gives you direct access to the Redux store.

You generally **shouldn't use it for normal state access**. Prefer `useSelector` and `useDispatch`.

### 5. Typed Redux hooks — important for TypeScript

Instead of repeatedly doing:

```tsx
useSelector((state: RootState) => ...)
```

you can create typed hooks:

```tsx
export const useAppSelector = useSelector.withTypes<RootState>();
export const useAppDispatch = useDispatch.withTypes<AppDispatch>();
```

Then:

```tsx
const user = useAppSelector((state) => state.user);

const dispatch = useAppDispatch();

dispatch(login(user));
```

This is the **recommended approach in modern Redux Toolkit + TypeScript applications**.

### Interview summary

| Hook            | Purpose                                  |
| --------------- | ---------------------------------------- |
| `useSelector()` | Read data from Redux state               |
| `useDispatch()` | Dispatch actions                         |
| `useStore()`    | Access the store directly, rarely needed |

**Easy way to remember:**
**Selector = get state**
**Dispatch = send action**
