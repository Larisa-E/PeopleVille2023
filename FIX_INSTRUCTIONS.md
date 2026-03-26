# PeopleVille Fix Instructions + Ready-to-Paste Code

Copy these versions into the matching files.

## 🔴 LATEST UPDATE (Use this section first)

This section reflects the **current state in your workspace now** and what is still needed to finish all assignment points.

### 1) Fix logic bug in `Hospital.cs`

**Problem**
- Current code has `if (!_villagers.Contains(villager));` (extra `;`).
- That makes `_villagers.Add(villager);` run every time.

**Replace file `PeopleVilleEngine/Locations/Hospital.cs` with:**

```csharp
namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;

public class Hospital : LocationBase, IService
{
    public override string Name => "Hospital";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager))
            _villagers.Add(villager);

        villager.Health++;
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
```

### 2) Fix logic bug + cleanup in `Shop.cs`

**Problem**
- Same extra `;` bug after `if`.
- Unused `using` lines.

**Replace file `PeopleVilleEngine/Locations/Shop.cs` with:**

```csharp
namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;

public class Shop : LocationBase, IService
{
    public override string Name => "Shop";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager))
            _villagers.Add(villager);

        if (villager.Money >= 100)
        {
            villager.Money -= 100;
            villager.Food += 10;
        }
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
```

### 3) Fix JSON file name + add try/catch in `VillagerNames.cs`

**Problem**
- Uses `name.json` instead of `names.json`.
- No parse safety/validation.

**In `PeopleVilleEngine/VillagerNames.cs`, inside `LoadNamesFromJsonFile()`, use this block:**

```csharp
private void LoadNamesFromJsonFile()
{
    string jsonFile = Path.Combine(AppContext.BaseDirectory, "lib", "names.json");
    if (!File.Exists(jsonFile))
        jsonFile = Path.Combine(Directory.GetCurrentDirectory(), "lib", "names.json");

    if (!File.Exists(jsonFile))
        throw new FileNotFoundException(jsonFile);

    string jsonData = File.ReadAllText(jsonFile);

    try
    {
        var namesData = JsonSerializer.Deserialize<NamesData>(jsonData);
        if (namesData?.MaleFirstNames == null || namesData.FemaleFirstNames == null || namesData.LastNames == null)
            throw new InvalidDataException("Invalid names.json format.");

        _maleFirstNames = namesData.MaleFirstNames;
        _femaleFirstNames = namesData.FemaleFirstNames;
        _lastNames = namesData.LastNames;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not load names.json: {ex.Message}");
        throw;
    }
}
```

### 4) Add simulation events + tick behavior in `Village.cs`

**Problem**
- Assignment asks for explicit event/delegate usage; currently missing.

**Add these members inside class `Village`:**

```csharp
public event EventHandler<string>? RandomEventHappened;
public event EventHandler<int>? TickHappened;
private int _tick;
```

**Add this method in `Village`:**

```csharp
public void RunTick()
{
    _tick++;
    TickHappened?.Invoke(this, _tick);

    if (Villagers.Count == 0 || Locations.Count == 0)
        return;

    var villager = Villagers[_random.Next(Villagers.Count)];
    var location = Locations[_random.Next(Locations.Count)];
    villager.MoveTo(location);

    if (location is LocationBase interactiveLocation)
        interactiveLocation.Interact(villager);

    RandomEventHappened?.Invoke(this, $"Tick {_tick}: {villager.FirstName} moved to {location.Name}.");
}
```

### 5) Add random starting item in `BaseVillager.cs`

**Problem**
- Assignment asks for random starting items; inventory exists but starts empty.

**In constructor `BaseVillager(Village village)`, append:**

```csharp
if (RNG.GetInstance().Next(0, 2) == 0)
    Inventory.Add(new PeopleVilleEngine.Items.Shoes());
else
    Inventory.Add(new PeopleVilleEngine.Items.Pants());
```

**Why this way**
- No extra architecture changes needed.
- Gives visible, real random starting item behavior.

### 6) Fix transaction API typo and use it in demo

