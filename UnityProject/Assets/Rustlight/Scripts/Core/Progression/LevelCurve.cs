using System;

namespace Rustlight.Core.Progression
{
    /// <summary>XP needed per level: round(baseXp * level^exponent), capped at <see cref="MaxLevel"/>.</summary>
    public sealed class LevelCurve
    {
        public int MaxLevel { get; }
        private readonly float _baseXp;
        private readonly float _exponent;

        public LevelCurve(int maxLevel = 30, float baseXp = 100f, float exponent = 1.5f)
        {
            if (maxLevel < 1) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            MaxLevel = maxLevel;
            _baseXp = baseXp;
            _exponent = exponent;
        }

        /// <summary>XP required to go from <paramref name="level"/> to the next one (0 at max level).</summary>
        public int XpToNext(int level)
        {
            if (level >= MaxLevel) return 0;
            return (int)Math.Round(_baseXp * Math.Pow(level, _exponent));
        }
    }

    /// <summary>Tracks the player's level and XP within the current level.</summary>
    [Serializable]
    public sealed class PlayerProgress
    {
        public int level = 1;
        public int xp;

        /// <summary>Adds XP, rolling over into as many levels as it pays for; returns levels gained.</summary>
        public int AddXp(int amount, LevelCurve curve)
        {
            if (amount <= 0) return 0;
            int gained = 0;
            xp += amount;
            while (level < curve.MaxLevel && xp >= curve.XpToNext(level))
            {
                xp -= curve.XpToNext(level);
                level++;
                gained++;
            }
            if (level >= curve.MaxLevel) xp = 0;
            return gained;
        }
    }
}
