using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClickToTranslate
{
    public partial class TranslationWindow : Window
    {
        private readonly AppSettings _settings;
        private string _sourceText;
        private bool _comboInitializing = true;

        // רשימת שפות נפוצות - קוד ISO + שם תצוגה
        private static readonly List<LanguageOption> SupportedLanguages = new()
        {
            new("he", "עברית"),
            new("en", "אנגלית"),
            new("ar", "ערבית"),
            new("ru", "רוסית"),
            new("es", "ספרדית"),
            new("fr", "צרפתית"),
            new("de", "גרמנית"),
            new("it", "איטלקית"),
            new("pt", "פורטוגזית"),
            new("nl", "הולנדית"),
            new("pl", "פולנית"),
            new("tr", "טורקית"),
            new("uk", "אוקראינית"),
            new("zh", "סינית"),
            new("ja", "יפנית"),
            new("ko", "קוריאנית"),
            new("hi", "הינדית"),
            new("th", "תאית"),
            new("vi", "ויאטנמית"),
            new("id", "אינדונזית")
        };

        public TranslationWindow(string text, AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            _sourceText = text;

            SourceTextBox.Text = text;
            EngineLabel.Text = $"· {(_settings.Engine == TranslationEngine.DeepL ? "DeepL" : "Google")}";
            Opacity = Math.Clamp(settings.WindowOpacity, 0.5, 1.0);

            PopulateLanguageCombo();
            PositionWindow();

            Loaded += async (_, _) => await TranslateAsync();
        }

        private void PopulateLanguageCombo()
        {
            TargetLangCombo.ItemsSource = SupportedLanguages;
            TargetLangCombo.DisplayMemberPath = "Display";
            TargetLangCombo.SelectedValuePath = "Code";
            TargetLangCombo.SelectedValue = _settings.TargetLanguage;
            _comboInitializing = false;
        }

        private async void TargetLangCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comboInitializing) return;
            if (TargetLangCombo.SelectedValue is string lang)
            {
                _settings.TargetLanguage = lang;
                await TranslateAsync();
            }
        }

        private async System.Threading.Tasks.Task TranslateAsync()
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                TranslatedTextBox.Text = "";
                DetectedLangLabel.Text = "";

                _sourceText = SourceTextBox.Text;
                if (string.IsNullOrWhiteSpace(_sourceText))
                {
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    TranslatedTextBox.Text = "(אין טקסט לתרגום)";
                    return;
                }

                var result = await TranslationService.TranslateAsync(_sourceText, _settings);

                TranslatedTextBox.Text = result.TranslatedText;

                if (!string.IsNullOrEmpty(result.DetectedSourceLanguage))
                {
                    DetectedLangLabel.Text = $"זוהה: {result.DetectedSourceLanguage}";
                }

                if (_settings.AutoCopyToClipboard && !string.IsNullOrEmpty(result.TranslatedText))
                {
                    try { Clipboard.SetText(result.TranslatedText); } catch { /* לעיתים ה-clipboard תפוס */ }
                }
            }
            catch (Exception ex)
            {
                TranslatedTextBox.Text = $"⚠️ שגיאה בתרגום:\n{ex.Message}";
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        #region UI Handlers

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TranslatedTextBox.Text))
                {
                    Clipboard.SetText(TranslatedTextBox.Text);
                    ShowCopiedFeedback();
                }
            }
            catch { /* clipboard עלול להיות תפוס */ }
        }

        private void ShowCopiedFeedback()
        {
            var original = TitleText.Text;
            TitleText.Text = "✓ הועתק!";
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1200)
            };
            timer.Tick += (_, _) => { TitleText.Text = original; timer.Stop(); };
            timer.Start();
        }

        private async void RetranslateButton_Click(object sender, RoutedEventArgs e)
        {
            await TranslateAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow(_settings) { Owner = this };
            if (win.ShowDialog() == true)
            {
                // הגדרות נשמרו - רענן תצוגה
                EngineLabel.Text = $"· {(_settings.Engine == TranslationEngine.DeepL ? "DeepL" : "Google")}";
                _comboInitializing = true;
                TargetLangCombo.SelectedValue = _settings.TargetLanguage;
                _comboInitializing = false;
            }
        }

        #endregion

        #region Positioning

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private void PositionWindow()
        {
            try
            {
                switch (_settings.WindowPosition?.ToLowerInvariant())
                {
                    case "center":
                        WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        break;
                    case "top-right":
                        var workArea = SystemParameters.WorkArea;
                        Left = workArea.Left + workArea.Width - Width - 20;
                        Top = workArea.Top + 20;
                        break;
                    case "top-left":
                        var waLeft = SystemParameters.WorkArea;
                        Left = waLeft.Left + 20;
                        Top = waLeft.Top + 20;
                        break;
                    case "cursor":
                    default:
                        if (GetCursorPos(out var pt))
                        {
                            // המרה מ-pixels פיזיים ל-DIPs של WPF
                            var source = PresentationSource.FromVisual(this) ??
                                         new System.Windows.Interop.HwndSource(
                                             new System.Windows.Interop.HwndSourceParameters());
                            double dpiScale = 1.0;
                            if (source?.CompositionTarget != null)
                            {
                                dpiScale = source.CompositionTarget.TransformToDevice.M11;
                            }

                            Left = (pt.X / dpiScale) + 15;
                            Top = (pt.Y / dpiScale) + 15;

                            // שמירה בתוך גבולות המסך
                            var wa = SystemParameters.WorkArea;
                            if (Left + Width > wa.Right) Left = wa.Right - Width - 10;
                            if (Top + Height > wa.Bottom) Top = wa.Bottom - Height - 10;
                            if (Left < wa.Left) Left = wa.Left + 10;
                            if (Top < wa.Top) Top = wa.Top + 10;
                        }
                        else
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        }
                        break;
                }
            }
            catch
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        #endregion

        // מחלקת עזר לקומבו
        private class LanguageOption
        {
            public string Code { get; }
            public string Display { get; }
            public LanguageOption(string code, string display)
            {
                Code = code;
                Display = $"{display} ({code})";
            }
        }
    }
}