**Problem**
- In `TransactionLogic.cs` method is `TansferMoney` (typo).

**Rename to:**

```csharp
public static bool TransferMoney(ITrader from, ITrader to, int amount)
```

### 7) Update `Program.cs` for visible console interactions (events + transaction)

**Replace `PeopleVille/Program.cs` with:**

```csharp
using PeopleVilleEngine;
using PeopleVilleEngine.Trading;

Console.WriteLine("PeopleVille");

var village = new Village();
Console.WriteLine(village);

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine($"[Event] {msg}");

for (int i = 0; i < 5; i++)
{
    village.RunTick();
}

if (village.Villagers.Count >= 2)
{
    var v1 = village.Villagers[0];
    var v2 = village.Villagers[1];

    var moneyTransferred = TransactionLogic.TransferMoney(v1, v2, 10);
    Console.WriteLine($"[Trade] Money transfer success: {moneyTransferred}");

    if (v1.Inventory.Count > 0)
    {
        var itemTransferred = TransactionLogic.TransferItem(v1, v2, v1.Inventory[0]);
        Console.WriteLine($"[Trade] Item transfer success: {itemTransferred}");
    }
}
```

### 8) Optional quality cleanup (recommended)

- Remove legacy duplicate model files if unused:
  - `PeopleVilleEngine/Locations/Location.cs`
  - `PeopleVilleEngine/Villagers/Villager.cs`
- If you keep them, mark clearly as legacy to avoid confusion in oral defense.

---

## Expected assignment status after applying section above

- Random starting items ✅
- Locations as objects ✅
- Citizens as objects and movement ✅
- Money + item transactions ✅
- Console-visible interactions ✅
- Events + delegates ✅
- Try/catch error handling ✅
- Extension/modding idea ✅
- Abstract + override + interface clearly used ✅

## Status check for files you said you already updated

Checked current workspace state:

- `PeopleVilleEngine/Items/Item.cs` → **NOT fixed yet**
  - Still uses typo namespace `PeopleVilleEngime.Items`.
  - Should be `PeopleVilleEngine.Items`.

- `PeopleVilleEngine/Items/Clothes.cs` → **NOT fixed yet**
  - `Clothes` still does not inherit from `Item`.
  - Constructor still ignores `name` and `value`.

- `PeopleVilleEngine/Locations/Hospital.cs` → **Partially OK**
  - Service logic is fine.
  - Can be cleaned: remove unused `using` statements.

- `PeopleVilleEngine/Locations/School.cs` → **Partially OK**
  - Service logic is fine.
  - Can be cleaned: remove unused `using` statements.

- `PeopleVilleEngine/Locations/Shop.cs` → **NOT fixed yet**
  - Still imports `PeopleVilleEngime.Items` (typo namespace).
  - Has several unused `using` statements.

- `PeopleVilleEngine/Locations/Location.cs` → **Still legacy/duplicate model**
  - This is the older `Location + Villager` model.
  - Keep only if intentionally legacy; otherwise delete.

- `PeopleVilleEngine/Locations/ILocation.cs` → **OK**
  - Interface shape is consistent with current `BaseVillager` model.

- `PeopleVilleEngine/Services/IService.cs` → **OK**
  - Interface contract is correct.
  - Minor cleanup possible (using is not needed for `BaseVillager` in global namespace).

- `PeopleVilleEngine/Villagers/Villager.cs` → **Still legacy/duplicate model**
  - Duplicates concept already represented by `BaseVillager` and derived types.
  - Keep only if intentionally legacy; otherwise delete.

Build check: **passes**.

## Assignment check (based on current code)

Below is the practical status for your assignment criteria in the current workspace.

- Random starting items: **Missing/weak**
  - `Item`/`Clothes` exist but are not integrated into villager inventory.
- Locations as objects: **Partially done**
  - `SimpleHouse` is used, but `School/Hospital/Shop` are services, not integrated into movement/location flow.
- Citizens as objects and movement: **Partially done**
  - Citizens are objects, but explicit movement simulation per tick is not implemented.
