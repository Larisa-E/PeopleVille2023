namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

