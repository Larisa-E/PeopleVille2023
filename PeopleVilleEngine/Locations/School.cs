namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;

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
