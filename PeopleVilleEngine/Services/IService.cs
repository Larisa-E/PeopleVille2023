using PeopleVilleEngine.Villagers;

namespace PeopleVilleEngine.Services;

public interface IService
{
    string Name { get; }
    void ProvideService(BaseVillager villager);
}

