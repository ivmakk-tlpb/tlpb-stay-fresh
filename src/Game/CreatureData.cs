using System.Collections.Generic;

namespace StayFresh.Game
{
    // Reads the animal products from the game data: the creature definitions of the creature manager and the item
    // identifiers of the inventory manager.
    internal static class CreatureData
    {
        // Null before the game has loaded its definitions.
        internal static AnimalProducts Load()
        {
            var manager = UnityEngine.Object.FindObjectOfType<GameLoop>()?.gameManager?.TryCast<GameManager>();
            var creatureMap = manager?.creatureManager?.TryCast<CreatureManager>()?.creatureMap;
            var itemMap = manager?.inventoryManager?.TryCast<InventoryManager>()?.itemMap;
            if (creatureMap == null || itemMap == null) return null;

            var creatures = new List<AnimalProducts.CreatureDrops>();
            var each = creatureMap.GetEnumerator();
            while (each.MoveNext())
            {
                var creature = each.Current.Value;
                if (creature != null) creatures.Add(new AnimalProducts.CreatureDrops(DropsOf(creature), creature.DeadPickupItemIdentifier));
            }
            var items = new List<string>();
            var keys = itemMap.GetEnumerator();
            while (keys.MoveNext()) items.Add(keys.Current.Key);
            return items.Count == 0 ? null : AnimalProducts.From(creatures, items);
        }

        private static List<string> DropsOf(Creature creature)
        {
            var result = new List<string>();
            var drops = creature.HarvestDrops;
            for (int i = 0; drops != null && i < drops.Count; i++)
                if (drops[i] != null) result.Add(drops[i].itemIdentifier);
            return result;
        }
    }
}
