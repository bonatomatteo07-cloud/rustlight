using System;
using System.Collections.Generic;

namespace Rustlight.Core.Achievements
{
    /// <summary>An achievement: its API name in Steamworks and the condition that unlocks it.</summary>
    public sealed class AchievementDefinition
    {
        /// <summary>Must match the "API Name" in Steamworks → Stats &amp; Achievements.</summary>
        public string ApiName { get; }
        public string DisplayName { get; }
        public Func<GameState, bool> IsMet { get; }

        public AchievementDefinition(string apiName, string displayName, Func<GameState, bool> isMet)
        {
            ApiName = apiName; DisplayName = displayName; IsMet = isMet;
        }
    }

    /// <summary>
    /// Checks conditions against the game state and records unlocks in the save, so achievements
    /// earned offline are still pushed to Steam the next time it is available.
    /// </summary>
    public sealed class AchievementTracker
    {
        public static readonly IReadOnlyList<AchievementDefinition> All = new[]
        {
            new AchievementDefinition("ACH_FIRST_RAIDER", "Primo sangue", s => s.Save.lifetime.kills >= 1),
            new AchievementDefinition("ACH_DEPOT_CLEARED", "Deposito ripulito",
                s => s.Quests.Get("q_raiders")?.rewarded == true),
            new AchievementDefinition("ACH_SCRAPPER", "Rottamaio", s => s.Save.lifetime.scrapEarned >= 1000),
            new AchievementDefinition("ACH_LEVEL_10", "Sopravvissuto esperto", s => s.Save.progress.level >= 10),
            new AchievementDefinition("ACH_RAIDER_100", "Flagello dei predoni", s => s.Save.lifetime.kills >= 100),
        };

        private readonly GameState _state;

        public AchievementTracker(GameState state) => _state = state;

        public bool IsUnlocked(string apiName) => _state.Save.unlockedAchievements.Contains(apiName);

        /// <summary>Unlocks every newly met achievement and returns their API names (empty if none).</summary>
        public List<string> Evaluate()
        {
            var unlocked = new List<string>();
            foreach (var a in All)
            {
                if (IsUnlocked(a.ApiName) || !a.IsMet(_state)) continue;
                _state.Save.unlockedAchievements.Add(a.ApiName);
                unlocked.Add(a.ApiName);
            }
            return unlocked;
        }
    }
}
