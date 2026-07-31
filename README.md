# Interactive Water Systems

A Unity 6 sample project containing two independent interactive 2D water implementations side by side:

1. `InteractiveWater` — a custom 2D/2.5D system built around Shader Graph, Custom Render Textures, generated meshes, and planar reflections.
2. `Cainos Interactive Pixel Water` — a pixel-art water system with collision-driven waves, distortion, underwater effects, bubbles, splashes, and buoyancy-oriented interaction.

> **Before publishing:** this project includes third-party Cainos assets under `Assets/Cainos`. Check your redistribution rights before making the repository public. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## At a glance

| System | Source | Demo scene | Main strengths |
| --- | --- | --- | --- |
| Custom `InteractiveWater` | `Assets/InteractiveWaterSystem` | `Assets/DemoScenes/Demo_WithLighting/Demo_WithLighting.unity` | Ambient waves, contact ripples, caustics, distortion, 2D lights, depth clipping, and planar reflections |
| Cainos Interactive Pixel Water | `Assets/Cainos/Interactive Pixel Water` | `Assets/Cainos/Interactive Pixel Water/Scene/SC Demo.unity` | Pixel-art surface waves, collision interaction, underwater tint, distortion, blur, light shafts, bubbles, and splash FX |

## Requirements

- Unity `6000.0.23f1` (Unity 6)
- Universal Render Pipeline `17.0.3`
- A graphics-capable desktop target supported by the selected Unity version

The package manifest and lock file are committed, so Unity Package Manager can restore the project dependencies from `Packages/manifest.json` and `Packages/packages-lock.json`.

## Quick start

1. Clone the repository.
2. Add the repository root to Unity Hub and open it with Unity `6000.0.23f1`.
3. Let Unity finish importing assets and resolving packages.
4. Open one of the demo scenes below and press Play.

### Demo scenes

- `Assets/DemoScenes/Demo_WithLighting/Demo_WithLighting.unity` — the default build scene and the full custom-system showcase.
- `Assets/DemoScenes/Demo_WaterOnly/Demo_WaterOnly.unity` — the custom system with a simpler presentation.
- `Assets/Cainos/Interactive Pixel Water/Scene/SC Demo.unity` — the Cainos interactive pixel-water showcase.
- `Assets/Cainos/Pixel Art Platformer - Village Props/Scene/SC Demo Scene - Village Props.unity` — supporting Cainos art content; it is not a third water implementation.

Only `Demo_WithLighting` is currently in `ProjectSettings/EditorBuildSettings.asset`. Open the other scenes directly in the Editor, or add them to Build Settings if you want them in a player build.

## Custom `InteractiveWater`

The custom system creates two generated meshes:

- A **top mesh** on the XZ plane for the water surface, waves, ripples, and reflections.
- A **front mesh** on the XY plane for caustics and distortion over sprites behind the water.

### Features

- GPU-driven ambient surface waves using `CRT_AmbientWave.shadergraph`.
- Contact-ripple simulation using `CRT_RippleSimulation.shader` and a Custom Render Texture.
- Configurable caustics and distortion in `FrontMesh.shadergraph`.
- Real-time planar reflections rendered by `SimplePlanarReflection.cs`.
- Compatibility with URP 2D lights through Sprite Lit materials.
- Depth clipping for sprites and meshes intersecting the water surface.
- Optional mouse-driven test ripples in the demo scene.

The core implementation is in:

```text
Assets/InteractiveWaterSystem/Scripts/InteractiveWater.cs
Assets/InteractiveWaterSystem/Scripts/SimplePlanarReflection.cs
Assets/InteractiveWaterSystem/ArtAssets/Shaders/Water/
Assets/DemoScenes/
```

To create a ripple from gameplay code, call the public method on the `InteractiveWater` component:

```csharp
using InteractiveWater;
using UnityEngine;

public class WaterImpactExample : MonoBehaviour
{
    [SerializeField] private InteractiveWater water;

    public void MakeRipple(Vector3 worldPosition)
    {
        water.CreateContactRippleAt(worldPosition, initialStrength: 0.5f);
    }
}
```

### Scene configuration

The custom system depends on project-level settings as well as the component itself:

- Layers `WaterSystem_Water` and `WaterSystem_Reflections` are defined in `ProjectSettings/TagManager.asset`.
- Sorting layers `WaterTopMesh` and `WaterFrontMesh` control the water render order.
- The 2D renderer has its Camera Sorting Layer Texture enabled and bounded at the water layers.
- A Global Volume is present in the lighting demo so URP 2D lights render correctly.

When moving the system into another project, reproduce these settings or adapt the component references and sorting-layer order to your own scene.

## Cainos Interactive Pixel Water

The Cainos implementation is a separate package-style system under `Assets/Cainos/Interactive Pixel Water`. Its `PixelWater` component generates the surface mesh and exposes inspector controls for:

- Collision and trigger interaction masks.
- Surface and underwater color/tint.
- Distortion, blur, light shafts, and ambient waves.
- Collision-driven waves and drag/buoyancy behavior.
- Bubble, splash, spark, and in-water particle effects.

Useful entry points include:

```text
Assets/Cainos/Interactive Pixel Water/Script/PixelWater.cs
Assets/Cainos/Interactive Pixel Water/Script/PixelWaterBubble.cs
Assets/Cainos/Interactive Pixel Water/Script/PixelWaterSplash.cs
Assets/Cainos/Interactive Pixel Water/Scene/SC Demo.unity
```

The included vendor documentation links to the [Cainos Interactive Pixel Water documentation](https://docs.cainos.net/interactive-pixel-water), including the [script reference](https://docs.cainos.net/interactive-pixel-water/script-reference).

## Screenshots and video

The custom system's original showcase material is included below. The video and playable demo were made for the lighting demo scene.

[Video showcase](https://youtu.be/nht-2tldh_Q) · [Playable demo](https://spookyfish.itch.io/interactive-water-system)

![Custom water system with URP 2D lighting](https://github.com/user-attachments/assets/c10eb81c-4c3c-4f5c-a34a-5a902e6926ed)

![Custom water system contact ripples](https://github.com/user-attachments/assets/04aac7ae-3f79-4ed9-9ede-f8dabeabc1b7)

![Custom water system planar reflections](https://github.com/user-attachments/assets/42e741f3-0d71-41fd-a026-3322ceee5a18)

## Repository layout

```text
Assets/
├── Cainos/                  Third-party pixel-water and supporting art assets
├── DemoScenes/              Demos for the custom InteractiveWater system
├── InteractiveWaterSystem/  Custom scripts, shaders, materials, and render textures
└── Settings/                URP renderer and 2D scene-template assets
Packages/                    Unity package manifest and lock file
ProjectSettings/             Unity version, layers, sorting layers, and build settings
```

Generated Unity folders such as `Library`, `Temp`, `Logs`, `obj`, `Build`, and `UserSettings` are intentionally ignored by Git. Unity regenerates them when the project is opened.

## Known status

This is a demonstration and experimentation project rather than a packaged Unity UPM module. The custom system's earlier documentation identifies planar-reflection normals and wave-projected distortion as areas for future refinement.

The optional Cainos `.unitypackage` patch archives are not tracked by this repository's Unity ignore rules. The already-imported project assets remain in the repository, but obtaining those vendor patch archives may be necessary when adapting the Cainos content to another render-pipeline or Unity-version setup.

## Licensing and attribution

This is a mixed-content repository, so no project-wide license is declared yet. Review [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) before publishing or reusing the assets. In particular, do not assume that the Cainos package, its supporting art, or the custom water-system code all share the same license.
