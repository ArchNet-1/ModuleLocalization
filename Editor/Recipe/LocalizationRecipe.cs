// ArchNet · Localization Module — Localization recipe (ArchNet Hub > Services > Localization)
// Creates Assets/ArchNet/Localization/Resources/Languages/en.json and fr.json with the texts of
// the ArchNet screens, puts a LocalizeLabel on every matching label of the ArchNet prefabs and
// scenes (menu, pause, settings, controls, credits, loading), and adds a Language row to the
// Settings menu when it exists. Click Update after adding screens.
// This assembly only compiles when the ArchNet Hub (com.archnet.hub 2.0.0+) is installed.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System.Collections.Generic;
using System.Linq;
using ArchNet.Core.Editor;
using ArchNet.Installer.Editor;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchNet.Module.Editor
{
    public sealed class LocalizationRecipe : ServiceRecipe
    {
        public const string RecipeId = "localization";
        public const string LanguagesFolder = RecipeRunner.RootFolder + "/Localization/Resources/Languages";

        // key → (English, French). The English text is also how labels of the ArchNet screens are recognised.
        static readonly (string key, string en, string fr)[] Texts =
        {
            ("language_name", "English", "Français"),
            ("play", "Play", "Jouer"), ("settings", "Settings", "Paramètres"), ("controls", "Controls", "Commandes"),
            ("credits", "Credits", "Crédits"), ("quit", "Quit", "Quitter"),
            ("pause", "Pause", "Pause"), ("paused", "Paused", "Pause"), ("resume", "Resume", "Reprendre"), ("main_menu", "Main menu", "Menu principal"),
            ("general", "General", "Général"), ("audio", "Audio", "Audio"), ("video", "Video", "Vidéo"),
            ("frame_rate", "Frame rate", "Images par seconde"), ("v_sync", "V-Sync", "Synchro verticale"),
            ("master", "Master", "Volume général"), ("music", "Music", "Musique"), ("effects", "Effects", "Effets"), ("interface", "Interface", "Interface"),
            ("display", "Display", "Affichage"), ("resolution", "Resolution", "Résolution"), ("quality", "Quality", "Qualité"), ("language", "Language", "Langue"),
            ("reset", "Reset", "Réinitialiser"), ("back", "Back", "Retour"), ("reset_keys", "Reset keys", "Réinitialiser les touches"),
            ("keyboard_mouse", "Keyboard / mouse", "Clavier / souris"), ("gamepad", "Gamepad", "Manette"),
            ("close", "Close", "Fermer"), ("loading", "Loading…", "Chargement…"),
        };

        public override string Id => RecipeId;
        public override string DisplayName => "Localization";
        public override string Description => "Your game in several languages: English and French files with the texts of the ArchNet screens, translated labels, a Language choice in Settings, and a window to edit keys (CSV import / export).";
        public override string Category => "UI";
        public override string FolderName => "Localization";
        public override bool HasTool => true;

        public override void Create(RecipeContext context)
        {
            context.WriteText("Resources/Languages/en.json", Json(t => t.en));
            context.WriteText("Resources/Languages/fr.json", Json(t => t.fr));
            AssetDatabase.Refresh();
            AddLanguageRow();
            Dictionary<string, string> keyOfText = Texts.Skip(1).ToDictionary(t => t.en, t => t.key);
            int labels = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { RecipeRunner.RootFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int added = Localize(root, keyOfText);
                    if (added == 0) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    labels += added;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { RecipeRunner.RootFolder }))
                labels += LocalizeScene(AssetDatabase.GUIDToAssetPath(guid), keyOfText);
            ArchNetLog.Info($"Localization: {labels} labels of the ArchNet screens now follow the language.");
        }

        public override void OpenTool() => LocalizationWindow.Open();

        static string Json(System.Func<(string key, string en, string fr), string> text) =>
            JsonConvert.SerializeObject(Texts.ToDictionary(t => t.key, text), Formatting.Indented) + "\n";

        // Static labels only: texts set by code (popups, key names, dropdown options, credits lines, tips) are left alone.
        static int Localize(GameObject root, Dictionary<string, string> keyOfText)
        {
            int added = 0;
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(text) || text.GetComponent<LocalizeLabel>() != null) continue;
                if (text.GetComponentInParent<TMP_Dropdown>(true) != null || text.GetComponentInParent<TMP_InputField>(true) != null) continue;
                if (IsDynamic(text.transform)) continue;
                if (!keyOfText.TryGetValue(text.text.Trim(), out string key)) continue;
                text.gameObject.AddComponent<LocalizeLabel>().Key = key;
                added++;
            }
            return added;
        }

        static bool IsDynamic(Transform transform)
        {
            for (Transform t = transform; t != null; t = t.parent)
            {
                if (t.name.Contains("Template") || t.name == "Key") return true;
                foreach (Component component in t.GetComponents<Component>())
                    if (component != null && (component.GetType().Name == "PopupView" || component.GetType().Name == "ControlRow")) return true;
            }
            return false;
        }

        static int LocalizeScene(string path, Dictionary<string, string> keyOfText)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool wasOpen = scene.isLoaded;
            if (wasOpen && scene.isDirty)
            {
                ArchNetLog.Warning($"Localization: {path} has unsaved changes and was skipped.", "Save the scene, then click Update on Localization.");
                return 0;
            }
            if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                int added = scene.GetRootGameObjects().Sum(root => Localize(root, keyOfText));
                if (added > 0) EditorSceneManager.SaveScene(scene);
                return added;
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }
        }

        // A Language row on the General page of the Settings menu, if the Settings service exists.
        static void AddLanguageRow()
        {
            const string settingsPath = RecipeRunner.RootFolder + "/Settings/Prefabs/Settings.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(settingsPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(settingsPath);
            try
            {
                if (root.GetComponentInChildren<LanguageDropdown>(true) != null) return;
                Transform page = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "General Page");
                if (page == null) return;
                UIBuilder.OptionRow("Language", page, out RectTransform slot);
                TMP_Dropdown dropdown = UIBuilder.Dropdown("Language", slot);
                dropdown.gameObject.AddComponent<LanguageDropdown>();
                PrefabUtility.SaveAsPrefabAsset(root, settingsPath);
                ArchNetLog.Info("Localization: a Language row was added to the Settings menu.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
