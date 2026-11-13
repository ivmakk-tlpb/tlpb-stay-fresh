

// A loaded game with creatures and items: a deer with its harvest drops, a rabbit with its dead pickup, and the items.
internal static class GameWorld
{
    public static GameManager Load(params string[] items)
    {
        var creatures = new CreatureManager();
        var drops = new Il2CppSystem.Collections.Generic.List<ItemDrop>();
        foreach (var drop in new[] { "deer_meat_raw", "deer_hide", "animal_fat", "bone" }) drops.Add(new ItemDrop { itemIdentifier = drop });
        creatures.creatureMap.Add("deer_doe", new Creature { HarvestDrops = drops });
        creatures.creatureMap.Add("rabbit", new Creature { DeadPickupItemIdentifier = "rabbit_item_dead" });
        var inventory = new InventoryManager();
        foreach (var item in items) inventory.itemMap.Add(item, new InventoryItem(item));
        var definitions = new ModelDefinitionManager();
        definitions.Items.Add("clay_jar_covered", Definition(("identifier", "clay_jar_covered"), ("contentsDecayFactor", 0.3f)));
        definitions.Items.Add("glass_bottle_covered", Definition(("identifier", "glass_bottle_covered"), ("copyFromIdentifier", "clay_jar_covered")));
        foreach (var item in items) definitions.Items.Add(item, Definition(("identifier", item)));
        var manager = new GameManager { creatureManager = creatures, inventoryManager = inventory, modelDefinitionManager = definitions };
        UnityEngine.Object.Scene.Add(new GameLoop { gameManager = manager });
        return manager;
    }

    public static ModelDefinition Definition(params (string Name, object Value)[] fields) =>
        new ModelDefinition(fields.ToDictionary(f => f.Name, f => f.Value));

    public static InventoryHolder Holder(string entity, float contentsDecayFactor = 1f)
    {
        var holder = new InventoryHolder { entity = new Entity { identifier = entity } };
        holder.contentsDecayFactor.Value = contentsDecayFactor;
        return holder;
    }

    // The inner inventory of a container stack.
    public static Inventory Inside(string container) =>
        new Inventory { parentStack = new InventoryItemStack(new InventoryItem(container)) };
}
