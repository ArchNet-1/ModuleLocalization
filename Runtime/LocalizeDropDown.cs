// ArchNet · Localization Module — dropdown
// Put it on a TextMeshPro dropdown whose options are translation keys: each option shows
// its translation and updates when the language changes.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArchNet.Module
{
    [RequireComponent(typeof(TMP_Dropdown))]
    [AddComponentMenu("ArchNet/Localize Dropdown")]
    public class LocalizeDropDown : MonoBehaviour
    {
        TMP_Dropdown _dropdown;
        readonly List<string> _keys = new List<string>();

        void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            foreach (TMP_Dropdown.OptionData option in _dropdown.options) _keys.Add(option.text);
        }

        void OnEnable()
        {
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDisable() => Localization.LanguageChanged -= Refresh;

        /// <summary>Shows every option in the current language.</summary>
        public void Refresh()
        {
            if (_dropdown == null) return;
            for (int i = 0; i < _keys.Count && i < _dropdown.options.Count; i++)
            {
                _dropdown.options[i].text = Localization.Get(_keys[i]);
            }
            _dropdown.RefreshShownValue();
        }
    }
}
