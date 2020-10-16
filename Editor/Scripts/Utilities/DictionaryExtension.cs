using System.Collections.Generic;

namespace ArchNet.Module.Localization
{
    /// <summary>
    /// [MODULE] - [ARCH NET] - [LOCALIZATION]
    /// author : LOUIS PAKEL
    /// </summar
    public static class DictionaryExtension
    {
        /// <summary>
        /// Description : Strip empty values in a dictionary
        /// </summary>
        /// <param name="dictionnaryToFilter"></param>
        /// <returns></returns>
        public static Dictionary<string, string> StripEmptyValues(this Dictionary<string, string> pDictionary)
        {
            // Clean dictionnary
            Dictionary<string, string> lResult = new Dictionary<string, string>();

            // Strip param dictionnary
            foreach (string key in pDictionary.Keys)
            {
                // Get the dictionnay value from the key
                pDictionary.TryGetValue(key, out string value);

                if (value != "")
                {
                    // Add value to clean dictionary
                    lResult.Add(key, value);
                }
            }
            return lResult;
        }
    }


}