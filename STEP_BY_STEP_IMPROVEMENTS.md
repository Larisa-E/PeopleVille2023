# PeopleVille – Simple Step-by-Step Improvements

This guide keeps the code easy to read and makes village behavior clearer.

---

## Goal

Make it easier to understand:
- what happens on each tick,
- why something happened,
- and how the simulation behaves over time.

---

## Step 1 — Give `Village` clear simulation rules

### Why
Magic numbers are hard to read. Named constants explain intent.

### Add these constants inside `Village` class
```csharp
private const int MinInitialVillagers = 10;
private const int MaxInitialVillagersExclusive = 24;
```

### Replace this line in `CreateVillage()`
```csharp
var villagers = _random.Next(10, 24);
```
with:
```csharp
var villagers = _random.Next(MinInitialVillagers, MaxInitialVillagersExclusive);
```

---

## Step 2 — Make tick behavior easy to follow

### Why
Current `RunTick()` works, but it does not explain edge cases very clearly.

### Replace `RunTick()` in `PeopleVilleEngine/Village.cs`
```csharp
public void RunTick()
{
    _tick++;
    TickHappened?.Invoke(this, _tick);

    if (Villagers.Count == 0)
    {
        RandomEventHappened?.Invoke(this, $"Tick {_tick}: No villagers in village.");
        return;
    }

    if (Locations.Count == 0)
    {
        RandomEventHappened?.Invoke(this, $"Tick {_tick}: No locations available.");
        return;
    }

    var villager = Villagers[_random.Next(Villagers.Count)];
    var location = Locations[_random.Next(Locations.Count)];
    villager.MoveTo(location);

    if (location is LocationBase interactiveLocation)
        interactiveLocation.Interact(villager);

    RandomEventHappened?.Invoke(
        this,
        $"Tick {_tick}: {villager.FirstName} {villager.LastName} moved to {location.Name}. " +
        $"Stats -> IQ:{villager.IQ}, Health:{villager.Health}, Money:{villager.Money}, Food:{villager.Food}");
}
```

Result: every tick now tells a clear story.

---

## Step 3 — Keep `Village` text output focused on useful info

### Why
Good `ToString()` output helps debugging and demos.

### Replace `ToString()` in `PeopleVilleEngine/Village.cs`
```csharp
public override string ToString()
{
    var homelessCount = Villagers.Count(v => !v.HasHome());
    var housedCount = Villagers.Count - homelessCount;

    return $"Village summary: Villagers={Villagers.Count}, Housed={housedCount}, Homeless={homelessCount}, Locations={Locations.Count}.";
}
```

---

## Step 4 — Make Program output beginner-friendly

### Why
Show simulation state and stop condition clearly.

### Replace `PeopleVille/Program.cs` with this
```csharp
using PeopleVilleEngine;

Console.WriteLine("PeopleVille");
var village = new Village();

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine($"[Event] {msg}");

Console.WriteLine(village);
Console.WriteLine("Running 5 demo ticks...\n");

for (int i = 0; i < 5; i++)
{
    village.RunTick();
}

Console.WriteLine();
Console.WriteLine("Final state:");
Console.WriteLine(village);
```

Result: the run is predictable and easier to explain.

---

## Step 5 — Optional: safer random usage style

### Why
Keep calls consistent and avoid repeated static access in constructors.

### In `BaseVillager` constructor, use one local random variable
```csharp
var random = RNG.GetInstance();

if (random.Next(0, 2) == 0)
    Inventory.Add(new PeopleVilleEngine.Items.Shoes());
else
    Inventory.Add(new PeopleVilleEngine.Items.Pants());

IsMale = random.Next(0, 2) == 0;
```

This is a readability improvement only.

---

## Step 6 — Optional: remove confusion from legacy classes

### Why
`Villager.cs` and `Location.cs` are old model classes and can confuse architecture.

### Suggested action
If not used, remove:
- `PeopleVilleEngine/Villagers/Villager.cs`
- `PeopleVilleEngine/Locations/Location.cs`

This keeps architecture aligned with:
- `BaseVillager`/`AdultVillager`/`ChildVillager`
- `ILocation`/`LocationBase`/`School`/`Hospital`/`Shop`/`SimpleHouse`

