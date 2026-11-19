using UnityEngine;

namespace Rustlight.Runtime.Player
{
    /// <summary>Three-quarter top-down follow camera with smoothing; frame-rate independent.</summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 11f, -8f);
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
            transform.rotation = Quaternion.LookRotation(target.position - transform.position + Vector3.up, Vector3.up);
        }
    }
}
