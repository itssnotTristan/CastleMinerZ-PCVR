# Setup and troubleshooting

## Folder layout

Use the folder opened by Steam → CastleMiner Z → Manage → Browse local files. After extracting the mod, it should look like this:

```text
CastleMiner Z/
  CastleMinerZ.exe             ← original Steam game
  DNA.Common.dll
  DNA.Steam.dll
  Content/
  Setup CastleMiner Z VR.cmd   ← run once
  Play CastleMiner Z VR.cmd    ← launch VR
  Generate HD Textures.cmd    ← optional, after setup
  CastleMinerZVR/
    CastleMinerZVR.exe
    Setup-CastleMinerZVR.ps1
    importer/
    game/                     ← created by setup
```

Avoid an extra nested ZIP folder. `CastleMinerZVR` must be directly inside the original game folder. Starting CastleMiner Z through its normal Steam Play button still starts the desktop game; use the new VR launcher for this mod.

## First setup

Run the original game once, then close it. Run **Setup CastleMiner Z VR.cmd**. Setup verifies the supported original executable/libraries, downloads the pinned vgmstream r2117 audio converter, and imports the required assets. It does not upload your game files or change the original executable.

Keep the setup window open until it reports completion. A failed import does not replace an existing imported game folder. Re-running successful setup keeps a backup of the previous import. For advanced/offline setup, download [vgmstream r2117](https://github.com/vgmstream/vgmstream/releases/tag/r2117), extract the Windows 64-bit ZIP, and pass its `vgmstream-cli.exe` path using the setup script's `-Decoder` option.

Setup checks the original game files at version **1.9.9.8**. This is an offline installation requirement; there is no Steam account login in the mod. Players still need their own Steam copy.

## Connect your headset

Connect Quest Link by USB, or enter Air Link with your PC. In the Meta Quest Link PC app, set Meta Quest Link as the active OpenXR runtime. Wake both controllers, enter the Link environment, then run **Play CastleMiner Z VR.cmd**.

Use a controller's pointing ray and trigger to select menu buttons. Choose Play Offline first to check tracking and controls. Recenter and adjust seated/standing height in Options if necessary.

## Optional HD textures

After a successful setup, run **Generate HD Textures.cmd**. It creates `CastleMinerZVR/graphics-hd` from your local game files using filtered 2× upscaling. The original imported textures remain in `game`. Existing HD packs are preserved. In the game, use HD textures On/Off, Block parallax On/Off, Parallax strength and the graphics preset options.

## Common problems

| Problem | Check |
| --- | --- |
| Installation required / unsupported original file | Verify game files in Steam; confirm the folder layout above and supported version 1.9.9.8. |
| XNA 4 is missing | Launch the original Steam game once to install its prerequisites. |
| Setup cannot download the converter | Check internet access to GitHub, or supply a locally downloaded r2117 converter using `-Decoder`. |
| Imported file verification fails | Keep the setup log and report the listed filenames. The previous import remains in place. |
| No headset or controllers | Enter Link/Air Link, select the Meta OpenXR runtime and wake both controllers before launching. |
| Game closes during launch | Open `%LOCALAPPDATA%/CastleMinerZ-PCVR/pcvr.log`. Check imported files and graphics drivers. |
| HD textures appear unchanged | Generate the local pack first, then enable HD textures in Options. |
| Can't see a friend's world | Both peers need the matching version. Ordinary Steam desktop servers and older Quest versions are not compatible. Internet hosting also needs the directory service and a working peer route. |

Setup logs are in `%LOCALAPPDATA%/CastleMinerZ-PCVR-Setup`, in the folder for that setup attempt. The game log and settings/saves are under `%LOCALAPPDATA%/CastleMinerZ-PCVR`.

## Update or uninstall

Close the VR game before updating. Extract the next compatible PC release into the same game folder. Follow that release's notes if a new import is required. Keep `game`, `graphics-hd` and your saves unless instructed otherwise.

To uninstall, remove the added `CastleMinerZVR` folder and the three added launcher files. The original Steam game files remain. Saves and settings stay under `%LOCALAPPDATA%/CastleMinerZ-PCVR`; remove them separately only if you want to erase them.
