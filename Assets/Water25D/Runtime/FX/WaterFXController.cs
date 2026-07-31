using UnityEngine;

namespace Water25D.FX
{
    /// <summary>
    /// Converts logical water events into optional pooled presentation effects.
    /// Gameplay events are emitted by the controller independently of this component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaterFXController : MonoBehaviour
    {
        private WaterFXPool _splashPool;
        private WaterFXPool _bubblePool;
        private Water25DController _water;
        private WaterFXDefinition _configuredSplash;
        private WaterFXDefinition _configuredBubble;
        private int _configuredMaximumPoolSize;
        private bool _configuredEnableEffects;
        private bool _configured;

        internal void Configure(
            Water25DController water,
            bool enableEffects,
            WaterFXDefinition splashDefinition,
            WaterFXDefinition bubbleDefinition,
            int maximumPoolSize)
        {
            _water = water;
            if (!Application.isPlaying || !enableEffects)
            {
                DisposeRuntimeResources();
                return;
            }

            var safeMaximum = Mathf.Clamp(maximumPoolSize, 1, 64);
            if (_configured &&
                _configuredEnableEffects == enableEffects &&
                _configuredSplash == splashDefinition &&
                _configuredBubble == bubbleDefinition &&
                _configuredMaximumPoolSize == safeMaximum)
            {
                return;
            }

            DisposeRuntimeResources();
            _splashPool = new WaterFXPool(transform, splashDefinition, false, safeMaximum);
            _bubblePool = new WaterFXPool(transform, bubbleDefinition, true, safeMaximum);
            _configuredEnableEffects = enableEffects;
            _configuredSplash = splashDefinition;
            _configuredBubble = bubbleDefinition;
            _configuredMaximumPoolSize = safeMaximum;
            _configured = true;
        }

        internal void HandleInteraction(WaterInteractionEvent interaction)
        {
            if (!_configured || _water == null)
            {
                return;
            }

            var position = _water.GetInteractionWorldPosition(interaction.Position);
            var strength = Mathf.Clamp01(interaction.RippleStrength);
            switch (interaction.Type)
            {
                case WaterInteractionEventType.SurfaceEnter:
                case WaterInteractionEventType.SurfaceExit:
                    _splashPool?.Spawn(position, interaction.Velocity, strength);
                    break;
                case WaterInteractionEventType.Submerged:
                    _bubblePool?.Spawn(position, interaction.Velocity + Vector2.up * 0.25f, Mathf.Max(0.25f, strength));
                    break;
            }
        }

        internal void DisposeRuntimeResources()
        {
            _splashPool?.Dispose();
            _bubblePool?.Dispose();
            _splashPool = null;
            _bubblePool = null;
            _configured = false;
        }

        private void Update()
        {
            _splashPool?.Tick(Time.deltaTime);
            _bubblePool?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            DisposeRuntimeResources();
        }

        private void OnDestroy()
        {
            DisposeRuntimeResources();
        }
    }
}
