using Rustlight.Core.Combat;
using Rustlight.Runtime.Combat;
using Rustlight.Runtime.Input;
using Rustlight.Runtime.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Rustlight.Runtime.Player
{
    /// <summary>
    /// Melee attack on <see cref="GameInput.Attack"/>. With mouse and keyboard the swing goes toward the
    /// cursor and hits the enemy nearest to it inside a cone; with a gamepad it auto-targets the nearest enemy.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField, Range(10f, 180f)] private float aimConeDegrees = 70f;
        [SerializeField] private float cooldown = 0.45f;
        [SerializeField] private float skillPower = 1f;
        [SerializeField] private LayerMask enemyLayers = ~0;

        private readonly Collider[] _hits = new Collider[16];
        private readonly IRandom _random = new SystemRandom();
        private Camera _camera;
        private float _nextAttackTime;

        private void Awake() => _camera = Camera.main;
        private void OnEnable() => GameInput.Attack.performed += OnAttack;
        private void OnDisable() => GameInput.Attack.performed -= OnAttack;

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            bool fromMouse = ctx.control.device is Mouse;
            if (fromMouse && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            bool aimAtCursor = !(ctx.control.device is Gamepad) && _camera != null;
            TryAttack(aimAtCursor);
        }

        /// <summary>Swings if off cooldown; returns true if an enemy was hit.</summary>
        public bool TryAttack(bool aimAtCursor)
        {
            if (Time.time < _nextAttackTime || GetComponent<PlayerController>().Health.IsDead) return false;
            _nextAttackTime = Time.time + cooldown;

            Vector3? aim = aimAtCursor ? CursorDirection() : null;
            if (aim.HasValue) transform.rotation = Quaternion.LookRotation(aim.Value);

            var target = FindTarget(aim);
            if (target == null) return false;

            if (!aim.HasValue)
            {
                Vector3 look = target.transform.position - transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(look);
            }
            var stats = GameServices.Instance.State.PlayerStats;
            target.Health.TakeHit(DamageCalculator.Compute(stats, target.Health.Stats, skillPower, _random));
            return true;
        }

        /// <summary>Flat direction from the player to where the cursor meets the ground plane.</summary>
        private Vector3? CursorDirection()
        {
            var ray = _camera.ScreenPointToRay(GameInput.Point.ReadValue<Vector2>());
            var plane = new Plane(Vector3.up, transform.position);
            if (!plane.Raycast(ray, out float enter)) return null;
            Vector3 dir = ray.GetPoint(enter) - transform.position;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.01f ? dir.normalized : (Vector3?)null;
        }

        /// <summary>Nearest living enemy in range; with an aim direction, only those inside the cone.</summary>
        private Enemy FindTarget(Vector3? aim)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, attackRange, _hits, enemyLayers,
                QueryTriggerInteraction.Ignore);
            Enemy best = null;
            float bestScore = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var enemy = _hits[i].GetComponentInParent<Enemy>();
                if (enemy == null || enemy.Health.IsDead) continue;
                Vector3 to = enemy.transform.position - transform.position;
                to.y = 0f;
                if (aim.HasValue && Vector3.Angle(aim.Value, to) > aimConeDegrees * 0.5f) continue;
                float score = to.sqrMagnitude;
                if (score < bestScore) { bestScore = score; best = enemy; }
            }
            return best;
        }
    }
}
