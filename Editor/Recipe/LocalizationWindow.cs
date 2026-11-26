// ArchNet · Localization Module — Localization tool (ArchNet Hub > Services > Localization > Open tool)
// Every key with its text in each language, side by side: missing texts are outlined, search,
// add a key or a language, import / export CSV for translators, then Save.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArchNet.Module.Editor
{
    public class LocalizationWindow : EditorWindow
    {
        const string StylePath = "Packages/com.archnet.hub/Editor/UI/Hub.uss";
        const float KeyWidth = 220f;
        const float TextWidth = 260f;

        Dictionary<string, Dictionary<string, string>> _languages = new Dictionary<string, Dictionary<string, string>>();
        string _search = "";
        bool _dirty;
        Label _status;
        VisualElement _rows;

        public static void Open()
        {
            var window = GetWindow<LocalizationWindow>();
            window.titleContent = new GUIContent("Localization");
            window.minSize = new Vector2(720, 400);
        }

        void CreateGUI()
        {
            Load();
            VisualElement root = rootVisualElement;
            root.AddToClassList("archnet-hub");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (style != null) root.styleSheets.Add(style);
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = 8;

            var toolbar = new VisualElement();
            toolbar.AddToClassList("row");
            toolbar.AddToClassList("toolbar");
            var search = new TextField { value = _search };
            search.textEdition.placeholder = "Search keys and texts…";
            search.style.minWidth = 200;
            search.RegisterValueChangedCallback(evt =>
            {
                _search = evt.newValue ?? "";
                BuildRows();
            });
            toolbar.Add(search);
            toolbar.Add(new Button(AddKey) { text = "Add key" });
            toolbar.Add(new Button(AddLanguage) { text = "Add language" });
            toolbar.Add(new Button(ImportCsv) { text = "Import CSV" });
            toolbar.Add(new Button(ExportCsv) { text = "Export CSV" });
            var spacer = new VisualElement();
            spacer.AddToClassList("grow");
            toolbar.Add(spacer);
            var save = new Button(Save) { text = "Save" };
            save.AddToClassList("primary");
            toolbar.Add(save);
            root.Add(toolbar);

            var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scroll.style.flexGrow = 1;
            _rows = new VisualElement();
            scroll.Add(_rows);
            root.Add(scroll);

            _status = new Label();
            _status.AddToClassList("status-bar");
            root.Add(_status);
            BuildRows();
        }

        void Load()
        {
            _languages.Clear();
            if (!Directory.Exists(LocalizationRecipe.LanguagesFolder)) return;
            foreach (string file in Directory.GetFiles(LocalizationRecipe.LanguagesFolder, "*.json"))
            {
                try
                {
                    _languages[Path.GetFileNameWithoutExtension(file)] =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file)) ?? new Dictionary<string, string>();
                }
                catch (JsonException exception)
                {
                    Debug.LogError($"[ArchNet] {file} is not valid JSON ({exception.Message}); it is left out until fixed.");
                }
            }
        }

        string[] Codes => _languages.Keys.OrderBy(c => c == "en" ? "" : c).ToArray();

        void BuildRows()
        {
            if (_rows == null) return;
            _rows.Clear();
            if (_languages.Count == 0)
            {
                var empty = new Label($"No language file in {LocalizationRecipe.LanguagesFolder}. Add the Localization service from ArchNet > Hub > Services.");
                empty.AddToClassList("intro");
                _rows.Add(empty);
                UpdateStatus();
                return;
            }

            string[] codes = Codes;
            var header = Line();
            header.Add(Cell(new Label("Key"), KeyWidth));
            foreach (string code in codes) header.Add(Cell(new Label($"{code} · {Localization.DisplayName(code)}"), TextWidth));
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            _rows.Add(header);

            IEnumerable<string> keys = _languages.Values.SelectMany(texts => texts.Keys).Distinct().OrderBy(k => k);
            foreach (string key in keys.Where(Matches))
            {
                var line = Line();
                line.Add(Cell(new Label(key), KeyWidth));
                foreach (string code in codes)
                {
                    string language = code;
                    _languages[language].TryGetValue(key, out string text);
                    var field = new TextField { value = text ?? "", multiline = true };
                    field.EnableInClassList("invalid", string.IsNullOrEmpty(text));
                    field.RegisterValueChangedCallback(evt =>
                    {
                        _languages[language][key] = evt.newValue;
                        field.EnableInClassList("invalid", string.IsNullOrEmpty(evt.newValue));
                        _dirty = true;
                        UpdateStatus();
                    });
                    line.Add(Cell(field, TextWidth));
                }
                line.Add(new Button(() =>
                {
                    foreach (Dictionary<string, string> texts in _languages.Values) texts.Remove(key);
                    _dirty = true;
                    BuildRows();
                }) { text = "✕", tooltip = $"Delete {key}" });
                _rows.Add(line);
            }
            UpdateStatus();
        }

        bool Matches(string key) => _search.Length == 0 ||
            key.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0 ||
            _languages.Values.Any(texts => texts.TryGetValue(key, out string text) && text != null && text.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0);

        void UpdateStatus()
        {
            if (_status == null) return;
            int keys = _languages.Values.SelectMany(t => t.Keys).Distinct().Count();
            IEnumerable<string> missing = Codes.Select(code => (code, count: _languages.Values.SelectMany(t => t.Keys).Distinct().Count(k => !_languages[code].TryGetValue(k, out string text) || string.IsNullOrEmpty(text))))
                .Where(m => m.count > 0).Select(m => $"{m.code}: {m.count} missing");
            _status.text = $"{keys} keys · " + (missing.Any() ? string.Join(" · ", missing) : "nothing missing") + (_dirty ? " · not saved" : "");
            _status.EnableInClassList("status--warning", _dirty || missing.Any());
        }

        void AddKey()
        {
            string key = "new_key";
            for (int i = 2; _languages.Values.Any(t => t.ContainsKey(key)); i++) key = $"new_key_{i}";
            foreach (Dictionary<string, string> texts in _languages.Values) texts[key] = "";
            _search = "";
            _dirty = true;
            BuildRows();
            ShowNotification(new GUIContent($"Added \"{key}\": rename it in the JSON or use it as is."));
        }

        void AddLanguage()
        {
            string path = EditorUtility.SaveFilePanelInProject("New language", "de", "json", "File name = language code (de, es, it…).", LocalizationRecipe.LanguagesFolder);
            if (string.IsNullOrEmpty(path)) return;
            string code = Path.GetFileNameWithoutExtension(path);
            IEnumerable<string> keys = _languages.Values.SelectMany(t => t.Keys).Distinct();
            _languages[code] = keys.ToDictionary(k => k, _ => "");
            _dirty = true;
            Save();
        }

        void Save()
        {
            Directory.CreateDirectory(LocalizationRecipe.LanguagesFolder);
            foreach (KeyValuePair<string, Dictionary<string, string>> language in _languages)
            {
                string path = $"{LocalizationRecipe.LanguagesFolder}/{language.Key}.json";
                var sorted = language.Value.OrderBy(p => p.Key == "language_name" ? "" : p.Key).ToDictionary(p => p.Key, p => p.Value);
                File.WriteAllText(path, JsonConvert.SerializeObject(sorted, Formatting.Indented) + "\n");
                AssetDatabase.ImportAsset(path);
            }
            _dirty = false;
            BuildRows();
            ShowNotification(new GUIContent("Saved. Texts update the next time the language is loaded."));
        }

        void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export for translators", "", "localization.csv", "csv");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllText(path, LocalizationCsv.Write(_languages));
            EditorUtility.RevealInFinder(path);
        }

        void ImportCsv()
        {
            string path = EditorUtility.OpenFilePanel("Import translations", "", "csv");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> language in LocalizationCsv.Read(File.ReadAllText(path)))
                {
                    if (!_languages.TryGetValue(language.Key, out Dictionary<string, string> texts)) _languages[language.Key] = texts = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> pair in language.Value) texts[pair.Key] = pair.Value;
                }
                _dirty = true;
                BuildRows();
            }
            catch (FormatException exception)
            {
                EditorUtility.DisplayDialog("Import translations", exception.Message, "OK");
            }
        }

        static VisualElement Line()
        {
            var line = new VisualElement();
            line.AddToClassList("row");
            line.style.marginBottom = 2;
            return line;
        }

        static VisualElement Cell(VisualElement content, float width)
        {
            content.style.width = width;
            content.style.flexShrink = 0;
            content.style.marginRight = 4;
            return content;
        }
    }
}
