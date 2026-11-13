// Fake game types: only the types and members that the mod source reaches.
using Il2CppInterop.Runtime.InteropTypes;


public class IGameManager : Il2CppObjectBase { }

public class GameManager : IGameManager
{
    public ICreatureManager creatureManager;
    public IInventoryManager inventoryManager;
    public IModelDefinitionManager modelDefinitionManager;
}

public enum ModelDefinitionType
{
    INVENTORY_ITEM = 4,
}

public class IModelDefinitionManager : Il2CppObjectBase { }

public class ModelDefinitionManager : IModelDefinitionManager
{
    public readonly Il2CppSystem.Collections.Generic.Dictionary<string, ModelDefinition> Items = new Il2CppSystem.Collections.Generic.Dictionary<string, ModelDefinition>();

    public Il2CppSystem.Collections.Generic.Dictionary<string, ModelDefinition> GetDefinitionsByType(ModelDefinitionType type) =>
        type == ModelDefinitionType.INVENTORY_ITEM ? Items : null;
}

// Stores its fields by the hash of the field name, as the game does: the Mono String.GetHashCode.
public class ModelDefinition : Il2CppObjectBase
{
    public static int identifierFieldHash = Hash("identifier");

    // A test sets it to make the read of a field fail.
    public bool Broken;

    private readonly Dictionary<int, object> fields = new Dictionary<int, object>();

    public ModelDefinition(Dictionary<string, object> fieldMap)
    {
        foreach (var field in fieldMap) fields[Hash(field.Key)] = field.Value;
    }

    public static int Hash(string text)
    {
        unchecked
        {
            int a = 5381, b = 5381;
            for (int i = 0; i < text.Length; i++)
            {
                if (i % 2 == 0) a = ((a << 5) + a) ^ text[i];
                else b = ((b << 5) + b) ^ text[i];
            }
            return a + b * 1566083941;
        }
    }

    public bool TryGetValue(int fieldHash, out Il2CppSystem.Object value)
    {
        if (Broken) throw new System.ArgumentException("broken definition");
        value = fields.TryGetValue(fieldHash, out var raw) ? new Il2CppSystem.Object(raw) : null;
        return value != null;
    }
}

public class SerializablePrimitiveField<T> : Il2CppObjectBase
{
    private T value;

    // How often the mod read the value, which allocates an interop object in the game.
    public int Reads;

    public T Value
    {
        get
        {
            Reads++;
            return value;
        }
        set { this.value = value; }
    }
}

public class GameLoop : UnityEngine.Object
{
    public IGameManager gameManager;
}

public class ICreatureManager : Il2CppObjectBase { }

public class CreatureManager : ICreatureManager
{
    public Il2CppSystem.Collections.Generic.Dictionary<string, Creature> creatureMap = new Il2CppSystem.Collections.Generic.Dictionary<string, Creature>();
}

public class ItemDrop : Il2CppObjectBase
{
    public string itemIdentifier;
}

public class Creature : Il2CppObjectBase
{
    public Il2CppSystem.Collections.Generic.List<ItemDrop> HarvestDrops;
    public string DeadPickupItemIdentifier;
}

public class IInventoryManager : Il2CppObjectBase { }

public class InventoryManager : IInventoryManager
{
    public Il2CppSystem.Collections.Generic.Dictionary<string, InventoryItem> itemMap = new Il2CppSystem.Collections.Generic.Dictionary<string, InventoryItem>();
}

public class InventoryItem : Il2CppObjectBase
{
    private readonly string identifier;

    // How often the mod read the identifier, which allocates a managed string in the game.
    public int IdentifierReads;

    public InventoryItem hardenedItem;
    public readonly HashSet<ItemTag> Tags = new HashSet<ItemTag>();
    public float DecayRate;

    public InventoryItem(string identifier) { this.identifier = identifier; }

    public bool HasTag(ItemTag tag) => Tags.Contains(tag);

    public string Identifier
    {
        get
        {
            IdentifierReads++;
            return identifier;
        }
    }
}

public class InventoryItemStack : Il2CppObjectBase
{
    private InventoryItem item;

    // A test sets it to make the read of the item fail.
    public bool Broken;

    public InventoryItemStack(InventoryItem item) { this.item = item; }

    public InventoryItem Item => Broken ? throw new System.InvalidOperationException("broken stack") : item;

    public float ContaminatedPercentage { get; set; }
    public float Condition { get; set; }
    public float PreservedPercentage { get; set; }
    public InventorySlot slot;
}

public class InventorySlot : Il2CppObjectBase
{
    public Inventory inventory;
}

public enum ItemTag
{
    CONSUMABLE = 1,
}

public class ItemHardenedSignalParams : Il2CppObjectBase
{
    public InventoryItemStack hardenedStack;
    public SubstanceMixture hardenedMixture;
}

public class ItemHardenedCommand : Il2CppObjectBase
{
    public ItemHardenedSignalParams signalParams;
}

public class Inventory : Il2CppObjectBase
{
    public InventoryItemStack parentStack;
    public Entity Owner;

