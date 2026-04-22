using System;
using System.Windows;
using Microsoft.Win32;

namespace ClickToTranslate
{
    public static class ThemeManager
    {
        public const string ThemeSystem = "system";
        public const string ThemeLight = "light";
        public const string ThemeDark = "dark";

        /// <summary>
        /// טוען את ערכת הצבעים המתאימה ואת סגנונות הבסיס למשאבי היישום.
        /// יש לקרוא לזה ב-OnStartup לפני יצירת חלונות.
        /// </summary>
        public static void Initialize(string themeSetting)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;
            dicts.Clear();
            dicts.Add(LoadThemeDictionary(themeSetting));
            dicts.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/ModernStyles.xaml", UriKind.Relative)
            });
        }

        /// <summary>
        /// ממיר הגדרה לוגית לשם ערכת נושא ממשי (system → dark/light לפי המערכת).
        /// </summary>
        public static string Resolve(string setting)
        {
            if (string.Equals(setting, ThemeLight, StringComparison.OrdinalIgnoreCase)) return ThemeLight;
            if (string.Equals(setting, ThemeDark, StringComparison.OrdinalIgnoreCase)) return ThemeDark;
            return IsSystemLight() ? ThemeLight : ThemeDark;
        }

        private static ResourceDictionary LoadThemeDictionary(string themeSetting)
        {
            var actual = Resolve(themeSetting);
            var path = actual == ThemeLight ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";
            return new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
        }

        private static bool IsSystemLight()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key?.GetValue("AppsUseLightTheme") is int value)
                {
                    return value == 1;
                }
            }
            catch { }
            return false;
        }
    }
}