---

## Recommended order to apply

1. Step 1 (constants)
2. Step 2 (`RunTick` clarity)
3. Step 3 (`ToString` clarity)
4. Step 4 (`Program` output)
5. Step 5 (style)
6. Step 6 (cleanup)

---

## Quick check after each step

- Build solution
- Run app
- Confirm ticks print clear event messages
- Confirm village summary looks correct

That is enough to get clean, simple, understandable behavior quickly.

---

# Add Easy Moves + Events (Console Friendly)

Use this section after the steps above. It adds simple behavior that is easy to explain and looks good in the console.

## Step 7 — Add readable event labels

### Why
Console output is much easier to read when each message has a clear prefix.

### Update your `Village.RunTick()` event text format
Use labels like:
- `[Move]`
- `[Event]`
- `[Social]`
- `[VillageEvent]`

### Example message style
```csharp
RandomEventHappened?.Invoke(this,
    $"[Move] Tick {_tick}: {villager.FirstName} {villager.LastName} moved to {location.Name}.");
```

---

## Step 8 — Add simple move rules before choosing random location

### Why
Moves should feel meaningful, not fully random.

### In `Village.RunTick()`, replace random location selection with this helper call
Replace:
```csharp
var location = Locations[_random.Next(Locations.Count)];
```
with:
```csharp
var location = DecideNextLocation(villager);
```

### Add helper method inside `Village` class
```csharp
private ILocation DecideNextLocation(BaseVillager villager)
{
    // Priority 1: low food -> go shop
    var shop = Locations.FirstOrDefault(l => l is Shop);
    if (villager.Food < 5 && shop != null)
        return shop;

    // Priority 2: low health -> go hospital
    var hospital = Locations.FirstOrDefault(l => l is Hospital);
    if (villager.Health < 3 && hospital != null)
        return hospital;

    // Priority 3: child -> often go school
    var school = Locations.FirstOrDefault(l => l is School);
    if (villager is PeopleVilleEngine.Villagers.ChildVillager && school != null && _random.Next(0, 100) < 70)
        return school;

    // Priority 4: sometimes go home if available
    if (villager.Home != null && _random.Next(0, 100) < 40)
        return villager.Home;

    // Fallback: random
    return Locations[_random.Next(Locations.Count)];
}
```

Result: villagers now move for reasons.

---

## Step 9 — Add one extra event after location interaction

### Why
A second event line makes the simulation feel alive.

### Add this call in `RunTick()` after `interactiveLocation.Interact(villager);`
```csharp
EmitPersonalEvent(villager, location);
```

### Add helper method inside `Village` class
```csharp
private void EmitPersonalEvent(BaseVillager villager, ILocation location)
{
    if (location is School)
    {
        RandomEventHappened?.Invoke(this, $"[Event] {villager.FirstName} learned something new. IQ is now {villager.IQ}.");
        return;
    }

    if (location is Hospital)
    {
        RandomEventHappened?.Invoke(this, $"[Event] {villager.FirstName} got treatment. Health is now {villager.Health}.");
        return;
    }

    if (location is Shop)
    {
        RandomEventHappened?.Invoke(this, $"[Event] {villager.FirstName} visited the shop. Money={villager.Money}, Food={villager.Food}.");
        return;
    }

    if (villager.Home != null && location == villager.Home && villager.Food > 0)
    {
        villager.Food -= 1;
        villager.Health += 1;
        RandomEventHappened?.Invoke(this, $"[Event] {villager.FirstName} ate at home. Food-1, Health+1.");
    }
}
```

Result: each move now has a clear follow-up story.

---

## Step 10 — Add simple social event (small random chance)

### Why
Villagers meeting at same location creates natural variation.

### Add this call near the end of `RunTick()`
```csharp
TryEmitSocialEvent(location);
```

### Add helper method inside `Village` class
```csharp
private void TryEmitSocialEvent(ILocation location)
{
    var atLocation = location.Villagers();
    if (atLocation.Count < 2)
        return;

    // 25% chance
    if (_random.Next(0, 100) >= 25)
        return;

    var first = atLocation[_random.Next(atLocation.Count)];
    var second = atLocation[_random.Next(atLocation.Count)];
    if (ReferenceEquals(first, second))
        return;

    first.Health += 1;
    second.Health += 1;

    RandomEventHappened?.Invoke(this,
        $"[Social] {first.FirstName} met {second.FirstName} at {location.Name}. Both feel better (+1 Health).");
}
```

