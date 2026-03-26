using PeopleVilleEngine;
using PeopleVilleEngine.Locations;
using PeopleVilleEngime.Items;
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

    public bool HasHome() => Home != null;

    protected BaseVillager(Village village)
    {
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

    public override string ToString()
    {
        return $"{FirstName} {LastName} ({Age} years)";
    }
}