using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ClickToTranslate
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // טעינת הגדרות
                var settings = SettingsManager.Load();

                // טעינת ערכת נושא לפני יצירת חלונות
                ThemeManager.Initialize(settings.Theme);

                // טיפול בארגומנטים מיוחדים (רישום/ביטול הרישום של URI)
                if (e.Args.Length > 0)
                {
                    var firstArg = e.Args[0];

                    if (firstArg.Equals("--register", StringComparison.OrdinalIgnoreCase))
                    {
                        UriProtocolHandler.Register();
                        MessageBox.Show("התוכנה נרשמה בהצלחה כ-protocol handler.\nעכשיו תוכל להשתמש ב-Click to Do.",
                            "ClickToTranslate", MessageBoxButton.OK, MessageBoxImage.Information);
                        Shutdown();
                        return;
                    }

                    if (firstArg.Equals("--unregister", StringComparison.OrdinalIgnoreCase))
                    {
                        UriProtocolHandler.Unregister();
                        MessageBox.Show("הרישום בוטל.", "ClickToTranslate", MessageBoxButton.OK, MessageBoxImage.Information);
                        Shutdown();
                        return;
                    }

                    if (firstArg.Equals("--settings", StringComparison.OrdinalIgnoreCase))
                    {
                        var settingsWin = new SettingsWindow(settings);
                        settingsWin.ShowDialog();
                        Shutdown();
                        return;
                    }

                    // המקרה העיקרי: הגיע טקסט/URI לתרגום
                    string textToTranslate = ExtractText(firstArg);

                    if (!string.IsNullOrWhiteSpace(textToTranslate))
                    {
                        var window = new TranslationWindow(textToTranslate, settings);
                        window.Show();
                        return; // לא עושים Shutdown - המשתמש יסגור את החלון
                    }
                }

                // אם לא הגיעו ארגומנטים - פתח את חלון ההגדרות
                var settingsWindow = new SettingsWindow(settings);
                settingsWindow.ShowDialog();
                Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהפעלה:\n{ex.Message}", "ClickToTranslate",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// חילוץ הטקסט מהארגומנט - יכול להיות URI של הפרוטוקול או טקסט ישיר
        /// פורמט ה-URI: clicktotranslate://translate?text=...
        /// </summary>
        private static string ExtractText(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return string.Empty;

            // אם זה URI של הפרוטוקול
            if (arg.StartsWith("clicktotranslate:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(arg);
                    var query = uri.Query;
                    if (!string.IsNullOrEmpty(query))
                    {
                        // חיפוש הפרמטר text=
                        var queryStr = query.TrimStart('?');
                        var pairs = queryStr.Split('&');
                        foreach (var pair in pairs)
                        {
                            var kv = pair.Split('=', 2);
                            if (kv.Length == 2 && kv[0].Equals("text", StringComparison.OrdinalIgnoreCase))
                            {
                                return Uri.UnescapeDataString(kv[1]);
                            }
                        }
                    }

                    // אם אין query, ייתכן שהטקסט נמצא בחלק של הנתיב
                    var path = uri.AbsolutePath.TrimStart('/');
                    return Uri.UnescapeDataString(path);
                }
                catch
                {
                    // אם פיענוח ה-URI נכשל, ננסה להשתמש בערך כפי שהוא
                    return arg.Substring("clicktotranslate:".Length).TrimStart('/');
                }
            }

            // ייתכן שהועברה נתיב לקובץ טקסט
            if (File.Exists(arg) && arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return File.ReadAllText(arg);
                }
                catch { /* להמשיך */ }
            }

            // אחרת - הטקסט עצמו הועבר כארגומנט
            return arg;
        }
    }
}
