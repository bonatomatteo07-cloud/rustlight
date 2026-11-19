using System;
using Rustlight.Core.Achievements;
using Rustlight.Core.Content;
using Rustlight.Core.Dlc;
using Rustlight.Core.Economy;
using Rustlight.Core.Items;
using Rustlight.Core.Progression;
using Rustlight.Core.Quests;
using Rustlight.Core.Save;
using Rustlight.Core.Stats;

namespace Rustlight.Core
{
    /// <summary>
    /// The whole game model built on one <see cref="SaveData"/>: every system writes straight into it,
    /// so saving is just serializing <see cref="Save"/>. DLC unlocks come from <see cref="Entitlements"/>
    /// at runtime and are never written to the save. Has no Unity dependency.
    /// </summary>
    public sealed class GameState
    {
        public const string DefaultSuit = "suit_default";
        public const string DefaultDrone = "drone_default";

        /// <summary>Ownership used when no store is available: owns nothing.</summary>
        private sealed class NoDlc : IDlcOwnership { public bool Owns(DlcDefinition dlc) => false; }

        public SaveData Save { get; }
        public LevelCurve Curve { get; }
        public StatBlock PlayerStats { get; } = new StatBlock();
        public ItemDatabase Items { get; }
        public Inventory Inventory { get; }
        public Equipment Equipment { get; }
        public Wallet Wallet { get; }
        public CosmeticShop Cosmetics { get; }
        public QuestLog Quests { get; }
        public Entitlements Entitlements { get; }
        public AchievementTracker Achievements { get; }

        /// <summary>Raised with the new level after the player levels up.</summary>
        public event Action<int> LeveledUp;

        public GameState(SaveData save, IDlcOwnership dlcOwnership = null, LevelCurve curve = null)
        {
            Save = SaveMigrator.Migrate(save);
            Curve = curve ?? new LevelCurve();
            Items = new ItemDatabase(GameContent.Items());
            ApplyLevelStats();
            Entitlements = new Entitlements(dlcOwnership ?? new NoDlc());
            Inventory = new Inventory(Items, Save.inventory, SaveData.DefaultInventoryCapacity + Entitlements.ExtraInventorySlots);
            Equipment = new Equipment(Items, Inventory, PlayerStats, Save.equipped);
            Wallet = new Wallet(Save.wallet);
            Cosmetics = new CosmeticShop(GameContent.Cosmetics(), Save, Wallet, Entitlements.GrantsCosmetic);
            Quests = new QuestLog(GameContent.Quests(), Save.quests);
            Achievements = new AchievementTracker(this);

            Cosmetics.Grant(DefaultSuit);
            Cosmetics.Grant(DefaultDrone);
            Cosmetics.EnsureWornAreOwned(DefaultSuit, DefaultDrone);
            Entitlements.Changed += OnEntitlementsChanged;
        }

        /// <summary>Gives XP and refreshes level-based stats when the player levels up.</summary>
        public void AddXp(int amount)
        {
            int gained = Save.progress.AddXp(amount, Curve);
            if (gained <= 0) return;
            ApplyLevelStats();
            LeveledUp?.Invoke(Save.progress.level);
        }

        /// <summary>Adds Scrap earned in play and counts it toward lifetime totals.</summary>
        public void EarnScrap(long amount)
        {
            if (amount <= 0) return;
            Wallet.AddScrap(amount);
            Save.lifetime.scrapEarned += amount;
        }

        /// <summary>Records a defeated enemy for quests and lifetime totals.</summary>
        public void RegisterKill(string enemyId)
        {
            Save.lifetime.kills++;
            Quests.Report(ObjectiveType.Kill, enemyId);
        }

        /// <summary>Grants a completed quest's rewards once; returns false if it is not claimable.</summary>
        public bool ClaimQuest(string questId)
        {
            var def = Quests.Claim(questId);
            if (def == null) return false;
            EarnScrap(def.RewardScrap);
            Wallet.AddCores(def.RewardCores);
            AddXp(def.RewardXp);
            return true;
        }

        private void OnEntitlementsChanged()
        {
            Inventory.SetCapacity(SaveData.DefaultInventoryCapacity + Entitlements.ExtraInventorySlots);
            Cosmetics.NotifyOwnershipChanged();
            Cosmetics.EnsureWornAreOwned(DefaultSuit, DefaultDrone);
        }

        private void ApplyLevelStats()
        {
            int lvl = Save.progress.level - 1;
            PlayerStats.SetBase(StatType.MaxHealth, 100 + 12 * lvl);
            PlayerStats.SetBase(StatType.Attack, 10 + 2 * lvl);
            PlayerStats.SetBase(StatType.Armor, 5 + lvl);
            PlayerStats.SetBase(StatType.CritChance, 0.05f);
            PlayerStats.SetBase(StatType.CritMultiplier, 1.5f);
            PlayerStats.SetBase(StatType.MoveSpeed, 5f);
        }
    }
}
