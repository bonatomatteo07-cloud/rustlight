using System;
using Rustlight.Core.Combat;
using Rustlight.Core.Stats;
using UnityEngine;

namespace Rustlight.Runtime.Combat
{
    /// <summary>Hit points for the player or an enemy, sized from a <see cref="StatBlock"/>'s MaxHealth.</summary>
    public sealed class Health : MonoBehaviour
    {
        public StatBlock Stats { get; private set; }
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;
        public int Max => Mathf.Max(1, Mathf.RoundToInt(Stats.Get(StatType.MaxHealth)));

        /// <summary>Damage taken and whether it was a critical hit.</summary>
        public event Action<int, bool> Damaged;
        public event Action Changed;
        public event Action Died;

        /// <summary>Binds the stats and refills health. Call once from the owner's Awake.</summary>
        public void Init(StatBlock stats)
        {
            Stats = stats;
            Current = Max;
            Changed?.Invoke();
        }

        public void TakeHit(HitResult hit)
        {
            if (IsDead) return;
            Current = Mathf.Max(0, Current - hit.Damage);
            Damaged?.Invoke(hit.Damage, hit.IsCrit);
            Changed?.Invoke();
            if (IsDead) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            Current = Mathf.Min(Max, Current + amount);
            Changed?.Invoke();
        }

        public void RefillAndRevive()
        {
            Current = Max;
            Changed?.Invoke();
        }
    }
}