---

## Step 11 — Add village-wide random event every few ticks

### Why
Global events make the world feel dynamic without much code.

### Add this call at the end of `RunTick()`
```csharp
TryEmitVillageEvent();
```

### Add helper method inside `Village` class
```csharp
private void TryEmitVillageEvent()
{
    // Every 5 ticks, 30% chance of a village event
    if (_tick % 5 != 0 || _random.Next(0, 100) >= 30)
        return;

    // Example: festival
    foreach (var villager in Villagers)
    {
        villager.Health += 1;
    }

    RandomEventHappened?.Invoke(this, "[VillageEvent] Festival day! Everyone gains +1 Health.");
}
```

---

## Step 12 — Keep output clean in `Program.cs`

### Why
You want readable console output, not noisy text.

### Use this event wiring pattern
```csharp
village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine(msg);
```

### Optional color support (easy)
```csharp
village.RandomEventHappened += (_, msg) =>
{
    var original = Console.ForegroundColor;

    if (msg.StartsWith("[VillageEvent]")) Console.ForegroundColor = ConsoleColor.Cyan;
    else if (msg.StartsWith("[Social]")) Console.ForegroundColor = ConsoleColor.Green;
    else if (msg.StartsWith("[Event]")) Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine(msg);
    Console.ForegroundColor = original;
};
```

---

## Suggested apply order for new behavior

1. Step 7 (labels)
2. Step 8 (move rules)
3. Step 9 (personal events)
4. Step 10 (social events)
5. Step 11 (village-wide events)
6. Step 12 (clean output)

After each step:
- Build
- Run
- Watch if console story is clear
- Keep only behavior that stays easy to understand

---

# Fix Pass (Apply these now to clean current output)

These steps fix the exact issues you currently see in console output.

## Step 13 — Remove duplicated `[Event]` prefix in Program

### File
`PeopleVille/Program.cs`

### Why
Your messages already include `[Event]` from `Village.cs`, so writing `[Event] {msg}` prints `[Event] [Event] ...`.

### Find
```csharp
village.RandomEventHappened += (_, msg) => Console.WriteLine($"[Event] {msg}");
```

### Replace with
```csharp
village.RandomEventHappened += (_, msg) => Console.WriteLine(msg);
```

---

## Step 14 — Fix no-location guard in `RunTick()`

### File
`PeopleVilleEngine/Village.cs`

### Why
If there are no locations, code should stop the tick early. Without `return`, it can still try to pick a location.

### Find in `RunTick()`
```csharp
if (Locations.Count == 0)
{
    RandomEventHappened?.Invoke(this, $"Tick {_tick}: No locations available.");
}
```

### Replace with
```csharp
if (Locations.Count == 0)
{
    RandomEventHappened?.Invoke(this, $"Tick {_tick}: No locations available.");
    return;
}
```

---

## Step 15 — Fix summary text typos

### File
`PeopleVilleEngine/Village.cs`

### Why
Output should be clean and professional.

### Find in `ToString()`
```csharp
return $"Village summary: Villagers={Villagers.Count} Housed={houseCount}, Homeless={homelessCount}, Lications={Locations.Count}.";
```

### Replace with
```csharp
return $"Village summary: Villagers={Villagers.Count}, Housed={houseCount}, Homeless={homelessCount}, Locations={Locations.Count}.";
```

---

## Step 16 — Make villager count limits consistent constants

### File
`PeopleVilleEngine/Village.cs`

### Why
Both min and max should be named constants for readability.

### Find fields
```csharp
private const int MinVillagers = 10;
private int MaxVillagers = 24;
```

### Replace with
```csharp
private const int MinVillagers = 10;
private const int MaxVillagersExclusive = 24;
```

### Then find in `CreateVillage()`
```csharp
var villagers = _random.Next(MinVillagers, MaxVillagers);
```

### Replace with
```csharp
var villagers = _random.Next(MinVillagers, MaxVillagersExclusive);
```

