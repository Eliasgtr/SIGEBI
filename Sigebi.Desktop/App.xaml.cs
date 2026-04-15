using System.Configuration;
using System.Data;
using System.Windows;

namespace Sigebi.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public void ApplyTheme(string themeName)
    {
        var dict = new ResourceDictionary();
        var uri = GetThemeUri(themeName);
        if (uri == null)
            return;

        dict.Source = uri;

        // Remove existing theme dictionaries (Themes/*)
        for (int i = Resources.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            var md = Resources.MergedDictionaries[i];
            if (md.Source != null && md.Source.OriginalString.Contains("Themes/"))
                Resources.MergedDictionaries.RemoveAt(i);
        }

        Resources.MergedDictionaries.Add(dict);
    }

    private static Uri? GetThemeUri(string themeName)
    {
        return themeName switch
        {
            "Dark" => new Uri("/Sigebi.Desktop;component/Themes/DarkTheme.xaml", UriKind.Relative),
            _ => new Uri("/Sigebi.Desktop;component/Themes/LightTheme.xaml", UriKind.Relative),
        };
    }
}

