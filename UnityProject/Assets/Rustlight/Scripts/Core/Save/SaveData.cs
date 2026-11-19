using System;
using System.Collections.Generic;
using Rustlight.Core.Items;
using Rustlight.Core.Progression;
using Rustlight.Core.Quests;

namespace Rustlight.Core.Save
{
    /// <summary>Both currencies, both earned in play: Scrap is common, Cores are rare (bosses, quests, crystals).</summary>
    [Serializable]
    public sealed class WalletData
    {
        public long scrap;
        public long cores;
    }

    /// <summary>Running totals that achievements are checked against.</summary>
    [Serializable]
    public sealed class LifetimeStats
    {
        public int kills;
        public long scrapEarned;
    }

    /// <summary>
    /// Everything persisted for one player. Plain public fields and Lists only, so Unity's
    /// JsonUtility and System.Text.Json can both serialize it.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;
        public const int DefaultInventoryCapacity = 20;

        public int version = CurrentVersion;
        public PlayerProgress progress = new PlayerProgress();
        public WalletData wallet = new WalletData();
        public List<ItemStack> inventory = new List<ItemStack>();
        public List<EquippedEntry> equipped = new List<EquippedEntry>();
        public List<string> ownedCosmetics = new List<string>();
        public string activeSuit = "";
        public string activeDrone = "";
        public List<QuestProgress> quests = new List<QuestProgress>();
        public LifetimeStats lifetime = new LifetimeStats();
        public List<string> unlockedAchievements = new List<string>();
        public string currentZone = "bunker";

        public static SaveData CreateNew() => new SaveData();
    }

    /// <summary>Upgrades older saves to <see cref="SaveData.CurrentVersion"/> and repairs missing fields.</summary>
    public static class SaveMigrator
    {
        /// <summary>Migrates in place and returns the same object; throws if the save is from a newer build.</summary>
        public static SaveData Migrate(SaveData data)
        {
            if (data == null) return SaveData.CreateNew();
            if (data.version > SaveData.CurrentVersion)
                throw new InvalidOperationException(
                    $"Save version {data.version} is newer than this build supports ({SaveData.CurrentVersion}).");

            // Future migrations go here, one step per version:
            // if (data.version < 2) { ...; data.version = 2; }

            data.progress ??= new PlayerProgress();
            if (data.progress.level < 1) data.progress.level = 1;
            data.wallet ??= new WalletData();
            data.inventory ??= new List<ItemStack>();
            data.inventory.RemoveAll(s => s == null || string.IsNullOrEmpty(s.itemId) || s.count <= 0);
            data.equipped ??= new List<EquippedEntry>();
            data.ownedCosmetics ??= new List<string>();
            data.activeSuit ??= "";
            data.activeDrone ??= "";
            data.quests ??= new List<QuestProgress>();
            data.lifetime ??= new LifetimeStats();
            data.unlockedAchievements ??= new List<string>();
            if (string.IsNullOrEmpty(data.currentZone)) data.currentZone = "bunker";
            data.version = SaveData.CurrentVersion;
            return data;
        }
    }
}
