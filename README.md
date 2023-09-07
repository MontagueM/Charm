# Charm - the Destiny tool that does everything

## What is this?

A new, flashier, fully C# version of my previous tool Phonon.

It is designed to support many versions of the Tiger engine, including many game versions of Destiny 2.

The tool focuses on providing as much access to the information in the game files as possible, ideal for artists and content preservation.

## How do I install and use it?

You'll first need at least one game installation.
Charm currently supports:

| Version | Description              | Where           |  Main manifest id   | Language manifest id |
|---------|--------------------------|-----------------|---------------------|----------------------|
| 2.6.0.1 | Shadowkeep first update  | DepotDownloader | 7002268313830901797 | 2399965969279284756  |
| 2.9.9.9 | Shadowkeep last update   | DepotDownloader | 4160053308690659072 | 4651412338057797072  |
| 3.4.0.2 | Beyond Light last update | DepotDownloader | 5631185797932644936 | 3832609057880895101  |
| 6.3.0.7 | Witch Queen last update  | DepotDownloader | 6051526863119423207 | 1078048403901153652  |
| N/A     | Lightfall latest         | Steam           | N/A                 | N/A                  |

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

- You'll need [.NET 7.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.400-windows-x64-installer) installed.
- Download the [latest release]() and run Charm.exe.
- Set the packages and export paths first.

If you encounter any problems, take a look in the `Logs/` folder, find the latest log file, and look at the exception.
Feel free to raise an issue on this repository on in DMR `#charm-tool-help` if you need help.

Also take a look at the [Charm wiki](https://github.com/MontagueM/DestinyDocs/blob/main/Charm/Home.md) for more info.

## Source 2: **Only supports S&Box**
 - [Import guide](https://github.com/DeltaDesigns/Charm/wiki/Source-2-Importing)
 - Generates .shader files for semi-accurate game shaders (similar to UE5 shaders)
 - Generates .vmat (material) and .vmdl (model) files for statics and maps

### Some tricks

* Middle click tabs to close them.
* In a packages view, you can type in any hash and it will take you to it. No need to look through all the packages.

## Reporting issues

If you experience any issue, you can register an issue in this repository. If the program has crashed, it is extremely valuable to provide the charm.log file.

## Learning and Contributing

To learn about how Charm works or to contribute, check out the [wiki](https://github.com/MontagueM/Charm/wiki).

## Sponsor

I put a sponsor on this project as some people wanted to contribute, if you want to you can help me out :)

## License

The Charm source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses.

## Credits

Thanks to Alcidine, BIOS, Carson Reed, Delta, and nblock for testing, feedback, and help throughout the project's development. HighRTT for audio help (RevorbStd and librevorb).
