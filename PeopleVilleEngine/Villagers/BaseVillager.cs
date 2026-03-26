using PeopleVilleEngine;
using PeopleVilleEngine.Locations;

public abstract class BaseVillager
{
    public int Age { get; protected set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsMale { get; set; }
    private Village _village;
    public ILocation? Home { get; set; } = null;
    public int IQ = 100;
    public int Money = 100;
    public int Food = 0;
    public int Health = 0;

    public bool HasHome() => Home != null;

    protected BaseVillager(Village village)
    {
        _village = village;
        IsMale = RNG.GetInstance().Next(0, 2) == 0;
        (FirstName, LastName) = village.VillagerNameLibrary.GetRandomNames(IsMale);
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} ({Age} years)";
    }
}