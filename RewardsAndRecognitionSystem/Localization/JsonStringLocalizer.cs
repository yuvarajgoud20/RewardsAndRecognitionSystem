using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, string> _localizations;

    public JsonStringLocalizer(string resourcesPath, string cultureName)
    {
        var filePath = Path.Combine(resourcesPath, $"{cultureName}.json");
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            _localizations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData) ?? new Dictionary<string, string>();
        }
        else
        {
            _localizations = new Dictionary<string, string>();
        }
    }

    public LocalizedString this[string name] =>
        new LocalizedString(name, _localizations.ContainsKey(name) ? _localizations[name] : name);

    public LocalizedString this[string name, params object[] arguments] =>
        new LocalizedString(name, string.Format(_localizations.ContainsKey(name) ? _localizations[name] : name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _localizations.Select(kv => new LocalizedString(kv.Key, kv.Value));

    public IStringLocalizer WithCulture(CultureInfo culture) => new JsonStringLocalizer("", culture.TwoLetterISOLanguageName);
}
