using System.Text.Json;
using Rustlight.Core;
using Rustlight.Core.Combat;
using Rustlight.Core.Items;
using Rustlight.Core.Monetization;
using Rustlight.Core.Progression;
using Rustlight.Core.Quests;
using Rustlight.Core.Save;
using Rustlight.Core.Stats;
using Xunit;

public class FixedRandom : IRandom
{
    private readonly float _v;
    public FixedRandom(float v) => _v = v;
    public float Next01() => _v;
}

public class StatsTests
{
    [Fact]
    public void FlatThenPercent_RemovedBySource()
    {
        var s = new StatBlock();
        s.SetBase(StatType.Attack, 10);
        s.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Flat, 5, "a"));
        s.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Percent, 0.2f, "b"));
        Assert.Equal(18f, s.Get(StatType.Attack), 3);
        Assert.Equal(1, s.RemoveModifiersFrom("a"));
        Assert.Equal(12f, s.Get(StatType.Attack), 3);
    }
}

public class ProgressionTests
{
    [Fact]
    public void XpRollsOverMultipleLevels_AndCapsAtMax()
    {
        var curve = new LevelCurve(maxLevel: 3, baseXp: 100, exponent: 1);
        var p = new PlayerProgress();
        Assert.Equal(1, p.AddXp(150, curve)); // L1->2 costs 100
        Assert.Equal(2, p.level);
        Assert.Equal(50, p.xp);
        Assert.Equal(1, p.AddXp(10_000, curve));
        Assert.Equal(3, p.level);
        Assert.Equal(0, p.xp);
    }
}

public class CombatTests
{
    [Fact]
    public void ArmorReducesDamage_CritMultiplies_MinimumOne()
    {
        var atk = new StatBlock();
        atk.SetBase(StatType.Attack, 20);
        atk.SetBase(StatType.CritChance, 0.5f);
        atk.SetBase(StatType.CritMultiplier, 2f);
        var def = new StatBlock();
        def.SetBase(StatType.Armor, 100);

        var normal = DamageCalculator.Compute(atk, def, 1f, new FixedRandom(0.9f));
        Assert.Equal(10, normal.Damage);
        Assert.False(normal.IsCrit);

        var crit = DamageCalculator.Compute(atk, def, 1f, new FixedRandom(0.1f));
        Assert.Equal(20, crit.Damage);
        Assert.True(crit.IsCrit);

        var weak = new StatBlock();
        Assert.Equal(1, DamageCalculator.Compute(weak, def, 1f, new FixedRandom(0.9f)).Damage);
    }
}

public class InventoryTests
{
    private static GameState NewState() => new GameState(SaveData.CreateNew());

    [Fact]
    public void StacksFillBeforeNewSlots_AndAddIsAllOrNothing()
    {
        var g = NewState();
        Assert.True(g.Inventory.TryAdd("scrap_metal", 150));
        Assert.Equal(2, g.Inventory.Stacks.Count);
        Assert.Equal(150, g.Inventory.CountOf("scrap_metal"));

        for (int i = 0; i < 18; i++) Assert.True(g.Inventory.TryAdd("pipe_wrench"));
        Assert.Equal(20, g.Inventory.Stacks.Count);
        Assert.False(g.Inventory.TryAdd("medkit"));          // full
        Assert.True(g.Inventory.TryAdd("scrap_metal", 48)); // fits in the partial stack
        Assert.False(g.Inventory.TryAdd("scrap_metal", 2)); // would need a new slot
        Assert.Equal(198, g.Inventory.CountOf("scrap_metal"));
    }

    [Fact]
    public void EquipSwapsAndUpdatesStats()
    {
        var g = NewState();
        float baseAtk = g.PlayerStats.Get(StatType.Attack);
        g.Inventory.TryAdd("pipe_wrench");
        g.Inventory.TryAdd("rail_carbine");

        Assert.True(g.Equipment.TryEquip("pipe_wrench"));
        Assert.Equal(baseAtk + 4, g.PlayerStats.Get(StatType.Attack), 3);

        Assert.True(g.Equipment.TryEquip("rail_carbine"));
        Assert.Equal(baseAtk + 12, g.PlayerStats.Get(StatType.Attack), 3);
        Assert.Equal(1, g.Inventory.CountOf("pipe_wrench"));
        Assert.Equal(0, g.Inventory.CountOf("rail_carbine"));

        Assert.True(g.Equipment.TryUnequip(EquipSlot.Weapon));
        Assert.Equal(baseAtk, g.PlayerStats.Get(StatType.Attack), 3);
    }

