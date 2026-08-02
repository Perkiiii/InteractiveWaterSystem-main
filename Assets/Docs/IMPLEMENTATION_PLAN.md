# Technical Review and Implementation Plan for an Improved Unity 2.5D Water System

> **Document status:** Architectural plan and implementation roadmap. This file describes the intended destination, delivery order, and acceptance criteria. It does not record current implementation progress.

## Document role

This document defines the target architecture, implementation phases, validation strategy, migration approach, and release criteria for a new self-contained water package under:

```text
Assets/Water25D/
```

The new package is developed inside the same Unity project as the two reference systems, but it must not become permanently dependent on either of them. The existing custom water and Cainos water remain preserved as baselines and behavioural references until migration and comparison work is complete.

Implementation progress is tracked separately in:

```text
Assets/Water25D/Documentation/STATUS.md
```

Create that status file when Phase 0 begins. Do not use this implementation plan as a task log, checklist of claimed completions, or substitute for validation evidence.

Performance numbers in this document are analytical estimates unless explicitly identified as measurements captured with Unity profiling tools.

## Naming convention

Use **2.5D Water** as the human-facing product and feature name. Use **`Water25D`** as the single technical identifier everywhere a dot would be invalid, awkward, or inconsistent.

| Context | Required name |
|---|---|
| Product or feature name in prose | `2.5D Water` |
| Package root | `Assets/Water25D/` |
| Root namespace | `Water25D` |
| Runtime assembly | `Water25D.Runtime` |
| Editor assembly | `Water25D.Editor` |
| EditMode test assembly | `Water25D.Tests.EditMode` |
| PlayMode test assembly | `Water25D.Tests.PlayMode` |
| Primary component | `Water25DController` |
| Custom Inspector | `Water25DEditor` |
| Root prefab or authored hierarchy name | `Water25D` |

Naming rules:

- Do not introduce alternate technical spellings of the package name.
- Keep ordinary module names concise where the package context is already clear, such as `WaterMeshBuilder`, `WaterRuntimeResources`, `WaterReflectionManager`, and `WaterStyleProfile`.
- Use the `Water25D` prefix where package identity, assembly identity, a primary public type, or collision avoidance benefits from it.
- New runtime code belongs in the `Water25D` namespace or a nested namespace such as `Water25D.Simulation`, `Water25D.Physics`, `Water25D.Rendering`, `Water25D.FX`, or `Water25D.Settings`.
- Editor code belongs in `Water25D.Editor` or a nested editor namespace.
- Repository documentation, `AGENTS.md`, assembly definitions, test names, sample names, status files, and migration instructions must use the same technical identifier.
- Renaming an already serialized Unity type later can break component references. Finalize these names before Phase 1 creates authored prefabs or scenes.

## Sources of truth and maintenance rules

When information conflicts, use the following precedence:

1. The current implementation task.
2. The repository root `AGENTS.md`.
3. This implementation plan for architectural intent and phase order.
4. `Assets/Water25D/Documentation/STATUS.md` for completed work, active phase, known issues, and temporary dependencies.
5. Actual repository code and serialized project settings for current implementation state.
6. README files and older explanatory documentation.

The following files are authoritative for environment versions:

```text
ProjectSettings/ProjectVersion.txt
Packages/manifest.json
Packages/packages-lock.json
```

Do not hard-code Unity, URP, Input System, Test Framework, or other package version numbers into this plan. Read the installed versions from those files whenever compatibility matters.

Update this plan only when architecture, phase order, public contracts, acceptance criteria, or package boundaries change. Routine implementation progress belongs in `STATUS.md`.

## Current Flat-Stylized Production Track

The original 13-phase roadmap below remains preserved as architectural history. The authoritative remaining implementation order is the four-phase flat-stylized production track summarized here and specified in detail in [`Assets/Water25D/Documentation/Design/FLAT_STYLIZED_IMPLEMENTATION_PLAN.md`](../Water25D/Documentation/Design/FLAT_STYLIZED_IMPLEMENTATION_PLAN.md). The resolved visual direction is recorded in [`Assets/Water25D/Documentation/Design/FLAT_STYLIZED_DESIGN_BRIEF.md`](../Water25D/Documentation/Design/FLAT_STYLIZED_DESIGN_BRIEF.md). Routine progress, completed milestones, and test evidence belong only in [`Assets/Water25D/Documentation/STATUS.md`](../Water25D/Documentation/STATUS.md).

Existing serialized controllers remain compatible with `SimulatedRipples`; genuinely new controllers use `FlatStylized`. In `FlatStylized`, the top remains geometrically flat and the Custom Render Texture (CRT) is neither allocated nor ticked. Completed foundation work includes flat/simulated routing; four-vertex, two-triangle flat-top geometry; fixed-capacity procedural rings; qualified downward and upward waterline crossings; shared fixed-capacity logical-body tracking; and flat-only, body-keyed contact foam. Fixed-capacity distance-spaced wakes are the next bounded implementation task.

The remaining production phases are:

1. **Wake completion and interaction validation** — Add fixed-capacity distance-spaced wakes with frame-rate-independent spacing, thresholds, a defined reversal policy, fading, and deterministic capacity handling. Render them analytically on the top and front, with tests and strict mode isolation.
2. **Rendering ownership and painterly interactions** — Make `WaterRenderingModule` the sole final `MaterialPropertyBlock` writer. Add optional ring, foam, and wake mask atlases with stable variants and rotations while preserving analytical fallbacks; final artwork is not required in this phase.
3. **Stylized water and reflection presentation** — Add shallow/deep colour, scrolling normal and detail layers, Fresnel response, stylized highlights, foam, refraction, and reflection treatment; add front distortion, caustics, shafts, and flow; and support shared planar plus camera-free stylized reflections. `FlatStylized` must not use vertex displacement.
4. **FX, tooling, migration, and production validation** — Add pooled splash variants, quality tiers, Inspector and validation tooling, explicit reversible migration, and profiling, lifecycle, clean-import, and packaging validation.

### Approved development references

The following are development references only:

