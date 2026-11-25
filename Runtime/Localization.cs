// ArchNet · Localization Module
// Translates your texts from simple JSON files: Resources/Languages/<code>.json holding
// { "key_play": "Play" }. Localization.Get("key_play") returns the text in the current
// language, and LocalizeLabel / LocalizeDropDown update themselves when it changes.
// Import the "Languages" sample from the Package Manager for example files.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ArchNet.Module
{
    public static class Localization
    {
        const string LanguageKey = "ArchNet.Language";
        const string Folder = "Languages";

        /// <summary>Language used when a key is missing in the current one.</summary>
        public static string FallbackLanguage { get; set; } = "en";

        static string _language;
        static Dictionary<string, string> _texts;

        /// <summary>Raised after SetLanguage, so on-screen texts can refresh.</summary>
        public static event Action LanguageChanged;

        /// <summary>Code of the current language (file name without .json): saved choice, else the system language.</summary>
        public static string Language => _language ??= PlayerPrefs.GetString(LanguageKey, SystemLanguageCode());

        /// <summary>Codes of every language file found in a Resources/Languages folder.</summary>
        public static IEnumerable<string> AvailableLanguages
        {
            get
            {
                var codes = new SortedSet<string>();
                foreach (TextAsset file in Resources.LoadAll<TextAsset>(Folder)) codes.Add(file.name);
                return codes;
            }
        }

        /// <summary>Text for a key in the current language; the key itself when no translation exists.</summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            _texts ??= Load(Language);
            return _texts.TryGetValue(key, out string text) ? text : key;
        }

        /// <summary>True when the key has a translation in the current or fallback language.</summary>
        public static bool Has(string key) => !string.IsNullOrEmpty(key) && (_texts ??= Load(Language)).ContainsKey(key);

        /// <summary>Switches language, saves the choice and refreshes every localized component.</summary>
        public static void SetLanguage(string code)
        {
            _language = code;
            _texts = Load(code);
            PlayerPrefs.SetString(LanguageKey, code);
            LanguageChanged?.Invoke();
        }

        // Fallback texts first, then the language's own non-empty texts on top.
        static Dictionary<string, string> Load(string code)
        {
            var texts = new Dictionary<string, string>();
            if (code != FallbackLanguage) Merge(texts, FallbackLanguage);
            Merge(texts, code);
            return texts;
        }

        // Every Resources/Languages folder (yours and packages') can hold a file for the language.
        static void Merge(Dictionary<string, string> texts, string code)
        {
            foreach (TextAsset file in Resources.LoadAll<TextAsset>(Folder))
            {
                if (file.name != code) continue;
                try
                {
                    var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(file.text);
                    foreach (var pair in entries)
                    {
                        if (!string.IsNullOrEmpty(pair.Value)) texts[pair.Key] = pair.Value;
                    }
                }
                catch (JsonException e)
                {
                    Debug.LogError($"[ArchNet] Localization: '{Folder}/{code}.json' is not valid JSON: {e.Message}", file);
                }
            }
        }

        static string SystemLanguageCode() => Application.systemLanguage switch
        {
            SystemLanguage.French => "fr",
            SystemLanguage.German => "de",
            SystemLanguage.Spanish => "es",
            SystemLanguage.Italian => "it",
            SystemLanguage.Portuguese => "pt",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Chinese or SystemLanguage.ChineseSimplified => "zh",
            _ => "en",
        };

        // Domain reload can be disabled in Play Mode settings: start each play session clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _language = null;
            _texts = null;
            LanguageChanged = null;
        }
    }
}
