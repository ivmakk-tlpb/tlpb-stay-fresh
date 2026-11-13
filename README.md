![version](https://img.shields.io/badge/version-1.0.0-blue) ![coverage](https://img.shields.io/badge/coverage-98%25-brightgreen)

# Stay Fresh

Stay Fresh is a mod for the Steam game *The Last Plague: Blight*. In the game, food you cook still carries Blight contamination from its ingredients. So your Blight keeps rising even when you eat only cooked food, and you end up drinking Spotted Red Mushroom Soup again and again to bring it down, at a cost to your health. Food also spoils fast. With this mod, cooking and smoking make clean food, and food, hides, and carcasses last longer. The blight zones and the other Blight risks stay as in the game.

## Features

- **Clean cooking.** Each food item that cooking makes starts with no contamination, also when the raw meat or the water was contaminated. Boiled water, teas, and soups in the cooking vessel start clean too. Food can pick up contamination again later, for example next to Blighted Fungus in your backpack.
- **Clean smoking.** Meat smoked on a Smoke Rack, such as Piece of Jerky, Smoked Bass Meat, and Smoked Trout Meat, starts with no contamination. Dried bones and dried flowers keep the contamination the game gives them.
- **Slower spoilage.** Items and liquids spoil 5 times more slowly than in the game (`SpoilageSlowdown` 5), in your backpack, in a storage, in a container, and on the ground. Set `0` and they do not spoil at all.
- **Slower animal products and carcasses.** Raw meat, hides, fat, and other animal products spoil 5 times more slowly (`AnimalSpoilageSlowdown` 5), also while they are still in a carcass. A carcass stays in the world 5 times as long before it disappears. Set `0` and animal loot does not spoil, and a carcass stays until you harvest it. Cooked, smoked, and cured meat count as food and use `SpoilageSlowdown`. Dead plants decay as in the game.
- **Stronger covered containers.** A Covered Clay Jar or a Covered Glass Bottle slows the spoilage of its items twice as much (`PreservationMultiplier`), also in your backpack, where the game gives a covered jar no effect. Berry juice in a covered container turns into vinegar twice as fast.
- **Faster salt preservation.** Salt in a container preserves the food twice as fast. The limit stays as in the game: salt slows spoilage by at most 90%.
- **Spoils in.** The tooltip of each item that spoils shows the time left at its current rate, such as "Spoils in 1 day 6 hours". The time counts the place of the item, the cold, the covered container, the salt, and the settings.
- **Fair play.** The settings add to the world option "Item Decay Rate Factor" and to the cold weather. Raw food, the Cured Sausage, the Blighted Fungus and the Slime that give off Blight, the disinfectants, the blight zones, and the Blight disease from any other source stay as in the game. Each part has its own setting, and `1` gives the game value. A spoilage setting of `0` stops only the loss of condition: salt preservation, smoking, and fermentation still work.

## Spoil times

| Item | Game | 5x | 10x |
|---|---|---|---|
| Raw Deer Meat | 8 hours | 1.6 days | 3.2 days |
| Cooked Wolf Meat | 19 hours | 3.9 days | 7.7 days |
| Piece of Jerky | 7.7 days | 39 days | 77 days |
| Mushrooms | 35 hours | 7.2 days | 14.5 days |
| Deer Hide | 40 hours | 8.3 days | 16.5 days |
| Carcass in the world | 28 hours | 5.8 days | 11.6 days |

Below 5 °C, the cold slows spoilage further, in the game and with the mod. The mod uses 5x by default, and you can set the slowdown for food and for animal products separately, from 1 (as in the game) to 20, or 0 for no spoilage (see [CONFIG.md](CONFIG.md)).

## Save safety

The mod writes nothing to the save. You can remove the mod at any time, and your save loads as before. The game then uses its own rates again. Food that the mod cooked clean stays clean.

## Multiplayer

Made and tested for single player. Co-op is not tested.

## Requirements

- The Last Plague: Blight 0.12.7 or later.
- The [BepInEx Pack for The Last Plague: Blight](https://www.nexusmods.com/thelastplagueblight/mods/9), the BepInEx 6 (IL2CPP) build for the game.

## Install

Download the zip from [GitHub Releases](https://github.com/ivmakk-tlpb/tlpb-stay-fresh/releases).

1. Install the [BepInEx Pack for The Last Plague: Blight](https://www.nexusmods.com/thelastplagueblight/mods/9). If no other mods were installed before, start the game once so BepInEx finishes setup, then quit.
2. Extract this mod's zip into the game folder (the folder with the game .exe). The DLL lands in `BepInEx\plugins`. Full path example:
   - Steam: `C:\Program Files (x86)\Steam\steamapps\common\The Last Plague\BepInEx\plugins\StayFresh.dll`

## Uninstall

Delete `StayFresh.dll` from the `BepInEx\plugins` folder. On the next game start, cooking and spoilage work as in the game.

## Configuration

To change mod settings, edit `BepInEx\config\com.ivmakk.thelastplagueblight.stayfresh.cfg` while the game is closed (see [CONFIG.md](CONFIG.md) for every setting). Changes apply at the next game start.

## Troubleshooting

If a game update renames a part of the game that the mod needs, only the feature that uses it turns off, with one warning in the log. The other features keep working.

Check `BepInEx\LogOutput.log` for the `Stay Fresh loaded.` line, and for warnings or errors from Stay Fresh.

## Build

Stay Fresh is a BepInEx 6 IL2CPP plugin. Building requires the .NET 8 SDK and a game installation with BepInEx. Start the game once after installing BepInEx to generate the IL2CPP interop assemblies.

```
dotnet build src/StayFresh.csproj -c Release
```

`Directory.Build.props` sets `GameDir` to the default Steam installation path. For another location, set the `GameDir` environment variable or pass `-p:GameDir=...` to the build command. The output DLL is `src\bin\Release\StayFresh.dll`.

Unit tests cover all files of `src/` and run with no game and no BepInEx, because the test project compiles the patch code against fake game types.

```
dotnet test tests/StayFresh.Tests
```

## Package

```
dotnet build src/StayFresh.csproj -c Release -p:Package=true
```

The zip is `dist/StayFresh-<version>.zip`, laid out as `BepInEx/plugins/StayFresh.dll`.

## License

Licensed under the GNU General Public License v3.0. Copyright (C) 2026 ivmakk. See [LICENSE](LICENSE).

You may reuse and modify this mod, but you must keep it open under the same license and give credit. Do not reupload it without credit.
