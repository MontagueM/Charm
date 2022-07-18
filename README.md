# Charm - the Destiny tool that does everything

## What is this?

A new, flashier, fully C# version of my previous tool Phonon. It is designed to only support the latest versions of the game. The tool focuses on providing as much access to the information in the game files as possible, ideal for artists and content preservation.

## How do I install and use it?

- You'll need [.NET 6.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.7-windows-x64-installer) installed.
- Download the latest release and run Charm.exe.
- Set the packages and export paths first.

If you encounter any problems, take a look in the charm.log file and look at the exception.

### Known issues

* `Unhandled Exception: System.ArgumentException: Invalid hash given: 2157969408.` - delete the empty paths.cache file and try again.

## Functionality

Here's a brief list of what Charm can currently do:

- Exract entities from the game API.
- Interoperability with DARE for extraction of higher quality textures than the API provides.
- Extraction of static models, maps, entity models, textures, audio.
- For entities, FK skeletons are provided where they exist.
- All geometry can export with shaders that can be imported into Unreal Engine 5 (see [tutorial](https://github.com/MontagueM/Charm/wiki/Charm-----UE5-Interoperability)).
- Viewing and extraction of information tied to activities.
- Batch extraction sorted into packages.
- Arrow keys for navigating the list of buttons easily.
- Search bar that can search hashes and names, and when in a package view, can search recursively for names.
- Entity names where possible.
- Music, dialogue, directives viewing and playback.

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

The Charm source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses. Symmetry is fully copywritten.

## Credits

Thanks to Alcidine, BIOS, Carson Reed, Delta, and nblock for testing, feedback, and help throughout the project's development. HighRTT for audio help (RevorbStd and librevorb).