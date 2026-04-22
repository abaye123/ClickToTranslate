# ClickToTranslate

כלי תרגום מהיר ל-Windows 11 המשתלב עם תכונת **Click to Do** החדשה. בחר טקסט במסך, לחץ על Click to Do, ופתח אותו בתוכנה זו לתרגום מיידי בחלונית צפה.

## ✨ תכונות

- 🌐 **שני מנועי תרגום**: Google Translate (חינמי, ללא מפתח) ו-DeepL (איכותי, דורש API key)
- 🎯 **שילוב עם Click to Do**: דרך רישום URI scheme מותאם אישית (`clicktotranslate://`)
- 🎨 **חלונית מינימליסטית** עם עיצוב מודרני, טופ-מוסט, גרירה חופשית
- ⚙️ **קובץ הגדרות JSON** חיצוני עם כל ההעדפות
- 🌍 **20+ שפות** נתמכות
- 📋 **העתקה אוטומטית** ללוח (אופציונלי)
- 📍 **מיקום חלונית** מותאם אישית (ליד הסמן / מרכז / פינה ימנית-עליונה)
- 🇮🇱 **ממשק בעברית** מלא עם תמיכת RTL

## 🛠️ דרישות מערכת

- Windows 10/11
- [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- לפיתוח/קומפילציה: .NET 8 SDK

## 🔨 קומפילציה

פתח PowerShell או CMD בתיקיית הפרויקט והרץ:

```powershell
dotnet restore
dotnet build -c Release
```

או ליצירת EXE יחיד להפצה:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

ה-EXE הסופי ימצא ב:
```
bin\Release\net8.0-windows\win-x64\publish\ClickToTranslate.exe
```

## 🚀 התקנה ראשונית

1. הפעל את `ClickToTranslate.exe` - ייפתח חלון ההגדרות.
2. לחץ על **"רשום פרוטוקול"** - זה רושם את ה-scheme `clicktotranslate://` ברג'יסטרי של Windows (תחת HKCU - לא דורש הרשאות אדמין).
3. הגדר:
   - מנוע תרגום מועדף (Google / DeepL)
   - שפת יעד ברירת מחדל
   - מפתח DeepL (אם בחרת ב-DeepL)
   - מיקום החלונית
4. לחץ "שמור".

### הגדרה מהירה דרך שורת פקודה:

```powershell
ClickToTranslate.exe --register     # רישום הפרוטוקול
ClickToTranslate.exe --unregister   # ביטול רישום
ClickToTranslate.exe --settings     # פתיחת חלון הגדרות
```

## 📖 שימוש עם Click to Do

> **הערה חשובה**: Click to Do הושק בעיקר למחשבי Copilot+ PC. דרך ההרחבה המלאה משתנה בין גרסאות Windows. שיטת ה-URI scheme מבטיחה שהתוכנה תעבוד עם כל אפליקציה שיכולה להפעיל URI handlers.

### אופציה 1: דרך Click to Do (Copilot+ PC)

1. סמן טקסט על המסך.
2. הפעל את Click to Do (Win+Click או לחיצה ימנית על טקסט נבחר).
3. בחר "פתח באמצעות..." או בחר אפליקציה מותאמת אישית.
4. התוכנה תופתח עם הטקסט המתורגם.

### אופציה 2: הפעלה ישירה (עובדת תמיד)

אפשר להפעיל את התוכנה עם טקסט מכל מקום:

```powershell
# דרך URI
start clicktotranslate://translate?text=Hello%20World

# דרך ארגומנט ישיר
ClickToTranslate.exe "Hello World"
```

### אופציה 3: קיצור מקשים עם PowerToys / AutoHotkey

אפשר להגדיר קיצור מקשים שלוקח את הטקסט הנבחר ומפעיל את התוכנה. דוגמה ל-AutoHotkey v2:

```autohotkey
^!t::  ; Ctrl+Alt+T
{
    SendInput "^c"
    Sleep 100
    Run 'ClickToTranslate.exe "' . A_Clipboard . '"'
}
```

## ⚙️ קובץ ההגדרות

ההגדרות נשמרות ב-JSON בכתובת:
```
%APPDATA%\ClickToTranslate\settings.json
```

דוגמה לתוכן:

```json
{
  "Engine": "Google",
  "TargetLanguage": "he",
  "SourceLanguage": "auto",
  "DeepLApiKey": "",
  "DeepLUseFreeTier": true,
  "WindowPosition": "cursor",
  "AutoCopyToClipboard": false,
  "WindowOpacity": 0.98
}
```

### קודי שפות נתמכים

`he`, `en`, `ar`, `ru`, `es`, `fr`, `de`, `it`, `pt`, `nl`, `pl`, `tr`, `uk`, `zh`, `ja`, `ko`, `hi`, `th`, `vi`, `id` — ועוד רבות (Google Translate תומך בכ-100 שפות).

## 🧱 מבנה הפרויקט

```
ClickToTranslate/
├── ClickToTranslate.csproj     # הגדרות פרויקט .NET
├── App.xaml / App.xaml.cs      # נקודת כניסה, טיפול בארגומנטים
├── TranslationWindow.xaml/.cs  # חלונית התרגום הצפה
├── SettingsWindow.xaml/.cs     # חלון ההגדרות
├── TranslationService.cs       # לוגיקת התרגום (Google + DeepL)
├── SettingsManager.cs          # טעינה/שמירה של JSON
└── UriProtocolHandler.cs       # רישום ב-Windows Registry
```

## ⚠️ מגבלות ידועות

- **Google Translate API הציבורי** (`translate.googleapis.com/translate_a/single`) אינו תיעוד רשמי; הוא שמיש לרוב אבל עלול להשתנות או לחסום שימוש חוזר. לשימוש מסחרי מומלץ לעבור ל-Google Cloud Translation API רשמי (דורש מפתח ותשלום).
- **Click to Do API מלא**: שילוב "נייטיבי" (כפתור בתוך תפריט Click to Do) דורש שימוש ב-Windows App Actions SDK עם אפליקציית UWP/WinUI packaged. שיטת ה-URI שבחרנו עובדת בכל מקום אבל דורשת בחירה ידנית של "פתח באמצעות".
- DeepL Free מוגבל ל-500,000 תווים בחודש.

## 📝 רישיון

הקוד מסופק כקוד פתוח לצרכי למידה ושימוש אישי. שים לב לתנאי השימוש של Google ו-DeepL.