- Money + item transactions: **Partially done**
  - Money/food fields exist, but no reusable transaction module (money transfer + item transfer).
- Visible interactions in console: **Partially done**
  - Village output exists, but no tick-based simulation logs/events.
- Events + delegates (Tick, RandomEvent, handler): **Missing**
  - No event-driven simulation loop in current code.
- Error handling with try/catch: **Weak**
  - Plugin loading and JSON parsing are not wrapped with friendly handling everywhere.
- Extension/modding idea with DLLs: **Present but fragile**
  - DLL loading exists, but needs safe guards.
- Abstract + override + interface clearly used: **Partially done**
  - `BaseVillager` abstract and override are present.
  - Need clearer override-driven behavior for assignment demonstration.

## What to add for the assignment (with direct code)

Use these additions if you want a clear, demonstrable assignment implementation.

### A) Add trader interface

Create `PeopleVilleEngine/Trading/ITrader.cs`

```csharp
namespace PeopleVilleEngine.Trading;

using PeopleVilleEngine.Items;

public interface ITrader
{
    int Money { get; set; }
    List<Item> Inventory { get; }
}
```

**Why**
- Gives concrete interface usage in transactions.
- Makes villagers tradable without tight coupling.

### B) Add transaction logic

Create `PeopleVilleEngine/Trading/TransactionLogic.cs`

```csharp
namespace PeopleVilleEngine.Trading;

using PeopleVilleEngine.Items;

public static class TransactionLogic
{
    public static bool TransferMoney(ITrader from, ITrader to, int amount)
    {
        if (amount <= 0 || from.Money < amount) return false;
        from.Money -= amount;
        to.Money += amount;
        return true;
    }

    public static bool TransferItem(ITrader from, ITrader to, Item item)
    {
        if (!from.Inventory.Contains(item)) return false;
        from.Inventory.Remove(item);
        to.Inventory.Add(item);
        return true;
    }
}
```

**Why**
- Covers assignment requirement for money/item transactions.
- Easy to demo in console.

### C) Extend `BaseVillager` for inventory + movement + trading

Update `PeopleVilleEngine/Villagers/BaseVillager.cs`

```csharp
using PeopleVilleEngine;
using PeopleVilleEngine.Items;
using PeopleVilleEngine.Locations;
using PeopleVilleEngine.Trading;

public abstract class BaseVillager : ITrader
{
    public int Age { get; protected set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsMale { get; set; }
    public ILocation? Home { get; set; } = null;
    public ILocation? CurrentLocation { get; private set; }
    public int IQ { get; set; } = 100;
    public int Money { get; set; } = 100;
    public int Food { get; set; } = 0;
    public int Health { get; set; } = 0;
    public List<Item> Inventory { get; } = new();

    public bool HasHome() => Home != null;

    protected BaseVillager(Village village)
    {
        IsMale = RNG.GetInstance().Next(0, 2) == 0;
        (FirstName, LastName) = village.VillagerNameLibrary.GetRandomNames(IsMale);
    }

    public void MoveTo(ILocation destination)
    {
        CurrentLocation = destination;
    }

    public override string ToString() => $"{FirstName} {LastName} ({Age} years)";
}
```

**Why**
- Makes villager movement explicit.
- Uses interface implementation (`ITrader`) in real code.

### D) Add tick event + random event in `Village`

Update `PeopleVilleEngine/Village.cs` with event members and tick method:

```csharp
public event EventHandler<string>? RandomEventHappened;
public event EventHandler<int>? TickHappened;
private int _tick;

public void RunTick()
{
    _tick++;
    TickHappened?.Invoke(this, _tick);

    if (Villagers.Count == 0 || Locations.Count == 0) return;

    var villager = Villagers[_random.Next(Villagers.Count)];
    var location = Locations[_random.Next(Locations.Count)];
    villager.MoveTo(location);

    RandomEventHappened?.Invoke(this, $"Tick {_tick}: {villager.FirstName} moved to {location.Name}.");
}
```