    [Fact]
    public void EquippedItemsReapplyAfterLoad()
    {
        var g = NewState();
        g.Inventory.TryAdd("rad_plating");
        g.Equipment.TryEquip("rad_plating");
        float armor = g.PlayerStats.Get(StatType.Armor);

        var reloaded = new GameState(RoundTrip(g.Save));
        Assert.Equal(armor, reloaded.PlayerStats.Get(StatType.Armor), 3);
    }

    internal static SaveData RoundTrip(SaveData s)
    {
        var opts = new JsonSerializerOptions { IncludeFields = true };
        return JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(s, opts), opts);
    }
}

public class PurchaseTests
{
    [Fact]
    public void ConsumableGrantedOncePerTransaction()
    {
        var g = new GameState(SaveData.CreateNew());
        Assert.Equal(GrantResult.Granted, g.Purchases.Grant(StoreCatalog.CoresSmall, "tx1"));
        Assert.Equal(GrantResult.AlreadyGranted, g.Purchases.Grant(StoreCatalog.CoresSmall, "tx1"));
        Assert.Equal(GrantResult.Granted, g.Purchases.Grant(StoreCatalog.CoresSmall, "tx2"));
        Assert.Equal(200, g.Wallet.Cores);
    }

    [Fact]
    public void NonConsumableRestoreDoesNotPayTwice()
    {
        var g = new GameState(SaveData.CreateNew());
        Assert.Equal(GrantResult.Granted, g.Purchases.Grant(StoreCatalog.StarterPack, "tx1"));
        Assert.Equal(GrantResult.AlreadyGranted, g.Purchases.Grant(StoreCatalog.StarterPack, "restore-tx"));
        Assert.Equal(300, g.Wallet.Cores);
        Assert.Equal(SaveData.DefaultInventoryCapacity + 10, g.Inventory.Capacity);
        Assert.True(g.Cosmetics.Owns("suit_scavenger"));

        var reloaded = new GameState(InventoryTests.RoundTrip(g.Save));
        Assert.Equal(SaveData.DefaultInventoryCapacity + 10, reloaded.Inventory.Capacity);
        Assert.True(reloaded.Purchases.IsEntitled(StoreCatalog.StarterPack));
    }

    [Fact]
    public void UnknownProductIsRejected()
    {
        var g = new GameState(SaveData.CreateNew());
        Assert.Equal(GrantResult.UnknownProduct, g.Purchases.Grant("com.other.thing", "tx"));
    }

    [Fact]
    public void LedgerIsBounded()
    {
        var g = new GameState(SaveData.CreateNew());
        for (int i = 0; i < PurchaseGrantService.LedgerSize + 50; i++)
            g.Purchases.Grant(StoreCatalog.CoresSmall, "tx" + i);
        Assert.Equal(PurchaseGrantService.LedgerSize, g.Save.processedTransactions.Count);
    }
}

public class CosmeticTests
{
    [Fact]
    public void BuyWithCoresThenWear()
    {
        var g = new GameState(SaveData.CreateNew());
        Assert.False(g.Cosmetics.TryBuy("suit_neon"));     // no cores
        g.Wallet.AddCores(500);
        Assert.True(g.Cosmetics.TryBuy("suit_neon"));
        Assert.Equal(100, g.Wallet.Cores);
        Assert.False(g.Cosmetics.TryBuy("suit_neon"));     // already owned
        Assert.False(g.Cosmetics.TryBuy("suit_vanguard")); // pack-only
        Assert.True(g.Cosmetics.TryWear("suit_neon"));
        Assert.Equal("suit_neon", g.Save.activeSuit);
    }
}

public class QuestTests
{
    [Fact]
    public void KillQuestCompletesAndRewardsOnce()
    {
        var g = new GameState(SaveData.CreateNew());
        Assert.True(g.Quests.Accept("q_raiders"));
        for (int i = 0; i < 7; i++) g.Quests.Report(ObjectiveType.Kill, "raider");
        Assert.True(g.Quests.Get("q_raiders").completed);
        Assert.Equal(5, g.Quests.Get("q_raiders").progress);
        Assert.True(g.ClaimQuest("q_raiders"));
        Assert.False(g.ClaimQuest("q_raiders"));
        Assert.Equal(60, g.Wallet.Scrap);
        Assert.Equal(2, g.Save.progress.level); // 120 XP > 100 needed
    }
}

public class SaveTests
{
    [Fact]
    public void MigratorRepairsNullsAndRejectsFutureVersions()
    {
        var broken = new SaveData { inventory = null, wallet = null, quests = null, currentZone = null };
        var fixedSave = SaveMigrator.Migrate(broken);
        Assert.NotNull(fixedSave.inventory);
        Assert.NotNull(fixedSave.wallet);
        Assert.Equal("bunker", fixedSave.currentZone);

        Assert.Throws<System.InvalidOperationException>(() =>
            SaveMigrator.Migrate(new SaveData { version = SaveData.CurrentVersion + 1 }));
    }
}
