using System.Collections.Generic;

namespace StayFresh
{
    // The covered container part of the spoilage factor of a stack, from the content factor of its holder and of the
    // item that holds the stack. Only the first case that matches counts, so a container in a container counts once:
    // - a holder factor below 1 (a placed covered container): the game applied it, so the mod adds 1 / PreservationMultiplier;
    // - a parent item with a content factor f below 1 (a covered container in the backpack or a storage): the game
    //   ignores it there, so the mod adds f / PreservationMultiplier;
    // - else 1.
    public sealed class ContainerRule
    {
        // One item definition: its identifier, the identifier it copies its fields from, and its own content factor.
        public readonly struct ItemRow
        {
            public readonly string Identifier;
            public readonly string CopyFrom;
            public readonly float? Factor;

            public ItemRow(string identifier, string copyFrom, float? factor)
            {
                Identifier = identifier;
                CopyFrom = copyFrom;
                Factor = factor;
            }
        }

        private readonly IReadOnlyDictionary<string, float> contentFactors;
        private readonly float preservation;

        public ContainerRule(IReadOnlyDictionary<string, float> contentFactors, float preservation)
        {
            this.contentFactors = contentFactors;
            this.preservation = preservation;
        }

        public float Factor(float holderFactor, string parentItem) => Factor(holderFactor, ContentFactor(parentItem));

        // The parent factor is the content factor of the item that holds the stack, 1 for none.
        public float Factor(float holderFactor, float parentFactor)
        {
            if (holderFactor < 1f) return 1f / preservation;
            return parentFactor < 1f ? parentFactor / preservation : 1f;
        }

        public float ContentFactor(string item) =>
            item != null && contentFactors.TryGetValue(item, out var factor) ? factor : 1f;

        // The content factor of each item that has one, also through the chain of copyFromIdentifier.
        public static Dictionary<string, float> ContentFactors(IEnumerable<ItemRow> rows)
        {
            var byId = new Dictionary<string, ItemRow>();
            foreach (var row in rows)
                if (row.Identifier != null) byId[row.Identifier] = row;
            var result = new Dictionary<string, float>();
            foreach (var row in byId.Values)
            {
                var current = row;
                var seen = new HashSet<string>();
                while (!current.Factor.HasValue && current.CopyFrom != null && seen.Add(current.Identifier) && byId.TryGetValue(current.CopyFrom, out var next))
                    current = next;
                if (current.Factor.HasValue) result[row.Identifier] = current.Factor.Value;
            }
            return result;
        }
    }
}
