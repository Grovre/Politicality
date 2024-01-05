using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using dotNS.Classes;

namespace PoliticalityApi;

public class NationContext
{
    public string Category { get; }
    public Dictionary<string, double> DeathPercentages { get; }
    public string Influence { get; }
    public string Motto { get; }
    public string Population { get; }
    public string FullName { get; }
    public string Religion { get; }
    public double TaxRate { get; }
    public string Type { get; }
    public string MajorIndustry { get; }
    public double PublicSectorPercentage { get; }
    public Dictionary<string, double> GovernmentSpendingPercentages { get; }
    public Dictionary<string, string> CountryProfile { get; }

    public NationContext(PublicNationInfo nationInfo)
    {
        Category = nationInfo.Category;
        DeathPercentages = nationInfo.Deaths.Select(d => (d.Name, d.Percentage)).ToDictionary();
        Influence = nationInfo.Influence;
        Motto = nationInfo.Motto;
        Population = nationInfo.Population.ToString();
        FullName = nationInfo.FullName;
        Religion = nationInfo.Religion;
        TaxRate = nationInfo.Tax;
        Type = nationInfo.Type;
        MajorIndustry = nationInfo.MajorIndustry;
        PublicSectorPercentage = nationInfo.PublicSector;
        GovernmentSpendingPercentages = typeof(Government)
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(f => (f.Name, (double)f.GetValue(nationInfo.Gov)!))
            .ToDictionary();
        CountryProfile = new()
        {
            { "Economy", nationInfo.Freedom.Economy },
            { "Political freedom", nationInfo.Freedom.PoliticalFreedom },
            { "Civil Rights", nationInfo.Freedom.CivilRights }
        };
    }
}