# CastleMiner Z PC VR

An unofficial PC VR mod for the Steam version of CastleMiner Z, by ItssnotTristan. Play with tracked head movement, motion controllers, two-handed weapons and VR menus. PC graphics include bloom, dynamic shadows, adjustable block parallax and four quality presets.

**[Download the PC VR beta](https://github.com/itssnotTristan/CastleMinerZ-PCVR/releases/tag/v0.56-public-beta)** · **[Full setup guide](SETUP.md)**

## Install

1. Install CastleMiner Z through Steam and launch the original game once. This beta supports game version **1.9.9.8**.
2. In Steam, right-click CastleMiner Z → **Manage → Browse local files**.
3. Download `CastleMinerZ-PCVR-v0.56-PublicBeta.zip` from Releases. Extract its **entire contents** into that game folder, beside `CastleMinerZ.exe`.
4. Run **Setup CastleMiner Z VR.cmd** and leave its window open until it says setup is complete. It converts the required files from your own installation. The first setup downloads the audio converter from its official release.
5. Enter **Quest Link or Air Link**, wake both controllers, and make Meta Quest Link your active OpenXR runtime.
6. Run **Play CastleMiner Z VR.cmd**. Point a controller at **Play Offline** and squeeze its trigger.

The ZIP contains the mod and setup tools. Original game textures, models, music and sounds are imported locally; they are not included in the download. The original game executable stays unchanged. Setup requires Windows 10/11 64-bit, the game's XNA 4 prerequisites and an OpenGL 4.5-capable graphics driver. Allow about 2 GB free for conversion and imported files, plus another 1.5 GB for the optional local HD pack.

## Controls

| Control | Action |
| --- | --- |
| Left stick | Walk |
| Left stick click | Sprint |
| Right stick | Turn / hotbar selection, depending on bindings |
| Right stick click | Switch hotbar row |
| Right A | Jump; press again in the air to double jump using stamina |
| Right trigger | Fire / use selected item |
| Right B | Reload, or eject a magazine in Manual mode |
| Grips | Grab items, hold a magazine or support a weapon |
| Left X | Interact with doors, storage and special blocks |
| Left Y | Inventory; hold for crafting |
| Left menu button | Pause / options |

Options includes seated/standing modes, turn speed, controller bindings, automatic/manual reload and hand calibration. Saved custom bindings can change these defaults.

## Graphics

Choose Performance, Balanced, Quality or Ultra in Options. Bloom and parallax can be adjusted independently. Hardware ray tracing is not included.

To create an optional HD pack from your local imports, run **Generate HD Textures.cmd** after setup. It applies filtered 2× upscaling on your PC and preserves original files. Switch it using **HD textures On/Off**. An existing HD pack is kept. Enhanced texture packs are not bundled in this public download.

## Multiplayer and updates

This PC beta uses **shared-v0.56**, network protocol **3**, guest checkpoint version **2**, and content fingerprint **15070507204261208425**. Quest peers need the matching v0.56 build and their own imported game files. Earlier Quest public updates, including v0.53, are not the matching release. Unmodified desktop Steam servers are not supported.

Use Nearby for the same network, or Host Internet / Join Internet for the shared internet world list. Internet connections depend on the directory service and a working peer connection. Different-home PC/Quest headset testing remains pending.

For this first PC public beta, download updates from this repository's Releases page. The existing Quest update feed has not been changed by this release. Future paired updates will identify the supported PC and Quest versions. Keep imported files and saves when updating.

## Status and support

This is a playable beta, with original-game feature parity still in progress. The public installer has been tested against the supported Steam installation; rendering and local host/guest checks are automated. This new public installation path still needs wearer testing. See [SETUP.md](SETUP.md) for logs, troubleshooting and uninstall steps.

This repository contains player downloads and local setup tools. Historical private development/test archives are kept separately. Please include your mod version, headset and `pcvr.log` when reporting a problem; remove personal information before sharing logs.

Dependencies include OpenXR, GLFW, GLEW, zlib, libdatachannel, mbedTLS, miniaudio and vgmstream. Their notices accompany the distribution. CastleMiner Z and its original content belong to their respective owners. This project is unofficial and is not endorsed by the game's developers.
