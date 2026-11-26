using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ArchNet.Module.Tests
{
    public class LocalizationCsvTests
    {
        [Test]
        public void TextsSurviveARoundTrip()
        {
            var languages = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> { ["play"] = "Play", ["quote"] = "Say \"hi\", then go", ["lines"] = "One\nTwo" },
                ["fr"] = new Dictionary<string, string> { ["play"] = "Jouer", ["quote"] = "Dis « salut », puis pars" },
            };

            Dictionary<string, Dictionary<string, string>> back = LocalizationCsv.Read(LocalizationCsv.Write(languages));

            Assert.AreEqual("Say \"hi\", then go", back["en"]["quote"]);
            Assert.AreEqual("One\nTwo", back["en"]["lines"]);
            Assert.AreEqual("Jouer", back["fr"]["play"]);
            Assert.AreEqual("", back["fr"]["lines"], "a text missing in a language comes back empty");
        }

        [Test]
        public void ReadsWindowsLineEndings()
        {
            Dictionary<string, Dictionary<string, string>> languages = LocalizationCsv.Read("key,en,fr\r\nplay,Play,Jouer\r\nquit,Quit,Quitter\r\n");
            Assert.AreEqual("Quitter", languages["fr"]["quit"]);
            Assert.AreEqual(2, languages["en"].Count);
        }

        [Test]
        public void RefusesAFileWithoutAKeyColumn() =>
            Assert.Throws<FormatException>(() => LocalizationCsv.Read("en,fr\nPlay,Jouer\n"));
    }
}
