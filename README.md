# Charm - the Destiny tool that does everything

## What is this?

A new, flashier, fully C# version of my previous tool Phonon.

It is designed to support many versions of the Tiger engine, including many game versions of Destiny 2.

The tool focuses on providing as much access to the information in the game files as possible, **ideal for artists and content preservation**.
> [!CAUTION]
> # Disclaimer
> * Before you go any further, understand that Charm ***IS NOT*** a datamining tool! While it can access many things in the game files, it's main purpose is focused towards **3D artists, content preservation and learning how the game works**!
> * Please **DO NOT** use this tool to spread leaks and spoilers or anything that may break Bungie's TOS. Don't ruin the experience for yourself and others. Uncover things the way they were intended!
> * Seeing this tool used for such acts can and will result in fewer and fewer public updates and releases. I enjoy maintaining and updating this for others, don't be the one to ruin it. (Looking at you Deaks. You will be the reason Charm stops being supported.)

## How do I install and use it?

You'll first need at least one game installation.
Charm currently supports:

| Version | Description              | Where           |  Main manifest id   | Language manifest id |
|---------|--------------------------|-----------------|---------------------|----------------------|
| ?.?.?.? | Rise Of Iron last update | Ask in DMR      |                     |                      |
| 2.6.0.1 | Shadowkeep first update  | DepotDownloader | 7002268313830901797 | 2399965969279284756  |
| 2.9.9.9 | Shadowkeep last update   | DepotDownloader | 4160053308690659072 | 4651412338057797072  |
| 3.4.0.2 | Beyond Light last update | DepotDownloader | 5631185797932644936 | 3832609057880895101  |
| 6.3.0.7 | Witch Queen last update  | DepotDownloader | 6051526863119423207 | 1078048403901153652  |
| 7.3.6.6 | Lightfall last update    | DepotDownloader | 7707143404100984016 | 5226038440689554798  |
| 8.2.6.4 | The Final Shape last update | DepotDownloader | 3593201409625956155 | 6975584800172104419 |
| N/A     | Renegades (Latest)      | Steam           | N/A                 | N/A                  |

If you just want to look at the latest release, you only need Destiny 2 downloaded on Steam.

Otherwise, you can download the DepotDownloader versions by
- Downloading [DepotDownloader](https://github.com/SteamRE/DepotDownloader/releases)
- Running it with the following arguments:
```
dotnet DepotDownloader.dll -app 1085660 -depot 1085661 -manifest {main_manifest_id} -username <username> -password <password> -dir <path> -validate
dotnet DepotDownloader.dll -app 1085660 -depot 1085662 -manifest {language_manifest_id} -username <username> -password <password> -dir <path> -validate

e.g.
dotnet DepotDownloader.dll -app 1085660 -depot 1085661 -manifest 4160053308690659072 -username myusername -password mypassword -dir "D:/DestinyCharmStore/v2601/" -validate
dotnet DepotDownloader.dll -app 1085660 -depot 1085662 -manifest 4651412338057797072 -username myusername -password mypassword -dir "D:/DestinyCharmStore/v2601/" -validate
```

After you've downloaded the version(s) you want:

- You'll need [.NET 8.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer) and [VC++ Redistributables](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version) installed.
- Download the [latest release](https://github.com/MontagueM/Charm/releases/latest) and run Charm.exe.
- Set the packages and export paths first.

If you encounter any problems, take a look in the `Logs/` folder, find the latest log file, and look at the exception.
Feel free to raise an issue on this repository on in DMR `#charm-tool-help` if you need help.

Also take a look at the [Charm wiki](https://github.com/MontagueM/DestinyDocs/blob/main/Charm/Home.md) for more info.

> [!TIP]
> ## Some tips and tricks
> * Middle click tabs to close them.
> * In a packages view, you can type in any hash and it will take you to it. No need to look through all the packages.
> * If you already have the hash of an Entity (Dynamic), you can press CTRL+D to enter 'Dev' view. Paste the hash into the box and press enter. It will open in a viewer and be exported

## Reporting issues

If you experience any issue, you can register an issue in this repository. If the program has crashed, it is extremely valuable to provide the charm.log file.

## Known issues
- The Animated Background may cause startup crashes for some people, set "AnimatedBackground" to false in your config.json file
- Textures will not export if the export path contains a period
- UI elements may not scale correctly for any resolution other than 1080p
- API crashes for some people (extremely rare?) (Related to DirectXTex)


## Screenshots
<p float="left">
    <img width="400" src="https://github.com/user-attachments/assets/3487d37f-5750-4715-9d5e-affbb53a87c0" />
    <img width="400" src="https://github.com/user-attachments/assets/31538576-8d63-4cc4-8f89-6ae50d0266a0" />
    <img width="400" src="https://github.com/user-attachments/assets/abfea0e5-71ad-4bec-813b-0b9095a25f25" />
    <img width="400" src="https://github.com/user-attachments/assets/0a48f0ea-0474-41e8-b465-9bf51d764705" />
    <img width="400" src="https://github.com/user-attachments/assets/9634f527-7062-4617-929d-266533eb7288" />
    <img width="400" src="https://github.com/user-attachments/assets/082ccc15-4684-4954-b3f1-b2b8cc4b1ee2" />
    <img width="400" src="https://github.com/user-attachments/assets/14705aee-c857-4cb4-b21a-add89b86c9b2" />
    <img width="400" src="https://github.com/user-attachments/assets/794bbffb-6c14-4b44-a63a-6f74a15092b0" />
</p>

## Blender
- Use the [Blender Importer addon](https://github.com/DeltaDesigns/d2-map-importer-addon) to simplify and automate importing maps and models into Blender.

## S&Box
 - [Import guide](https://github.com/DeltaDesigns/Charm/wiki/S&Box-Importing)
 - Generates .shader files for accurate game shaders
 - Generates .vmat (material) and .vmdl (model) files

## Unreal Engine
- Unreal Engine importing isn't supported at this current moment. All the discoveries with maps (skyboxes, lights, etc) and rendering and what not have made things a little complicated and I (Delta) personally have little experience in scripting for UE

## Sponsor

I put a sponsor on this project as some people wanted to contribute, if you want to you can help me out :)

## License

The Charm source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses.

## Credits

Thanks to Alcidine, BIOS, Carson Reed, Delta, and nblock for testing, feedback, and help throughout the project's development. HighRTT for audio help (RevorbStd and librevorb).
