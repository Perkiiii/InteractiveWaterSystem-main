using UnityEngine;

namespace Water25D.FX
{
    /// <summary>
    /// Immutable authoring data for one pooled water effect. The prefab is optional;
    /// a package-owned particle fallback is created when no project prefab is assigned.
    /// </summary>
    [CreateAssetMenu(fileName = "WaterFXDefinition", menuName = "Water 2.5D/FX Definition")]
    public sealed class WaterFXDefinition : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        [Min(1)] [SerializeField] private int _prewarmCount = 8;
        [Min(0.05f)] [SerializeField] private float _lifetime = 0.7f;
        [Min(0.01f)] [SerializeField] private float _size = 0.18f;
        [Min(0.01f)] [SerializeField] private float _speed = 1.4f;
        [SerializeField] private Color _color = new Color(0.78f, 0.95f, 1f, 0.85f);

        public GameObject Prefab => _prefab;
        public int PrewarmCount => Mathf.Max(1, _prewarmCount);
        public float Lifetime => Mathf.Max(0.05f, _lifetime);
        public float Size => Mathf.Max(0.01f, _size);
        public float Speed => Mathf.Max(0.01f, _speed);
        public Color Color => _color;

        private void OnValidate()
        {
            _prewarmCount = Mathf.Max(1, _prewarmCount);
            _lifetime = Mathf.Max(0.05f, _lifetime);
            _size = Mathf.Max(0.01f, _size);
            _speed = Mathf.Max(0.01f, _speed);
        }
    }
}
