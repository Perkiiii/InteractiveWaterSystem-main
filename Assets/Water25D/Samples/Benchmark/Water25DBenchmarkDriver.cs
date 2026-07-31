using System;
using System.Collections.Generic;
using UnityEngine;

namespace Water25D.Samples.Benchmark
{
    /// <summary>
    /// Deterministic benchmark load generator. It uses fixed object placement and a fixed
    /// impact schedule so CRT, reflection and FX configurations can be compared fairly.
    /// </summary>
    public sealed class Water25DBenchmarkDriver : MonoBehaviour
    {
        [Header("Water Load")]
        [Min(1)] [SerializeField] private int _waterBodyCount = 4;
        [SerializeField] private Vector2 _waterSpacing = new Vector2(24f, 0f);
        [SerializeField] private Vector2 _waterSize = new Vector2(20f, 6.5f);
        [Min(0.1f)] [SerializeField] private float _physicalDepth = 10f;

        [Header("Object Load")]
        [Range(0, 64)] [SerializeField] private int _dynamicBodyCount = 10;
        [SerializeField] private Vector2 _objectSpacing = new Vector2(1.2f, 0.8f);

        [Header("Deterministic Schedule")]
        [SerializeField] private int _randomSeed = 1337;
        [Min(1)] [SerializeField] private int _impactIntervalFrames = 30;
        [Range(1, 32)] [SerializeField] private int _impactsPerInterval = 1;
        [SerializeField] private bool _buildOnStart = true;

        private readonly List<Water25DController> _waters = new List<Water25DController>(32);
        private Transform _generatedRoot;
        private System.Random _random;
        private int _frame;

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildBenchmark();
            }
        }

        private void Update()
        {
            if (_waters.Count == 0)
            {
                return;
            }

            _frame++;
            var interval = Mathf.Max(1, _impactIntervalFrames);
            if (_frame % interval != 0)
            {
                return;
            }

            for (var impactIndex = 0; impactIndex < Mathf.Max(1, _impactsPerInterval); impactIndex++)
            {
                for (var waterIndex = 0; waterIndex < _waters.Count; waterIndex++)
                {
                    var water = _waters[waterIndex];
                    var u = (float)_random.NextDouble();
                    var v = (float)_random.NextDouble();
                    var localPosition = new Vector3(u * water.TopSurfaceSize.x, water.WaterlineLocalY, v * water.TopSurfaceSize.y);
                    water.CreateContactRippleAt(water.transform.TransformPoint(localPosition), 0.35f, (impactIndex & 1) == 0);
                }
            }
        }

        [ContextMenu("Build Deterministic Benchmark")]
        public void BuildBenchmark()
        {
            ClearGenerated();
            _random = new System.Random(_randomSeed);
            _frame = 0;
            _generatedRoot = new GameObject("Water25D Benchmark Generated").transform;
            _generatedRoot.SetParent(transform, false);
            _waters.Clear();

            var waterCount = Mathf.Clamp(_waterBodyCount, 1, 32);
            for (var i = 0; i < waterCount; i++)
            {
                var waterObject = new GameObject("Water25D Benchmark Water " + i);
                waterObject.transform.SetParent(_generatedRoot, false);
                waterObject.transform.localPosition = new Vector3(_waterSpacing.x * i, _waterSpacing.y * i, 0f);
                var water = waterObject.AddComponent<Water25DController>();
                water.SetDimensions(_waterSize, _physicalDepth);
                _waters.Add(water);
            }

            var bodyCount = Mathf.Clamp(_dynamicBodyCount, 0, 64);
            for (var i = 0; i < bodyCount; i++)
            {
                var bodyObject = new GameObject("Water25D Benchmark Body " + i);
                bodyObject.transform.SetParent(_generatedRoot, false);
                bodyObject.transform.localPosition = new Vector3(
                    (i % 8) * _objectSpacing.x + 1f,
                    2f + (i / 8) * _objectSpacing.y,
                    0f);
                var body = bodyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 1f;
                bodyObject.AddComponent<CircleCollider2D>();
            }
        }

        [ContextMenu("Clear Generated Benchmark Objects")]
        public void ClearGenerated()
        {
            if (_generatedRoot == null)
            {
                return;
            }

            DestroyOwnedObject(_generatedRoot.gameObject);
            _generatedRoot = null;
            _waters.Clear();
        }

        private void OnDestroy()
        {
            ClearGenerated();
        }

        private static void DestroyOwnedObject(UnityEngine.Object objectToDestroy)
        {
            if (objectToDestroy == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(objectToDestroy);
            }
            else
            {
                DestroyImmediate(objectToDestroy);
            }
        }
    }
}