**Why**
- Satisfies event/delegate requirement clearly (`TickHappened`, `RandomEventHappened`).
- Produces visible simulation output.

### E) Console demo wiring in `Program.cs`

Replace program body with:

```csharp
using PeopleVilleEngine;

Console.WriteLine("PeopleVille");
var village = new Village();

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine($"[Event] {msg}");

Console.WriteLine(village);
for (int i = 0; i < 5; i++)
{
    village.RunTick();
}
```

**Why**
- Gives a direct, observable assignment demo in console without GUI.

### F) Required try/catch hardening

1) In `Village.cs`, wrap plugin load per DLL:

```csharp
foreach (var libraryFile in libraryFiles)
{
    try
    {
        LoadVillagerCreatorFactoriesFromType(Assembly.LoadFrom(libraryFile).ExportedTypes, villageCreators, loadedCreatorTypeNames);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Plugin load failed for '{libraryFile}': {ex.Message}");
    }
}
```

2) In `VillagerNames.cs`, wrap JSON parse with friendly message + rethrow:

```csharp
try
{
    var namesData = JsonSerializer.Deserialize<NamesData>(jsonData);
    if (namesData?.MaleFirstNames == null || namesData.FemaleFirstNames == null || namesData.LastNames == null)
        throw new InvalidDataException("Invalid names.json format.");
    _maleFirstNames = namesData.MaleFirstNames;
    _femaleFirstNames = namesData.FemaleFirstNames;
    _lastNames = namesData.LastNames;
}
catch (Exception ex)
{
    Console.WriteLine($"Could not load names.json: {ex.Message}");
    throw;
}
```

**Why**
- Meets explicit try/catch requirement.
- Prevents silent crash and improves debugging.

### G) If examiner asks for stronger override example

You can keep current architecture and add this:

Create `PeopleVilleEngine/Locations/LocationBase.cs`

```csharp
namespace PeopleVilleEngine.Locations;

public abstract class LocationBase : ILocation
{
    protected readonly List<BaseVillager> _villagers = new();
    public abstract string Name { get; }
    public virtual List<BaseVillager> Villagers() => _villagers;
    public abstract void Interact(BaseVillager villager);
}
```

Then make `School/Hospital/Shop` inherit `LocationBase` and override `Interact(...)`.

**Why**
- Demonstrates abstract class + override in a highly visible way.

### G1) Exact implementation after `LocationBase` is created

Use these exact class versions.

Replace `PeopleVilleEngine/Locations/School.cs`:

```csharp
namespace PeopleVilleEngine.Locations;

public class School : LocationBase, IService
{
    public override string Name => "School";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager))
            _villagers.Add(villager);

        villager.IQ++;
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
```

Replace `PeopleVilleEngine/Locations/Hospital.cs`:

```csharp
namespace PeopleVilleEngine.Locations;

public class Hospital : LocationBase, IService
{
    public override string Name => "Hospital";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager))
            _villagers.Add(villager);

        villager.Health++;
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
```

Replace `PeopleVilleEngine/Locations/Shop.cs`:

```csharp
namespace PeopleVilleEngine.Locations;

public class Shop : LocationBase, IService
{
    public override string Name => "Shop";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager))
            _villagers.Add(villager);

        if (villager.Money >= 100)
        {
            villager.Money -= 100;
            villager.Food += 10;
        }
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
```

Also add this `using` in each of these three files:

```csharp
using PeopleVilleEngine.Services;
```

Also add core locations once in `Village` so they exist in simulation:

```csharp
private void AddCoreLocations()
{
    if (!Locations.Any(l => l is School)) Locations.Add(new School());
    if (!Locations.Any(l => l is Hospital)) Locations.Add(new Hospital());
    if (!Locations.Any(l => l is Shop)) Locations.Add(new Shop());
}
```

Call it in constructor before village generation:

```csharp
public Village()
{
    Console.WriteLine("Creating villager");
    AddCoreLocations();
    CreateVillage();
}
```

If you already added `RunTick()`, apply interaction on movement:

