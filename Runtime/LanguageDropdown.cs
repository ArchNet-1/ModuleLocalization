// ArchNet · Localization Module — language dropdown
// Put it on a TextMeshPro dropdown: it lists every language file (by its "language_name") and
// switches the game's language when the player picks one. The choice is saved.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ArchNet.Module
{
    [RequireComponent(typeof(TMP_Dropdown))]
    [AddComponentMenu("ArchNet/Language Dropdown")]
    public class LanguageDropdown : MonoBehaviour
    {
        TMP_Dropdown _dropdown;
        List<string> _codes = new List<string>();

        void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            _dropdown.onValueChanged.AddListener(index =>
            {
                if (index >= 0 && index < _codes.Count) Localization.SetLanguage(_codes[index]);
            });
        }

        void OnEnable()
        {
            _codes = Localization.AvailableLanguages.ToList();
            _dropdown.ClearOptions();
            _dropdown.AddOptions(_codes.Select(Localization.DisplayName).ToList());
            _dropdown.SetValueWithoutNotify(Mathf.Max(0, _codes.IndexOf(Localization.Language)));
        }
    }
}
