Jasne. Zróbmy to jako **mini-kurs pod rozmowę techniczną**: Angular + RxJS + NgRx, z naciskiem na pytania typu _„dlaczego?”, „co się stanie?”, „kiedy tego nie użyć?”_.

## 1. Najpierw problem bez NgRx

Załóżmy aplikację pokazującą statki.

Mamy:

```typescript
export interface Ship {
  id: number;
  name: string;
  status: "ACTIVE" | "MAINTENANCE";
}
```

Najprostszy komponent:

```typescript
export class ShipsComponent {
  ships: Ship[] = [];
  loading = false;
  error: string | null = null;

  constructor(private shipService: ShipService) {}

  loadShips() {
    this.loading = true;

    this.shipService.getShips().subscribe({
      next: (ships) => {
        this.ships = ships;
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      },
    });
  }
}
```

To działa.

### Pytanie rekrutera

> **Co jest nie tak z tym podejściem?**

Nic fundamentalnie złego.

Problem pojawia się, kiedy **wiele komponentów potrzebuje tych samych danych**.

```text
ShipsPage ─────────┐
                   │
Dashboard ─────────┼──► GET /ships
                   │
Map ───────────────┘
```

Możesz wtedy wykonać kilka requestów.

Dodatkowo każdy komponent zaczyna zarządzać:

```text
ships
loading
error
selectedShip
filters
pagination
...
```

I pojawia się problem synchronizacji.

---

# 2. Gdzie tutaj wchodzi RxJS?

Zamiast:

```typescript
ships: Ship[];
```

możesz mieć:

```typescript
ships$: Observable<Ship[]>;
```

Service:

```typescript
getShips(): Observable<Ship[]> {
  return this.http.get<Ship[]>('/api/ships');
}
```

Component:

```typescript
ships$ = this.shipService.getShips();
```

Template:

```html
<div *ngFor="let ship of ships$ | async">{{ ship.name }}</div>
```

Teraz dane są **strumieniem**.

```text
HTTP
 │
 ▼
Observable<Ship[]>
 │
 ├── Component A
 ├── Component B
 └── Component C
```

### Pytanie rekrutera

> **Czy Observable przechowuje state?**

Nie w sensie NgRx Store.

Observable jest przede wszystkim **mechanizmem reprezentowania wartości/asynchronicznego strumienia w czasie**.

Może być używany do state management, ale sam Observable nie oznacza automatycznie globalnego state management.

---

# 3. Kiedy pojawia się NgRx?

Załóżmy, że mamy:

```text
                    ┌── Ships page
                    │
Backend ──► State ──┼── Dashboard
                    │
                    ├── Map
                    │
                    └── Notifications
```

Chcemy mieć jedno źródło prawdy:

```text
                 NgRx Store
                     │
       ┌─────────────┼──────────────┐
       ▼             ▼              ▼
     ships          user       notifications
```

To jest główny cel.

---

# 4. State

Definiujemy:

```typescript
export interface ShipsState {
  ships: Ship[];
  loading: boolean;
  error: string | null;
}
```

Initial state:

```typescript
export const initialState: ShipsState = {
  ships: [],
  loading: false,
  error: null,
};
```

Czyli:

```text
ships
├── ships[]
├── loading
└── error
```

---

# 5. Action

Action mówi:

> **co się wydarzyło**

```typescript
export const loadShips = createAction("[Ships Page] Load Ships");
```

To nie jest request.

To tylko informacja:

```text
"Someone requested loading ships"
```

Możesz mieć:

```typescript
export const loadShips = createAction("[Ships Page] Load Ships");

export const shipsLoaded = createAction("[Ships API] Ships Loaded", props<{ ships: Ship[] }>());

export const loadShipsFailed = createAction("[Ships API] Load Ships Failed", props<{ error: string }>());
```

---

# 6. Reducer

Reducer:

```typescript
(state, action) → newState
```

Na przykład:

```typescript
export const shipsReducer = createReducer(
  initialState,

  on(loadShips, (state) => ({
    ...state,
    loading: true,
    error: null,
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

To jest bardzo ważne:

### Reducer NIE robi:

```text
HTTP
database
WebSocket
logging
random()
```

Reducer powinien być:

```text
pure
synchronous
predictable
```

---

# 7. Effect

No dobrze.

Skoro reducer nie może robić HTTP, to kto?

**Effect.**

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

To jest najważniejszy flow do zapamiętania:

```text
Component
    │
    │ dispatch(loadShips())
    ▼
 Action
    │
    ├───────────────┐
    │               │
    ▼               ▼
Reducer          Effect
    │               │
    │               ▼
    │             HTTP
    │               │
    │               ▼
    │        shipsLoaded()
    │               │
    └───────► Reducer
                    │
                    ▼
                  State
