namespace PeopleVilleEngine.Locations;

using PeopleVilleEngime.Items;
using PeopleVilleEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public class Shop : IService
{
    public string Name { get; set; } = "Shop";

    public void ProvideService(BaseVillager villager)
    {
        if (villager.Money>=100)
        {

            villager.Money -= 100;
            villager.Food += 10;

        }
    }
}
