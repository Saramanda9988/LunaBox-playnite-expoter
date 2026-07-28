# LunaBox Exporter for Playnite

**English** | [简体中文](README.zh-CN.md)

A C# generic plugin for Playnite 10 that exports your Playnite game library as a UTF-8 JSON file supported by LunaBox.

The plugin targets PlayniteSDK 6.4.0, which shipped with the first stable Playnite 10 release. It can therefore be loaded by all released Playnite 10 versions.

## Exported data

Each exported game can include its name, developers, description, rating, release date, cover, local launch executable, installation directory, tracking process, completion status, tags, and source identifiers. Steam games also include the Steam launch information used by LunaBox.

Playnite stores total accumulated playtime rather than individual play sessions. The plugin therefore does not generate artificial session records.

## Installation

1. Download the `.pext` package from the [latest GitHub Release](../../releases/latest).
2. Open the package, confirm the installation in Playnite, and restart Playnite.

## Usage

1. In Playnite Desktop Mode, open **Extensions** and select **Export library for LunaBox**.
2. Choose a location for the generated JSON file.
3. Open the LunaBox game library and select **Add Game → Import from Playnite**.
4. Select the exported JSON file, review the preview, and start the import.

## Local build

```powershell
.\build.ps1
```

Build output is written to `bin\Release`. For development, add this directory in **Playnite → Settings → For developers → External extensions**.

To create a `.pext` package, provide the `Toolbox.exe` included with Playnite:

```powershell
.\build.ps1 -ToolboxPath "C:\Playnite\Toolbox.exe"
```

## Limitations

Non-Steam games launched through store integrations often do not expose a local launch executable that another application can use. These games remain in the exported JSON and appear in the LunaBox import preview with an empty launch executable.
