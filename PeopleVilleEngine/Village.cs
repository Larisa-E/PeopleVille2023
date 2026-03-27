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

    public event EventHandler<string>? RandomEventHappened;
    public event EventHandler<int>? TickHappened;
    private int _tick;

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
        var villagers = _random.Next(10, 24);
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
        return $"Village has {Villagers.Count} villagers, where {Villagers.Count(v => v.HasHome() == false)} are homeless.";
    }
}