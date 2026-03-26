namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;
using System;

public class School : IService
{
    public string Name { get; set; } = "School";

    public void ProvideService(BaseVillager villager)
    {
        villager.IQ++;
    }
}