```csharp
var villager = Villagers[_random.Next(Villagers.Count)];
var location = Locations[_random.Next(Locations.Count)];
villager.MoveTo(location);

if (location is LocationBase interactiveLocation)
    interactiveLocation.Interact(villager);
```

**Why this works**
- `LocationBase` provides shared villager storage and enforces override contract.
- Each location now has explicit custom behavior (`IQ`, `Health`, `Food/Money`) through `override Interact`.
- This is a strong examiner-friendly example of **abstract class + override + object interaction**.
- Keeping `IService` at the same time preserves your original service contract and shows clear interface usage.

## 1) Replace `PeopleVilleEngine/RNG.cs`

**What changed**
- Replaced manual lock-based singleton with `Lazy<RNG>`.
- Made `Random` field readonly.

**Why**
- `Lazy<T>` is simpler and thread-safe by design.
- Reduces synchronization code and maintenance risk.

```csharp
namespace PeopleVilleEngine;
sealed public class RNG
{
    private static readonly Lazy<RNG> _rng = new(() => new RNG());
    private readonly Random _random;

    private RNG()
    {
        _random = new Random();
    }

    public static RNG GetInstance() => _rng.Value;

    public int Next(int max) => _random.Next(max);
    public int Next(int min, int max) => _random.Next(min, max);
}
```

## 2) Replace `PeopleVilleEngine/VillagerNames.cs`

**What changed**
- Replaced singleton with `Lazy<VillagerNames>`.
- Switched name arrays to `Array.Empty<string>()`.
- Added robust path resolution (`AppContext.BaseDirectory` + fallback).
- Added deserialize null/shape validation.
- Reused the class RNG instance instead of fetching RNG repeatedly.

**Why**
- Prevents race conditions during singleton initialization.
- Avoids null issues and invalid JSON crashes later in execution.
- Makes file loading stable across different startup working directories.

```csharp
using System.Text.Json;

namespace PeopleVilleEngine;
public class VillagerNames
{
    private string[] _maleFirstNames = Array.Empty<string>();
    private string[] _femaleFirstNames = Array.Empty<string>();
    private string[] _lastNames = Array.Empty<string>();
    private readonly RNG _random;
    private static readonly Lazy<VillagerNames> _instance = new(() => new VillagerNames());

    private VillagerNames()
    {
        _random = RNG.GetInstance();
        LoadNamesFromJsonFile();
    }

    public static VillagerNames GetInstance() => _instance.Value;

    private void LoadNamesFromJsonFile()
    {
        string jsonFile = Path.Combine(AppContext.BaseDirectory, "lib", "names.json");
        if (!File.Exists(jsonFile))
            jsonFile = Path.Combine(Directory.GetCurrentDirectory(), "lib", "names.json");

        if (!File.Exists(jsonFile))
            throw new FileNotFoundException(jsonFile);

        string jsonData = File.ReadAllText(jsonFile);
        var namesData = JsonSerializer.Deserialize<NamesData>(jsonData);

        if (namesData?.MaleFirstNames == null || namesData.FemaleFirstNames == null || namesData.LastNames == null)
            throw new InvalidDataException("Invalid names.json format.");

        _maleFirstNames = namesData.MaleFirstNames;
        _femaleFirstNames = namesData.FemaleFirstNames;
        _lastNames = namesData.LastNames;
    }

    private string GetRandomName(string[] names)
    {
        if (names.Length == 0)
            throw new IndexOutOfRangeException("Names data not properly loaded with names.");

        int index = _random.Next(names.Length);
        return names[index];
    }

    public string GetRandomFirstName(bool isMale) => GetRandomName(isMale ? _maleFirstNames : _femaleFirstNames);
    public string GetRandomLastName() => GetRandomName(_lastNames);

    public (string firstname, string lastname) GetRandomNames(bool isMale) => (GetRandomFirstName(isMale), GetRandomLastName());

    private class NamesData
    {
        public string[]? MaleFirstNames { get; set; }
        public string[]? FemaleFirstNames { get; set; }
        public string[]? LastNames { get; set; }
    }
}
```

