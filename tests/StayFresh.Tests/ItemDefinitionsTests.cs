using StayFresh;
using StayFresh.Game;
using Xunit;

public class ItemDefinitionsTests : GameTestBase
{
    [Fact]
    public void The_content_factors_come_from_the_item_definitions_with_copy_from()
    {
        GameWorld.Load("stone");

        var factors = ItemDefinitions.LoadContentFactors();

        Assert.Equal(new Dictionary<string, float> { ["clay_jar_covered"] = 0.3f, ["glass_bottle_covered"] = 0.3f }, factors);
    }

    [Fact]
    public void Before_the_game_loads_there_are_no_factors()
    {
        Assert.Null(ItemDefinitions.LoadContentFactors());

        GameWorld.Load().modelDefinitionManager = null;
        Assert.Null(ItemDefinitions.LoadContentFactors());
    }

    [Fact]
    public void A_value_with_a_decimal_comma_or_a_whole_number_is_read()
    {
        var manager = GameWorld.Load();
        var definitions = (ModelDefinitionManager)manager.modelDefinitionManager;
        definitions.Items.Add("comma_jar", GameWorld.Definition(("identifier", "comma_jar"), ("contentsDecayFactor", "0,5")));
        definitions.Items.Add("whole_jar", GameWorld.Definition(("identifier", "whole_jar"), ("contentsDecayFactor", 1)));
        definitions.Items.Add("bad_jar", GameWorld.Definition(("identifier", "bad_jar"), ("contentsDecayFactor", "none")));
        definitions.Items.Add("double_jar", GameWorld.Definition(("identifier", "double_jar"), ("contentsDecayFactor", 0.25)));
        definitions.Items.Add("flag_jar", GameWorld.Definition(("identifier", "flag_jar"), ("contentsDecayFactor", true)));
        definitions.Items.Add("number_name", GameWorld.Definition(("identifier", 7), ("copyFromIdentifier", "clay_jar_covered")));

        var factors = ItemDefinitions.LoadContentFactors();

        Assert.Equal(0.5f, factors["comma_jar"]);
        Assert.Equal(1f, factors["whole_jar"]);
        Assert.Equal(0.25f, factors["double_jar"]);
        Assert.False(factors.ContainsKey("bad_jar"));
        Assert.False(factors.ContainsKey("flag_jar"));
        Assert.Equal(0.3f, factors["number_name"]);
    }

    [Fact]
    public void The_load_writes_one_verbose_line_with_the_factors()
    {
        GameWorld.Load();
        Plugin.Verbose.Value = true;

        ItemDefinitions.LoadContentFactors();

        Assert.Contains("Item definitions: content factors clay_jar_covered 0.3, glass_bottle_covered 0.3", Plugin.Log.Debugs);
        Assert.Contains("Item definitions: the content factor of clay_jar_covered is a Single", Plugin.Log.Debugs);
    }

    [Fact]
    public void A_game_with_another_field_hash_gives_no_factors_and_one_warning()
    {
        GameWorld.Load();
        var hash = ModelDefinition.identifierFieldHash;
        ModelDefinition.identifierFieldHash = 1;
        try
        {
            Assert.Empty(ItemDefinitions.LoadContentFactors());
        }
        finally
        {
            ModelDefinition.identifierFieldHash = hash;
        }
        Assert.Equal(new[] { "Stay Fresh cannot read the item definitions of this game version: covered containers spoil as in the game" }, Plugin.Log.Warnings);
    }

    [Fact]
    public void No_item_with_the_key_gives_no_factors_and_one_warning()
    {
        var manager = GameWorld.Load();
        var definitions = new ModelDefinitionManager();
        definitions.Items.Add("clay_jar_covered", GameWorld.Definition(("identifier", "clay_jar_covered"), ("contentDecay", 0.3f)));
        manager.modelDefinitionManager = definitions;

        Assert.Empty(ItemDefinitions.LoadContentFactors());
        Assert.Equal(new[] { "Stay Fresh found no content factor in the item definitions: covered containers spoil as in the game" }, Plugin.Log.Warnings);
    }
}
