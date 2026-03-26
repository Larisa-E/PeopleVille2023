namespace PeopleVilleEngine.Locations;

using PeopleVilleEngine.Services;
using PeopleVilleEngine.Villagers;
using System;

public class Location
{
	public string Name { get; set; }
	public List<Villager> People { get; set; }

	public Location(string name)
	{	Name = name;
		People = new List<Villager>();
	}
}

