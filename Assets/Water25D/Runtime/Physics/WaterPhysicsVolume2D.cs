using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Full underwater volume. Buoyancy is provided by BuoyancyEffector2D;
    /// optional custom drag is applied once per logical Rigidbody2D contact.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaterPhysicsVolume2D : MonoBehaviour
    {
        private readonly WaterLogicalBodyContactTracker _tracker = new WaterLogicalBodyContactTracker();
        private Water25DController _water;
        private LayerMask _interactionLayers;
        private bool _customDragEnabled;
        private float _customLinearDrag;
        private float _customAngularDrag;

        public int LogicalContactCount => _tracker.LogicalBodyCount;
        public int DroppedTrackedBodyCount => _tracker.DroppedBodyCount;
        public int ColliderSampleOverflowCount => _tracker.ColliderOverflowCount;

        internal void Configure(
            Water25DController water,
            LayerMask interactionLayers,
            bool customDragEnabled,
            float customLinearDrag,
            float customAngularDrag,
            int maximumTrackedBodies)
        {
            _water = water;
            _interactionLayers = interactionLayers;
            _customDragEnabled = customDragEnabled;
            _customLinearDrag = Mathf.Max(0f, customLinearDrag);
            _customAngularDrag = Mathf.Max(0f, customAngularDrag);
            _tracker.Configure(maximumTrackedBodies);
        }

        private void Awake()
        {
            if (_water == null)
            {
                _water = GetComponentInParent<Water25DController>();
            }
        }

        private void FixedUpdate()
        {
            if (_water == null || !_water.isActiveAndEnabled)
            {
                return;
            }

            _tracker.CleanupInvalid();
            if (!_customDragEnabled || _tracker.LogicalBodyCount == 0)
            {
                return;
            }

            for (var i = 0; i < _tracker.LogicalBodyCount; i++)
            {
                if (!_tracker.TryGetBodyAt(i, out var body) || body.bodyType != RigidbodyType2D.Dynamic)
                {
                    continue;
                }

                if (_customLinearDrag > 0f)
                {
                    body.AddForce(-body.linearVelocity * (_customLinearDrag * body.mass), ForceMode2D.Force);
                }

                if (_customAngularDrag > 0f && Mathf.Abs(body.angularVelocity) > 0.001f)
                {
                    body.AddTorque(-body.angularVelocity * (_customAngularDrag * body.inertia), ForceMode2D.Force);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            if (!_tracker.TryAdd(other, out var bodyBecameActive) || !bodyBecameActive || _water == null)
            {
                return;
            }

            var point = other.bounds.center;
            var velocity = other.attachedRigidbody.linearVelocity;
            _water.NotifyInteraction(new WaterInteractionEvent(
                _water,
                other.attachedRigidbody,
                other,
                new Vector2(point.x, point.y),
                velocity,
                0f,
                WaterInteractionEventType.Submerged));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            if (!_tracker.TryRemove(other, out var bodyBecameInactive) || !bodyBecameInactive || _water == null)
            {
                return;
            }

            var point = other.bounds.center;
            var velocity = other.attachedRigidbody != null ? other.attachedRigidbody.linearVelocity : Vector2.zero;
            _water.NotifyInteraction(new WaterInteractionEvent(
                _water,
                other.attachedRigidbody,
                other,
                new Vector2(point.x, point.y),
                velocity,
                0f,
                WaterInteractionEventType.Resurfaced));
        }

        private void OnDisable()
        {
            ClearContacts();
        }

        internal void ClearContacts()
        {
            _tracker.Clear();
        }

        private bool CanInteract(Collider2D other)
        {
            return isActiveAndEnabled && _water != null && _water.isActiveAndEnabled &&
                   other != null && other.attachedRigidbody != null &&
                   (_interactionLayers.value & (1 << other.gameObject.layer)) != 0;
        }
    }
}
