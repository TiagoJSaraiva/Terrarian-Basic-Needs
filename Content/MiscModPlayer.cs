using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ChallengingTerrariaMod.Content.Consumables;

namespace ChallengingTerrariaMod.Content
{
    public class MeuModPlayer : ModPlayer
{
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> items = new List<Item>();

        Item apple = new Item();
        apple.SetDefaults(ItemID.Apple);
        apple.stack = 5;
        items.Add(apple);

        return items; 
    }
}
}