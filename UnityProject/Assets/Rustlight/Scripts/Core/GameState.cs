using System;
using Rustlight.Core.Content;
using Rustlight.Core.Economy;
using Rustlight.Core.Items;
using Rustlight.Core.Monetization;
using Rustlight.Core.Progression;
using Rustlight.Core.Quests;
using Rustlight.Core.Save;
using Rustlight.Core.Stats;

namespace Rustlight.Core
{
    /// <summary>
    /// The whole game model built on one <see cref="SaveData"/>: every system writes straight into it,
    /// so saving is just serializing <see cref="Save"/>. Has no Unity dependency.
    /// </summary>
    public sealed class GameState
    {
        public SaveData Save { get; }
        public LevelCurve Curve { get; }
        public StatBlock PlayerStats { get; } = new StatBlock();
        public ItemDatabase Items { get; }
        public Inventory Inventory { get; }
        public Equipment Equipment { get; }
        public Wallet Wallet { get; }
        public CosmeticShop Cosmetics { get; }
        public QuestLog Quests { get; }
        public PurchaseGrantService Purchases { get; }

        /// <summary>Raised with the new level after the player levels up.</summary>
        public event Action<int> LeveledUp;

        public GameState(SaveData save, LevelCurve curve = null)
        {
            Save = SaveMigrator.Migrate(save);
            Curve = curve ?? new LevelCurve();
            Items = new ItemDatabase(GameContent.Items());
            ApplyLevelStats();
            Inventory = new Inventory(Items, Save.inventory, Save.inventoryCapacity);
            Equipment = new Equipment(Items, Inventory, PlayerStats, Save.equipped);
            Wallet = new Wallet(Save.wallet);
            Cosmetics = new CosmeticShop(GameContent.Cosmetics(), Save, Wallet);
            Quests = new QuestLog(GameContent.Quests(), Save.quests);
            Purchases = new PurchaseGrantService(Save, Wallet, Inventory, Cosmetics);

            Cosmetics.Grant("suit_default");
            Cosmetics.Grant("drone_default");
            if (Save.activeSuit == "") Save.activeSuit = "suit_default";
            if (Save.activeDrone == "") Save.activeDrone = "drone_default";
        }

        /// <summary>Gives XP and refreshes level-based stats when the player levels up.</summary>
        public void AddXp(int amount)
        {
            int gained = Save.progress.AddXp(amount, Curve);
            if (gained <= 0) return;
            ApplyLevelStats();
            LeveledUp?.Invoke(Save.progress.level);
        }

        /// <summary>Grants a completed quest's rewards once; returns false if it is not claimable.</summary>
        public bool ClaimQuest(string questId)
        {
            var def = Quests.Claim(questId);
            if (def == null) return false;
            Wallet.AddScrap(def.RewardScrap);
            AddXp(def.RewardXp);
            return true;
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
