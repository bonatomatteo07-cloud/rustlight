using Rustlight.Core.Stats;
using Rustlight.Runtime.Combat;
using Rustlight.Runtime.Services;
using UnityEngine;
using Rustlight.Runtime.Input;

namespace Rustlight.Runtime.Player
{
    /// <summary>
    /// Camera-relative movement with a CharacterController, driven by <see cref="GameInput.Move"/>
    /// (WASD or the gamepad left stick, both rebindable). Speed comes from the MoveSpeed stat,
    /// so gear like servo boots applies automatically.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float turnSpeedDegrees = 720f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Transform respawnPoint;

        private CharacterController _controller;
        private float _verticalVelocity;

        public Health Health { get; private set; }

        private void Awake()
        {
            Instance = this;
            _controller = GetComponent<CharacterController>();
            Health = GetComponent<Health>();
            Health.Init(GameServices.Instance.State.PlayerStats);
            Health.Died += Respawn;
            GameServices.Instance.State.LeveledUp += OnLeveledUp;

            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void OnDestroy()
        {
            if (GameServices.Instance != null) GameServices.Instance.State.LeveledUp -= OnLeveledUp;
            if (Instance == this) Instance = null;
        }

        private void OnLeveledUp(int newLevel) => Health.RefillAndRevive();

        private void Update()
        {
            if (Health.IsDead) return;

            Vector2 input = Vector2.ClampMagnitude(GameInput.Move.ReadValue<Vector2>(), 1f);
            Vector3 direction = CameraRelative(input);
            float speed = GameServices.Instance.State.PlayerStats.Get(StatType.MoveSpeed);

            _verticalVelocity = _controller.isGrounded ? -2f : _verticalVelocity + gravity * Time.deltaTime;
            Vector3 velocity = direction * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (direction.sqrMagnitude > 0.001f)
            {
                var target = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeedDegrees * Time.deltaTime);
            }
        }

        private Vector3 CameraRelative(Vector2 input)
        {
            if (cameraTransform == null) return new Vector3(input.x, 0f, input.y);
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            return forward * input.y + right * input.x;
        }

        /// <summary>Death is not a fail state: back to the respawn point with full health.</summary>
        private void Respawn()
        {
            if (respawnPoint != null)
            {
                _controller.enabled = false; // CharacterController overrides direct position changes
                transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
                _controller.enabled = true;
            }
            Health.RefillAndRevive();
            GameServices.Instance.SaveNow();
        }
    }
}
