namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Hospital : IService
{
    public string Name { get; set; } = "Hospital";

    public void ProvideService(BaseVillager villager)
    {
        if (villager == null) return;
        villager.Health++;
    }
}

