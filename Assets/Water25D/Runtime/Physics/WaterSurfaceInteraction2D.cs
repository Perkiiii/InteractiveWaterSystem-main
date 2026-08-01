using System.Collections.Generic;
using UnityEngine;

namespace Water25D
{
    /// <summary>
    /// Thin surface-crossing trigger. It tracks Rigidbody2D contacts rather than colliders
    /// so multi-collider characters generate one logical entry and exit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaterSurfaceInteraction2D : MonoBehaviour
    {
        private sealed class BodyContact
        {
            public Rigidbody2D Body;
            public Collider2D FirstCollider;
            public int ColliderCount;
        }

        private readonly List<BodyContact> _contacts = new List<BodyContact>(8);
        private Water25DController _water;
        private LayerMask _solidInteractionLayers;
        private LayerMask _triggerInteractionLayers;
        private bool _includeTriggerColliders;

        public int LogicalContactCount => _contacts.Count;

        internal void Configure(
            Water25DController water,
            LayerMask solidInteractionLayers,
            LayerMask triggerInteractionLayers,
            bool includeTriggerColliders)
        {
            _water = water;
            _solidInteractionLayers = solidInteractionLayers;
            _triggerInteractionLayers = triggerInteractionLayers;
            _includeTriggerColliders = includeTriggerColliders;
        }

        private void Awake()
        {
            if (_water == null)
            {
                _water = GetComponentInParent<Water25DController>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            var body = other.attachedRigidbody;
            if (body == null)
            {
                return;
            }

            var contact = FindContact(body);
            if (contact != null)
            {
                contact.ColliderCount++;
                return;
            }

            contact = new BodyContact
            {
                Body = body,
                FirstCollider = other,
                ColliderCount = 1
            };
            _contacts.Add(contact);

            var point = other.bounds.center;
            var velocity = body.linearVelocity;
            var strength = _water != null ? _water.CalculateImpactStrength(velocity) : 0f;
            var eventData = new WaterInteractionEvent(
                _water,
                body,
                other,
                new Vector2(point.x, point.y),
                velocity,
                strength,
                WaterInteractionEventType.SurfaceEnter);

            if (_water != null)
            {
                _water.CreateSurfaceImpactAt(
                    _water.GetInteractionWorldPosition(eventData.Position),
                    strength,
                    velocity.y >= 0f,
                    _water.GetImpactRadius(velocity));
                _water.NotifyInteraction(eventData);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!CanInteract(other))
            {
                return;
            }

            var body = other.attachedRigidbody;
            if (body == null)
            {
                return;
            }

            var contact = FindContact(body);
            if (contact == null)
            {
                return;
            }

            contact.ColliderCount--;
            if (contact.ColliderCount > 0)
            {
                return;
            }

            var point = other.bounds.center;
            var velocity = body.linearVelocity;
            var strength = _water != null ? _water.CalculateImpactStrength(velocity) : 0f;
            RemoveContact(contact);

            if (_water != null)
            {
                var eventData = new WaterInteractionEvent(
                    _water,
                    body,
                    other,
                    new Vector2(point.x, point.y),
                    velocity,
                    strength,
                    WaterInteractionEventType.SurfaceExit);
                _water.CreateSurfaceImpactAt(
                    _water.GetInteractionWorldPosition(eventData.Position),
                    strength,
                    velocity.y >= 0f,
                    _water.GetImpactRadius(velocity));
                _water.NotifyInteraction(eventData);
            }
        }

        private void OnDisable()
        {
            _contacts.Clear();
        }

        private bool CanInteract(Collider2D other)
        {
            if (!isActiveAndEnabled || _water == null || other == null)
            {
                return false;
            }

            if (other.isTrigger && !_includeTriggerColliders)
            {
                return false;
            }

            var mask = other.isTrigger ? _triggerInteractionLayers : _solidInteractionLayers;
            return (mask.value & (1 << other.gameObject.layer)) != 0;
        }

        private BodyContact FindContact(Rigidbody2D body)
        {
            for (var i = 0; i < _contacts.Count; i++)
            {
                if (_contacts[i].Body == body)
                {
                    return _contacts[i];
                }
            }

            return null;
        }

        private void RemoveContact(BodyContact contact)
        {
            var index = _contacts.IndexOf(contact);
            if (index < 0)
            {
                return;
            }

            var lastIndex = _contacts.Count - 1;
            _contacts[index] = _contacts[lastIndex];
            _contacts.RemoveAt(lastIndex);
        }
    }
}
