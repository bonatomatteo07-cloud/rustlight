using System;
using Rustlight.Core.Stats;

namespace Rustlight.Core.Combat
{
    /// <summary>Random source abstraction so combat rolls can be seeded in tests.</summary>
    public interface IRandom
    {
        /// <summary>Returns a value in [0, 1).</summary>
        float Next01();
    }

    /// <summary><see cref="IRandom"/> backed by System.Random.</summary>
    public sealed class SystemRandom : IRandom
    {
        private readonly Random _random;
        public SystemRandom(int? seed = null) => _random = seed.HasValue ? new Random(seed.Value) : new Random();
        public float Next01() => (float)_random.NextDouble();
    }

    /// <summary>Outcome of a single hit.</summary>
    public readonly struct HitResult
    {
        public readonly int Damage;
        public readonly bool IsCrit;
        public HitResult(int damage, bool isCrit) { Damage = damage; IsCrit = isCrit; }
    }

    /// <summary>
    /// Damage = attack * skillPower * 100 / (100 + armor), times the crit multiplier on a crit.
    /// Armor gives diminishing returns and never fully negates a hit (minimum 1 damage).
    /// </summary>
    public static class DamageCalculator
    {
        public static HitResult Compute(StatBlock attacker, StatBlock defender, float skillPower, IRandom random)
        {
            float attack = attacker.Get(StatType.Attack) * skillPower;
            float armor = defender.Get(StatType.Armor);
            float raw = attack * 100f / (100f + armor);

            bool crit = random.Next01() < attacker.Get(StatType.CritChance);
            if (crit)
            {
                float mult = attacker.Get(StatType.CritMultiplier);
                raw *= mult < 1f ? 1.5f : mult;
            }

            int damage = (int)Math.Round(raw);
            return new HitResult(damage < 1 ? 1 : damage, crit);
        }
    }
}
