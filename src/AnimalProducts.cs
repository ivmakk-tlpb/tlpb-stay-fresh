using System;
using System.Collections.Generic;

namespace StayFresh
{
    // The item identifiers of the animal products: the parts of a carcass harvest, the dead small animals, and the
    // raw meat that the player makes (the identifiers with the suffix _raw). Cooked, smoked, and cured meat are food.
    public sealed class AnimalProducts
    {
        private const string RawSuffix = "_raw";

        // One creature of the game data: its harvest drops and its dead pickup item, each of them can be null.
        public readonly struct CreatureDrops
        {
            public readonly IEnumerable<string> HarvestDrops;
            public readonly string DeadPickup;

            public CreatureDrops(IEnumerable<string> harvestDrops, string deadPickup)
            {
                HarvestDrops = harvestDrops;
                DeadPickup = deadPickup;
            }
        }

        private readonly HashSet<string> items;

        private AnimalProducts(HashSet<string> items) { this.items = items; }

        public int Count => items.Count;

        public static AnimalProducts From(IEnumerable<CreatureDrops> creatures, IEnumerable<string> itemIdentifiers)
        {
            var items = new HashSet<string>();
            foreach (var creature in creatures)
            {
                if (creature.HarvestDrops != null)
                    foreach (var drop in creature.HarvestDrops)
                        if (!string.IsNullOrEmpty(drop)) items.Add(drop);
                if (!string.IsNullOrEmpty(creature.DeadPickup)) items.Add(creature.DeadPickup);
            }
            foreach (var item in itemIdentifiers)
                if (item != null && item.EndsWith(RawSuffix, StringComparison.Ordinal)) items.Add(item);
            return new AnimalProducts(items);
        }

        public bool Contains(string item) => item != null && items.Contains(item);
    }
}
