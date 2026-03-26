using PeopleVilleEngine;
using PeopleVilleEngine.Trading;

Console.WriteLine("PeopleVille");
var village = new Village();

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine($"[Event] {msg}");

Console.WriteLine(village);
for (int i = 0; i < 5; i++)
{
    village.RunTick();
}