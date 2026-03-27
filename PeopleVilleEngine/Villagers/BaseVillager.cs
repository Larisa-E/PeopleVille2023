using PeopleVilleEngine;
using PeopleVilleEngine.Locations;
using PeopleVilleEngine.Trading;

public abstract class BaseVillager : ITrader
{
    public int Age { get; protected set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsMale { get; set; }
    private Village _village;
    public ILocation? Home { get; set; } = null;
    public ILocation? CurrentLocation { get; private set; } 
    public int IQ { get; set; } = 100;
    public int Money { get; set; } = 100;
    public int Food { get; set; } = 0;
    public int Health { get; set; } = 0;
    public List<Item> Inventory { get; } = new();
    private readonly object _turnSync = new();
    private CancellationTokenSource? _turnCts;
    private Task? _turnTask;

    public bool HasHome() => Home != null;
    public bool IsTurnLoopRunning => _turnTask is { IsCompleted: false };

    protected BaseVillager(Village village)
    {
        Food = RNG.GetInstance().Next(0, 10);
        if (RNG.GetInstance().Next(0, 2) == 0)
            Inventory.Add(new PeopleVilleEngine.Items.Shoes());
        else
            Inventory.Add(new PeopleVilleEngine.Items.Pants());

        _village = village;
        IsMale = RNG.GetInstance().Next(0, 2) == 0;
        (FirstName, LastName) = village.VillagerNameLibrary.GetRandomNames(IsMale);
    }

    public void MoveTo(ILocation destination)
    {
        CurrentLocation = destination;
    }

    public void StartTurnLoop()
    {
        lock (_turnSync)
        {
            if (_turnTask is { IsCompleted: false })
                return;

            _turnCts = new CancellationTokenSource();
            _turnTask = Task.Run(() => TakeTurn(_turnCts.Token), _turnCts.Token);
        }
    }

    public async Task StopTurnLoopAsync()
    {
        CancellationTokenSource? cts;
        Task? task;

        lock (_turnSync)
        {
            cts = _turnCts;
            task = _turnTask;
        }

        if (cts == null || task == null)
            return;

        cts.Cancel();

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_turnSync)
            {
                _turnCts?.Dispose();
                _turnCts = null;
                _turnTask = null;
            }
        }
    }

    private async Task TakeTurn(CancellationToken token)
    {
        var random = RNG.GetInstance();

        while (!token.IsCancellationRequested)
        {
            if (_village.Locations.Count > 0)
            {
                var location = _village.Locations[random.Next(_village.Locations.Count)];
                MoveTo(location);

                if (location is LocationBase interactiveLocation)
                    interactiveLocation.Interact(this);
            }

            Console.WriteLine($"[VillagerTurn] {FirstName} on thread {Environment.CurrentManagedThreadId}");

            var delayMs = Random.Shared.Next(3000, 10001);
            await Task.Delay(delayMs, token);
        }

    }
    public override string ToString()
    {
        return $"{FirstName} {LastName} ({Age} years)";
    }
}