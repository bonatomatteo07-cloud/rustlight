using Rustlight.Core.Combat;
using Rustlight.Runtime.Combat;
using Rustlight.Runtime.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rustlight.Runtime.Player
{
    /// <summary>
    /// Attack button with soft auto-aim: hits the nearest living enemy in range and turns to face it.
    /// Bound to Space / gamepad South; on phones add an "On-Screen Button" with Control Path &lt;Gamepad&gt;/buttonSouth.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float cooldown = 0.45f;
        [SerializeField] private float skillPower = 1f;
        [SerializeField] private LayerMask enemyLayers = ~0;

        private readonly Collider[] _hits = new Collider[16];
        private readonly IRandom _random = new SystemRandom();
        private InputAction _attack;
        private float _nextAttackTime;

        private void Awake()
        {
            _attack = new InputAction("Attack", InputActionType.Button);
            _attack.AddBinding("<Keyboard>/space");
            _attack.AddBinding("<Gamepad>/buttonSouth");
            _attack.performed += _ => TryAttack();
        }

        private void OnEnable() => _attack.Enable();
        private void OnDisable() => _attack.Disable();
        private void OnDestroy() => _attack.Dispose();

        /// <summary>Also callable from a UI Button's OnClick.</summary>
        public void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;
            var target = FindNearestEnemy();
            if (target == null) return;

            _nextAttackTime = Time.time + cooldown;
            Vector3 look = target.transform.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(look);

            var stats = GameServices.Instance.State.PlayerStats;
            target.Health.TakeHit(DamageCalculator.Compute(stats, target.Health.Stats, skillPower, _random));
        }

        private Enemy FindNearestEnemy()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, attackRange, _hits, enemyLayers,
                QueryTriggerInteraction.Ignore);
            Enemy best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var enemy = _hits[i].GetComponentInParent<Enemy>();
                if (enemy == null || enemy.Health.IsDead) continue;
                float d = (enemy.transform.position - transform.position).sqrMagnitude;
                if (d < bestDistance) { bestDistance = d; best = enemy; }
            }
            return best;
        }
    }
}
