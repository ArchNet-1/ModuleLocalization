<p align="center">
  <a href="https://archnet-plateform.fr"><img src="https://archnet-plateform.fr/ArchNetLogoWithBck.png" alt="ArchNet" width="240"></a>
</p>

# ArchNet · Localization Module

Translates your texts from JSON files (Resources/Languages/<code>.json); labels and dropdowns update when the language changes.

- Unity **6000.0** or newer · Needs `com.unity.ugui`, `com.unity.nuget.newtonsoft-json`.
- Part of [ArchNet](https://archnet-plateform.fr), free and open-source Unity packages

## Install

*Window > Package Manager > + > Install package from git URL…*

```
https://github.com/ArchNet-1/ModuleLocalization.git
```

## Use

Create `Resources/Languages/en.json` (`{ "key_play": "Play" }`) and one file per language, add **Localize Label** to your texts, then `Localization.SetLanguage("fr")`. Import the **Languages** sample for examples.

## Add it to your game (one click)

*ArchNet > Hub > Services > Localization > Add to game* creates `Assets/ArchNet/Localization/Resources/Languages/en.json` and `fr.json` with the texts of the ArchNet screens, makes every matching label of the ArchNet menus follow the language, and adds a **Language** row to the Settings menu (`LanguageDropdown`). Click **Update** after adding screens.

**Open tool**: every key in every language side by side (missing texts outlined), search, add a key or a language (`de.json`…), **Export CSV** for translators and **Import CSV** back, then Save. Give each file a `language_name` key (`"Français"`) for the language list.

## License

MIT, see [LICENSE.md](LICENSE.md).
