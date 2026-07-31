using System.Collections.Generic;
using UnityEngine;

namespace Water25D.FX
{
    /// <summary>
    /// Fixed-capacity effect pool. All entries are created during configuration;
    /// exhaustion rejects the request instead of instantiating during gameplay.
    /// </summary>
    public sealed class WaterFXPool : System.IDisposable
    {
        private sealed class Entry
        {
            public GameObject Root;
            public ParticleSystem Particles;
            public float Remaining;
            public bool Active;
        }

        private readonly Transform _owner;
        private readonly WaterFXDefinition _definition;
        private readonly bool _bubbleStyle;
        private readonly List<Entry> _entries;
        private Material _fallbackMaterial;
        private bool _disposed;

        public int Capacity => _entries.Count;
        public int ActiveCount { get; private set; }

        public WaterFXPool(Transform owner, WaterFXDefinition definition, bool bubbleStyle, int capacity)
        {
            _owner = owner;
            _definition = definition;
            _bubbleStyle = bubbleStyle;
            var safeCapacity = Mathf.Clamp(
                definition != null ? definition.PrewarmCount : capacity,
                1,
                Mathf.Max(1, capacity));
            _entries = new List<Entry>(safeCapacity);
            CreateFallbackMaterial();
            for (var i = 0; i < safeCapacity; i++)
            {
                _entries.Add(CreateEntry(i));
            }
        }

        public bool Spawn(Vector3 position, Vector2 velocity, float strength)
        {
            if (_disposed)
            {
                return false;
            }

            Entry entry = null;
            for (var i = 0; i < _entries.Count; i++)
            {
                if (!_entries[i].Active)
                {
                    entry = _entries[i];
                    break;
                }
            }

            if (entry == null)
            {
                return false;
            }

            var safeStrength = Mathf.Clamp01(strength);
            var lifetime = _definition != null ? _definition.Lifetime : (_bubbleStyle ? 1.2f : 0.55f);
            var size = (_definition != null ? _definition.Size : (_bubbleStyle ? 0.08f : 0.18f)) * (0.7f + safeStrength * 0.6f);
            var speed = (_definition != null ? _definition.Speed : (_bubbleStyle ? 0.45f : 1.4f)) * (0.75f + safeStrength * 0.5f);
            var color = _definition != null ? _definition.Color : new Color(0.78f, 0.95f, 1f, 0.85f);

            entry.Active = true;
            entry.Remaining = lifetime;
            ActiveCount++;
            entry.Root.transform.SetPositionAndRotation(position, GetRotation(velocity));
            entry.Root.SetActive(true);
            entry.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = entry.Particles.main;
            main.startLifetime = lifetime;
            main.startSize = size;
            main.startSpeed = speed;
            main.startColor = color;
            entry.Particles.Clear();
            entry.Particles.Play(false);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_disposed || ActiveCount == 0)
            {
                return;
            }

            var safeDeltaTime = Mathf.Max(0f, deltaTime);
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (!entry.Active)
                {
                    continue;
                }

                entry.Remaining -= safeDeltaTime;
                if (entry.Remaining <= 0f)
                {
                    Return(entry);
                }
            }
        }

        public void ReturnAll()
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Active)
                {
                    Return(_entries[i]);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            for (var i = 0; i < _entries.Count; i++)
            {
                DestroyOwnedObject(_entries[i].Root);
            }

            _entries.Clear();
            ActiveCount = 0;
            DestroyOwnedObject(_fallbackMaterial);
            _fallbackMaterial = null;
        }

        private Entry CreateEntry(int index)
        {
            GameObject root;
            if (_definition != null && _definition.Prefab != null)
            {
                root = Object.Instantiate(_definition.Prefab, _owner);
                root.name = "Water25D FX " + index;
            }
            else
            {
                root = new GameObject("Water25D FX " + index);
                root.transform.SetParent(_owner, false);
            }

            var particles = root.GetComponentInChildren<ParticleSystem>();
            if (particles == null)
            {
                particles = root.AddComponent<ParticleSystem>();
            }

            ConfigureParticles(particles);
            root.SetActive(false);
            return new Entry
            {
                Root = root,
                Particles = particles
            };
        }

        private void ConfigureParticles(ParticleSystem particles)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = _bubbleStyle ? 1.2f : 0.55f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _bubbleStyle ? 8 : 16;
            main.startLifetime = _bubbleStyle ? 1.2f : 0.55f;
            main.startSpeed = _bubbleStyle ? 0.45f : 1.4f;
            main.startSize = _bubbleStyle ? 0.08f : 0.18f;
            main.startColor = _definition != null ? _definition.Color : new Color(0.78f, 0.95f, 1f, 0.85f);

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(_bubbleStyle ? 6 : 10)) });

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = _bubbleStyle ? 12f : 35f;
            shape.radius = _bubbleStyle ? 0.03f : 0.12f;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = _fallbackMaterial;
            }
        }

        private void CreateFallbackMaterial()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return;
            }

            _fallbackMaterial = new Material(shader)
            {
                name = "Water25D FX Material (Runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void Return(Entry entry)
        {
            entry.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            entry.Root.SetActive(false);
            entry.Remaining = 0f;
            entry.Active = false;
            ActiveCount = Mathf.Max(0, ActiveCount - 1);
        }

        private static Quaternion GetRotation(Vector2 velocity)
        {
            if (velocity.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(Vector3.forward, new Vector3(velocity.x, velocity.y, 0f));
        }

        private static void DestroyOwnedObject(Object objectToDestroy)
        {
            if (objectToDestroy == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(objectToDestroy);
            }
            else
            {
                Object.DestroyImmediate(objectToDestroy);
            }
        }
    }
}
