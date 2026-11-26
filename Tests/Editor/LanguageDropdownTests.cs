using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace ArchNet.Module.Tests
{
    public class LanguageDropdownTests
    {
        const string LanguageKey = "ArchNet.Language";

        [Test]
        public void FollowsALanguageChangedFromCode()
        {
            var codes = Localization.AvailableLanguages.ToList();
            Assume.That(codes.Count, Is.GreaterThanOrEqualTo(2), "needs two files in a Resources/Languages folder");
            bool hadChoice = PlayerPrefs.HasKey(LanguageKey);
            string previous = Localization.Language;
            var go = new GameObject("Language", typeof(TMP_Dropdown), typeof(LanguageDropdown));
            var component = go.GetComponent<LanguageDropdown>();
            try
            {
                Localization.SetLanguage(codes[0]);
                Call(component, "Awake");
                Call(component, "OnEnable"); // Edit Mode tests do not run MonoBehaviour messages

                Localization.SetLanguage(codes[1]);

                Assert.AreEqual(1, go.GetComponent<TMP_Dropdown>().value);
            }
            finally
            {
                Call(component, "OnDisable");
                Object.DestroyImmediate(go);
                Localization.SetLanguage(previous);
                if (!hadChoice) PlayerPrefs.DeleteKey(LanguageKey);
            }
        }

        static void Call(Object target, string message) =>
            target.GetType().GetMethod(message, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(target, null);
    }
}
