# Configuration

Every setting lives in `BepInEx\config\com.ivmakk.thelastplagueblight.stayfresh.cfg`, written the first time you run the game with the mod installed. Edit it with any text editor while the game is closed. Changes apply at the next game start. A value outside its range is set to the nearest end of the range. A value that is already in the file stays after a mod update, also when the new version has another default: to get the new default, delete the line and start the game.

The settings add to the world option "Item Decay Rate Factor", the cold weather, and salt preservation of the game. A spoilage slowdown of `0` stops only the loss of condition: salt preservation, smoking, and fermentation still work. Above `0`, a slowdown goes from `1` (as in the game) to `20`, and a value between `0` and `1` counts as `1`, because the mod never makes spoilage faster. The highest value is `20` because a higher slowdown makes the step of each frame so small that the game loses it, so the item would stop spoiling. At a high slowdown, a Covered Clay Jar in the cold can still slow a hide or fat so much that it stops spoiling.

## General

| Setting | Default | Values | What it does |
|---|---|---|---|
| `CookingRemovesContamination` | `true` | `true` / `false` | Each food item that cooking or smoking makes starts with no contamination, also boiled water, teas, and soups in the cooking vessel. Dried bones and dried flowers keep the game value. With `false`, cooking and smoking give the contamination of the game. |
| `SpoilageSlowdown` | `5` | `0`, or `1` to `20` | How many times more slowly each item and liquid spoils, other than an animal product. `5` spoils 5 times more slowly, and `2` at half the speed. With `0`, these items do not spoil. Cooked, smoked, and cured meat use this value. With `1`, spoilage is as in the game. |
| `AnimalSpoilageSlowdown` | `5` | `0`, or `1` to `20` | How many times more slowly raw meat, hides, furs, fat, and other animal products spoil, also while they are still in a carcass, and how many times longer a carcass stays in the world. `5` makes both 5 times slower. With `0`, animal products do not spoil, and a carcass stays in the world until you harvest it, so each carcass you leave stays in your save. With `1`, both are as in the game. |
| `PreservationMultiplier` | `2.0` | `1.0` to `10.0` | A Covered Clay Jar or a Covered Glass Bottle slows the spoilage of its items this many times more, also in your backpack. Salt preservation builds this many times faster, and juice in a covered container ferments this many times faster. With `1.0`, all three are as in the game. |
| `Verbose` | `false` | `true` / `false` | Writes extra diagnostic lines to the BepInEx log for troubleshooting. Keep off in normal play. |