## 3) Replace `PeopleVilleEngine/Village.cs`

**What changed**
- Added guard when no creators are found.
- Added deduplication of discovered creator types.
- Ignored abstract/interface creator types.
- Resolved plugin folder from output directory first.
- Fixed text typos (`creator`, `has`).

**Why**
- Prevents index/out-of-range runtime failure when creator list is empty.
- Avoids duplicate plugin loading and duplicate villager generation behavior.
- Makes plugin discovery reliable when run from IDE, test runner, or CLI.

```csharp
namespace PeopleVilleEngine;
using PeopleVilleEngine.Villagers.Creators;
using PeopleVilleEngine.Locations;
using System.Reflection;
using System.Linq;

public class Village
{
    private readonly RNG _random = RNG.GetInstance();
    public List<BaseVillager> Villagers { get; } = new();
    public List<ILocation> Locations { get; } = new();
    public VillagerNames VillagerNameLibrary { get; } = VillagerNames.GetInstance();

    public Village()
    {
        Console.WriteLine("Creating villager");
        CreateVillage();
    }

    private void CreateVillage()
    {
        var villagers = _random.Next(10, 24);
        Console.ForegroundColor = ConsoleColor.Red;

        var villageCreators = LoadVillagerCreatorFactories();
        Console.ResetColor();
        Console.WriteLine();

        if (villageCreators.Count == 0)
            throw new InvalidOperationException("No IVillagerCreator implementations were found.");

        int villageCreatorindex = 0;

        for (int i = 0; i < villagers; i++)
        {
            var created = false;
            do
            {
                created = villageCreators[villageCreatorindex].CreateVillager(this);
                villageCreatorindex = villageCreatorindex + 1 < villageCreators.Count ? villageCreatorindex + 1 : 0;
            } while (!created);
        }

        Console.ResetColor();
    }

    private List<IVillagerCreator> LoadVillagerCreatorFactories()
    {
        var villageCreators = new List<IVillagerCreator>();
        var loadedCreatorTypeNames = new HashSet<string>(StringComparer.Ordinal);

        LoadVillagerCreatorFactoriesFromType(
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()),
            villageCreators,
            loadedCreatorTypeNames);

        var libPath = Path.Combine(AppContext.BaseDirectory, "lib");
        if (!Directory.Exists(libPath))
            libPath = Path.Combine(Directory.GetCurrentDirectory(), "lib");

        if (Directory.Exists(libPath))
        {
            var libraryFiles = Directory.EnumerateFiles(libPath).Where(f => Path.GetExtension(f) == ".dll");
            foreach (var libraryFile in libraryFiles)
            {
                LoadVillagerCreatorFactoriesFromType(
                    Assembly.LoadFrom(libraryFile).ExportedTypes,
                    villageCreators,
                    loadedCreatorTypeNames);
            }
        }

        return villageCreators;
    }

    private void LoadVillagerCreatorFactoriesFromType(
        IEnumerable<Type> inputTypes,
        List<IVillagerCreator> outputVillagerCreators,
        HashSet<string> loadedCreatorTypeNames)
    {
        var createVillagerInterface = typeof(IVillagerCreator);
        var creatorTypes = inputTypes
            .Where(p => createVillagerInterface.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        foreach (var type in creatorTypes)
        {
            var typeKey = type.FullName ?? type.Name;
            if (!loadedCreatorTypeNames.Add(typeKey))
                continue;

            Console.WriteLine($"Village creator loaded: {type}");

            if (Activator.CreateInstance(type) is IVillagerCreator creator)
                outputVillagerCreators.Add(creator);
        }
    }

    public override string ToString()
    {
        return $"Village has {Villagers.Count} villagers, where {Villagers.Count(v => v.HasHome() == false)} are homeless.";
    }
}
```

## 4) Replace `PeopleVilleEngine/Villagers/ChildVillager.cs`

**What changed**
- Regenerates `FirstName` after overriding `IsMale` in the `(age, isMale)` constructor.

**Why**
- Base constructor already generated names using previous gender value.
- Keeps `IsMale` and first name consistent.

