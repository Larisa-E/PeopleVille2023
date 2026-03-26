# PeopleVille – My Project Story

## 1) The goal
I wanted to build a small C# city simulation where citizens and locations are objects. Citizens should move, trade money and items, and interact visibly in the console. I also needed to show OOP concepts (abstract class, override, interfaces), events/delegates, error handling, and modding support.

---

## 2) How I worked
I worked in small steps. I did not try to solve everything at once.

First, I built the core city model. Then I added movement. After that, I added item and money transactions. Next, I added events so the simulation had a clear timeline. Finally, I improved stability with try/catch and kept plugin support through DLL loading.

I used this workflow because it made debugging easier and helped me see progress after every build.

---

## 3) The story of what I added

### Step 1: I created the city foundation
I used `Village` as the main coordinator. It creates villagers, keeps locations, and controls simulation flow.

I used:
- `Village` for orchestration
- `BaseVillager` as a shared villager model
- `ILocation` as a contract for location behavior

This gave me a clean starting structure.

### Step 2: I made locations smarter with OOP
I added `LocationBase` as an abstract class, then let `School`, `Hospital`, and `Shop` inherit from it.

Each location overrides `Interact(...)`:
- School increases IQ
- Hospital increases Health
- Shop handles money/food changes

I added this to clearly demonstrate abstract class + override in real behavior.

### Step 3: I used interfaces to keep code flexible
I used interfaces as contracts:
- `IService` for service-like locations
- `ILocation` for movement and location handling
- `ITrader` for trading participants
- `IVillagerCreator` for extension/modding creators

I did this to keep the design loosely coupled and easier to extend.

### Step 4: I added simulation time with events
In `Village.cs` I added:
- `TickHappened`
- `RandomEventHappened`
- `_tick`
- `RunTick()`

What these do:
- `TickHappened` sends the current tick number
- `RandomEventHappened` sends readable event text
- `_tick` stores the current simulation step
- `RunTick()` advances time, moves a random villager, runs interaction, and raises both events

I added this because events/delegates were required, and they make the simulation easy to follow.

### Step 5: I implemented movement
I added `MoveTo(ILocation destination)` and `CurrentLocation` in `BaseVillager`.
Then I used them inside `RunTick()` to move villagers between random locations.

I added this to satisfy the movement requirement and make behavior realistic.

### Step 6: I added random starting items
In the villager constructor, I added a random starter item (for example Shoes or Pants) to `Inventory`.

I added this because the assignment requires citizens to start with random items.

### Step 7: I added trading
I created `TransactionLogic` with:
- `TransferMoney(...)`
- `TransferItem(...)`

I also added a simple trade demo in `Program.cs` so results are visible in console.

I added this to satisfy money + item transactions between citizens.

### Step 8: I made interactions visible in console
In `Program.cs`, I subscribed to events and printed:
- ticks
- random events
- transaction results

I added this because no heavy GUI was required, but interactions must be visible.

### Step 9: I improved error handling
I added try/catch in two important places:
- plugin DLL loading (`Village.cs`)
- JSON name loading (`VillagerNames.cs`)

I added this so one bad file/plugin does not crash the whole app.

### Step 10: I kept modding support
I kept loading creator classes from DLLs in `lib`, using `IVillagerCreator` and reflection.

I added/kept this so new features can be plugged in later without changing the core engine.

---

## 4) Feature-to-file map (where each part is in code)

- **City and citizens as objects**
  - `PeopleVilleEngine/Village.cs`
  - `PeopleVilleEngine/Villagers/BaseVillager.cs`
  - `PeopleVilleEngine/Villagers/AdultVillager.cs`
  - `PeopleVilleEngine/Villagers/ChildVillager.cs`
  - `PeopleVilleEngine/Locations/ILocation.cs`
  - `PeopleVilleEngine/Locations/SimpleHouse.cs`

- **Movement between locations**
  - `PeopleVilleEngine/Villagers/BaseVillager.cs` (`CurrentLocation`, `MoveTo(...)`)
  - `PeopleVilleEngine/Village.cs` (`RunTick()` movement logic)

- **Items, money, and citizen-to-citizen trade**
  - `PeopleVilleEngine/Items/Item.cs`
  - `PeopleVilleEngine/Items/Clothes.cs`
  - `PeopleVilleEngine/Villagers/BaseVillager.cs` (`Money`, `Inventory`, starter items)
  - `PeopleVilleEngine/Trading/ITrader.cs`
  - `PeopleVilleEngine/Trading/TransactionLogic.cs`
  - `PeopleVille/Program.cs` (trade demo output)

- **Special locations with different behavior**
  - `PeopleVilleEngine/Locations/LocationBase.cs`
  - `PeopleVilleEngine/Locations/School.cs`
  - `PeopleVilleEngine/Locations/Hospital.cs`
  - `PeopleVilleEngine/Locations/Shop.cs`
  - `PeopleVilleEngine/Village.cs` (`AddCoreLocations()`)

- **Events and timeline in simulation**
  - `PeopleVilleEngine/Village.cs` (`TickHappened`, `RandomEventHappened`, `_tick`, `RunTick()`)
  - `PeopleVille/Program.cs` (event subscriptions and console logs)

- **Error handling (`try/catch`)**
  - `PeopleVilleEngine/Village.cs` (plugin DLL loading)
  - `PeopleVilleEngine/VillagerNames.cs` (JSON file parsing/loading)

- **Plugin/modding support**
  - `PeopleVilleEngine/Village.cs` (DLL scan + reflection loading)
  - `PeopleVilleEngine/Villagers/Creators/IVillagerCreator.cs` (plugin contract)
  - `PeopleVilleVillagerHomeless/Creator/HomelessVillageCreator.cs` (example plugin)

---

## 5) Why this structure is beginner-friendly
I separated responsibilities so each file has one clear role:

- `Village` → controls simulation
- `BaseVillager` → citizen state and movement
- `LocationBase` + derived classes → location-specific behavior
- `TransactionLogic` → trade rules
- `Program.cs` → console demo

This makes the project easier to explain, test, and debug.

---

## 6) Work summary
I worked in a feature branch, split work into small tasks, implemented them one by one, and checked the result with build and runtime output.

---

## 7) Final result
I completed a working PeopleVille simulation that demonstrates movement, transactions, events, error handling, OOP principles, and modding support, aligned with the assignment requirements.
