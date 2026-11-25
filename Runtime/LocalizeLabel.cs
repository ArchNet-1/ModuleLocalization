// ArchNet · Localization Module — label
// Put it on a TextMeshPro text: it shows the translation of its key and updates when the
// language changes. Leave Key empty to use the text typed in the component as the key.
// Docs: https://archnet-plateform.fr/#ModuleLocalization

using TMPro;
using UnityEngine;

namespace ArchNet.Module
{
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("ArchNet/Localize Label")]
    public class LocalizeLabel : MonoBehaviour
    {
        [Tooltip("Translation key. Empty = the component's text.")]
        [SerializeField] string _key;

        TMP_Text _label;

        public string Key { get => _key; set { _key = value; Refresh(); } }

        void Awake()
        {
            _label = GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(_key)) _key = _label.text;
        }

        void OnEnable()
        {
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDisable() => Localization.LanguageChanged -= Refresh;

        /// <summary>Shows the translation of the key in the current language.</summary>
        public void Refresh()
        {
            if (_label != null) _label.text = Localization.Get(_key);
        }
    }
}