- [Ameye's Stylized Water Shader](https://ameye.dev/notes/stylized-water-shader/) for colour, Fresnel, normal, foam, refraction, caustic, and reflection presentation.
- [Minions Art — Shader Graph Interactive Water](https://www.patreon.com/minionsart/posts/shader-graph-30490169) for painterly ripple, mask, distortion, edge-foam, and depth-colour ideas.
- [Unity — How to Make Nature Shaders with Shader Graph in 2022 LTS](https://unity.com/blog/engine-platform/nature-shaders-with-shader-graph-in-2022-lts) for the Japanese-garden stream's restrained motion, flow-line, edge-highlight, ripple, foam, and reflection composition.

Water25D reimplements selected concepts using project-owned C#, HLSL, shaders, profiles, tests, and artwork. These references are not runtime, editor, serialized, or package dependencies. Access to a source asset does not by itself grant redistribution permission. Third-party source code, Shader Graphs, scripts, textures, materials, particles, prefabs, and pipeline assets must not be distributed without explicit licensing approval.

### Repository evidence map

The principal reference files are:

```text
Assets/InteractiveWaterSystem/Scripts/InteractiveWater.cs
Assets/InteractiveWaterSystem/Scripts/SimplePlanarReflection.cs
Assets/InteractiveWaterSystem/ArtAssets/Shaders/Water/
Assets/InteractiveWaterSystem/ArtAssets/Materials/Water/
Assets/Cainos/Interactive Pixel Water/Script/PixelWater.cs
Assets/Cainos/Interactive Pixel Water/Script/PixelWaterBubble.cs
Assets/Cainos/Interactive Pixel Water/Script/PixelWaterSplash.cs
Assets/Cainos/Interactive Pixel Water/Editor/PixelWaterEditor.cs
Assets/Settings/Renderer2D.asset
Assets/Settings/UniversalRP.asset
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/ProjectVersion.txt
Packages/manifest.json
THIRD_PARTY_NOTICES.md
```

The exact filenames present in the repository remain the source of truth. If a listed path changes, update this evidence map and the relevant migration instructions.

## Scope, boundaries, and non-goals

### In scope

- Preserve the custom system's XZ top surface and XY front-water presentation.
- Build a project-owned modular runtime, editor, simulation, physics, reflection, interaction, FX, profile, migration, and validation framework.
- Keep visual GPU ripple simulation separate from flat 2D gameplay buoyancy.
- Provide a production path that scales across multiple water bodies and quality tiers.
- Make the result self-contained enough to move into another compatible Unity project with documented setup steps.
- Preserve the reference systems until the new package passes comparison, migration, and acceptance tests.

### Out of scope unless explicitly approved

- Combining the custom controller and Cainos controller into one inherited or partially copied class.
- Copying Cainos scripts, shaders, textures, prefabs, particle systems, editor tooling, or other vendor content into `Assets/Water25D/`.
- Rewriting the entire system in one change.
- Making the compute backend the first production implementation.
- Using GPU-to-CPU height readback as the default gameplay buoyancy model.
- Requiring Unity MCP, Lucid Editor, demo-scene assets, or either reference system at package runtime.
- Deleting or destructively converting the original prefabs before migration acceptance.
- Claiming profiler, rendering, compilation, or test results that were not actually produced.

### Package independence target

The completed `Assets/Water25D/` tree must not contain permanent C# references, serialized GUID references, shader dependencies, material dependencies, prefab dependencies, or hard-coded asset paths into:

```text
Assets/InteractiveWaterSystem/
Assets/Cainos/
Assets/DemoScenes/
Assets/Cainos/Third Party/Lucid Editor/
```

Temporary migration or comparison adapters are allowed only when the active task explicitly requires them. Record each temporary dependency in `STATUS.md`, explain why it exists, and state the phase in which it will be removed.

Project-level settings such as layers, sorting layers, renderer configuration, or Camera Sorting Layer Texture setup may still be required. Those requirements must be validated and documented rather than silently assumed.

## Executive assessment and architecture map

This plan reviews the custom and Cainos water implementations in `Perkiiii/InteractiveWaterSystem-main`, analyses their likely performance characteristics, and defines a production-oriented hybrid for a Unity 6 URP side-view game.

The custom water is built around two generated meshes, two Custom Render Textures, Shader Graph materials, a surface interaction trigger, and a manually rendered reflection camera. The Cainos package is a separate CPU spring-water implementation with a substantially more mature authoring, physics, interaction, and FX layer.

The central recommendation is:

> **Retain the custom system’s two-plane 2.5D presentation, but replace its monolithic controller with modular rendering, simulation, physics, interaction, reflection, FX, profile, and editor systems. Optimise the existing Custom Render Texture solver before committing to a compute-shader rewrite.**

The best target architecture is:

```text
Water25D
├── Water25DController
├── TopSurface
│   ├── MeshFilter
│   ├── MeshRenderer
│   └── WaterVisibilityReporter
├── FrontSurface
│   ├── MeshFilter
│   └── MeshRenderer
├── SurfaceCrossingTrigger
│   ├── BoxCollider2D
│   └── WaterSurfaceInteraction2D
├── BuoyancyVolume
│   ├── BoxCollider2D
│   ├── BuoyancyEffector2D
│   └── WaterPhysicsVolume2D
├── ReflectionAnchor
│   └── registered with WaterReflectionManager
└── FXRoot
    └── WaterFXController
```

The retained visual model would remain:

```text
XZ top plane
    GPU contact ripples
    analytical ambient waves
    foam / edge treatment
    optional planar reflection
            │
            ▼
front seam at the waterline
            │
            ▼
XY front plane
    underwater tint
    scene distortion
    caustics
    depth fade
    bubbles and underwater particles
```

The gameplay and presentation layers should be deliberately decoupled:

```text
GPU height field
    controls visible wave displacement only

Flat 2D buoyancy surface
    controls Rigidbody2D gameplay

Surface-crossing trigger
    creates ripples, splashes and events

Full underwater trigger
    controls buoyancy, drag, bubbles and submerged state
```

This decoupling avoids GPU-to-CPU readback, preserves responsive 2D physics, and prevents visual resolution from determining gameplay behaviour. Unity’s `BuoyancyEffector2D` already exposes a flat surface level, density, flow, linear drag, angular drag and collider filtering, making it appropriate for a side-view game whose actual physics remain two-dimensional.

### Recommended implementation decision

| Area | Recommendation |
|---|---|
| Contact-ripple backend | Begin with an optimised Custom Render Texture backend |
| Long-term optional backend | Explicit compute-shader ping-pong, behind a shared interface |
| Ambient waves | Remove the ambient Custom Render Texture and evaluate the waves analytically |
| Buoyancy | Flat `BuoyancyEffector2D` surface, independent of visual ripple height |
| Surface interaction | Dedicated thin trigger with velocity-sensitive impact generation |
| Underwater interaction | Separate full volume for buoyancy, drag and submerged state |
| Reflections | One adaptive reflection render per compatible water-plane group |
| Low-quality reflection | Stylised gradient or screen-derived approximation |
| Materials | Template assets plus per-instance runtime ownership where values differ |
| Ripple textures | Runtime-owned, rectangular, world-relative, no mipmaps |
| Inspector | Project-owned custom editor with grouped panels and scene handles |
| FX | Pooled splash, bubble and particle instances |
| Profiles | Shared immutable style and quality `ScriptableObject` profiles |

The expected result is not merely “the existing 2.5D water with buoyancy added”. It is a small water framework in which simulation quality, reflection quality, physics, visuals and authoring are independently controllable.

### Package portability criteria

A package candidate is not considered self-contained merely because all new scripts are under one folder. It must also satisfy these conditions:

- Runtime and editor assemblies compile without either reference water system present.
- Package prefabs, profiles, materials, shaders, and samples resolve only package-owned or explicitly documented Unity-package dependencies.
- Runtime-generated meshes, materials, render textures, buffers, cameras, and helper objects have explicit owners and cleanup paths.
- Required layers, sorting layers, renderer settings, and quality settings are detected or clearly documented.
- A clean compatible Unity project can import the package and run its sample or validation scene after following the documented setup steps.
- Removing `Assets/InteractiveWaterSystem/` and `Assets/Cainos/` from a test copy does not break package compilation or package-owned serialized references.
- No package feature depends on Unity MCP being installed or connected.

### Architectural decision discipline

The class names and paths in this plan are architectural defaults. Change them only when an actual repository or Unity constraint justifies the change. Record any deliberate deviation in `STATUS.md`, including the reason, affected public contract, and migration consequence.

Do not silently replace an approved design with a different pattern because it is easier to implement in one task.

## Repository audit and performance hotspot analysis

### Current custom water structure

`InteractiveWater.cs` currently owns nearly every responsibility:

- Serialized geometry dimensions and vertex counts.
- References to top, front, ambient-wave and ripple materials.
- References to two Custom Render Textures and a reflection texture.
- Mesh generation.
- hierarchy creation.
- Collider creation.
- Mouse testing.
- Impact queueing.
- Ripple simulation updates.
- Material dimension updates.
- Trigger interaction.

The default top surface is 20 by 6.5 world units and uses a 200 by 130 grid. That creates 26,000 vertices and 51,342 triangles before any shader displacement. The front panel uses only two rows, so its geometry cost is comparatively small.

The controller’s physics tick does the following:

```csharp
_rippleSimulationTexture.ClearUpdateZones();
UpdateNewRipples();
_rippleSimulationTexture.Update(
    _rippleSimulationIterationPerFrame);
```

The default iteration count is five, and the project’s fixed timestep is `0.02` seconds, meaning 50 physics updates per second. Only one queued ripple is removed and inserted during each physics tick.

### Ripple-solver workload

The ripple texture is currently:

- 1024 × 1024.
- `R16G16B16A16_SFloat`.
- Double-buffered.
- Bilinearly filtered.
- Mipmapped.
- Configured to generate mipmaps.
- Updated on demand.

The propagation shader samples the current texel and four direct neighbours. It stores the new height in red and the prior height in green, thereby implementing a second-order finite-difference wave solver.

At the repository defaults, the analytical workload is approximately:

```text
1,048,576 texels
× 5 full propagation passes
× 50 physics ticks per second
= 262,144,000 propagated texels per second
```

Because each propagated texel performs approximately five state-texture reads, the solver requests roughly 1.31 billion state samples per second before counting impact passes, double-buffer operations, mip generation, top/front rendering or reflection work. These are workload counts rather than measured GPU milliseconds; actual cost depends heavily on GPU architecture, bandwidth, driver and Unity’s scheduling.

This is disproportionate for a 20 by 6.5 world-unit surface. The texture has more than one million cells while the visible mesh has only 26,000 vertices, so much of the simulated detail cannot become geometric detail on the top plane.

### Current memory footprint

The ripple and ambient textures both use graphics format 48, which Unity defines as four 16-bit floating-point channels: eight bytes per texel. The ripple solver only needs two channels for its current and previous heights.

Approximate persistent texture memory is:

| Resource | Approximate allocation |
|---|---:|
| One 1024² RGBA-half base level | 8 MiB |
| Ripple CRT, complete mip chain and two buffers | 21.3 MiB |
| Ambient CRT, complete mip chain and one buffer | 10.7 MiB |
| 960 × 540 RGBA8 reflection plus D16 depth | 3.0 MiB |
| Combined listed texture state | About 35 MiB |

These figures assume a complete conventional mip chain and exclude alignment, driver overhead, temporary render targets and any internal copy resources. The committed formats and dimensions support the estimate, but the Unity Memory Profiler should be treated as the source of truth on each target platform.

A 256 × 128 two-channel half-float ping-pong simulation without mipmaps would require approximately 0.25 MiB. That is roughly a 98.8% reduction compared with the current mipmapped, double-buffered RGBA-half ripple allocation.

### Double-buffering and update zones

The controller adds two zones when a ripple is available:

- A full-texture propagation zone.
- A 1% × 1% impact zone.

Both specify `needSwap = true`.

Unity documents that a Custom Render Texture update zone can request a buffer swap before the following zone, and that double-buffered Custom Render Textures can incur a texture copy on each swap. Unity explicitly warns that the cost becomes significant with high resolutions and frequent updates.

Therefore, the current configuration is a serious profiling target. It should not be stated without measurement that Unity performs a particular exact number of full copies per fixed tick, because Custom Render Texture scheduling and repeated `Update(count)` calls can produce implementation-dependent command ordering. What can be stated confidently is that:

1. The solver needs previous-state isolation.
2. Both current zones request swaps.
3. The texture is large and updated frequently.
4. Unity warns that double-buffer swaps can copy full texture content.

The Frame Debugger and Render Graph diagnostics should be used to count the actual update draws and copy operations in a player-equivalent configuration. Unity’s Frame Debugger exposes individual rendering events and URP’s Render Graph diagnostics can reveal generated passes and resources.

### Mipmaps provide little value here

Generated mipmaps are not needed by the propagation equation. It samples direct neighbours one base-level texel away, so lower-resolution mip levels do not contribute to the simulation. Unity regenerates mipmaps for mipmapped render textures when automatic mip generation is enabled, adding work after rendering.

Mipmaps could theoretically help when a presentation shader samples a heavily minified height texture, but that does not justify regenerating an entire state-texture mip chain after every simulation update. A better arrangement is:

- Keep simulation state at mip level zero only.
- Sample level zero explicitly for vertex displacement.
- Generate a lower-cost presentation texture only if distant minification demonstrably needs it.

The recommended ripple descriptor is therefore:

```text
Format:          R16G16_SFloat where supported
Mipmaps:         disabled
Auto mipmaps:    disabled
Filtering:       bilinear
Wrap:            clamp
sRGB:            false
Random write:    true only for the compute backend
```

### Ambient-wave texture

The ambient-wave resource is another 1024² RGBA-half Custom Render Texture with generated mipmaps. Unlike the contact simulation, it is configured for continuous real-time updates and is not double-buffered.

Its material contains only three directional wave bands with frequency, amplitude, speed and direction parameters, and its Shader Graph uses sine-wave calculations. This is a deterministic, stateless function of UV and time.

It should be replaced by a shared analytical function:

```hlsl
float EvaluateDirectionalWave(
    float2 worldXZ,
    float2 direction,
    float frequency,
    float amplitude,
    float speed,
    float time)
{
    direction = normalize(direction);
    float phase =
        dot(worldXZ, direction) * frequency +
        time * speed;

    return sin(phase) * amplitude;
}
```

The top and front shaders can call the same function. This removes one persistent 1024² texture, its mip chain and one full-screen-equivalent off-screen update each rendered frame. It trades that cost for a handful of sine and arithmetic operations wherever water is actually drawn.

That trade should still be benchmarked: on very large full-screen water coverage, transcendental arithmetic can be expensive. However, because the top wave is principally needed for vertex displacement and the front seam rather than every simulated texel, direct evaluation is likely to be substantially cheaper than continuously rendering and mipmapping over one million texels.

### Planar reflection

`SimplePlanarReflection` executes in `LateUpdate`, reflects the main camera across the water plane, builds an oblique clipping matrix and calls `_reflectionCam.Render()`. It skips rendering only when the main camera is below the water surface.

Its default scale is `0.5` in each image dimension. A half-width, half-height target contains one quarter of the full-resolution pixel count, but still requires another scene cull, render submission and draw sequence.

This is likely to be the most variable and often the most expensive part of the system because its cost depends on:

- Main-camera resolution.
- Reflection resolution scale squared.
- Number and complexity of reflected objects.
- Material and lighting complexity.
- Shadow settings.
- Number of separately rendered water bodies.
- Reflection culling mask.
- Whether reflected objects are fill-rate or geometry limited.

The current script creates or finds a reflection camera through hierarchy-index assumptions and would render independently for every water object carrying the component.

### Main scene rendering

The custom water has at least two principal main-camera draws: the top mesh and front mesh. The actual pass and draw count can be higher because Shader Graph targets, 2D lighting, sorting and URP internals may introduce additional passes.

The 2D renderer has Camera Sorting Layer Texture enabled, no downsampling, and no custom renderer features. The front-water material can therefore sample the scene captured up to a sorting-layer boundary without installing another behind-water renderer feature.

Unity’s 2D Renderer supports no downsampling, 2× bilinear, 4× box and 4× bilinear downsampling for the Camera Sorting Layer Texture. This provides a useful quality control for underwater distortion and tinting.

### CPU and allocation concerns

Although the custom solver runs on the GPU, the current implementation still has several CPU-side weaknesses:

| Concern | Effect |
|---|---|
| `Camera.Render()` every `LateUpdate` | Additional culling, submission and render-thread work |
| One impact dequeued per fixed tick | Burst impacts become delayed |
| New two-element update-zone array per inserted impact | Managed allocation whenever a ripple is inserted |
| Mesh arrays allocated during each regeneration | Editor and runtime allocation spikes |
| Generated meshes not explicitly disposed before replacement | Potential editor leaks and stale generated objects |
| Child indices used as component references | Fragile hierarchy and migration behaviour |
| `Mathf.Log(mask.value, 2)` used as layer index | Valid only when exactly one mask bit is set |
| Shared material assets assigned | One water can alter all waters using the asset |
| Shared Custom Render Texture assets | Multiple water instances can share simulation state |
| No visibility or activity suspension | Every enabled water simulates continuously |

These concerns follow directly from the controller’s current lifecycle, mesh generation, layer assignment, update-zone construction and shared-material usage.

### Comparison with the Cainos CPU system

The Cainos implementation updates a one-dimensional spring chain rather than a two-dimensional texture. It uses eight horizontal vertices per world unit, performs several neighbour-spreading iterations, then uploads the changed mesh vertices each physics tick. Its cost grows mainly with water width and spread-iteration count, rather than width multiplied by visual depth.

It is normally the cheaper simulation because it solves vastly fewer cells. Nevertheless, its rendering can still become expensive when blur, distortion, underwater effects and particles are active. The hybrid system should take its gameplay and editor ideas—not its CPU spring solver—because adding both wave solvers would duplicate work and create two competing definitions of the visible surface.

## Simulation and reflection alternatives

### Ripple-solver comparison

| Alternative | Advantages | Disadvantages | Recommendation |
|---|---|---|---|
| Existing CRT | Already integrated; Shader Graph-compatible; simple authoring | 1024² fixed square, mipmapped RGBA-half, frequent updates, opaque swap behaviour, one impact per tick | Do not ship unchanged |
| Optimised CRT | Lowest migration cost; supports rectangular state; no compute requirement; retains current shader structure | Still subject to CRT scheduling and possible swap-copy cost; batching is less flexible | **Recommended first production step** |
| Fragment-shader manual ping-pong | Explicit two-texture ownership; predictable swaps; broad compatibility | Requires custom command scheduling; impact batching still needs careful draw setup | Strong fallback if CRT swapping remains costly |
| Compute ping-pong | Explicit resource control; efficient batched impacts; easy RG state; scalable dispatch | More implementation complexity; platform checks; compute and random-write format constraints | Add only after profiling justifies it |
| CPU two-dimensional solver | Easy CPU access for physics | Poor scaling at useful resolutions; texture upload cost | Not recommended |
| CPU one-dimensional springs | Very cheap and physics-readable | Cannot create true XZ radial ripples | Keep only for purely 2D water |

### World-relative rectangular resolution

Resolution should be based on world-space texel density rather than a fixed square asset:

```csharp
int width = RoundUpToMultiple(
    Mathf.CeilToInt(worldWidth * texelsPerUnit),
    8);

int depth = RoundUpToMultiple(
    Mathf.CeilToInt(worldDepth * texelsPerUnit),
    8);
```

The two axes should have approximately equal world-space cell dimensions:

```text
worldWidth / resolutionX
≈
worldDepth / resolutionZ
```

This matters for both efficiency and appearance. The current default water is approximately 3.08 times wider than it is deep, yet its simulation texture is square. A UV-space circular ripple consequently represents unequal world-space distances on X and Z unless the shader compensates for aspect ratio.

At 16 texels per world unit, the current 20 by 6.5 surface would need approximately:

```text
320 × 104 cells
```

rather than:

```text
1024 × 1024 cells
```

At 30 simulation updates per second and two propagation passes per update, 320 × 104 would process about two million propagation cells per second, less than 1% of the repository default’s 262 million cells per second.

### Stable timestep and wave speed

The current shader exposes a single `_Spread` coefficient and uses equal coefficients for X and Z. It also makes propagation speed depend on how many times `Update()` is repeated. This means that changing simulation frequency, texture resolution or iteration count changes the apparent physics.

A more principled update is:

```text
sx = (waveSpeed × substepDuration / cellSizeX)²
sz = (waveSpeed × substepDuration / cellSizeZ)²

next =
    2 × current
    - previous
    + sx × (left + right - 2 × current)
    + sz × (up + down - 2 × current)

next *= dampingForThisSubstep
```

For the conventional explicit two-dimensional finite-difference wave equation, a practical stability requirement is:

```text
sx + sz ≤ 1
```

A production implementation should apply a safety margin, such as `0.9`, and either reduce the substep duration or limit the requested wave speed when the condition would be exceeded. The precise visual tuning still requires empirical testing because the current system is an artistic height field rather than a physically calibrated body of water.

Damping should also be expressed independently of update frequency. Instead of storing a per-update multiplier such as `0.999`, store a decay rate per second:

```csharp
float dampingThisStep =
    Mathf.Exp(-decayPerSecond * substepDuration);
```

An alternative migration formula is:

```csharp
float dampingThisStep = Mathf.Pow(
    dampingAtReferenceStep,
    substepDuration / referenceStep);
```

This keeps approximate decay behaviour consistent when switching between 20, 30 and 60 simulation updates per second.

The inspector should expose:

```text
Wave speed in world units per second
Decay per second
Simulation frequency
Maximum substeps
Texels per world unit
Impact radius in world units
Impact strength
```

It should not expose an unqualified “iterations per frame” slider as the main physical speed control.

### Multiple impacts in one simulation step

The current FIFO queue processes one impact per fixed tick. At 50 Hz, ten simultaneous contacts can take up to 0.2 seconds to enter the texture, which will feel visibly delayed.

The improved backend should process all impacts collected since its last simulation update, subject to a configurable safety cap.

For an optimised CRT backend:

1. Reuse a preallocated update-zone list.
2. Add all impact zones.
3. Inject the impacts.
4. Perform the requested propagation substeps.
5. Clear the queue without allocating.

For a compute backend:

1. Upload a compact `WaterImpact` structured buffer.
2. Run one impact-injection kernel.
3. Dispatch the propagation kernel.
4. Swap height-state textures.
5. Repeat only the propagation stage for remaining substeps.

Example data:

```csharp
public struct WaterImpact
{
    public Vector2 uv;
    public float radiusU;
    public float radiusV;
    public float strength;
}
```

Impact radius should be specified in world units and converted independently along each texture axis:

```text
radiusU = worldRadius / waterWorldWidth
radiusV = worldRadius / waterWorldDepth
```

This preserves the same physical impact size when changing water dimensions or texture resolution.

### Visibility and inactivity suspension

A water body should simulate only when at least one of the following applies:

- It is visible to a gameplay camera.
- It received a recent impact.
- It contains an effect that must continue propagating before becoming visible.
- Its estimated wave energy remains above a threshold.
- It is explicitly forced active by gameplay.

A simple first implementation can use:

```text
On impact:
    lastImpactTime = current time

Simulate when:
    visible
    AND current time < lastImpactTime + idleTimeout
```

A better implementation periodically estimates energy from known impact strengths and exponential damping without reading the texture back. This is conservative but avoids asynchronous GPU readback.

Unity visibility callbacks can be useful, but they should not be the sole production criterion because editor Scene cameras and other cameras can influence renderer visibility. Use an explicit gameplay-camera frustum test or a central water-visibility manager. Unity documents renderer visibility callbacks as camera-dependent, which is why an explicit camera policy is safer.

When an off-screen water receives an impact, there are two reasonable behaviours:

- Simulate normally until its idle timeout, preserving continuity.
- Record the impact and fast-forward a capped number of substeps when it next becomes visible.

The first is simpler and preferable unless a level has many simultaneously active off-screen waters.

### Optimised CRT versus compute

The compute backend should not be the first rewrite. The existing fragment equation is simple and an optimised CRT will answer whether the main problem is resolution and update policy rather than the CRT mechanism itself.

Move to compute if profiling shows one or more of these:

- CRT double-buffer copies remain a significant GPU cost.
- Many water bodies need independent simulations.
- Hundreds of impacts must be batched efficiently.
- Explicit resource pooling or texture arrays are required.
- CRT update-zone scheduling prevents deterministic ordering.
- A lower-channel format cannot be used reliably through the current CRT path.

The compute backend would use:

```text
HeightStateA: RGHalf
HeightStateB: RGHalf
ImpactBuffer: StructuredBuffer<WaterImpact>
```

with kernels:

```text
CSClear
CSInjectImpacts
CSPropagate
```

Dispatch size is calculated from the kernel’s `numthreads` declaration and the state-texture dimensions. Unity requires runtime compute support checks through `SystemInfo.supportsComputeShaders`, and supported random-write formats should be validated per platform.

A shared interface keeps the choice reversible:

```csharp
public interface IWaterRippleSimulator : IDisposable
{
    Texture HeightTexture { get; }
    bool IsInitialised { get; }

    void Initialise(
        Vector2 worldSize,
        WaterQualityProfile quality);

    void QueueImpact(in WaterImpact impact);
    void Simulate(float elapsedTime);
    void SetSimulationEnabled(bool enabled);
    void Clear();
}
```

### Reflection alternatives

**Every-frame planar reflection** gives the highest temporal fidelity but scales poorly when duplicated. It is appropriate only for a high-quality tier, a single important plane, or scenes with exceptionally cheap reflection masks.

**Adaptive planar reflection** should be the standard tier. A central manager updates when:

- The main camera moved beyond a position threshold.
- The main camera rotated beyond an angle threshold.
- The maximum frame interval has elapsed.
- An important reflected object marked the group dirty.
- A newly visible water requires a current texture.

**Shared reflection** is viable for water surfaces that have the same:

- Plane normal.
- Plane height within a small tolerance.
- Reflection culling mask.
- Reflection quality profile.
- Camera and projection context.

One reflected camera and texture can serve every registered surface in such a group. The top shader should receive a reflection view-projection matrix or use consistent projected screen coordinates rather than relying on object-local UVs.

**Low-quality approximation** should avoid a second camera entirely. Suitable options for this stylised 2.5D game are:

- A colour gradient with distorted highlights.
- A static cubemap or authored reflection strip.
- A vertically mirrored strip sampled from the 2D Camera Sorting Layer Texture.
- A blurred scene-colour approximation near the front seam.

A mirrored sorting-layer texture will not reproduce objects outside the main view and is not a true reflection, but this limitation is often acceptable on a low tier. The current project already enables Camera Sorting Layer Texture, so the fallback can use the existing renderer facility.

## Proposed hybrid runtime, physics and authoring system

### Runtime module responsibilities

| Module | Responsibility |
|---|---|
| `Water25DController` | Holds dimensions and profiles; coordinates modules |
| `WaterRuntimeResources` | Creates, owns and disposes meshes, materials and render textures |
| `WaterMeshBuilder` | Builds top and front geometry without gameplay responsibilities |
| `IWaterRippleSimulator` | Common simulation API |
| `CustomRenderTextureRippleSimulator` | First production backend |
| `ComputeRippleSimulator` | Optional high-scalability backend |
| `WaterSurfaceInteraction2D` | Detects surface crossings and creates logical impacts |
| `WaterPhysicsVolume2D` | Tracks submerged bodies and applies optional custom drag |
| `WaterDepthAnchor` | Resolves a 2D object’s intended Z contact lane |
| `WaterReflectionManager` | Groups planes and schedules adaptive reflection renders |
| `WaterVisibilityReporter` | Reports gameplay-camera visibility |
| `WaterFXController` | Requests splash, bubble, foam and underwater effects |
| `WaterFXPool` | Reuses effect instances without runtime Instantiate/Destroy churn |
| `WaterStyleProfile` | Shared artistic settings |
| `WaterQualityProfile` | Shared performance and resolution settings |
| `Water25DEditor` | Foldouts, validation, actions and scene handles |

### Assembly and dependency boundaries

Create assembly definitions when the package begins to contain runtime or test code. The intended boundaries are:

```text
Water25D.Runtime.asmdef
    Runtime code only
    No UnityEditor reference
    No reference-system assembly dependencies

Water25D.Editor.asmdef
    Editor-only
    References Water25D.Runtime
    Included only on the Editor platform

Water25D.Tests.EditMode.asmdef
    EditMode tests
    References Runtime and Editor only when the test requires editor APIs

Water25D.Tests.PlayMode.asmdef
    PlayMode tests
    References Runtime
```

Keep public runtime contracts in the runtime assembly. Migration tooling that needs legacy types should be isolated in a dedicated editor-only migration assembly or behind an explicitly temporary compile boundary so the final package can compile after the legacy systems are removed.

Avoid circular references between runtime modules. Prefer plain data contracts and interfaces at module boundaries rather than direct dependencies on concrete implementations.

### Public API principles

The package should expose a small stable API rather than every implementation detail:

- `Water25DController` as the primary authored component.
- `IWaterRippleSimulator` as the simulation-backend contract.
- `WaterImpact` for visual ripple requests.
- `WaterInteraction` for physics and FX events.
- C# events and optional UnityEvents for enter, exit, splash, ripple, submerge, and resurface.
- Profile assets for shared style and quality configuration.
- Explicit methods for resetting simulation, rebuilding authored data, and requesting impacts where gameplay needs them.

Internal resource, reflection-group, pooling, and contact-tracking implementations should remain internal unless a demonstrated extension requirement justifies public exposure.

### Resource ownership

The existing custom component assigns shared materials and references shared Custom Render Texture assets. This makes multiple water bodies vulnerable to shared visual settings and shared ripple state.

The improved ownership model should be:

```text
Project assets
    immutable material templates
    WaterStyleProfile assets
    WaterQualityProfile assets
    effect prefabs

Per water instance
    generated top mesh
    generated front mesh
    runtime ripple state
    per-instance property data
    optional material instances

Per reflection group
    reflection camera
    reflection render texture
    reflection view-projection data

Global or scene-level
    FX pools
    reflection manager
    optional simulation resource pool
```

`WaterStyleProfile` should contain appearance, not mutable runtime state:

```text
Top colours and foam
Front shallow/deep colours
Distortion
Caustics
Ambient wave bands
Ripple displacement scales
Reflection appearance
Splash and bubble colours
FX prefab references
```

`WaterQualityProfile` should contain cost controls:

```text
Simulation texels per unit
Minimum and maximum dimensions
Simulation updates per second
Maximum substeps
Maximum impacts per step
Idle timeout
Top mesh vertices per unit
Reflection mode
Reflection resolution scale
Reflection update interval
Sorting-layer texture expectation
FX density multiplier
```

Water size, fill/depth, interaction layers and buoyancy tuning remain instance settings because they differ physically between level objects.

### Buoyancy and underwater physics

The Cainos water automatically creates or configures a trigger collider and `BuoyancyEffector2D`, updates the effector’s surface level when the water’s fill changes, and filters participating layers.

That model ports cleanly, but the improved 2.5D prefab should use two colliders rather than one.

**Buoyancy volume**

```text
Width:   water width
Height:  front-water physical depth
Top:     waterline
Bottom:  waterline - physical depth
```

It should be a trigger, be used by the effector, and cover the complete playable underwater region.

**Surface-crossing trigger**

```text
Width:      water width
Height:     small configurable crossing band
Centre Y:   waterline
```

It should not be used by the buoyancy effector. Its only purpose is detecting legitimate crossings.

This avoids a common failure in which entering the buoyancy volume through its side or bottom incorrectly produces a surface splash.

The initial physics configuration should be:

```text
BuoyancyEffector2D
    Surface Level: waterline
    Density: designer setting
    Flow: optional
    Linear Drag: zero if custom drag is enabled
    Angular Drag: zero if custom drag is enabled
    Collider Mask: interaction mask

WaterPhysicsVolume2D
    Optional custom linear drag
    Optional custom angular drag
    Submerged Rigidbody2D tracking
    Bubble requests
```

The Cainos system applies force opposite to linear velocity and torque opposite to angular velocity while a Rigidbody2D remains in the water.

A more robust custom drag model can account for mass and submerged fraction, but the first milestone should retain an artist-friendly behaviour:

```csharp
Vector2 force =
    -linearDragCoefficient *
    rigidbody.linearVelocity;

float torque =
    -angularDragCoefficient *
    rigidbody.angularVelocity;
```

Do not simultaneously use strong effector drag and strong custom drag unless that double damping is intentional.

### Why buoyancy should remain flat

The visual ripple texture should not drive `BuoyancyEffector2D.surfaceLevel`.

Reading the height texture back would introduce:

- GPU-to-CPU synchronisation.
- Readback latency.
- Sampling and coordinate complexity.
- Platform-specific behaviour.
- Coupling between quality settings and gameplay.
- Problems when the simulation is suspended off-screen.

The visual wave amplitude is normally small enough that a flat gameplay surface remains convincing. Splash particles, object bobbing and contact ripples provide the perceptual connection.

Where physically responsive bobbing is important, add a lightweight CPU-side cosmetic bob signal to selected objects rather than reading back the entire GPU height field.

### Impact generation

The current custom system uses random strength between `0.2` and `0.6`, while only direction is influenced by the Rigidbody2D’s vertical velocity.

The improved system should derive strength from measurable interaction:

```text
impact strength =
    normalised vertical speed
    × object-size multiplier
    × optional mass multiplier
    × profile multiplier
```

A practical formula is:

```csharp
float speedFactor = Mathf.InverseLerp(
    minImpactSpeed,
    maxImpactSpeed,
    Mathf.Abs(rb.linearVelocity.y));

float sizeFactor = Mathf.Clamp(
    collider.bounds.size.x / referenceWidth,
    minSizeMultiplier,
    maxSizeMultiplier);

float strength = Mathf.Clamp01(
    speedFactor *
    sizeFactor *
    rippleStrengthMultiplier);
```

The impact location should use the collider’s surface intersection rather than blindly using `transform.position`.

Because Rigidbody2D ignores Z, the system needs a depth policy:

| Depth mode | Behaviour |
|---|---|
| Transform Z | Uses the object’s current visual Z |
| Fixed lane | Uses a configured front, centre or rear water lane |
| Anchor transform | Uses a child transform placed at the visible water contact point |
| Custom provider | Gameplay component calculates the contact depth |

`WaterDepthAnchor` should default to Transform Z but allow explicit overrides.

### Multi-collider bodies

The interaction system should track logical Rigidbody2D contacts rather than only Collider2D events. A character with a body collider, feet trigger and interaction trigger can otherwise create repeated splashes.

Use a dictionary or pooled map:

```text
Rigidbody2D instance ID
    active submerged collider count
    active surface-trigger count
    last splash time
    last known velocity
    fully submerged state
```

The first qualifying crossing produces the enter event; the final qualifying exit produces the exit event.

### FX strategy

The Cainos system has mature splash selection, bubble trails, surface particles and underwater particles, but it creates splash and bubble objects through `Instantiate`.

The hybrid should retain the configuration concepts while replacing creation with pooling.

```text
WaterFXPool
├── SplashTiny pool
├── SplashSmall pool
├── SplashLarge pool
├── BubbleTrail pool
├── SurfaceFoam pool
└── UnderwaterBurst pool
```

Each effect request contains:

```csharp
public readonly struct WaterInteraction
{
    public readonly Collider2D Collider;
    public readonly Rigidbody2D Rigidbody;
    public readonly Vector3 WorldPosition;
    public readonly Vector2 Velocity;
    public readonly float Strength;
    public readonly float ObjectWidth;
    public readonly bool Entering;
}
```

Splash selection can use object width and speed thresholds, much like the Cainos configuration. Bubble emission should scale with collider area and remain attached only while the body is submerged. Continuous surface and underwater particle systems should scale their emission regions and rates from water width, depth or volume. Cainos already uses per-unit emission scaling and resizes particle shapes when the water changes.

For the 2.5D look:

- Main splash sprites should sit close to the front seam.
- Secondary droplets can travel slightly into Z.
- Bubble trails should inherit the object’s configured depth anchor.
- Foam particles can be distributed over the top XZ surface.
- Underwater motes can use the front XY volume with a small Z thickness.

### Events

Expose both UnityEvents for designer wiring and C# events for systems code:

```text
OnEnterWater
OnExitWater
OnSplash
OnRipple
OnFullySubmerged
OnResurface
```

Each should carry the same `WaterInteraction` payload rather than only a level integer and position. Cainos currently exposes a splash UnityEvent but not a unified event model.

### Inspector layout

Cainos achieves its panelled layout through `FoldoutGroup` attributes, a custom Lucid Editor inspector, extensive tooltips, Undo integration and scene handles.

The new system should reproduce the workflow in project-owned editor code:

| Panel | Principal controls |
|---|---|
| Basic | Width, visual depth, physical depth, waterline/fill, interaction layers |
| Rendering | Top and front colours, transparency, foam, seam controls |
| Distortion | Enabled, direction, scale, strength, speed |
| Caustics | Enabled, colour, tiling, scale, strength, power, speed |
| Reflection | Mode, culling mask, plane group, resolution, update policy |
| Ripples | Backend, texels/unit, speed, decay, rate, strength, radius |
| Ambient Waves | Three or four directional bands, global multiplier |
| Physics | Buoyancy, density, flow, linear drag, angular drag |
| FX | Splash configs, bubbles, foam, surface particles, underwater particles |
| Events | Enter, exit, splash, ripple, submerge, resurface |
| Performance | Calculated resolution, memory, cell updates/s, visibility state |
| Advanced | Material templates, shaders, runtime ownership and debugging |
| Actions | Repair hierarchy, rebuild meshes, reset simulation, create instances |

The Performance panel should display derived values:

```text
Top mesh vertices and triangles
Ripple texture dimensions
Persistent ripple memory
Simulation cells per second
Estimated reflection pixel count
Whether mipmaps are disabled
Whether runtime resources are accidentally shared
Whether the water is currently simulating
```

The editor should provide three scene handles:

- X width.
- Z visual depth.
- Y physical/front depth.

All handle changes must use Unity Undo and update meshes, triggers and bounds consistently. Cainos’ current editor already demonstrates the desired Undo and scene-handle workflow for two dimensions.

A standalone editor is preferable to making the new system depend on Lucid Editor. The included Lucid copy is MIT-licensed but archived upstream and no longer an ideal foundation for new long-term tooling; retaining the same visual organisation through standard Unity editor APIs avoids adding another package dependency.

### Destination-project setup and validation

The package should include an editor validator or setup report that checks, without silently overwriting project settings:

- Required Unity and URP capabilities are available.
- The active renderer is compatible with the package's front-water scene-colour approach.
- Camera Sorting Layer Texture is configured when the selected style requires it.
- Required sorting layers exist and are ordered correctly.
- Required physics layers and collision masks exist.
- Reflection culling masks exclude water and reflection-camera helper layers.
- Required shader resources and material templates are assigned.
- Compute support is checked only when the compute backend is selected.
- Runtime graphics formats are supported before creating render textures.
- Sample and migration assets do not contain missing references.

The validator should provide actionable instructions and repair buttons only where a safe, deterministic edit is possible. Project settings must not be changed merely because a package component was selected in the Inspector.

## Implementation roadmap and file-by-file plan

### Delivery rules and phase gates

Implementation proceeds through independently reviewable phases. A phase may be split into several tasks, but a task should not opportunistically implement later phases.

For every task:

1. State the active phase and exact slice being implemented.
2. Identify files allowed to change.
3. Preserve a compiling intermediate state where possible.
4. Inspect the complete diff for reference-system edits and accidental Unity reserialization.
5. Run available validation and disclose unavailable Unity validation.
6. Update `STATUS.md` only when the task genuinely changes milestone state.

A phase is complete only when its exit criteria are met. Code existing in the repository is not, by itself, proof that the phase is complete.

### Phase 0 — Baseline and package foundation

**Goal:** establish a reproducible baseline, package ownership boundary, status record, and deterministic comparison workflow before replacement implementation begins.

**Deliverables:**

- Preserve the original custom and Cainos systems unchanged.
- Confirm the active Unity and package versions from project files.
- Create only the package directories needed for this phase.
- Create `Assets/Water25D/Documentation/STATUS.md` from the status template in this plan.
- Record the original custom-water prefab hierarchy, component references, material values, texture descriptors, renderer dependencies, layers, sorting layers, and demo-scene setup.
- Capture baseline screenshots or golden images for representative camera positions.
- Create or document a deterministic benchmark scene and test driver with fixed random seeds and repeatable impact trajectories.
- Record which benchmark measurements require a development player and target hardware.
- Record licensing and provenance constraints that affect migration or redistribution.
- Add initial assembly definitions only if runtime or test code is introduced during this phase.

**Recommended baseline artifacts:**

```text
Assets/Water25D/Documentation/STATUS.md
Assets/Water25D/Documentation/BASELINE.md
Assets/Water25D/Documentation/SETUP_REQUIREMENTS.md
Assets/Water25D/Tests/ or Samples/Benchmark/ only when needed
```

**Exit criteria:**

- The baseline scene and configuration can be reproduced without guesswork.
- Reference-system files have no unintended changes.
- The current rendering and interaction behaviour is documented.
- Known project-level dependencies are listed.
- The next structural-refactor task has an explicit comparison target.

**Do not:**

- Implement the replacement controller.
- Retune source materials.
- Modify Shader Graph assets.
- Delete or migrate the original prefab.
- Record estimates as profiler measurements.

### Phase 1 — Structural refactor with visual parity

**Goal:** create the package-owned controller, hierarchy validation, mesh builder, and coordination boundaries while preserving the current rendered result.

The first implementation should replace the legacy all-in-one initialisation concept with clearly separated operations:

```text
ValidateOrRepairHierarchy()
InitialiseRuntimeResources()
RebuildGeometry()
RebuildPhysicsVolumes()
ApplyStyleSettings()
ApplyQualitySettings()
ResetSimulation()
RegisterReflectionSurface()
```

Changing a colour should call only `ApplyStyleSettings()`. Changing wave speed should apply simulation or quality parameters without rebuilding geometry. Changing dimensions should rebuild geometry and volumes, and recreate simulation state only when the calculated texture dimensions change.

Avoid calling a complete initialisation path from `OnValidate`, because doing so can:

- Rebuild a large mesh while dragging an unrelated Inspector field.
- Clear simulation state.
- Reallocate render textures.
- Mutate shared assets.
- Disrupt reflection cameras.
- Produce excessive Undo and serialized changes.

**Deliverables:**

- `Water25DController` as a thin coordinator.
- `WaterMeshBuilder` for top and front geometry.
- Hierarchy validation or repair logic.
- Package-owned top and front rendering templates or temporary adapters clearly marked for removal.
- Visual-comparison hooks or a comparison scene.
- EditMode tests for deterministic mesh counts, bounds, UV mapping, and dimension changes where practical.

**Exit criteria:**

- The package-owned hierarchy can be created or repaired deterministically.
- Top and front geometry match the baseline within documented tolerances.
- Unrelated Inspector changes do not rebuild all resources.
- No reference-system source files were refactored to support the package.
- Visual parity has been checked in Unity, or the missing Unity check is explicitly recorded.

### Phase 2 — Runtime resource ownership and disposal

**Goal:** eliminate accidental shared mutable state and establish explicit lifecycle ownership before adding further systems.

**Deliverables:**

- `WaterRuntimeResources` or equivalent explicit resource owner.
- Runtime-created ripple state rather than committed shared simulation state.
- Correct ownership of generated meshes, material instances or property blocks, render textures, buffers, helper cameras, and helper GameObjects.
- Safe cleanup for component disable, destruction, domain reload, Play Mode exit, scene unload, runtime resize, and resource replacement.
- Validation warnings for shared mutable resources.
- Tests proving two water bodies do not share ripple state or per-instance material values unintentionally.

**Resource policy:**

```text
Project assets
    immutable templates and profiles

Per water instance
    generated top mesh
    generated front mesh
    mutable ripple state
    impact queue
    contact state
    per-instance material data

Per reflection group
    reflection camera
    reflection texture
    projection data

Scene or global owner
    FX pools
    reflection manager
    optional shared allocation pools
```

**Exit criteria:**

- Two water instances can run without contaminating each other's state.
- Rebuild and teardown paths do not leave stale generated resources.
- Normal play does not instantiate materials or recreate textures repeatedly.
- Template assets remain unchanged during Play Mode.

### Phase 3 — Authoring, profiles, and Inspector workflow

**Goal:** provide a project-owned authoring experience before the system becomes more complex.

**Deliverables:**

- `WaterStyleProfile` for shared artistic settings.
- `WaterQualityProfile` for cost and quality controls.
- Grouped custom Inspector using standard Unity editor APIs.
- X width, Z visual depth, and Y physical-depth scene handles.
- Undo/Redo support for all authored changes.
- Derived performance and memory information in the Inspector.
- Validation messages and explicit actions such as repair hierarchy, rebuild geometry, reset simulation, and create instance-owned resources.
- Setup validator for layers, sorting layers, renderer requirements, materials, and profiles.

Do not make the new package depend on Lucid Editor or Odin Inspector.

**Exit criteria:**

- Common authoring operations do not require editing serialized files manually.
- Undo and Redo restore dimensions, hierarchy, meshes, and trigger sizes correctly.
- Shared profiles remain immutable at runtime.
- Performance-derived values update without triggering full resource rebuilds.

### Phase 4 — Separate physics volumes and buoyancy

**Goal:** establish stable flat 2D gameplay buoyancy independent of visual ripple height.

Create two separate colliders:

**Buoyancy volume**

```text
Width:   water width
Height:  physical underwater depth
Top:     waterline
Bottom:  waterline - physical depth
```

It should be a trigger, support `BuoyancyEffector2D`, and cover the complete playable underwater region.

**Surface-crossing trigger**

```text
Width:      water width
Height:     small configurable crossing band
Centre Y:   waterline
```

It should not be used by the buoyancy effector. Its purpose is legitimate surface-crossing detection.

**Deliverables:**

- `WaterPhysicsVolume2D`.
- Buoyancy-effector setup and validation.
- Optional custom linear and angular drag with clear interaction rules.
- Submerged Rigidbody2D tracking.
- Tests across common collider shapes and body masses.
- Side-entry and bottom-entry tests proving they do not create surface splashes.

Do not drive `BuoyancyEffector2D.surfaceLevel` from GPU texture readback.

**Exit criteria:**

- Objects float predictably on a flat gameplay surface.
- Resizing the water updates both physical volumes correctly.
- Effector drag and custom drag cannot be unintentionally doubled.
- Physics remains functional while visual simulation is disabled or suspended.

### Phase 5 — Interaction, depth policy, and logical contact tracking

**Goal:** produce immediate, deterministic, depth-aware logical impacts and events without multi-collider duplication.

**Deliverables:**

- `WaterSurfaceInteraction2D`.
- `WaterContactTracker` keyed by logical Rigidbody2D identity.
- `WaterDepthAnchor` with transform-Z, fixed-lane, anchor-transform, and custom-provider policies.
- `WaterImpact` and `WaterInteraction` data contracts.
- Velocity-sensitive impact strength.
- World-relative impact radius.
- Collider-surface intersection or a documented equivalent for impact location.
- All pending impacts processed per simulation update up to a configurable cap.
- Enter, exit, splash, ripple, fully submerged, and resurface events.
- Cooldown or deduplication behaviour for noisy collider configurations.

A practical initial strength model is:

```csharp
float speedFactor = Mathf.InverseLerp(
    minImpactSpeed,
    maxImpactSpeed,
    Mathf.Abs(rb.linearVelocity.y));

float sizeFactor = Mathf.Clamp(
    collider.bounds.size.x / referenceWidth,
    minSizeMultiplier,
    maxSizeMultiplier);

float strength = Mathf.Clamp01(
    speedFactor *
    sizeFactor *
    rippleStrengthMultiplier);
```

**Exit criteria:**

- A multi-collider character generates one logical entry and exit.
- Simultaneous impacts do not wait in a one-impact-per-fixed-tick backlog.
- Impact position and depth mapping are correct at representative lanes and bounds.
- Trigger and non-trigger masks work independently.

### Phase 6 — Optimised Custom Render Texture simulation

**Goal:** replace the fixed high-cost shared ripple asset with an instance-owned, rectangular, world-relative, no-mipmap CRT backend.

Implement:

```text
Rectangular world-relative resolution
Two-channel half-float state where supported
No mipmaps
No automatic mip generation
Configurable simulation frequency
Bounded catch-up substeps
Preallocated impact storage
All pending impacts processed per step
World-relative impact radii
Idle and visibility suspension
Per-second damping
Aspect-correct propagation coefficients
Runtime format and capability checks
```

Use a custom accumulator rather than tying visual simulation directly to the 2D physics timestep:

```csharp
_accumulator += Time.deltaTime;

while (_accumulator >= simulationStep &&
       substeps < maximumCatchUpSteps)
{
    SimulateOneStep(simulationStep);
    _accumulator -= simulationStep;
    substeps++;
}
```

The finite-difference coefficients should derive from world-space cell size and substep duration:

```text
sx = (waveSpeed × substepDuration / cellSizeX)²
sz = (waveSpeed × substepDuration / cellSizeZ)²
```

Apply a documented stability margin so that the explicit update does not exceed its supported bound.

**Deliverables:**

- `CustomRenderTextureRippleSimulator` behind `IWaterRippleSimulator`.
- Runtime descriptor calculation and format fallback.
- Batch impact injection without steady-state allocation.
- Visibility and idle policy.
- Numerical validation for supported parameter ranges.
- Benchmark comparison against the original configuration.

**Exit criteria:**

- The backend owns independent state per active water body.
- The default medium configuration uses rectangular world-relative resolution.
- Simulation does not generate mipmaps.
- Multiple impacts appear without visible FIFO delay.
- Normal play produces zero managed allocation from the simulation path after warm-up.
- Measured results are clearly separated from analytical estimates.

### Phase 7 — Analytical ambient waves

**Goal:** remove the stateless continuously updated ambient-wave CRT after a visually equivalent analytical replacement is available.

Use a shared HLSL function such as:

```hlsl
float EvaluateDirectionalWave(
    float2 worldXZ,
    float2 direction,
    float frequency,
    float amplitude,
    float speed,
    float time)
{
    direction = normalize(direction);
    float phase =
        dot(worldXZ, direction) * frequency +
        time * speed;

    return sin(phase) * amplitude;
}
```

Both top and front rendering should use the same wave definition so the top surface and front seam remain coherent.

**Deliverables:**

- `WaterAmbientWaves.hlsl` or equivalent package-owned shared function.
- Quality-controlled wave-band count.
- Top and front shader integration.
- Visual comparison at the seam, edges, and representative camera distances.
- Removal of the ambient CRT dependency only after validation.

**Exit criteria:**

- Ambient-wave appearance remains within documented visual tolerances.
- The ambient CRT and its update cost are absent from the new package runtime.
- Low, medium, and high profiles can select appropriate wave-band counts.

### Phase 8 — Shared adaptive reflection management

**Goal:** replace per-water always-rendering reflection cameras with centrally managed compatible reflection groups and quality fallbacks.

Create:

```text
WaterReflectionManager
    Dictionary<ReflectionGroupKey, ReflectionGroup>
```

A group key should include relevant values such as:

```text
Plane normal
Quantised plane height
Culling mask
Quality profile
Camera identity
Projection context where necessary
```

Each group owns:

```text
One disabled reflection camera
One render texture
Last rendered camera pose
Last update frame or time
Registered visible water surfaces
Dirty state
Reflection view-projection data
```

Update a group when camera motion, camera rotation, maximum interval, important reflected-object changes, visibility, or explicit invalidation requires it.

**Deliverables:**

- `WaterReflectionManager` and `WaterReflectionGroup`.
- Registration and deregistration lifecycle.
- Recursion-safe culling policy.
- Adaptive update policy.
- Reflection-disabled and stylised fallback modes.
- Tests for coplanar grouping and non-coplanar separation.

**Exit criteria:**

- Reflection cost scales with active compatible groups rather than raw water-body count.
- Reflection cameras never render themselves or one another.
- Invisible groups do not render.
- Reflection-disabled quality creates no reflection-camera render.

### Phase 9 — Pooled FX and presentation events

**Goal:** add project-owned splash, bubble, foam, droplet, and underwater effects without runtime creation churn.

Implement a pool API such as:

```csharp
public interface IWaterFXPool
{
    WaterFXHandle Spawn(
        WaterFXDefinition definition,
        in WaterInteraction interaction);
}
```

**Deliverables:**

- `WaterFXController`.
- `WaterFXPool` with explicit owner and cleanup.
- Project-owned `WaterFXDefinition` assets.
- Configurable prewarm counts and predictable exhaustion policy.
- Entry and exit splash selection using speed and size.
- Bubble attachment and release behaviour.
- Surface and underwater continuous-effect scaling.
- Event-to-FX wiring that does not make gameplay depend on visual effects.

**Exit criteria:**

- Normal gameplay does not repeatedly `Instantiate` or `Destroy` water effects.
- Pools return instances correctly and survive scene transitions according to their ownership policy.
- FX remain depth-aware and visually aligned with the 2.5D presentation.
- Cainos assets are not copied into the package.

### Phase 10 — Optional compute-backend experiment

**Goal:** determine whether explicit compute ping-pong provides a meaningful target-device improvement over the optimised CRT backend.

Do not begin this phase until Phase 6 is complete and benchmarked.

The compute backend should preserve the same settings and public `HeightTexture` contract as the CRT backend:

```text
HeightStateA: RGHalf or validated fallback
HeightStateB: RGHalf or validated fallback
ImpactBuffer: StructuredBuffer<WaterImpact>

Kernels:
    CSClear
    CSInjectImpacts
    CSPropagate
```

Compare backends using identical:

```text
Water dimensions
State dimensions
State format
Simulation frequency
Substeps
Wave speed
Damping
Impacts
Rendering materials
Camera setup
Target hardware
```

**Exit criteria:**

- Capability checks and fallback behaviour are correct.
- Visual behaviour is sufficiently equivalent for the same public settings.
- Benchmark evidence shows whether compute is materially better, neutral, or worse on intended hardware.
- Compute becomes a default only when the measured benefit justifies maintenance and compatibility cost.

A valid outcome is to keep compute experimental or omit it from the first release.

### Phase 11 — Migration tooling and reversible conversion

**Goal:** convert existing custom-water instances without destroying the baseline or requiring vendor content.

A migration wizard should:

```text
Read old InteractiveWater settings
Create or repair the new hierarchy
Create a WaterStyleProfile from current material values
Create or assign a WaterQualityProfile
Map top and front dimensions
Map sorting layers
Map reflection mask and scale
Create buoyancy and surface triggers
Create package-owned runtime descriptors
Register the reflection plane
Disable, but initially retain, old components
Produce warnings and a conversion summary
```

Migration tooling may reference legacy types only in editor-only code. Isolate that dependency so the runtime package compiles without legacy assemblies.

**Exit criteria:**

- Conversion is reversible until acceptance tests pass.
- Original components and serialized values are retained during the verification period.
- The wizard reports unsupported or ambiguous mappings instead of guessing silently.
- Migrated prefabs contain no unintended vendor references.

### Phase 12 — Packaging, clean-project import, and production validation

**Goal:** prove the system is self-contained, documented, performant, and safe to move into another compatible project.

**Deliverables:**

- Package README and setup guide.
- Required renderer, layer, sorting-layer, and physics configuration documentation.
- Sample water prefab and minimal sample scene using package-owned assets.
- API and profile documentation.
- Upgrade and migration notes.
- Clean-project import test.
- Missing-reference and dependency scan.
- Target-device benchmark captures.
- Release acceptance report.

**Clean-project import procedure:**

1. Create or use a clean compatible Unity project.
2. Import only `Assets/Water25D/` and explicitly documented package dependencies.
3. Reproduce required project settings using the setup guide or validator.
4. Open the sample scene.
5. Run EditMode and PlayMode tests.
6. Build a development player for at least one intended target.
7. Confirm no references resolve into the source repository's custom, Cainos, or demo folders.

**Exit criteria:**

- Runtime and editor assemblies compile in the clean project.
- Sample content renders and interacts correctly after documented setup.
- No package-owned asset has a missing serialized reference.
- No steady-state runtime allocation violates the accepted budget.
- Target-device results meet the release budget or documented quality-tier limits.
- Licensing and attribution documentation is complete for every distributed asset.

### Existing-file changes

| Existing file or asset | Proposed action |
|---|---|
| `InteractiveWater.cs` | Preserve as baseline; later reduce to a migration façade only if explicitly required |
| `SimplePlanarReflection.cs` | Preserve as baseline; deprecate only after the reflection manager passes comparison |
| `CRT_RippleSimulation.shader` | Reference equation; package-owned replacement adds separate X/Z coefficients and timestep-independent controls |
| `RippleSimulation.asset` | Do not use as shared runtime state in the new package |
| `AmbientWave.asset` | Preserve until analytical waves are validated; not a package runtime dependency afterward |
| `CRT_AmbientWave.shadergraph` | Behavioural reference for the analytical package-owned wave function |
| `TopMesh.shadergraph` | Visual reference; package replacement samples runtime ripple state and analytical waves |
| `FrontMesh.shadergraph` | Visual reference; package replacement retains tint, caustics, distortion, depth fade, and seam coherence |
| `TopMesh.mat` | Immutable baseline/template reference; never mutate for per-instance state |
| `FrontMesh.mat` | Immutable baseline/template reference; never mutate for per-instance state |
| `PixelWater.cs` | Behavioural reference only; do not merge or copy its spring solver |
| `PixelWaterEditor.cs` | Workflow reference for tooltips, groups, Undo, and handles |
| `Renderer2D.asset` | Project-level reference; document or validate required sorting-texture setup |
| `UniversalRP.asset` | Project-level reference; add quality variants only when measured and explicitly required |
| `THIRD_PARTY_NOTICES.md` | Keep current; update only when distributed content or provenance findings change |

### Target file structure

These paths and names are architectural defaults. Create directories only when the active phase needs them.

```text
Assets/Water25D/
├── Runtime/
│   ├── Water25D.Runtime.asmdef
│   ├── Core/
│   │   ├── Water25DController.cs
│   │   ├── WaterMeshBuilder.cs
│   │   └── WaterRuntimeResources.cs
│   ├── Simulation/
│   │   ├── IWaterRippleSimulator.cs
│   │   ├── WaterImpact.cs
│   │   ├── CustomRenderTextureRippleSimulator.cs
│   │   ├── ComputeRippleSimulator.cs
│   │   └── WaterRipple.compute
│   ├── Physics/
│   │   ├── WaterPhysicsVolume2D.cs
│   │   ├── WaterSurfaceInteraction2D.cs
│   │   ├── WaterContactTracker.cs
│   │   └── WaterDepthAnchor.cs
│   ├── Rendering/
│   │   ├── WaterReflectionManager.cs
│   │   ├── WaterReflectionGroup.cs
│   │   ├── WaterVisibilityReporter.cs
│   │   └── WaterAmbientWaves.hlsl
│   ├── FX/
│   │   ├── WaterFXController.cs
│   │   ├── WaterFXPool.cs
│   │   ├── WaterInteraction.cs
│   │   └── WaterFXDefinition.cs
│   └── Settings/
│       ├── WaterStyleProfile.cs
│       └── WaterQualityProfile.cs
├── Editor/
│   ├── Water25D.Editor.asmdef
│   ├── Water25DEditor.cs
│   ├── WaterSceneHandles.cs
│   ├── WaterEditorValidation.cs
│   ├── WaterProjectSetupValidator.cs
│   └── Migration/
│       └── InteractiveWaterMigrationWizard.cs
├── Shaders/
├── Materials/
├── Profiles/
├── Prefabs/
├── Samples/
│   ├── Minimal/
│   └── Benchmark/
├── Tests/
│   ├── EditMode/
│   │   └── Water25D.Tests.EditMode.asmdef
│   └── PlayMode/
│       └── Water25D.Tests.PlayMode.asmdef
└── Documentation/
    ├── STATUS.md
    ├── BASELINE.md
    ├── SETUP_REQUIREMENTS.md
    ├── MIGRATION.md
    ├── API.md
    └── Benchmarks/
```

### Status tracking template

Create `Assets/Water25D/Documentation/STATUS.md` when Phase 0 starts, using this minimum structure:

```md
# Water25D Implementation Status

## Current phase

Phase number, phase name, and active task slice.

## Completed

Only work that has been implemented and validated to the stated level.

## In progress

Files and behaviour currently being changed.

## Validation evidence

Commands, Unity test runs, scenes, captures, profiler data, and dates.

## Validation still required

Exact Unity Editor, player-build, rendering, physics, or target-device checks not yet performed.

## Known issues and risks

Current defects, uncertainty, compatibility risks, and performance concerns.

## Temporary dependencies

Every temporary reference to legacy custom water, Cainos, demo content, or migration-only code, including its removal phase.

## Architectural deviations

Any deliberate departure from this plan, with reason and migration consequence.

## Next task

One bounded next implementation task, not the entire remaining roadmap.
```

### Recommended delivery summary

| Phase | Result |
|---|---|
| 0. Baseline | Reproducible source baseline, status file, setup requirements, and deterministic comparison workflow |
| 1. Structural refactor | Package-owned hierarchy and modular coordination with visual parity |
| 2. Resource ownership | Correct independent runtime state and disposal |
| 3. Authoring | Profiles, grouped Inspector, validation, Undo, and scene handles |
| 4. Physics | Separate buoyancy and crossing volumes with flat 2D gameplay surface |
| 5. Interaction | Depth-aware logical contacts, events, and immediate batched impacts |
| 6. CRT optimisation | Rectangular no-mip instance-owned state with adaptive scheduling |
| 7. Ambient optimisation | Analytical ambient waves replace the ambient CRT |
| 8. Reflection | Shared adaptive reflection groups and camera-free fallbacks |
| 9. FX | Pooled package-owned splashes, bubbles, foam, and underwater effects |
| 10. Compute experiment | Optional backend measured fairly against optimised CRT |
| 11. Migration | Reversible prefab and settings conversion workflow |
| 12. Production validation | Clean-project import, documentation, target-device benchmarks, and release acceptance |

## Benchmarking, quality tiers and acceptance criteria

### Benchmark evidence and provenance

Every recorded benchmark result must include:

- Unity version read from `ProjectSettings/ProjectVersion.txt`.
- Relevant package versions.
- Commit SHA or exact working-tree state.
- Scene and test-driver version.
- Target hardware, operating system, graphics API, resolution, and quality profile.
- Development-player or Editor context.
- Warm-up duration, capture duration, and number of frames or samples.
- Whether VSync, frame caps, deep profiling, safety checks, and GPU profiling were enabled.
- Exact water count, visible-water count, reflection-group count, impact load, FX load, and simulation configuration.
- Raw capture location or reproducible steps.

Do not compare configurations that differ in visual settings unless the difference is the subject of the test. Do not label estimated cell-update counts or texture-memory arithmetic as measured GPU cost.

Store summaries under:

```text
Assets/Water25D/Documentation/Benchmarks/
```

Large profiler captures or platform-specific binaries should be stored according to repository policy rather than committed automatically.

### Baseline benchmark scene

The benchmark must be built before optimisation so design decisions are measured against the current implementation.

Use a deterministic test scene with:

| Variable | Test values |
|---|---|
| Water bodies | 1, 4, 8 |
| Visible water bodies | All visible, half visible, none visible |
| Dynamic Rigidbody2D objects | 0, 10, 30 |
| Simultaneous impacts | 1, 8, 32 |
| Ripple resolution | 128², 256², 512², 1024² |
| Rectangular resolution | 256×64, 320×104, 512×128 |
| Propagation substeps | 1, 2, 3, 5 |
| Simulation rate | 20, 30, 60 Hz |
| Reflection | Off, approximation, planar |
| Reflection scale | 0.25, 0.5, 1.0 per dimension |
| Reflection interval | Every 1, 2, 4 frames |
| Camera Sorting Texture | None, 2×, 4× downsampling |
| Screen output | 1080p and target-device native resolution |
| FX load | None, normal play, stress burst |

Run the benchmark with fixed random seeds and scripted object trajectories so every configuration receives the same contacts.

### Metrics

Record:

```text
Main-thread frame time
Render-thread frame time
GPU frame time
Water simulation GPU time
Reflection GPU time
Water main-pass GPU time
Batches and SetPass calls
Draw calls
Rendered vertices and triangles
Render Graph pass count
Texture memory
Mesh memory
GC allocations per frame
Peak allocation during initialisation
99th-percentile frame time
Impact-to-visible-ripple latency
```

Unity’s Rendering Profiler exposes batches, SetPass calls, triangles and vertices; the Frame Debugger exposes individual rendering events; the Memory Profiler and Memory module expose texture, mesh and managed-memory usage; and Profile Analyzer assists comparison across captured frame ranges.

Profile in a development player on each intended hardware class. Editor profiling is useful for diagnostics but includes editor cameras, inspectors and other overhead that can distort visibility, memory and CPU measurements.

### Suggested initial quality profiles

These are benchmark starting points, not final promises.

| Setting | Low | Medium | High |
|---|---:|---:|---:|
| Ripple texels per unit | 8 | 16 | 24–32 |
| Minimum ripple size | 64×32 | 128×48 | 256×64 |
| Maximum ripple size | 256×64 | 512×192 | 1024×384 |
| Simulation rate | 20 Hz | 30 Hz | 60 Hz |
| Propagation substeps | 1 | 2 | 2–3 |
| Maximum impacts per step | 8 | 32 | 64 |
| Idle timeout | 1.0 s | 2.0 s | 3.0 s |
| Top mesh vertices per unit | 4–6 | 8–10 | 12–16 |
| Reflection | Stylised fallback | Planar, 0.25 scale | Planar, 0.5 scale |
| Reflection update | No camera render | Every 3–4 frames | Every 2 frames |
| Sorting texture | 4× downsample | 2× downsample | None or 2× |
| FX density | 0.5× | 1× | 1.25× |
| Ambient wave bands | 2 | 3 | 4 |

A reflection scale of `0.25` per dimension means 6.25% of full-resolution pixels; `0.5` means 25%. This is why resolution scale is a powerful cost control.

### Initial performance budget

Until target hardware is fixed, use relative budgets:

```text
Complete water feature:
    no more than 8–10% of target frame time

Steady-state managed allocations:
    0 bytes per frame

Normal play:
    no RenderTexture recreation
    no mesh regeneration
    no material instantiation
    no particle Instantiate/Destroy

Inactive off-screen water:
    near-zero simulation cost

Reflection:
    cost scales with reflection-plane groups,
    not raw water-body count
```

For a 60 Hz target, an 8% budget is about 1.33 milliseconds; for a 30 Hz target, it is about 2.67 milliseconds. This budget must include simulation, main water shading, reflection and FX, not only the ripple kernel.

### Automated functional tests

The test suite should verify:

1. Editor resizing supports Undo and Redo.
2. Width changes rebuild top/front meshes and both trigger volumes.
3. Depth changes preserve the waterline and adjust physical volume correctly.
4. Runtime resource creation does not modify template assets.
5. Two water bodies never share ripple state unintentionally.
6. A shared style profile does not imply shared mutable materials.
7. Multiple impacts in one update are all visible without FIFO delay.
8. Ripple mapping is correct at all four top-plane corners and centre.
9. World-space circular impacts remain approximately circular on rectangular water.
10. Impacts outside the bounds are rejected or clamped according to policy.
11. Rigidbody2D objects float at predictable levels across common collider shapes.
12. Multi-collider characters produce one logical entry and exit.
13. Trigger and non-trigger interaction masks work independently.
14. Entry through the side or bottom of the buoyancy volume does not create a splash.
15. Entering and exiting through the surface creates correctly directed ripples.
16. Disabling custom drag leaves only effector drag.
17. Pooled FX return to their pools and do not leak.
18. Reflection cameras cannot render themselves or other reflection cameras.
19. Coplanar waters share one reflection group.
20. Non-coplanar waters receive distinct reflection groups.
21. Reflection-disabled quality removes the reflection-camera render.
22. Invisible idle waters suspend simulation.
23. An impact wakes a suspended simulation.
24. Runtime resizing safely releases and recreates relevant resources.
25. Simulation remains finite under maximum supported strength and speed.
26. Runtime and editor assemblies compile when reference-system folders are absent from a test copy.
27. Package-owned prefabs, materials, profiles, shaders and samples contain no serialized references into reference-only folders.
28. Setup validation reports missing layers, sorting layers, renderer settings and unsupported formats without silently changing the project.
29. Migration-only assemblies are editor-only and can be removed without breaking runtime compilation.
30. A clean-project sample can be configured using only the documented setup requirements.
31. Disabling or destroying water releases generated meshes, textures, buffers, cameras and pooled ownership correctly.
32. Domain reload and Enter Play Mode configuration do not leave stale package-owned resources.
33. Quality-profile switching does not mutate shared assets or leak old runtime resources.
34. Renderer or graphics-format incompatibility produces a clear fallback or validation error rather than undefined output.
35. Package-owned code, assembly definitions, documentation, samples, prefabs, and tests use `Water25D` consistently and contain no stale alternate technical identifier.
36. Runtime types use the `Water25D` namespace or an approved nested namespace, and editor types remain in editor-only assemblies and namespaces.

### Performance acceptance tests

A release candidate should pass:

```text
Zero steady-state GC allocation
No unexpected render-texture allocation spikes
No mesh regeneration during normal gameplay
Stable 99th-percentile frame time
Measured reduction at every lower quality tier
Near-zero idle simulation work
Reflection cost proportional to active plane groups
No delayed impact backlog under the supported burst limit
No material or texture state contamination between water bodies
```

A specific comparison should establish whether compute is justified:

```text
Optimised CRT versus compute
    identical dimensions
    identical state format
    identical simulation rate
    identical substeps
    identical impacts
    identical visuals
```

Compute should become the default only if it produces a meaningful target-device improvement after its extra maintenance and compatibility cost are considered.

### Release-candidate gates

Before calling the package production-ready, verify:

```text
Architecture
    Package-owned runtime has no permanent reference-system dependency
    Runtime and editor assembly boundaries are valid
    Public API and profiles are documented

Serialization
    No missing references
    No accidental shared mutable assets
    No modified vendor GUIDs or copied vendor content

Runtime
    Correct lifecycle cleanup
    Zero steady-state managed allocation in accepted normal-play scenarios
    Stable behaviour across resize, disable, scene unload and quality changes

Rendering
    Top/front seam coherence
    Reflection recursion prevention
    Quality fallbacks work
    Required renderer configuration is validated

Physics
    Flat buoyancy remains deterministic
    Multi-collider bodies produce logical events
    Side and bottom volume entry do not create false surface splashes

Performance
    Target-device captures exist
    Lower tiers produce measured reductions
    Reflection scales by group
    Inactive water approaches zero simulation cost

Packaging
    Clean-project import passes
    Setup guide is sufficient
    Sample scene uses package-owned content
    `Water25D` naming is consistent across folders, namespaces, assemblies, types, docs, and samples
    No stale alternate technical identifiers remain
    Licensing and attribution are complete
```

### Recommended first production milestone

The first shippable hybrid should use:

```text
Current XZ top and XY front presentation
Standalone grouped custom inspector
Scene width/depth/height handles
Separate surface and buoyancy triggers
BuoyancyEffector2D
Optional custom velocity-proportional drag
Velocity-based impacts
All impacts processed per simulation update
320×104 default-medium ripple state for 20×6.5 water
RGHalf state if supported
No ripple mipmaps
30 Hz simulation
Two propagation substeps
Analytical ambient waves
Adaptive shared reflection at 0.25–0.5 scale
Pooled entry and exit splashes
Pooled bubble trails
Benchmark scene comparing original and improved prefabs
```

At 320 × 104, 30 Hz and two substeps, the raw propagation-cell workload is under 1% of the original 1024², 50 Hz, five-iteration configuration while preserving a comparable world-space cell aspect ratio.

## Risks, licensing and migration

### Risk register

| Risk | Severity | Mitigation |
|---|---|---|
| Cainos redistribution rights are unclear | Critical | Reimplement concepts in project-owned code; do not redistribute vendor assets without confirmed rights |
| Custom water provenance/licence is unresolved | Critical | Establish origin, permissions and attribution before publication |
| CRT swap behaviour remains expensive | High | Profile actual copies; provide manual ping-pong or compute backend |
| Compute unsupported or slower on some platforms | High | Runtime support checks; retain CRT fallback; benchmark target GPUs |
| Numerical instability after resolution/rate changes | High | Derive X/Z coefficients from timestep and cell size; enforce stability bound |
| Shared materials or textures leak state | High | Explicit runtime ownership and validation warnings |
| Reflection camera recursion | High | Dedicated excluded layer and manager-enforced culling |
| Many unique reflection planes recreate per-water cost | High | Quantise/group compatible planes; use fallback on minor surfaces |
| Reflection updates visibly stutter | Medium | Camera thresholds, maximum intervals and tier-specific policy |
| Shader Graph migration changes the visual result | Medium | Golden-image comparison and staged replacement |
| Camera Sorting Layer Texture unavailable in another renderer | Medium | Editor validation and documented renderer setup |
| 2D physics and visual Z disagree | Medium | `WaterDepthAnchor` and explicit lane policies |
| Multi-collider objects duplicate events | Medium | Rigidbody-level contact counting |
| Editor regeneration leaks meshes or textures | Medium | Central resource owner and disposal tests |
| Pool exhaustion creates spikes | Medium | Prewarm, hard caps and low-priority rejection |
| Analytical ambient waves increase shader ALU | Low–medium | Benchmark vertex/pixel variants; reduce bands by quality tier |
| Low-tier reflection approximation looks incorrect | Low–medium | Treat it as stylised rather than physically exact |

The repository’s own notices state that Cainos content is vendor material whose redistribution rights must be verified, that the included Lucid Editor copy carries an MIT attribution requirement, and that the custom water’s original provenance also needs confirmation. The repository intentionally has no single root licence because its content has mixed origins.

The safe implementation policy is:

- Use Cainos scripts as behavioural references.
- Reimplement inspector grouping, buoyancy integration, drag, event and FX architecture in new code.
- Do not copy vendor prefabs, textures, shaders or particle assets into a distributable package without a valid licence.
- Preserve the Lucid licence only if retaining Lucid code.
- Establish the custom system’s provenance before relicensing or publishing derivatives.

### Migration plan

**Preserve the original first.** Create a version-control branch, duplicate the existing prefab and record screenshots, settings and profiler captures. The original component should remain available during the transition.

**Introduce the new controller beside the old one.** In the first migration stage, the new controller can reference the existing mesh materials and texture assets while reproducing the same dimensions.

**Move geometry creation.** Generate top and front meshes through `WaterMeshBuilder`; compare vertex positions and rendered output with the original.

**Create runtime resources.** Stop mutating the committed CRT and render-texture assets. Instantiate runtime descriptors and bind them to per-instance or per-group materials.

**Replace simulation scheduling.** Reproduce the old solver visually, then introduce rectangular resolution, no mipmaps, adaptive rate and multi-impact batching one change at a time.

**Add the two physics volumes.** Create the full buoyancy volume and thin crossing trigger without changing rendering.

**Add profiles and inspector.** Copy current material values into a generated `WaterStyleProfile`; map the old dimensions, vertex counts, sorting layers and reflection mask into the new fields.

**Replace reflection.** Register the surface with the reflection manager, verify matching projection, then disable `SimplePlanarReflection`.

**Replace ambient CRT.** Add analytical wave functions to both shaders, compare the seam and top displacement, then remove the ambient CRT reference.

**Port FX and events.** Configure pooled replacement effects. Do not make migration depend on Cainos prefabs being present.

**Validate and remove the legacy component.** Keep an automated or editor-driven comparison report before destroying the old component.

A migration wizard should:

```text
Read old InteractiveWater settings
Create or repair the new hierarchy
Create a WaterStyleProfile from current material values
Create or assign a WaterQualityProfile
Map top/front dimensions
Map sorting layers
Map reflection mask and scale
Create buoyancy and surface triggers
Create per-instance runtime resource descriptors
Register the reflection plane
Disable, but initially retain, the old components
Produce a warnings and conversion summary
```

The conversion should be reversible until the new prefab passes functional and performance acceptance tests.

### Open decisions to resolve during implementation

These questions should be resolved with repository constraints and measurements rather than guessed in advance:

- Primary target platforms and frame-rate targets.
- Minimum supported graphics capabilities and graphics APIs.
- Whether the first release ships as an `Assets/Water25D/` package, a Unity `.unitypackage`, or later becomes a UPM package.
- Whether a screen-derived low-quality reflection is visually acceptable or a simpler authored gradient is preferable.
- Which rendering features require Camera Sorting Layer Texture and which can degrade gracefully without it.
- Whether `R16G16_SFloat` is supported on all intended targets or requires a fallback format.
- Whether compute provides enough measured benefit to ship.
- How many simultaneous visible water bodies and reflection groups the game must support.
- Whether selected gameplay objects need optional cosmetic CPU bobbing beyond flat buoyancy.
- Which project-owned art, audio, and FX assets will be distributed with the package.
- What licensing and attribution apply to any retained or derived custom-water code and assets.

Record resolved decisions in package documentation and update this plan only when the architectural contract changes.

### Final recommendation

The most defensible production path is:

1. **Modularise the existing implementation without altering its appearance.**
2. **Add the Cainos-inspired inspector, buoyancy, drag, interaction, events and pooled FX.**
3. **Replace the fixed 1024² ripple resource with a rectangular, world-relative, no-mipmap CRT.**
4. **Decouple wave behaviour from fixed timestep and iteration count.**
5. **Remove the stateless ambient-wave texture.**
6. **Centralise and adaptively schedule reflections.**
7. **Benchmark before developing or adopting compute as the default.**

This approach preserves what makes the 2.5D system distinctive—the visible XZ surface and XY underwater face—while replacing its demonstration-oriented architecture with one that is author-friendly, scalable across multiple water bodies, measurable, and suitable for long-term game production.

## How to use this plan with Codex

Give Codex one bounded phase task at a time. A good task identifies:

- The active phase.
- The exact deliverable or defect.
- The files or package area allowed to change.
- Required behaviour and non-goals.
- Validation that can run in the current environment.
- Manual Unity checks that must be reported if unavailable.

Example:

```text
Implement Phase 0 only. Create the initial Water25D package documentation,
STATUS.md, baseline inventory, naming audit, and deterministic benchmark specification.
Use `Water25D` consistently for all technical identifiers and retain `2.5D Water`
for human-facing prose.
Do not implement replacement runtime water code and do not modify the custom
or Cainos reference systems. Inspect the current project settings and list all
manual Unity baseline captures still required.
```

Do not ask Codex to “implement the plan” as one task. The plan is intentionally phased so each change can be reviewed, validated, and reverted independently.
