namespace PeopleVilleEngine;
sealed public class RNG
{
    private static readonly Lazy<RNG> _rng = new(() => new RNG());
    private readonly Random _random;

    private RNG()
    {
        _random = new Random();
    }

    public static RNG GetInstance() => _rng.Value;
   
    public int Next(int max) => _random.Next(max);
    public int Next(int min, int max) => _random.Next(min, max);
}