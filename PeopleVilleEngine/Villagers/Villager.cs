using PeopleVilleEngine.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleVilleEngine.Villagers
{
    public class Villager
    {
        public string Name {  get; set; }
        public int Age { get; set; }
        public required Location CurrentLocation { get; set; }   

        public Villager(string name, int age) 
        {
            Name = name;
            Age = age;
        }
    }
}
