# <img width="24" height="24" src="https://github.com/user-attachments/assets/bf2bab5c-2a96-4517-9a2a-59d061046830" /> Charm - the Destiny tool that does (almost) everything

## What is this?
Charm is the successor to [Phonon](https://github.com/MontagueM/Phonon) and is designed for ***3D artists, content creators, content preservation, and nerds who like the inner workings of the Tiger Engine***.
It supports many versions of Destiny 2 and it's main focus is on the games 3D models.

> [!CAUTION]
> # Disclaimer
> * Before you go any further, understand that Charm ***IS NOT a datamining tool!!***
> * While it can access many things in the game files, it's main purpose is focused towards **3D artists, content preservation and learning how the game works**!
> * Please ***DO NOT*** use this tool to spread leaks and spoilers or anything that may break Bungie's TOS. Don't ruin the experience for yourself and others. Uncover things the way they were intended!
> * Seeing this tool used for such acts WILL result in fewer public updates and the removal of certain features (this unfortunately has already happened).
> * I enjoy maintaining and updating this for others, don't be the one to ruin it. (Looking at you Deaks. You will be the reason Charm stops being supported.)

## How do I install and use it?

You'll first need at least one game installation.
Charm currently supports:

| Version | Description              | Where           |  Main manifest id   | Language manifest id |
|---------|--------------------------|-----------------|---------------------|----------------------|
| D1 2.6.0.2 | Rise Of Iron last update | Ask in [DMR](https://discord.gg/DestinyModelRips)      |                     |                      |
| D2 2.6.0.1 | Shadowkeep first update  | DepotDownloader | 7002268313830901797 | 2399965969279284756  |
| 2.9.9.9 | Shadowkeep last update   | DepotDownloader | 4160053308690659072 | 4651412338057797072  |
| 3.4.0.2 | Beyond Light last update | DepotDownloader | 5631185797932644936 | 3832609057880895101  |
| 6.3.0.7 | Witch Queen last update  | DepotDownloader | 6051526863119423207 | 1078048403901153652  |
| 7.3.6.6 | Lightfall last update    | DepotDownloader | 7707143404100984016 | 5226038440689554798  |
| 8.2.6.4 | The Final Shape last update | DepotDownloader | 3593201409625956155 | 6975584800172104419 |
| 9.5.0.1+     | Renegades (Latest)      | Steam           | N/A                 | N/A                  |

If you just want to look at the latest release, you only need Destiny 2 downloaded on Steam, Epic Games, or the Windows Store

Otherwise, you can download older versions using DepotDownloader.
- Download [DepotDownloader](https://github.com/SteamRE/DepotDownloader/releases)
- Run it with the following arguments:
```
DepotDownloader.exe -app 1085660 -depot 1085661 -manifest {main_manifest_id} -username <username> -password <password> -dir <path> -validate
DepotDownloader.exe -app 1085660 -depot 1085662 -manifest {language_manifest_id} -username <username> -password <password> -dir <path> -validate

e.g.
DepotDownloader.exe -app 1085660 -depot 1085661 -manifest 4160053308690659072 -username myusername -password mypassword -dir "D:/DestinyCharmStore/v2601/" -validate
DepotDownloader.exe -app 1085660 -depot 1085662 -manifest 4651412338057797072 -username myusername -password mypassword -dir "D:/DestinyCharmStore/v2601/" -validate
```

After you've downloaded the version(s) you want:

- You'll need [.NET 8.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer) and [VC++ Redistributables](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version) installed.
- Download the [latest release](https://github.com/MontagueM/Charm/releases/latest), extract the entire archive, and run Charm.exe.
- Select the specific game version you wish to use Charm with. 
- You will be prompted to set the games packages path and Charm's export path before you can continue.
    - For example: `C:\Program Files\Steam\steamapps\common\Destiny 2\packages` for the game, `G:\Charm Output` for the output

~~Also take a look at the [Charm wiki](https://github.com/MontagueM/DestinyDocs/blob/main/Charm/Home.md) for more info.~~ (Outdated)

> [!TIP]
> ## Some tips and tricks
> * Middle click tabs to close them.
> * In a packages view, you can type in any hash and it will take you to it. No need to look through all the packages.
> * If you already have the hash of an Entity (Dynamic), you can press CTRL+D while on the Main Menu to enter 'Dev' view. Paste the hash into the box and press enter. It will open in a viewer and be exported.

## Reporting issues
If you experience any issues, bugs, or crashes, feel free to create an issue in this repository or in the Destiny Model Rips [Discord](https://discord.gg/DestinyModelRips) `#charm-tool-help` channel.
It would help greatly if you provide the latest crash log (`/Logs` folder) and steps to reproduce the issue.

## Known issues
- The Animated Background may cause startup crashes for some people, set "AnimatedBackground" to false in your config.json file if this the case.
- Textures will not export if the export path contains a period or a special character.
- UI elements may not scale correctly for any resolution other than 1080p.
- Package Path Cache creation may get stuck in rare instances, simply restart the program.
- Steam updates can sometimes fail to remove old package files which can/will cause crashes.
    - A complete uninstall/reinstall of the game is the easiest solution.

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

## Unreal Engine
- Unreal Engine importing is no longer supported at this current moment. All the discoveries with maps (skyboxes, lights, etc) and rendering and what not have made things a little complicated. Anyone is more than welcome to contribute on this front.

## Sponsor

I put a sponsor on this project as some people wanted to contribute, if you want to you can help me out :)

## License

The Charm source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses.

## Credits

- Thanks to Alcidine, BIOS, Carson Reed, Delta, and nblock for testing, feedback, and help throughout the project's development. HighRTT for audio help (RevorbStd and librevorb).
- I (Delta) want to thank Mont for initially creating this program and allowing me to continue to update/support Charm while also giving me a passion in reverse engineering the Tiger Engine.
