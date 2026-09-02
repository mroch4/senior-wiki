# 1

**Angular Standalone Components** to podejście, w którym komponent Angulara **nie musi należeć do `NgModule`** (`@NgModule`). Komponent deklaruje samodzielnie, jakich innych komponentów, dyrektyw czy pipe'ów potrzebuje.

### Klasyczne podejście

Dawniej często wyglądało to tak:

```typescript
@NgModule({
  declarations: [AppComponent, UserComponent],
  imports: [BrowserModule, FormsModule],
})
export class AppModule {}
```

Komponent sam nie mówił, czego potrzebuje — robił to moduł.

### Standalone component

```typescript
@Component({
  selector: "app-user",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./user.component.html",
})
export class UserComponent {}
```

Teraz `UserComponent` jest **samowystarczalny**. W `imports` określa bezpośrednio swoje zależności.

Można też importować inne standalone components:

```typescript
@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [UserComponent, HeaderComponent],
  template: `
    <app-header />
    <app-user />
  `,
})
export class DashboardComponent {}
```

### Bootstrap aplikacji

Nie potrzebujesz już koniecznie `AppModule`:

```typescript
bootstrapApplication(AppComponent);
```

Zamiast:

```typescript
platformBrowserDynamic().bootstrapModule(AppModule);
```

### Dlaczego Angular to wprowadził?

Najważniejsze korzyści:

- **mniej boilerplate'u** — mniej `NgModule`
- **jawne zależności** — patrzysz na `imports` komponentu i wiesz, czego używa
- **łatwiejszy lazy loading**
- **lepsza tree-shaking / mniejszy bundle** w odpowiednich przypadkach
- prostsza struktura dużych aplikacji

### W praktyce

W nowoczesnym Angularze możesz mieć strukturę:

```text
AppComponent
 ├── HeaderComponent
 ├── NavigationComponent
 └── DashboardComponent
      ├── ShipComponent
      ├── MaintenanceComponent
      └── EventsComponent
```

Każdy komponent może być standalone i importować tylko to, czego faktycznie potrzebuje.

**Na rozmowie rekrutacyjnej** warto powiedzieć:

> „Standalone components pozwalają tworzyć komponenty niezależne od NgModules. Komponent sam deklaruje swoje zależności w `imports`, a aplikację można uruchomić przez `bootstrapApplication`. W nowych wersjach Angulara jest to preferowane podejście i upraszcza architekturę oraz lazy loading.”

Jedna ważna rzecz: `standalone: true` było początkowo opcjonalnym mechanizmem, ale **w nowszych Angularach standalone jest domyślnym podejściem**.

---

# 2

W Angularze **moduły (`NgModule`)** służą do grupowania powiązanych elementów aplikacji: komponentów, dyrektyw, pipe'ów i zależności.

### 1. Podstawowy moduł

Przykładowo aplikacja może mieć:

```text
AppModule
 ├── Components
 ├── Services
 └── Modules
      ├── UsersModule
      ├── ShipsModule
      └── MaintenanceModule
```

Moduł:

```typescript
@NgModule({
  declarations: [UserComponent, UserListComponent],
  imports: [CommonModule],
  exports: [UserComponent],
})
export class UsersModule {}
```

- `declarations` — rzeczy należące do tego modułu
- `imports` — moduły, których ten moduł potrzebuje
- `exports` — elementy, które udostępniamy innym modułom
- `providers` — dependency injection

### 2. Zagnieżdżone moduły

Moduł może importować inny moduł.

Na przykład:

```text
AppModule
│
├── UsersModule
│    └── SharedModule
│
├── ShipsModule
│    └── SharedModule
│
└── MaintenanceModule
     └── SharedModule
```

`UsersModule`:

```typescript
@NgModule({
  declarations: [UserComponent],
  imports: [CommonModule, SharedModule],
})
export class UsersModule {}
```

Czyli:

```text
AppModule
   ↓
UsersModule
   ↓
SharedModule
```

`UsersModule` może korzystać z tego, co `SharedModule` eksportuje.

---

### 3. `imports` vs `exports`

To bardzo częste pytanie na rozmowie.

Jeżeli mamy:

```typescript
@NgModule({
  declarations: [ButtonComponent],
  exports: [ButtonComponent],
})
export class SharedModule {}
```

i:

```typescript
@NgModule({
  declarations: [UserComponent],
  imports: [SharedModule],
})
export class UsersModule {}
```

to `UserComponent` może używać:

```html
<app-button></app-button>
```

Dlaczego?

Bo:

```text
SharedModule
   │
   ├── declares ButtonComponent
   │
   └── exports ButtonComponent
             ↓
        UsersModule
             ↓
       UserComponent
```

Samo `declarations` **nie wystarczy**. Jeśli chcesz, żeby komponent był dostępny poza modułem, musi być odpowiednio `exported`.

---

### 4. `declarations` mają ograniczony zakres

To ważna zasada:

**Komponent może być zadeklarowany tylko w jednym `NgModule`.**

Czyli nie możesz zrobić:

```typescript
@NgModule({
  declarations: [UserComponent],
})
export class UsersModule {}
```

i jednocześnie:

```typescript
@NgModule({
  declarations: [UserComponent], // ❌
})
export class AdminModule {}
```

Zamiast tego możesz wyeksportować go z `UsersModule` i zaimportować `UsersModule` w `AdminModule`.

---

### 5. Feature modules

W większych aplikacjach często robiło się moduły według funkcjonalności:

```text
app/
├── core/
│   └── core.module.ts
│
├── shared/
│   └── shared.module.ts
│
├── ships/
│   ├── ships.module.ts
│   ├── ship-list/
│   └── ship-details/
│
├── maintenance/
│   ├── maintenance.module.ts
│   └── ...
│
└── app.module.ts
```

Na przykład:

```text
ShipsModule
 ├── ShipListComponent
 ├── ShipDetailsComponent
 ├── ShipService
 └── ShipRoutes
```

To jest **feature module** — moduł reprezentujący konkretny obszar biznesowy.

---

### 6. Lazy-loaded modules

Dużą zaletą takiego podziału było **lazy loading**.

Na przykład użytkownik wchodzi na:

```text
/ships
```

Angular może dopiero wtedy załadować:

```text
ShipsModule
```

Zamiast ładować całą aplikację od razu:

```text
Browser
  ↓
AppModule
  ↓
ShipsModule
MaintenanceModule
AdminModule
ReportsModule
...
```

ładowane jest tylko to, czego potrzeba:

```text
Browser
  ↓
AppModule
  ↓
ShipsModule   ← dopiero gdy /ships
```

---

### A jak to wygląda dzisiaj?

To jest istotne w kontekście Twojego poprzedniego pytania o **standalone components**.

Starszy Angular:

```text
AppModule
 ├── ShipsModule
 │    ├── ShipListComponent
 │    └── ShipDetailsComponent
 │
 └── MaintenanceModule
```

Nowoczesny Angular może wyglądać bardziej jak:

```text
AppComponent
 ├── ShipsComponent
 │    ├── ShipListComponent
 │    └── ShipDetailsComponent
 │
 └── MaintenanceComponent
```

bez konieczności tworzenia `NgModule` dla każdego feature'a.

**Czyli `NgModule` nadal istnieje i możesz go spotkać w starszych/dużych projektach, ale standalone components są obecnie preferowanym podejściem.**

Na rozmowie warto rozróżnić:

> **Module nesting** — jeden `NgModule` importuje drugi i dzięki `exports` udostępnia jego elementy dalej.
> **Standalone** — komponent sam deklaruje swoje zależności i nie potrzebuje być deklarowany w `NgModule`.
