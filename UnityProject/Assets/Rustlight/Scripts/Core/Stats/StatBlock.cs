using System;
using System.Collections.Generic;

namespace Rustlight.Core.Stats
{
    /// <summary>
    /// Base stats plus a stack of modifiers. Final value = (base + flat) * (1 + sum of percents),
    /// so the result never depends on the order items were equipped.
    /// </summary>
    public sealed class StatBlock
    {
        private readonly Dictionary<StatType, float> _base = new Dictionary<StatType, float>();
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        /// <summary>Raised whenever a base value or modifier changes.</summary>
        public event Action Changed;

        public void SetBase(StatType stat, float value)
        {
            _base[stat] = value;
            Changed?.Invoke();
        }

        public float GetBase(StatType stat) => _base.TryGetValue(stat, out var v) ? v : 0f;

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Changed?.Invoke();
        }

        /// <summary>Removes every modifier owned by <paramref name="source"/>; returns how many were removed.</summary>
        public int RemoveModifiersFrom(string source)
        {
            int removed = _modifiers.RemoveAll(m => m.source == source);
            if (removed > 0) Changed?.Invoke();
            return removed;
        }

        /// <summary>Computes the final value of a stat, clamped at zero.</summary>
        public float Get(StatType stat)
        {
            float flat = 0f, percent = 0f;
            foreach (var m in _modifiers)
            {
                if (m.stat != stat) continue;
                if (m.kind == ModifierKind.Flat) flat += m.value;
                else percent += m.value;
            }
            float value = (GetBase(stat) + flat) * (1f + percent);
            return value < 0f ? 0f : value;
        }
    }
}