---

## Step 17 — Remove unused using

### File
`PeopleVilleEngine/Village.cs`

### Why
Unused imports make files noisy.

### Remove this line
```csharp
using System.Runtime.Serialization;
```

---

## Optional polish — tune shop cost so behavior is more varied

### File
`PeopleVilleEngine/Locations/Shop.cs`

### Why
Current cost is high (`100`), so villagers quickly hit `Money=0` and all look the same in output.

### Find
```csharp
if (villager.Money >= 100)
{
    villager.Money -= 100;
    villager.Food += 10;
}
```

### Replace with
```csharp
if (villager.Money >= 20)
{
    villager.Money -= 20;
    villager.Food += 5;
}
```

This keeps events active for longer and looks better in console logs.

---

## Quick validation after this fix pass

1. Build
2. Run
3. Confirm:
   - no `[Event] [Event]` duplicates
   - `Locations` spelled correctly in summary
   - no crashes if locations list is empty
   - more varied shop behavior if optional polish is applied

---

# Threading Story (Do this manually, step by step)

This section is only documentation. Apply these changes yourself in the files listed.

## What threads/tasks you will have

### Thread/Task A — Simulation loop (in `Program.cs`)
- **Where:** `PeopleVille/Program.cs`
- **Job:** runs `village.RunTick()` every 500 ms.
- **Why:** keeps village alive in background.

### Thread/Task B — Monitor loop (in `Program.cs`)
- **Where:** `PeopleVille/Program.cs`
- **Job:** prints village summary every 2 seconds.
- **Why:** gives live status while simulation runs.

### Synchronization lock — State safety (in `Village.cs`)
- **Where:** `PeopleVilleEngine/Village.cs`
- **Job:** `lock (_sync)` around `RunTick()` logic.
- **Why:** only one updater at a time can edit villagers/locations.

---

## Step T1 — Prepare `Village.cs` for safe ticking

### File
`PeopleVilleEngine/Village.cs`

### Add fields inside `class Village`
```csharp
private readonly object _sync = new();
private CancellationTokenSource? _simulationCts;
private Task? _simulationTask;
public bool IsSimulationRunning => _simulationTask is { IsCompleted: false };
```

### Why
- `_sync` is the gatekeeper.
- `_simulationCts` is the stop signal.
- `_simulationTask` is the running background engine.

---

## Step T2 — Make `RunTick()` thread-safe

### File
`PeopleVilleEngine/Village.cs`

### Replace `RunTick()` with
```csharp
public void RunTick()
{
    lock (_sync)
    {
        _tick++;
        TickHappened?.Invoke(this, _tick);

        if (Villagers.Count == 0)
        {
            RandomEventHappened?.Invoke(this, $"[Event] Tick {_tick}: No villagers in village.");
            return;
        }

        if (Locations.Count == 0)
        {
            RandomEventHappened?.Invoke(this, $"[Event] Tick {_tick}: No locations available.");
            return;
        }

        var villager = Villagers[_random.Next(Villagers.Count)];
        var location = DecideNextLocation(villager);
        villager.MoveTo(location);

        if (location is LocationBase interactiveLocation)
            interactiveLocation.Interact(villager);

        EmitPersonalEvent(villager, location);
        TryEmitSocialEvent(location);
        TryEmitVillageEvent();

        RandomEventHappened?.Invoke(
            this,
            $"[Move] Tick {_tick}: {villager.FirstName} {villager.LastName} moved to {location.Name}. " +
            $"Stats -> IQ:{villager.IQ}, Health:{villager.Health}, Money:{villager.Money}, Food:{villager.Food}");
    }
}
```

### Why
This prevents race conditions when background tasks and other code touch village state.

---

## Step T3 — Add start/stop simulation methods

### File
`PeopleVilleEngine/Village.cs`

