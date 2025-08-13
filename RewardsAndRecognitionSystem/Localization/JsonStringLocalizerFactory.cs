using Microsoft.Extensions.Localization;
using System.Globalization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;

    public JsonStringLocalizerFactory(IWebHostEnvironment env)
    {
        _resourcesPath = Path.Combine(env.ContentRootPath, "Localization");
    }

    public IStringLocalizer Create(Type resourceSource) =>
        new JsonStringLocalizer(_resourcesPath, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

    public IStringLocalizer Create(string baseName, string location) =>
        new JsonStringLocalizer(_resourcesPath, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
}
