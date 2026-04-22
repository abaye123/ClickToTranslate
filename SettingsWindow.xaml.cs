using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ClickToTranslate
{
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;

        private static readonly List<LanguageItem> Languages = new()
        {
            new("he", "עברית"), new("en", "אנגלית"), new("ar", "ערבית"),
            new("ru", "רוסית"), new("es", "ספרדית"), new("fr", "צרפתית"),
            new("de", "גרמנית"), new("it", "איטלקית"), new("pt", "פורטוגזית"),
            new("nl", "הולנדית"), new("pl", "פולנית"), new("tr", "טורקית"),
            new("uk", "אוקראינית"), new("zh", "סינית"), new("ja", "יפנית"),
            new("ko", "קוריאנית"), new("hi", "הינדית"), new("th", "תאית"),
            new("vi", "ויאטנמית"), new("id", "אינדונזית")
        };

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            // מילוי קומבו שפות
            TargetLangCombo.ItemsSource = Languages;
            TargetLangCombo.DisplayMemberPath = "Display";
            TargetLangCombo.SelectedValuePath = "Code";
            TargetLangCombo.SelectedValue = _settings.TargetLanguage;

            // מנוע
            EngineCombo.SelectedIndex = _settings.Engine == TranslationEngine.DeepL ? 1 : 0;

            // DeepL
            DeepLKeyBox.Text = _settings.DeepLApiKey;
            DeepLFreeTierCheck.IsChecked = _settings.DeepLUseFreeTier;

            // מיקום
            foreach (ComboBoxItem item in PositionCombo.Items)
            {
                if ((string)item.Tag! == _settings.WindowPosition)
                {
                    PositionCombo.SelectedItem = item;
                    break;
                }
            }
            if (PositionCombo.SelectedItem == null) PositionCombo.SelectedIndex = 0;

            AutoCopyCheck.IsChecked = _settings.AutoCopyToClipboard;

            SettingsPathText.Text = $"קובץ הגדרות: {SettingsManager.GetSettingsPath()}";

            UpdateRegisterStatus();
        }

        private void UpdateRegisterStatus()
        {
            RegisterStatusText.Text = UriProtocolHandler.IsRegistered()
                ? "✓ הפרוטוקול רשום במערכת."
                : "⚠ הפרוטוקול עדיין לא רשום. לחץ על 'רשום פרוטוקול' כדי לאפשר ל-Click to Do לפתוח את התוכנה.";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // מנוע
            _settings.Engine = EngineCombo.SelectedIndex == 1
                ? TranslationEngine.DeepL
                : TranslationEngine.Google;

            // שפת יעד
            if (TargetLangCombo.SelectedValue is string code)
            {
                _settings.TargetLanguage = code;
            }

            _settings.DeepLApiKey = DeepLKeyBox.Text.Trim();
            _settings.DeepLUseFreeTier = DeepLFreeTierCheck.IsChecked == true;
            _settings.AutoCopyToClipboard = AutoCopyCheck.IsChecked == true;

            if (PositionCombo.SelectedItem is ComboBoxItem posItem && posItem.Tag is string pos)
            {
                _settings.WindowPosition = pos;
            }

            SettingsManager.Save(_settings);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UriProtocolHandler.Register();
                MessageBox.Show("הפרוטוקול נרשם בהצלחה.", "ClickToTranslate",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateRegisterStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"שגיאה ברישום: {ex.Message}", "ClickToTranslate",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnregisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UriProtocolHandler.Unregister();
                MessageBox.Show("הרישום בוטל.", "ClickToTranslate",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateRegisterStatus();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"שגיאה: {ex.Message}", "ClickToTranslate",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class LanguageItem
        {
            public string Code { get; }
            public string Display { get; }
            public LanguageItem(string code, string name)
            {
                Code = code;
                Display = $"{name} ({code})";
            }
        }
    }
}