### Add methods
```csharp
public void StartSimulation(TimeSpan tickInterval)
{
    lock (_sync)
    {
        if (_simulationTask is { IsCompleted: false })
            return;

        _simulationCts = new CancellationTokenSource();
        _simulationTask = SimulationLoopAsync(tickInterval, _simulationCts.Token);
    }
}

public async Task StopSimulationAsync()
{
    CancellationTokenSource? cts;
    Task? simulationTask;

    lock (_sync)
    {
        cts = _simulationCts;
        simulationTask = _simulationTask;
    }

    if (cts == null || simulationTask == null)
        return;

    cts.Cancel();

    try
    {
        await simulationTask;
    }
    catch (OperationCanceledException)
    {
        // expected when stopping simulation
    }
    finally
    {
        lock (_sync)
        {
            _simulationCts?.Dispose();
            _simulationCts = null;
            _simulationTask = null;
        }
    }
}

private async Task SimulationLoopAsync(TimeSpan tickInterval, CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        RunTick();
        await Task.Delay(tickInterval, token);
    }
}
```

### Why
- `StartSimulation` starts engine once.
- `StopSimulationAsync` stops gracefully.
- `Task.Delay` controls tick pace.

### Story explanation (simple)

Think of this like turning on and off a village machine:

1. **`StartSimulation(...)`**
   - Lock the control panel (`lock (_sync)`) so two people cannot press Start at the same time.
   - If machine is already running, do nothing.
   - Create a stop remote (`CancellationTokenSource`).
   - Start the background loop task and keep a reference to it.

2. **`SimulationLoopAsync(...)`**
   - Loop forever until stop is requested.
   - Each loop does one `RunTick()`.
   - Then waits (`Task.Delay`) for the interval (for example 500 ms).

3. **`StopSimulationAsync()`**
   - Read current stop remote/task safely.
   - If nothing is running, return.
   - Call `Cancel()` to ask loop to stop.
   - `await simulationTask` to wait for clean shutdown.
   - Clear references and dispose resources in `finally`.

### Line-by-line key points

- `if (_simulationTask is { IsCompleted: false }) return;`
  - prevents double-start.
- `_simulationCts = new CancellationTokenSource();`
  - creates cancel signal.
- `_simulationTask = SimulationLoopAsync(...);`
  - launches background simulation loop.
- `cts.Cancel();`
  - sends stop request.
- `await simulationTask;`
  - waits until loop exits.
- `catch (OperationCanceledException)`
  - expected when delay is canceled.
- `_simulationCts = null; _simulationTask = null;`
  - reset state so simulation can start again later.

---

## Step T4 — Wire both tasks in `Program.cs`

### File
`PeopleVille/Program.cs`

### Replace content with
```csharp
using PeopleVilleEngine;

Console.WriteLine("PeopleVille");
var village = new Village();

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine(msg);

Console.WriteLine(village);
village.StartSimulation(TimeSpan.FromMilliseconds(500));

using var monitorCts = new CancellationTokenSource();
var monitorTask = Task.Run(async () =>
{
    while (!monitorCts.Token.IsCancellationRequested)
    {
        Console.WriteLine($"[Monitor] {village}");
        await Task.Delay(2000, monitorCts.Token);
    }
}, monitorCts.Token);

Console.WriteLine("Simulation started in background.");
Console.WriteLine("Press ENTER to stop...\n");
Console.ReadLine();

monitorCts.Cancel();
await village.StopSimulationAsync();

try
{
    await monitorTask;
}
catch (OperationCanceledException)
{
    // expected on shutdown
}

Console.WriteLine();
Console.WriteLine("Final state:");
Console.WriteLine(village);
```

### Why
Now you have two simple background tasks:
1. simulation ticks,
2. status monitor.

### Story explanation (simple)

`Program.cs` is now the "control room":

- It starts the village simulation.
- It starts a second monitor task that prints summary every 2 seconds.
- It waits for ENTER from user.
- It sends stop signal and waits for both tasks to finish cleanly.

This gives you a live simulation without blocking the app.

### Which thread/task is where

- **Task A (in `Village.cs`)**: `SimulationLoopAsync` → does game ticks.
- **Task B (in `Program.cs`)**: `monitorTask` → prints `[Monitor]` lines.
- **Main thread (in `Program.cs`)**: waits for ENTER and coordinates shutdown.

---

## Step T5 — Quick test checklist

1. Build.
2. Run.
3. Confirm tick lines appear every ~500 ms.
4. Confirm monitor lines appear every ~2 s.
5. Press ENTER.
6. Confirm app shuts down cleanly.

