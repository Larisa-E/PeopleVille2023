namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;

public class Hospital : LocationBase,  IService
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

