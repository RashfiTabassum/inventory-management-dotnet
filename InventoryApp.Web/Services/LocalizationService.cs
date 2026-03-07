using System.Text.Json;

namespace InventoryApp.Web.Services
{
    public class LocalizationService
    {
        private Dictionary<string, string> _strings = new();
        private string _currentLanguage = "en";

        public string CurrentLanguage => _currentLanguage;

        public event Action? OnLanguageChanged;

        public LocalizationService()
        {
            LoadLanguage("en");
        }

        public void SetLanguage(string lang)
        {
            if (_currentLanguage == lang) return;
            _currentLanguage = lang;
            LoadLanguage(lang);
            OnLanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            return _strings.TryGetValue(key, out var value)
                ? value : key;
        }

        private void LoadLanguage(string lang)
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                $"Strings.{lang}.json");

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            _strings = JsonSerializer
                .Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }
    }
}