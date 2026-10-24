Survival in Terraria reaches a new level of realism!

This mod transforms your journey into a constant struggle against your own needs. Beyond fighting monsters, you must now battle Hunger and Sleep, which directly affect your performance in combat and exploration.

Face the extremes of nature with a Temperature system that can leave you freezing or overheated. And keep your Sanity intact, as stress and the fear of the unknown can have unpredictable consequences on your mind.

Prepare for an adventure where your greatest battle is against your own body and the dangers of nature.

Link to the mod in the steam workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3555532206

## Development

The mod must live in tModLoader's `ModSources` folder to build (the project imports `..\tModLoader.targets`).

- `Common/Logic/` holds the game rules (hunger, rest, sanity, temperature, meter sprites...) as plain C# without any Terraria reference, so they can be unit tested.
- `Content/` holds the tModLoader side: the players collect the game state, call the logic and apply the buffs. Needs are simulated only on the owner's client (`Player.whoAmI == Main.myPlayer`); vanilla syncs the resulting buffs to the other players.
- `Localization/` holds every player-facing text, in English (`en-US`) and Brazilian Portuguese (`pt-BR`). Both files must have the same keys.

Run the unit tests (no game needed):

```
dotnet test Tests/ChallengingTerrariaMod.Tests
```

Check that the mod compiles without packaging it into the Mods folder:

```
dotnet msbuild ChallengingTerrariaMod.csproj -restore -t:Compile
```
