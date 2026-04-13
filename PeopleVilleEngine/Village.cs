namespace PeopleVilleEngine;
using PeopleVilleEngine.Villagers.Creators;
using PeopleVilleEngine.Locations;
using PeopleVilleEngine.Trading;
using System.Reflection;
using System.Linq;

public class Village
{
    private readonly RNG _random = RNG.GetInstance();
    public List<BaseVillager> Villagers { get; } = new();
    public List<ILocation> Locations { get; } = new();
    public VillagerNames VillagerNameLibrary { get; } = VillagerNames.GetInstance();

    public event EventHandler<string>? RandomEventHappened;
    public event EventHandler<int>? TickHappened;
    private int _tick;
    private const int MinVillagers = 10;
    private int MaxVillagersExclusive = 24;
    private readonly object _sync = new(); // is the gate and only one worker can change village state at a time
    private CancellationTokenSource? _simulationCts; // stop 
    private Task? _simulationTask; // start button and running
    public bool IsSimulationsRunning => _simulationTask is { IsCompleted: false };

    public void RunTick()
    {
        lock (_sync)
        // protect RunTick() with lock. each tick is a sfae transaction like update and then emit events
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
            var location = DecideNextLocation(villager);
            villager.MoveTo(location);

            if (location is LocationBase interactiveLocation)
                interactiveLocation.Interact(villager);

            EmitPersonalEvent(villager, location);
            TryEmitSocialEvent(location);
            TryEmitTradeEvent(location);
            //TryEmitVillageEvent();

            RandomEventHappened?.Invoke(
                this,
                $"[Move] Tick {_tick}: {villager.FirstName} moved to {location.Name}. " +
                $"Stats -> IQ:{villager.IQ}, Health:{villager.Health}, Money:{villager.Money}, Food:{villager.Food}");
        }
    }

    private void EmitPersonalEvent(BaseVillager villager, ILocation location)
    {
        if (location is School)
        {
            RandomEventHappened?.Invoke(this, $"[Even] {villager.FirstName} learned something new. IQ is now{villager.IQ}.");
        }

        if (location is Hospital)
        {
            RandomEventHappened?.Invoke(this, $"[Even] {villager.FirstName} got treatment. Health is now{villager.Health}.");
        }


        if (location is Shop)
        {
            RandomEventHappened?.Invoke(this, $"[Even] {villager.FirstName} visisted the shop. Money={villager.Money}, Food={villager.Food}.");
        }

        if (villager.Home != null && location == villager.Home && villager.Food > 0)
        {
            villager.Food -= 1;
            villager.Health += 1;
            RandomEventHappened?.Invoke(this, $"[Event] {villager.FirstName} ate at home. Food-1, Health+1.");
        }
    }

    private void TryEmitSocialEvent(ILocation location)
    {
        var atLocation = location.Villagers();
        if (atLocation.Count < 2)
            return;

        if (_random.Next(0, 100) >= 25) // 25% chance
            return;

        var first = atLocation[_random.Next(atLocation.Count)];
        var second = atLocation[_random.Next(atLocation.Count)];
        if (ReferenceEquals(first, second))
            return;

        first.Health += 1;
        second.Health += 1;

        RandomEventHappened?.Invoke(
            this,
            $"[Social] {first.FirstName} met {second.FirstName} at {location.Name}. Both feel better (+1 Health).");
    }

    private void TryEmitTradeEvent(ILocation location)
    {
        var atLocation = location.Villagers();
        if (atLocation.Count < 2)
            return;

        if (_random.Next(0, 100) >= 20) // 20% chance
            return;

        var from = atLocation[_random.Next(atLocation.Count)];
        var to = atLocation[_random.Next(atLocation.Count)];

        if (ReferenceEquals(from, to))
            return;

        var doMoneyTrade = _random.Next(0, 2) == 0;

        if (doMoneyTrade)
        {
            if (from.Money <= 0)
                return;

            var amount = _random.Next(1, Math.Min(from.Money, 20) + 1);
            if (!TransactionLogic.TransferMoney(from, to, amount))
                return;

            RandomEventHappened?.Invoke(
                this,
                $"[Trade] {from.FirstName} gave {amount} money to {to.FirstName} at {location.Name}.");

            return;
        }

        if (from.Inventory.Count == 0)
            return;

        var item = from.Inventory[_random.Next(from.Inventory.Count)];
        if (!TransactionLogic.TransferItem(from, to, item))
            return;

        RandomEventHappened?.Invoke(
            this,
            $"[Trade] {from.FirstName} gave {item.Name} to {to.FirstName} at {location.Name}.");
    }

    //private void TryEmitVillageEvent()
    //{

    //}

    public void StartSimulation(TimeSpan tickInterval)
    {
        lock (_sync)
        {
            if (_simulationTask is { IsCompleted: false })
                return;

            StartVillagerTurnLoops(); // start all villager threads
            _simulationCts = new CancellationTokenSource(); // create stop signal for this run
            _simulationTask = SimulationLoopAsync(tickInterval, _simulationCts.Token); // start background tick loop
        }
    }

    public async Task StopSimulationAsync()
    {
        CancellationTokenSource? cts;
        Task? simulationTask;
        List<BaseVillager> villagerSnapshot;

        lock (_sync)
        {
            cts = _simulationCts;
            simulationTask = _simulationTask;
            villagerSnapshot = Villagers.ToList();
        }

        if (cts == null || simulationTask == null)
            return;

        cts.Cancel();

        try
        {
            await simulationTask; // waits until loop exits
        }
        catch (OperationCanceledException)
        {
            // expected when simulation loop is canceled
        }

        try
        {
            var stopTasks = villagerSnapshot.Select(v => v.StopTurnLoopAsync());
            await Task.WhenAll(stopTasks);
        }
        catch (OperationCanceledException)
        {
            // expected when villager loops are canceled
        }
        finally
        {
            lock(_sync)
            {
                _simulationCts?.Dispose();
                _simulationCts = null;
                _simulationTask = null;
                // reset so simulations can start again
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

    public Village()
    {
        Console.WriteLine("Creating villager");

        // core simulation locations that ensures school/hospital/shop are always available in the simulation
        AddCoreLocations();

        CreateVillage();
    }

    private void AddCoreLocations()
    {
        if (!Locations.Any(l => l is School)) Locations.Add(new School());
        if (!Locations.Any(l => l is Hospital)) Locations.Add(new Hospital());
        if (!Locations.Any(l => l is Shop)) Locations.Add(new Shop());
    }

    private void CreateVillage()
    {
        var villagers = _random.Next(MinVillagers, MaxVillagersExclusive);
        Console.ForegroundColor = ConsoleColor.Red;

        var villageCreators = LoadVillagerCreatorFactories();
        Console.ResetColor();
        Console.WriteLine();

        // prevents index errors in the loop below if no plugins/creators are available
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
        // loading DLL: error handling
        LoadVillagerCreatorFactoriesFromType(
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()),
            villageCreators);

        if (!Directory.Exists("lib"))
            return villageCreators;

        var libraryFiles = Directory.EnumerateFiles("lib").Where(f => Path.GetExtension(f) == ".dll");

        foreach (var libraryFile in libraryFiles)
        {
            try
            {
                LoadVillagerCreatorFactoriesFromType(Assembly.LoadFrom(libraryFile).ExportedTypes, villageCreators);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Plugin load failed for '{libraryFile}': {ex.Message}");
            }
        }

        return villageCreators;
    }

    private void LoadVillagerCreatorFactoriesFromType(IEnumerable<Type> inputTypes, List<IVillagerCreator> outputVillagerCreators)
    {
        var createVillagerInterface = typeof(IVillagerCreator);
        var creatorTypes = inputTypes
            .Where(p => createVillagerInterface.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        foreach (var type in creatorTypes)
        {
            Console.WriteLine($"Village creator loaded: {type}");

            if (Activator.CreateInstance(type) is IVillagerCreator creator)
                outputVillagerCreators.Add(creator);
        }
    }

    public override string ToString()
    {
        var homelessCount = Villagers.Count(v => !v.HasHome());
        var houseCount = Villagers.Count - homelessCount;

        return $"Village summary: Villagers={Villagers.Count} Housed={houseCount}, Homeless={homelessCount}, Locations={Locations.Count}.";
    }

    // reasons for villagers to move
    private ILocation DecideNextLocation(BaseVillager villager)
    {
        // priority 1: if low food, go shop
        var shop = Locations.FirstOrDefault(l => l is Shop);
        if (villager.Food < 5 && shop != null)
            return shop;

        // priority 2: low health, so go hospital
        var hospital = Locations.FirstOrDefault(l => l is Hospital);
        if (villager.Health < 3 && hospital != null)
            return hospital;

        // priority 3: child go tho school
        var school = Locations.FirstOrDefault(l => l is School);
        if (villager is PeopleVilleEngine.Villagers.ChildVillager && school != null && _random.Next(0, 100) < 70)
            return school;

        return Locations[_random.Next(Locations.Count)];
    }

    private void StartVillagerTurnLoops()
    {
        foreach (var villager in Villagers)
        {
            villager.StartTurnLoop();
        }
    }

}