```

---

# 8. Dlaczego Effect używa RxJS?

Bo Effect jest naturalnie **strumieniem Actions**:

```typescript
this.actions$;
```

To Observable.

Dlatego możemy robić:

```typescript
this.actions$.pipe(
  ofType(loadShips),
  switchMap(...),
  map(...),
  catchError(...)
)
```

Czyli Effect to w dużym uproszczeniu:

> **RxJS pipeline reagujący na Actions i wykonujący side effects.**

---

# 9. Selector

Nie chcemy wszędzie robić:

```typescript
store.select((state) => state.ships.ships);
```

Tworzymy selector:

```typescript
export const selectShipsState = createFeatureSelector<ShipsState>("ships");

export const selectShips = createSelector(selectShipsState, (state) => state.ships);

export const selectShipsLoading = createSelector(selectShipsState, (state) => state.loading);
```

Component:

```typescript
ships$ = this.store.select(selectShips);

loading$ = this.store.select(selectShipsLoading);
```

Template:

```html
<div *ngIf="loading$ | async">Loading...</div>

<div *ngFor="let ship of ships$ | async">{{ ship.name }}</div>
```

---

# 10. Cała architektura

Teraz masz:

```text
                   Angular Component
                          │
                          │ dispatch()
                          ▼
                       Action
                          │
              ┌───────────┴───────────┐
              ▼                       ▼
           Reducer                  Effect
              │                       │
              │                       │ HTTP
              │                       ▼
              │                    Backend
              │                       │
              │                       ▼
              │                 shipsLoaded()
              │                       │
              └───────────┬───────────┘
                          ▼
                        Store
                          │
                       Selector
                          │
                          ▼
                      Observable
                          │
                          ▼
                       Component
```

To jest **core NgRx**.

---

# 11. Najważniejsze pytanie: po co mi NgRx?

Rekruter:

> **Dlaczego tutaj użyłbyś NgRx?**

Dobra odpowiedź:

> Użyłbym NgRx, jeśli state jest współdzielony przez wiele niezależnych części aplikacji, ma złożone przejścia, wymaga obsługi wielu źródeł zmian albo chcę mieć jedno źródło prawdy i przewidywalny przepływ zmian.

Nie:

> "Bo NgRx jest lepszy."

---

# 12. Kiedy NIE użyć NgRx?

To bardzo częste pytanie.

Jeżeli mam:

```typescript
isModalOpen = false;
```

albo:

```typescript
searchText = "";
```

to prawdopodobnie nie potrzebuję:

```text
Action
   ↓
Reducer
   ↓
Store
   ↓
Selector
```

Wystarczy:

```typescript
isModalOpen = false;
```

albo lokalny:

```typescript
signal(false);
```

### Zasada

> **State powinien znajdować się możliwie blisko miejsca, które go potrzebuje.**

Nie wrzucaj wszystkiego do globalnego Store.

---

# 13. Jak uniknąć wielokrotnego requestu?

Załóżmy:

```text
Component A ──► GET /ships
Component B ──► GET /ships
Component C ──► GET /ships
```

Z NgRx:

```text
              GET /ships
                  │
                  ▼
               Effect
                  │
                  ▼
             Store: ships
             /      |      \
            ▼       ▼       ▼
           A        B       C
```

Request może być wykonany raz, a wynik jest współdzielony przez Store.

Ale uwaga:

> **NgRx samo z siebie nie gwarantuje, że request wykona się tylko raz.**

Jeżeli wielokrotnie dispatchujesz:

```typescript
loadShips();
```

Effect może wielokrotnie wykonać HTTP.

Możesz więc potrzebować mechanizmu typu:

```typescript
if (!loaded) {
  dispatch(loadShips());
}
```

albo odpowiedniej logiki w Effect.

---

# 14. `switchMap` vs `mergeMap` vs `concatMap` vs `exhaustMap`

To jest **bardzo rekrutacyjne**.

Załóżmy:

```typescript
this.actions$.pipe(
  ofType(searchShips),
  switchMap(...)
)
```

### `switchMap`

Nowy request anuluje poprzednią subskrypcję.

Idealny dla:

```text
search
autocomplete
filters
```

```text
search A ────────X
        search B ────────X
                 search C ─────────► result
```

Chcesz wynik najnowszego wyszukiwania.

---

### `mergeMap`

Wszystkie requesty mogą działać równolegle.

```text
request A ───────────────►
request B ───────►
request C ────────────────►
```

Dobry, gdy operacje są niezależne.

---

### `concatMap`

Wykonuje jeden po drugim:

```text
A ─────►
        B ─────►
                C ─────►
