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
