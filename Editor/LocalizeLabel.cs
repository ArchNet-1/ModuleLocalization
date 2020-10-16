using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArchNet.Module.Localization
{
    /// <summary>
    /// [MODULE] - [ARCH NET] - [LOCALIZATION] - Localize a label component
    /// author : LOUIS PAKEL
    /// </summar
    public class LocalizeLabel : MonoBehaviour
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
                // Component is a localize key
                if (gameObject.GetComponent<TextMeshProUGUI>().text.Contains("key_"))
                {
                    // Get the localize value
                    ModuleLocalization.Instance().localizations.TryGetValue(gameObject.GetComponent<TextMeshProUGUI>().text, out string value);
                    
                    // Set label
                    gameObject.GetComponent<TextMeshProUGUI>().text = value;
                }
                else
                {
                    Debug.LogError(Constants.ERROR_406);
                }
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError(Constants.ERROR_405);
            }
        }
    }
}