```

Dobry, kiedy **kolejność ma znaczenie**.

---

### `exhaustMap`

Ignoruje kolejne akcje, dopóki pierwszy request się nie skończy.

```text
click A ───────────────►
click B     ❌
click C       ❌
```

Bardzo przydatne np. dla:

```text
Submit
Login
Save
```

gdy nie chcesz wielokrotnego kliknięcia wysłać kilku requestów.

---

# 15. Co powoduje re-render?

Rekruter:

> **Co spowoduje ponowne renderowanie komponentu?**

W kontekście:

```typescript
ships$ = this.store.select(selectShips);
```

gdy selector dostarczy nową wartość, Observable emituje.

Angular może wtedy zaktualizować widok.

Przy `OnPush` ważne są m.in.:

```text
@Input reference changes
Observable emission through async pipe
signal changes
events
```

Dlatego immutable state jest tak ważny.

Zamiast:

```typescript
state.ships.push(ship); // ❌
```

robisz:

```typescript
{
  ...state,
  ships: [...state.ships, ship]
}
```

Masz nową referencję.

---

# 16. Selector jako memoizacja

To również warto znać.

```typescript
export const selectActiveShips = createSelector(selectShips, (ships) => ships.filter((ship) => ship.status === "ACTIVE"));
```

NgRx może zapamiętywać wynik selektora dla tych samych inputów.

Czyli selector jest nie tylko:

> "weź mi dane"

ale również mechanizmem **derived state + memoization**.

---

# 17. Gdzie powinien znajdować się state?

To świetne pytanie architektoniczne.

### Lokalny UI state

```text
isModalOpen
selectedTab
inputValue
```

→ komponent / signal

### Feature state

```text
ships
loading
error
filters
```

→ NgRx feature store, jeśli jest współdzielony/złożony

### Global state

```text
currentUser
permissions
app configuration
```

→ globalny Store

### Server state

```text
ships from API
orders
customers
```

Tu trzeba się zastanowić, czy **NgRx rzeczywiście jest potrzebny**, czy lepszy będzie mechanizm przeznaczony do server-state/cache.

---

# 18. Najbardziej rekrutacyjne pytanie

Rekruter pokazuje:

```typescript
loadShips$ = createEffect(() =>
  this.actions$.pipe(
    ofType(loadShips),
    switchMap(() => this.service.getShips().pipe(map((ships) => shipsLoaded({ ships }))))
  )
);
```

I pyta:

> **Ile requestów wykona się, jeśli `loadShips` zostanie dispatchowane 5 razy?**

Odpowiedź:

**Potencjalnie 5.**

Ale ponieważ jest `switchMap`, poprzednia subskrypcja zostanie zastąpiona przez nową.

W przypadku Angular `HttpClient` oznacza to zazwyczaj anulowanie poprzedniego requestu/subskrypcji.

```text
load A ─── HTTP A ───X
load B ───── HTTP B ───X
load C ─────── HTTP C ───X
load D ───────── HTTP D ───X
load E ─────────── HTTP E ─────►
```

To **nie znaczy**, że backend na pewno nie zobaczy wcześniejszych requestów — anulowanie subskrypcji po stronie klienta nie jest tym samym co cofnięcie żądania, które już dotarło do serwera.

---

# 19. Pytanie o równoległość

> Co jeśli użyję `mergeMap`?

```typescript
mergeMap(() => this.service.getShips());
```

Wtedy:

```text
load A ─── HTTP A ─────────►
load B ───── HTTP B ──────►
load C ── HTTP C ─────────►
```

Wszystkie mogą być wykonywane równolegle.

Dlatego wybór operatora jest **decyzją biznesową**, a nie stylistyczną.

---

# 20. Najważniejszy mental model

Zapamiętaj ten jeden diagram:

```text
             USER / EVENT
                   │
                   ▼
                ACTION
                   │
             ┌─────┴─────┐
             ▼           ▼
          REDUCER      EFFECT
             │           │
             │           ▼
             │         API
             │           │
             │           ▼
             │       NEW ACTION
             │           │
             └─────┬─────┘
                   ▼
                 STORE
                   │
                   ▼
               SELECTOR
                   │
                   ▼
               OBSERVABLE
                   │
                   ▼
                ANGULAR
                   │
                   ▼
                   UI
```

I wtedy możesz odpowiadać na większość pytań:

**Action** → _co się wydarzyło?_

**Reducer** → _jak zmienić state?_

**Effect** → _jaki side effect wykonać?_

**Store** → _gdzie trzymamy współdzielony state?_

**Selector** → _jaki fragment state potrzebuję?_

**Observable/RxJS** → _jak reagować na wartości/zdarzenia w czasie?_

**Angular** → _jak pokazać aktualny state użytkownikowi?_

To jest dużo ważniejsze na rozmowie niż znajomość samej składni NgRx.
