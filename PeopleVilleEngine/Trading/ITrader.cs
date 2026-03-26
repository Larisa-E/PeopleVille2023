using PeopleVilleEngime.Items;

namespace PeopleVilleEngine.Trading
{
    public interface ITrader
    {
        int Money { get; set; }
        List<Item> Inventory { get; }
    }
}
