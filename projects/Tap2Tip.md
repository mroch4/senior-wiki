# Tap2Tip

## Actors (app users)

- **Receiver** - a person who owns a QR code (token) and receives a tip for a Customer, aka Rc
- **Customer** - a person who scans QR code and gives a tip to the Receiver, aka Cs

## Problems to be solved

### Cashless restaurant guest

1. Dinner costs `135 Cu`
2. Cs appreciate serving
3. Cs says `Please round it up to 150 Cu`
4. Rc puts `150 Cu` into the POS
5. Tip goes to the restaurant owner, not a waiter.

**Note** that it's only for cashless payments. If a Cs pays with cash then the difference (15 Cs) goes to the common jar and would only then be divided among all the waiters equally, which may not be fair.

### Rc's POV

Rc would like to have money on the account immadiately, without boss/company knowing how much he/she was tipped.
That's a case of `Uber` driver essentially, where a Cs can tip a driver after a ride from the app directly.

## Target

- waiters working in a restuarant
- `Uber`, `Bolt` and other 'taxi' drivers
- `UberEats`, `Glovo`, `Wolt` delivery men
- street musicians

## Glossary

**App URL** - https://www.tap2tip.pl

**Cu** - currency unit (for example: PLN, EUR, USD)

**Identity Token** - a unique Base62 identifier (`8f72a91c` for example) shared with a Customer in order to get a tip, aka IdT

**IdT carriers**

- 1st stage: QR code printed out or shown on a mobile phone
- 2nd stage: NFC chip within a wristband or a plastic card

**POS** - point of sale (payment terminal)

## User Flows

### Registering an account

1. Rc enters app URL
2. Rc clicks `Create Account`
3. Rc selects sign in provider (`Google`, `Facebook`)
4. Setup wizard is run
5. Rc inputs a required nickname (public name displayed for validation purposes, i.e. "Anna", "Witalij")
6. Rc inputs a required description (description displayed for validation purposes, i.e. "Starbucks at Main Street, Warsaw", "Uber, Toyota Prius, WA 12345")
7. Rc inputs a required phone number (to enable immediate cash transfer used for "BLIK na telefon", for example)
8. Rc inputs an account number (to enable cash transfer methods)

### Transactions lookup

1. Rc enters app URL
2. Rc signs in
3. Rc clicks `My Account`
4. Rc clicks `Transaction History`
5. Rc sees a table with two columns. Each row contains information (formatted in regards to the current locale, `Pl-pl` for example) about a tip received:

- `Date` - date & time
- `Amount` value followed up by Cu

6. Rc sees the following figures at the top of the page:

- total sum of all the received tips
- last 30 days sum of received tips
- this month's sum of received tips

### Tipping

1. Rc shares the IdT with a Cs: `8f72a91c`
2. Cs scans IdT using mobile phone's camera and gets navigated to the Rc's landing page: `https://www.tap2tip.pl/t/8f72a91c` ('Tip Anna')
3. Cs sees Rc's nickname and description and can validate if receieved token is correct
4. Cs clicks a pill with predefined values like `2 PLN`, `5 PLN`, `10 PLN` or can input a custom value
5. Cs clicks 'Tip' button
6. Cs's bank app is opened to confirm a transaction

## Stack

## Architecture

Solution should follow the principles of a Clean Architecture. Let's start with modular monolith in order to easily migrate to micoservices architecture if the startup succeeds.
