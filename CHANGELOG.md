# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-11-13

### Added

- Each food item that cooking makes starts with no contamination (`CookingRemovesContamination`). Boiled water, teas, and soups in the cooking vessel start clean too.
- Meat smoked on a Smoke Rack, such as Piece of Jerky, starts with no contamination. Dried bones and dried flowers keep the game value.
- Items and liquids spoil 5 times more slowly than in the game (`SpoilageSlowdown` 5). With `0`, they do not spoil.
- Raw meat, hides, fat, and other animal products spoil 5 times more slowly, also inside a carcass, and a carcass stays in the world 5 times as long (`AnimalSpoilageSlowdown` 5). With `0`, they do not spoil, and a carcass stays until you harvest it.
- A Covered Clay Jar or a Covered Glass Bottle slows spoilage twice as much, also in the backpack. Salt preservation builds and berry juice ferments twice as fast (`PreservationMultiplier`).
- The tooltip of an item that spoils shows the time left, such as "Spoils in 1 day 6 hours".
- The settings add to the world option "Item Decay Rate Factor" and to the cold. Raw food, the Blight sources, and the blight zones stay as in the game.
- The mod writes nothing to the save. Made and tested for single player.
