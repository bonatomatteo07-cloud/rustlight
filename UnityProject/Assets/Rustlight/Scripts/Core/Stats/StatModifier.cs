using System;

namespace Rustlight.Core.Stats
{
    /// <summary>How a modifier combines with the base value.</summary>
    public enum ModifierKind
    {
        /// <summary>Added to the base value before percentages.</summary>
        Flat,
        /// <summary>Summed with other percent modifiers, then applied once (0.1 = +10%).</summary>
        Percent,
    }

    /// <summary>A single change to a stat, tagged with the source that owns it so it can be removed later.</summary>
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModifierKind kind;
        public float value;
        public string source;

        public StatModifier(StatType stat, ModifierKind kind, float value, string source = "")
        {
            this.stat = stat;
            this.kind = kind;
            this.value = value;
            this.source = source;
        }

        /// <summary>Returns a copy of this modifier owned by <paramref name="newSource"/>.</summary>
        public StatModifier WithSource(string newSource) => new StatModifier(stat, kind, value, newSource);
    }
}