    // The inventory at the top of the chain of parent stacks: a test sets the slot of each parent stack.
    public Inventory GetRootInventory()
    {
        var inventory = this;
        while (inventory.parentStack?.slot?.inventory != null) inventory = inventory.parentStack.slot.inventory;
        return inventory;
    }

    public Entity GetOwnerEntity() => Owner;
}

public class AdvancedGameOptions : Il2CppObjectBase
{
    public float decayRateFactor;
}

public struct GameOptions
{
    public AdvancedGameOptions advancedGameOptions;
}

public class Game : Il2CppObjectBase
{
    public GameOptions gameOptions;
}

public class World : Il2CppObjectBase
{
    public float temperature = 20f;
    public Game game = new Game();
}

public class Entity : Il2CppObjectBase
{
    public string identifier;
    public World world;

    // The components that GetCachedComponent finds. A test adds them.
    public readonly List<Il2CppObjectBase> Components = new List<Il2CppObjectBase>();

    public T GetCachedComponent<T>(bool write) where T : Il2CppObjectBase
    {
        foreach (var component in Components)
            if (component is T found) return found;
        return null;
    }
}

public class CraftingIngredient : Il2CppObjectBase
{
    public string itemIdentifier;
    public string substanceIdentifier;
}

public class CookingRecipe : Il2CppObjectBase
{
    private readonly Il2CppSystem.Collections.Generic.List<CraftingIngredient> outputs = new Il2CppSystem.Collections.Generic.List<CraftingIngredient>();

    public CookingRecipe(string identifier, params CraftingIngredient[] outputs)
    {
        Identifier = identifier;
        foreach (var output in outputs) this.outputs.Add(output);
    }

    // A test sets it to make the read of the outputs fail.
    public bool Broken;

    public string Identifier { get; }

    public Il2CppSystem.Collections.Generic.List<CraftingIngredient> GetOutputIngredients() =>
        Broken ? throw new System.InvalidOperationException("broken recipe") : outputs;
}

public class CookingRecipeResult : Il2CppObjectBase
{
    public float contaminatedPercentage;
}

public class CookingManager : Il2CppObjectBase { }

public class SubstanceMixture : Il2CppObjectBase
{
    public float contaminatedPercentage;
}

public class ICooker : Il2CppObjectBase
{
    public SubstanceMixture Mixture;

    public SubstanceMixture GetContainerSubstanceMixture() => Mixture;
}

public class CookingCompleteEffect : Il2CppObjectBase
{
    public Entity cookerEntity;
    public CookingRecipe recipe;
}

public class IInventoryHolder : Il2CppObjectBase { }

// The component that an entity holds: it wraps the InventoryHolder.
public class InventoryHolderView : IInventoryHolder
{
    public InventoryHolder entityComponent;
}

public class InventoryHolder : IInventoryHolder
{
    public Entity entity;
    public SerializablePrimitiveField<float> contentsDecayFactor = new SerializablePrimitiveField<float> { Value = 1f };
}

// The tooltip window: its lines are the children of its transform, in sibling order.
public class TooltipUI : UnityEngine.Component
{
    public TooltipLineUI AddLine(string text, bool shown = true)
    {
        var line = new TooltipLineUI { text = new TMPro.TextMeshProUGUI { text = text } };
        line.gameObject.activeSelf = shown;
        transform.Add(line.transform);
        return line;
    }

    public List<string> Shown()
    {
        var result = new List<string>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.gameObject.activeSelf) result.Add(child.GetComponent<TooltipLineUI>()?.text?.text);
        }
        return result;
    }
}

public class TooltipLineUI : UnityEngine.Component
{
    public TMPro.TextMeshProUGUI text;
}

public class ShowTooltipEventHandler : Il2CppObjectBase
{
    // The text of each line that the mod adds. A test reads them.
    public readonly List<string> Lines = new List<string>();

    public TooltipLineUI addTooltipLine(TooltipUI tooltipUI, string text, bool bold)
    {
        Lines.Add(text);
        return tooltipUI.AddLine(text);
    }
}

// The localization of the game: the English text of each key, as "game.days" gives "days".
public static class L
{
    public static string T(string key) => key.Substring(key.IndexOf('.') + 1);
}

public class ICreature : Il2CppObjectBase { }

public class DecayableView : Il2CppObjectBase
{
    public Entity entity;
    public SerializablePrimitiveField<float> decayRate = new SerializablePrimitiveField<float> { Value = 0.0001f };
    public SerializablePrimitiveField<float> rainDecayRate = new SerializablePrimitiveField<float> { Value = 0.0002f };
    private float decayAmount;

    // How often the game sent the fully decayed signal.
    public int FullyDecayedSignals;

    public float GetDecayAmount() => decayAmount;

    public void SetDecayAmount(float amount) { decayAmount = amount; }

    // The step of DecayableView.Update: the scaled time x (decayRate + wetness x rainDecayRate), at most up to 1,
    // where the game sends the fully decayed signal.
    public void Update(float scaledSeconds, float wetness = 0f)
    {
        decayAmount += scaledSeconds * (decayRate.Value + wetness * rainDecayRate.Value);
        if (decayAmount < 1f) return;
        decayAmount = 1f;
        FullyDecayedSignals++;
    }
}
