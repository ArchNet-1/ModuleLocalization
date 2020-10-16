using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArchNet.Module.Localization
{
    /// <summary>
    /// [MODULE] - [ARCH NET] - [LOCALIZATION] - Localize a drop down component
    /// author : LOUIS PAKEL
    /// </summar
    public class LocalizeDropDown : MonoBehaviour
    {
        [SerializeField, Tooltip("Localize the component on start")]
        // Localize on start
        private bool _localizeOnStart = true;

        void Start()
        {
            // Localize on start
            if (_localizeOnStart)
            {
                // Localize
                Localize();
            }
        }

        /// <summary>
        /// Description : Localize the component
        /// </summary>
        public void Localize()
        {
            try
            {
                // try to localize every child of the dropdown
                for (int i = 0; i < gameObject.GetComponent<TMP_Dropdown>().options.Count; i++)
                {

                    // Component is a localize key
                    if (gameObject.GetComponent<TMP_Dropdown>().options[i].text.Contains("key_"))
                    {
                        // Get the localize value
                        ModuleLocalization.Instance().localizations.TryGetValue(gameObject.GetComponent<TMP_Dropdown>().options[i].text, out string value);

                        // Set dropdown
                        gameObject.GetComponent<TMP_Dropdown>().options[i].text = value;
                    }
                    else
                    {
                        Debug.LogError(Constants.ERROR_406);
                    }
                }

            }
            catch (KeyNotFoundException)
            {
                Debug.LogError(Constants.ERROR_405);
            }
        }
    }
}