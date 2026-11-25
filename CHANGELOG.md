# Changelog

## [2.0.0] - 2026-11-25

Unity 6 release.

### Changed
- Requires Unity 6000.0 or newer.
- Code split into `Runtime/` (game code) and `Editor/` (Editor-only assembly, never in builds).
- Namespace `ArchNet.<Category>`; every file starts with a short description and a link to the documentation.
- Files moved with their `.meta`: existing scenes, prefabs and assets keep their references.
- Language chosen at runtime and saved (was hard-coded to fr_FR); system language by default.
- Language files are read from any Resources/Languages folder (they were outside Resources: crash).
- Labels keep their key, so the language can change again.
- Removed texts left from another project; Newtonsoft JSON dependency declared.
