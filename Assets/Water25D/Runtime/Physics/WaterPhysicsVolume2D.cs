using System.Collections.Generic;
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
        private sealed class BodyContact
        {
            public Rigidbody2D Body;
            public int ColliderCount;
        }

        private readonly List<BodyContact> _contacts = new List<BodyContact>(8);
        private Water25DController _water;
        private LayerMask _interactionLayers;
        private bool _customDragEnabled;
        private float _customLinearDrag;
        private float _customAngularDrag;

        public int LogicalContactCount => _contacts.Count;

        internal void Configure(
            Water25DController water,
            LayerMask interactionLayers,
            bool customDragEnabled,
            float customLinearDrag,
            float customAngularDrag)
        {
            _water = water;
            _interactionLayers = interactionLayers;
            _customDragEnabled = customDragEnabled;
            _customLinearDrag = Mathf.Max(0f, customLinearDrag);
            _customAngularDrag = Mathf.Max(0f, customAngularDrag);
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
            if (!_customDragEnabled || _contacts.Count == 0)
            {
                return;
            }

            for (var i = _contacts.Count - 1; i >= 0; i--)
            {
                var body = _contacts[i].Body;
                if (body == null)
                {
                    _contacts.RemoveAt(i);
                    continue;
                }

                if (body.bodyType != RigidbodyType2D.Dynamic)
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

            _contacts.Add(new BodyContact { Body = body, ColliderCount = 1 });
            if (_water != null)
            {
                var point = other.bounds.center;
                var velocity = body.linearVelocity;
                _water.NotifyInteraction(new WaterInteractionEvent(
                    _water,
                    body,
                    other,
                    new Vector2(point.x, point.y),
                    velocity,
                    0f,
                    WaterInteractionEventType.Submerged));
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

            RemoveContact(contact);
            if (_water != null)
            {
                var point = other.bounds.center;
                var velocity = body.linearVelocity;
                _water.NotifyInteraction(new WaterInteractionEvent(
                    _water,
                    body,
                    other,
                    new Vector2(point.x, point.y),
                    velocity,
                    0f,
                    WaterInteractionEventType.Resurfaced));
            }
        }

        private void OnDisable()
        {
            _contacts.Clear();
        }

        private bool CanInteract(Collider2D other)
        {
            return isActiveAndEnabled && other != null &&
                   (_interactionLayers.value & (1 << other.gameObject.layer)) != 0;
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
