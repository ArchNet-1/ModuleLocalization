using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace ArchNet.Module.Localization
{
    /// <summary>
    /// [MODULE] - [ARCH NET] - [LOCALIZATION] Module localization manager
    /// author : LOUIS PAKEL
    /// </summary>
    public class ModuleLocalization
    {
        // Dictionary of localizations
        public Dictionary<string, string> localizations { get; private set; }

        // Dictionary of language
        private Dictionary<string, string> _languages = new Dictionary<string, string>();

        // Current language
        public string _currentLanguage = "";

        #region Singleton

        // Singleton
        private static ModuleLocalization instance;

        public static ModuleLocalization Instance()
        {
            if (instance == null)
            {
                instance = new ModuleLocalization();
            }

            return instance;
        }

        #endregion

        public ModuleLocalization()
        {
            try
            {
                // Set Localization
                localizations = new Dictionary<string, string>();

                // -------------- ADD NEW LANGUAGE HERE ---------------

                _languages.Add("fr_FR", "fr_Fr");
                _languages.Add("en_GB", "en");

                // ----------------------------------------------------

                // Get current language
                _languages.TryGetValue(LocalizationPath.lang, out _currentLanguage);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError(Constants.ERROR_405);
            }

            try
            {
                // Init Localizations
                InitLocalizations();
            }
            catch (FileLoadException)
            {
                Debug.LogError(Constants.ERROR_406);
            }
        }

        /// <summary>
        /// Description : Initiate localizations
        /// </summary>
        private void InitLocalizations()
        {
            // Set default language
            TextAsset _defaultLocalization = (TextAsset)Resources.Load(LocalizationPath.DefaultIsoCodePath, typeof(TextAsset));

            // Set localizations with default language
            localizations = JsonConvert.DeserializeObject<Dictionary<string, string>>(_defaultLocalization.text);

            // Get current language from ressources
            if (Resources.Load(LocalizationPath.IsoCodePath + _currentLanguage, typeof(TextAsset)))
            {
                // Select current localize file
                TextAsset _currentLocalizationFile = (TextAsset)Resources.Load(LocalizationPath.IsoCodePath + _currentLanguage, typeof(TextAsset));

                // Set current localization
                Dictionary<string, string> _currentLocalization = JsonConvert.DeserializeObject<Dictionary<string, string>>(_currentLocalizationFile.text);

                // Strip empty values ( non localize  value )
                _currentLocalization = _currentLocalization.StripEmptyValues();

                // Set value 
                _currentLocalization.ToList().ForEach(x => localizations[x.Key] = x.Value);
            }
        }
    }
}
