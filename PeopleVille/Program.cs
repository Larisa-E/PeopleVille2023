using PeopleVilleEngine;

Console.WriteLine("PeopleVille");
var village = new Village();

village.TickHappened += (_, tick) => Console.WriteLine($"[Tick] {tick}");
village.RandomEventHappened += (_, msg) => Console.WriteLine(msg);

Console.WriteLine(village);
village.StartSimulation(TimeSpan.FromMilliseconds(500));

using var monitorCts = new CancellationTokenSource();
var monitorTask = Task.Run(async () =>
{
    while (!monitorCts.Token.IsCancellationRequested)
    {
        Console.WriteLine($"[Monitor] {village}");
        await Task.Delay(2000, monitorCts.Token);
    }
}, monitorCts.Token);

Console.WriteLine("Simulation started in background.");
Console.WriteLine("Press ENTER to stop...\n");
Console.ReadLine();

monitorCts.Cancel();
await village.StopSimulationAsync();

try
{
    await monitorTask;
}
catch (OperationCanceledException)
{
    // shutdown
}

Console.WriteLine();
Console.WriteLine("Final state:");
Console.WriteLine(village);