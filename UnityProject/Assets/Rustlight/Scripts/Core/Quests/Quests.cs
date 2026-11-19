using System;
using System.Collections.Generic;

namespace Rustlight.Core.Quests
{
    public enum ObjectiveType { Kill, Collect, Reach }

    /// <summary>Static data for a quest with a single counted objective.</summary>
    public sealed class QuestDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public ObjectiveType Objective { get; }
        public string TargetId { get; }
        public int Required { get; }
        public int RewardXp { get; }
        public int RewardScrap { get; }

        public QuestDefinition(string id, string title, ObjectiveType objective, string targetId,
            int required, int rewardXp, int rewardScrap)
        {
            Id = id; Title = title; Objective = objective; TargetId = targetId;
            Required = required < 1 ? 1 : required; RewardXp = rewardXp; RewardScrap = rewardScrap;
        }
    }

    /// <summary>Saved progress for one accepted quest.</summary>
    [Serializable]
    public sealed class QuestProgress
    {
        public string questId;
        public int progress;
        public bool completed;
        public bool rewarded;
    }

    /// <summary>Accepts quests, counts gameplay events toward them and hands out rewards once.</summary>
    public sealed class QuestLog
    {
        private readonly Dictionary<string, QuestDefinition> _defs = new Dictionary<string, QuestDefinition>();
        private readonly List<QuestProgress> _progress;

        public event Action<QuestDefinition> Completed;

        public QuestLog(IEnumerable<QuestDefinition> definitions, List<QuestProgress> progress)
        {
            foreach (var d in definitions) _defs[d.Id] = d;
            _progress = progress;
        }

        public QuestProgress Get(string questId) => _progress.Find(p => p.questId == questId);

        public bool Accept(string questId)
        {
            if (!_defs.ContainsKey(questId) || Get(questId) != null) return false;
            _progress.Add(new QuestProgress { questId = questId });
            return true;
        }

        /// <summary>Counts an event (e.g. Kill "raider") toward every active matching quest.</summary>
        public void Report(ObjectiveType type, string targetId, int amount = 1)
        {
            foreach (var p in _progress)
            {
                if (p.completed || !_defs.TryGetValue(p.questId, out var def)) continue;
                if (def.Objective != type || def.TargetId != targetId) continue;
                p.progress = Math.Min(def.Required, p.progress + amount);
                if (p.progress >= def.Required)
                {
                    p.completed = true;
                    Completed?.Invoke(def);
                }
            }
        }

        /// <summary>Marks a completed quest as rewarded and returns its definition, or null if not claimable.</summary>
        public QuestDefinition Claim(string questId)
        {
            var p = Get(questId);
            if (p == null || !p.completed || p.rewarded || !_defs.TryGetValue(questId, out var def)) return null;
            p.rewarded = true;
            return def;
        }
    }
}
