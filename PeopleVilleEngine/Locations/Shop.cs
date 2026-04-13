namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;

public class Shop : LocationBase, IService
{
    public override string Name => "Shop";

    public override void Interact(BaseVillager villager)
    {
        if (!_villagers.Contains(villager)) 
            _villagers.Add(villager);

        if (villager.Money >= 20)
        {
            villager.Money -= 20;
            villager.Food += 5;
        }
    }

    public void ProvideService(BaseVillager villager) => Interact(villager);
}
