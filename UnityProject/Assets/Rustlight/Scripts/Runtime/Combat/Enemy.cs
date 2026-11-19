using System;
using Rustlight.Core.Combat;
using Rustlight.Core.Stats;
using Rustlight.Runtime.Player;
using Rustlight.Runtime.Services;
using UnityEngine;
using UnityEngine.AI;

namespace Rustlight.Runtime.Combat
{
    /// <summary>
    /// Melee enemy: idles until the player is within aggro range, chases on the NavMesh, hits in range.
    /// On death it pays XP and Scrap, rolls its loot table and counts toward Kill quests.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class Enemy : MonoBehaviour
    {
        [Serializable]
        public struct LootEntry
        {
            public string itemId;
            [Range(0f, 1f)] public float chance;
        }

        [Header("Identity")]
        [Tooltip("Id used by Kill quests, e.g. 'raider'.")]
        [SerializeField] private string questTargetId = "raider";

        [Header("Stats")]
        [SerializeField] private float maxHealth = 40f;
        [SerializeField] private float attack = 8f;
        [SerializeField] private float armor = 5f;

        [Header("Behaviour")]
        [SerializeField] private float aggroRange = 10f;
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Rewards")]
        [SerializeField] private int xpReward = 25;
        [SerializeField] private int scrapReward = 8;
        [SerializeField] private LootEntry[] loot = Array.Empty<LootEntry>();

        private readonly IRandom _random = new SystemRandom();
        private NavMeshAgent _agent;
        private Health _health;
        private float _nextAttackTime;

        public Health Health => _health;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            var stats = new StatBlock();
            stats.SetBase(StatType.MaxHealth, maxHealth);
            stats.SetBase(StatType.Attack, attack);
            stats.SetBase(StatType.Armor, armor);
            stats.SetBase(StatType.CritChance, 0.05f);
            stats.SetBase(StatType.CritMultiplier, 1.5f);
            _health.Init(stats);
            _health.Died += OnDied;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (_health.IsDead || player == null || player.Health.IsDead)
            {
                if (_agent.isOnNavMesh) _agent.isStopped = true;
                return;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > aggroRange || !_agent.isOnNavMesh) return;

            if (distance > attackRange)
            {
                _agent.isStopped = false;
                _agent.SetDestination(player.transform.position);
                return;
            }

            _agent.isStopped = true;
            Vector3 look = player.transform.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(look);

            if (Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + attackCooldown;
                player.Health.TakeHit(DamageCalculator.Compute(_health.Stats, player.Health.Stats, 1f, _random));
            }
        }

        private void OnDied()
        {
            var state = GameServices.Instance.State;
            state.AddXp(xpReward);
            state.EarnScrap(scrapReward);
            foreach (var entry in loot)
                if (_random.Next01() < entry.chance) state.Inventory.TryAdd(entry.itemId);
            state.RegisterKill(questTargetId);

            if (_agent.isOnNavMesh) _agent.isStopped = true;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            Destroy(gameObject, 2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