```csharp
namespace PeopleVilleEngine.Villagers;
public class ChildVillager : BaseVillager
{
    public ChildVillager(Village village) : base(village)
    {
        Age = RNG.GetInstance().Next(0, 18);
    }

    public ChildVillager(Village village, int age) : this(village)
    {
        Age = age;
    }

    public ChildVillager(Village village, int age, bool isMale) : this(village, age)
    {
        IsMale = isMale;
        FirstName = village.VillagerNameLibrary.GetRandomFirstName(IsMale);
    }
}
```

## 5) Replace `PeopleVilleEngine/Villagers/BaseVillager.cs`

**What changed**
- Converted public fields (`IQ`, `Money`, `Food`, `Health`) to properties.
- Added safe default values for string properties.
- Removed unused private village state from class storage.

**Why**
- Properties are safer and easier to evolve (validation/logic later).
- Avoids nullable warnings and accidental uninitialized strings.
- Removes dead state that was not used.

```csharp
using PeopleVilleEngine;
using PeopleVilleEngine.Locations;

public abstract class BaseVillager
{
    public int Age { get; protected set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsMale { get; set; }
    public ILocation? Home { get; set; } = null;
    public int IQ { get; set; } = 100;
    public int Money { get; set; } = 100;
    public int Food { get; set; } = 0;
    public int Health { get; set; } = 0;

    public bool HasHome() => Home != null;

    protected BaseVillager(Village village)
    {
        IsMale = RNG.GetInstance().Next(0, 2) == 0;
        (FirstName, LastName) = village.VillagerNameLibrary.GetRandomNames(IsMale);
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} ({Age} years)";
    }
}
```

## 6) Replace `PeopleVilleEngine/Items/Item.cs`

**What changed**
- Fixed namespace typo to `PeopleVilleEngine.Items`.

**Why**
- Keeps item types in one consistent namespace and avoids fragile imports.

```csharp
namespace PeopleVilleEngine.Items;

public abstract class Item
{
    public string Name { get; set; }
    public int Value { get; set; }

    protected Item(string name, int value)
    {
        Name = name;
        Value = value;
    }
}
```

## 7) Replace `PeopleVilleEngine/Items/Clothes.cs`

**What changed**
- Made `Clothes` inherit from `Item`.
- Wired constructor values into `base(name, value)`.

**Why**
- Restores inheritance design so clothes are actual items with stored value/name.
- Prevents silent loss of constructor data.

```csharp
namespace PeopleVilleEngine.Items;

public abstract class Clothes : Item
{
    protected Clothes(string name, int value) : base(name, value)
    {
    }
}

public class Shoes : Clothes
{
    public Shoes() : base("Shoes", 40) { }
}

public class Pants : Clothes
{
    public Pants() : base("Pants", 30) { }
}
```

## 8) Replace `PeopleVilleEngine/Locations/Shop.cs`

**What changed**
- Corrected item namespace import.
- Removed unrelated/unused imports.

**Why**
- Keeps dependencies clean and reduces confusion.
- Avoids compile issues if namespace cleanup is applied.

```csharp
namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Items;
using PeopleVilleEngine.Services;

public class Shop : IService
{
    public string Name { get; set; } = "Shop";

    public void ProvideService(BaseVillager villager)
    {
        if (villager.Money >= 100)
        {
            villager.Money -= 100;
            villager.Food += 10;
        }
    }
}
```

## 9) Remove unused duplicate model files

**What changed**
- Removed legacy duplicate models (`Location`, `Villager`) if unused.

**Why**
- These conflict conceptually with the active `BaseVillager` + `ILocation` design.
- Reduces future maintenance mistakes and accidental mixed usage.

Delete these files if they are not used anywhere:

- `PeopleVilleEngine/Locations/Location.cs`
- `PeopleVilleEngine/Villagers/Villager.cs`

## 10) Final step

**What changed**
- Rebuild and verify after manual copy.

**Why**
- Confirms all namespace, constructor, and plugin loading changes still compile together.

Run a full build after you paste all changes.
