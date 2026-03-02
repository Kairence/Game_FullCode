## XSpawner archive

The legacy `XSpawns` dataset was moved from `Scripts/XSpawner Rev1.4/XSpawns` to this archive path to keep runtime script sources cleaner while preserving data.

Compatibility is preserved in `XmlSpawner.LocateFile` by adding fallback lookup roots (after normal lookup order):

- `Scripts/XSpawner Rev1.4`
- `Archive/XSpawner`

This keeps existing commands that still reference `XSpawns/...` working without requiring changes to command tables.
