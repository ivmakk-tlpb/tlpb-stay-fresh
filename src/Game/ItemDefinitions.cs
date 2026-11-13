using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StayFresh.Game
{
    // Reads the content factor (contentsDecayFactor) of each item from the item definitions that the game loads. The
    // InventoryItem model has no such member, so the read goes to the definition, which stores each field by the hash of
    // its name (FieldHash), checked against the game's own hash of "identifier".
    internal static class ItemDefinitions
    {
        private const string FactorField = "contentsDecayFactor";
        private const string CopyField = "copyFromIdentifier";
        private const string IdentifierField = "identifier";

        // Null before the game has loaded its definitions.
        internal static Dictionary<string, float> LoadContentFactors()
        {
            var manager = UnityEngine.Object.FindObjectOfType<GameLoop>()?.gameManager?.TryCast<GameManager>()?.modelDefinitionManager?.TryCast<ModelDefinitionManager>();
            var definitions = manager?.GetDefinitionsByType(ModelDefinitionType.INVENTORY_ITEM);
            if (definitions == null) return null;

            int factorKey = FieldHash.Of(FactorField), copyKey = FieldHash.Of(CopyField), identifierKey = FieldHash.Of(IdentifierField);
            if (identifierKey != ModelDefinition.identifierFieldHash)
            {
                Plugin.Log.LogWarning("Stay Fresh cannot read the item definitions of this game version: covered containers spoil as in the game");
                return new Dictionary<string, float>();
            }
            var rows = new List<ContainerRule.ItemRow>();
            var each = definitions.GetEnumerator();
            while (each.MoveNext())
            {
                var definition = each.Current.Value;
                if (definition == null) continue;
                var identifier = Text(definition, identifierKey) ?? each.Current.Key;
                rows.Add(new ContainerRule.ItemRow(identifier, Text(definition, copyKey), Number(definition, identifier, factorKey)));
            }
            var factors = ContainerRule.ContentFactors(rows);
            if (factors.Count == 0)
                Plugin.Log.LogWarning("Stay Fresh found no content factor in the item definitions: covered containers spoil as in the game");
            else
                Plugin.Debug("Item definitions: content factors " + string.Join(", ",
                    factors.OrderBy(f => f.Key, System.StringComparer.Ordinal).Select(f => f.Key + " " + f.Value.ToString(CultureInfo.InvariantCulture))));
            return factors;
        }

        // A boxed IL2CPP value: ToString of the interop object does not reach the IL2CPP ToString, so the read goes by
        // the IL2CPP type of the value.
        private static string Text(ModelDefinition definition, int key)
        {
            if (!definition.TryGetValue(key, out var value) || value == null) return null;
            return value.GetIl2CppType().Name == "String" ? Il2CppInterop.Runtime.IL2CPP.Il2CppStringToManaged(value.Pointer) : null;
        }

        private static float? Number(ModelDefinition definition, string identifier, int key)
        {
            if (!definition.TryGetValue(key, out var value) || value == null) return null;
            var type = value.GetIl2CppType().Name;
            if (identifier == "clay_jar_covered") Plugin.Debug("Item definitions: the content factor of clay_jar_covered is a " + type);
            switch (type)
            {
                case "Single": return value.Unbox<float>();
                case "Double": return (float)value.Unbox<double>();
                case "Int32": return value.Unbox<int>();
                case "String":
                    var text = Il2CppInterop.Runtime.IL2CPP.Il2CppStringToManaged(value.Pointer)?.Replace(',', '.');
                    return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : (float?)null;
                default: return null;
            }
        }
    }
}
