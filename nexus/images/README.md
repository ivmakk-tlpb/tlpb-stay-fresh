# Images

- `content.json` - the text, the icons, and the crops of the thumbnail and the page background. The `nexus-mod-images` tool renders both images from it.
- `01-thumb.jpg` - 1920x1080, the first gallery image, so Nexus uses it as the thumbnail in the mods grid. The title ("Stay" in white, "Fresh" in the accent), two captions with the game's germs icon (`disease`, filled in the Blight green `#8fa31a`, with a red slash that the tool draws) and the Raw Wolf Meat texture (`Texture2D/wolf_meat_raw`, 256 px, cut to its content), and the Fire Pit window of `src/cooked-on.jpg` at 1:1 with the tooltip of the Cooked Wolf Meat.
- `02-cooked-without-mod.jpg` - 1920x1080, gallery. Without the mod: Cooked Wolf Meat from raw meat with 20% contamination shows "10% contaminated" and no spoil time. From `src/cooked-off.jpg`, scaled down.
- `03-cooked-with-mod.jpg` - 1920x1080, gallery. With the mod: the same Cooked Wolf Meat shows no contamination and "Spoils in 3 days 20 hours". From `src/cooked-on.jpg`, scaled down.
- `04-carcass-meat.jpg` - 1920x1080, gallery. With the mod at its default 5x: the Raw Deer Meat in the Doe carcass window shows "Spoils in 1 day 14 hours" (8 hours in the game). From `src/carcass-meat.jpg`, scaled down.
- `05-carcass-hide.jpg` - 1920x1080, gallery. With the mod at its default 5x: the Deer Hide in the same carcass shows "Spoils in 1 week 1 day" (40 hours in the game). From `src/carcass-hide.jpg`, scaled down.
- `banner.jpg` - 1300x372, the page background. The lake shore, the pines, and the green Blight fog of `src/camp.jpg`, darker at the left and the bottom, so the mod name reads.
- `src/` - the screenshots, taken on 2026-11-13 at 3840x2160 on game version 0.12.7 in game driver sessions on a test save (the carcass shots in session `20261113-180122-981-session`), and stored as JPG at quality 95.

The gallery crops are `[1000, 280, 2560, 1440]` of the source screenshots, scaled to 1920x1080.

The description has no inline image. If one is added, upload the file to the Nexus images of the mod first and use the hosted URL.
