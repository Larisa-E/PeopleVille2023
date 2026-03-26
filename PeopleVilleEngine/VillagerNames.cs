using System.Data;
using System.Text.Json;

namespace PeopleVilleEngine;
public class VillagerNames
{
    private string[] _maleFirstNames = Array.Empty<string>();
    private string[] _femaleFirstNames = Array.Empty<string>();
    private string[] _lastNames = Array.Empty<string>();
    RNG _random;
    private static readonly Lazy<VillagerNames> _instance = new(() => new VillagerNames());

    private VillagerNames()
    {
        _random = RNG.GetInstance();
        LoadNamesFromJsonFile();
    }

    public static VillagerNames GetInstance() => _instance.Value;
   
    private void LoadNamesFromJsonFile()
    {
        string jsonFile = Path.Combine(AppContext.BaseDirectory, "lib", "names.json");
        if (!File.Exists(jsonFile))
            jsonFile = Path.Combine(Directory.GetCurrentDirectory(), "lib", "names.json");

        if (!File.Exists(jsonFile))
            throw new FileNotFoundException(jsonFile);
        
        string jsonData = File.ReadAllText(jsonFile);

        try
        {
            var namesData = JsonSerializer.Deserialize<NamesData>(jsonData);
            if (namesData?.MaleFirstNames == null || namesData.FemaleFirstNames == null || namesData.LastNames == null)
                throw new InvalidDataException("Invalid names.json format.");

            _maleFirstNames = namesData.MaleFirstNames;
            _femaleFirstNames = namesData.FemaleFirstNames;
            _lastNames = namesData.LastNames;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load names.json: {ex.Message}");
            throw;
        }
    }

    private string GetRandomName(string[] names)
    {
        if (names.Length == 0)
            throw new IndexOutOfRangeException("Names data not properly loaded with names.");

        int index = RNG.GetInstance().Next(names.Length);
        return names[index];
    }

    public string GetRandomFirstName(bool isMale) => GetRandomName(isMale ? _maleFirstNames : _femaleFirstNames);
    public string GetRandomLastName() => GetRandomName(_lastNames);

    public (string firstname, string lastname) GetRandomNames(bool isMale) => (GetRandomFirstName(isMale), GetRandomLastName());

    private class NamesData
    {
        public string[] MaleFirstNames { get; set; }
        public string[] FemaleFirstNames { get; set; }
        public string[] LastNames { get; set; }
    }
}


