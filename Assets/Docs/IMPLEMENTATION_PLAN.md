# Technical Review and Implementation Plan for an Improved Unity 2.5D Water System

## Executive assessment and architecture map

This report answers the attached research brief by reviewing the custom and Cainos water implementations in `Perkiiii/InteractiveWaterSystem-main`, analysing their likely performance characteristics, and proposing a production-oriented hybrid for a Unity 6 URP side-view game. fileciteturn0file0

The repository targets Unity `6000.0.23f1` with URP `17.0.3`. Its custom water is built around two generated meshes, two Custom Render Textures, Shader Graph materials, a 2D surface trigger, and a manually rendered reflection camera. The Cainos package is a separate CPU spring-water implementation with a substantially more mature authoring, physics, interaction, and FX layer. fileciteturn20file0 fileciteturn21file0 fileciteturn37file0

The central recommendation is:

> **Retain the custom system’s two-plane 2.5D presentation, but replace its monolithic controller with modular rendering, simulation, physics, interaction, reflection, FX, profile, and editor systems. Optimise the existing Custom Render Texture solver before committing to a compute-shader rewrite.**

The best target architecture is:

```text
Water2_5D
├── Water2_5DController
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

This decoupling avoids GPU-to-CPU readback, preserves responsive 2D physics, and prevents visual resolution from determining gameplay behaviour. Unity’s `BuoyancyEffector2D` already exposes a flat surface level, density, flow, linear drag, angular drag and collider filtering, making it appropriate for a side-view game whose actual physics remain two-dimensional. citeturn3search0

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
- Trigger interaction. fileciteturn4file0

The default top surface is 20 by 6.5 world units and uses a 200 by 130 grid. That creates 26,000 vertices and 51,342 triangles before any shader displacement. The front panel uses only two rows, so its geometry cost is comparatively small. fileciteturn4file0 citeturn8calculator0turn8calculator1

The controller’s physics tick does the following:

```csharp
_rippleSimulationTexture.ClearUpdateZones();
UpdateNewRipples();
_rippleSimulationTexture.Update(
    _rippleSimulationIterationPerFrame);
```

The default iteration count is five, and the project’s fixed timestep is `0.02` seconds, meaning 50 physics updates per second. Only one queued ripple is removed and inserted during each physics tick. fileciteturn4file0 fileciteturn38file0

### Ripple-solver workload

The ripple texture is currently:

- 1024 × 1024.
- `R16G16B16A16_SFloat`.
- Double-buffered.
- Bilinearly filtered.
- Mipmapped.
- Configured to generate mipmaps.
- Updated on demand. fileciteturn11file0 fileciteturn40file0

The propagation shader samples the current texel and four direct neighbours. It stores the new height in red and the prior height in green, thereby implementing a second-order finite-difference wave solver. fileciteturn8file0

At the repository defaults, the analytical workload is approximately:

```text
1,048,576 texels
× 5 full propagation passes
× 50 physics ticks per second
= 262,144,000 propagated texels per second
```

Because each propagated texel performs approximately five state-texture reads, the solver requests roughly 1.31 billion state samples per second before counting impact passes, double-buffer operations, mip generation, top/front rendering or reflection work. These are workload counts rather than measured GPU milliseconds; actual cost depends heavily on GPU architecture, bandwidth, driver and Unity’s scheduling. citeturn8calculator2turn8calculator3

This is disproportionate for a 20 by 6.5 world-unit surface. The texture has more than one million cells while the visible mesh has only 26,000 vertices, so much of the simulated detail cannot become geometric detail on the top plane.

### Current memory footprint

The ripple and ambient textures both use graphics format 48, which Unity defines as four 16-bit floating-point channels: eight bytes per texel. The ripple solver only needs two channels for its current and previous heights. fileciteturn11file0 fileciteturn12file0 fileciteturn40file0

Approximate persistent texture memory is:

| Resource | Approximate allocation |
|---|---:|
| One 1024² RGBA-half base level | 8 MiB |
| Ripple CRT, complete mip chain and two buffers | 21.3 MiB |
| Ambient CRT, complete mip chain and one buffer | 10.7 MiB |
| 960 × 540 RGBA8 reflection plus D16 depth | 3.0 MiB |
| Combined listed texture state | About 35 MiB |

These figures assume a complete conventional mip chain and exclude alignment, driver overhead, temporary render targets and any internal copy resources. The committed formats and dimensions support the estimate, but the Unity Memory Profiler should be treated as the source of truth on each target platform. fileciteturn11file0 fileciteturn12file0 fileciteturn35file0 fileciteturn40file0 citeturn9calculator0turn9calculator1turn9calculator2turn9calculator3

A 256 × 128 two-channel half-float ping-pong simulation without mipmaps would require approximately 0.25 MiB. That is roughly a 98.8% reduction compared with the current mipmapped, double-buffered RGBA-half ripple allocation. citeturn9calculator4turn9calculator5

### Double-buffering and update zones

The controller adds two zones when a ripple is available:

- A full-texture propagation zone.
- A 1% × 1% impact zone.

Both specify `needSwap = true`. fileciteturn4file0

Unity documents that a Custom Render Texture update zone can request a buffer swap before the following zone, and that double-buffered Custom Render Textures can incur a texture copy on each swap. Unity explicitly warns that the cost becomes significant with high resolutions and frequent updates. citeturn2search0turn2search4turn2search10

Therefore, the current configuration is a serious profiling target. It should not be stated without measurement that Unity performs a particular exact number of full copies per fixed tick, because Custom Render Texture scheduling and repeated `Update(count)` calls can produce implementation-dependent command ordering. What can be stated confidently is that:

1. The solver needs previous-state isolation.
2. Both current zones request swaps.
3. The texture is large and updated frequently.
4. Unity warns that double-buffer swaps can copy full texture content.

The Frame Debugger and Render Graph diagnostics should be used to count the actual update draws and copy operations in a player-equivalent configuration. Unity’s Frame Debugger exposes individual rendering events and URP’s Render Graph diagnostics can reveal generated passes and resources. citeturn7search9turn7search10

### Mipmaps provide little value here

Generated mipmaps are not needed by the propagation equation. It samples direct neighbours one base-level texel away, so lower-resolution mip levels do not contribute to the simulation. Unity regenerates mipmaps for mipmapped render textures when automatic mip generation is enabled, adding work after rendering. fileciteturn8file0 fileciteturn11file0 citeturn2search12

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

The ambient-wave resource is another 1024² RGBA-half Custom Render Texture with generated mipmaps. Unlike the contact simulation, it is configured for continuous real-time updates and is not double-buffered. fileciteturn12file0

Its material contains only three directional wave bands with frequency, amplitude, speed and direction parameters, and its Shader Graph uses sine-wave calculations. This is a deterministic, stateless function of UV and time. fileciteturn28file0 fileciteturn31file0

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

`SimplePlanarReflection` executes in `LateUpdate`, reflects the main camera across the water plane, builds an oblique clipping matrix and calls `_reflectionCam.Render()`. It skips rendering only when the main camera is below the water surface. fileciteturn5file0

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

The current script creates or finds a reflection camera through hierarchy-index assumptions and would render independently for every water object carrying the component. fileciteturn5file0

### Main scene rendering

The custom water has at least two principal main-camera draws: the top mesh and front mesh. The actual pass and draw count can be higher because Shader Graph targets, 2D lighting, sorting and URP internals may introduce additional passes.

The 2D renderer has Camera Sorting Layer Texture enabled, no downsampling, and no custom renderer features. The front-water material can therefore sample the scene captured up to a sorting-layer boundary without installing another behind-water renderer feature. fileciteturn24file0

Unity’s 2D Renderer supports no downsampling, 2× bilinear, 4× box and 4× bilinear downsampling for the Camera Sorting Layer Texture. This provides a useful quality control for underwater distortion and tinting. citeturn3search6

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

These concerns follow directly from the controller’s current lifecycle, mesh generation, layer assignment, update-zone construction and shared-material usage. fileciteturn4file0

### Comparison with the Cainos CPU system

The Cainos implementation updates a one-dimensional spring chain rather than a two-dimensional texture. It uses eight horizontal vertices per world unit, performs several neighbour-spreading iterations, then uploads the changed mesh vertices each physics tick. Its cost grows mainly with water width and spread-iteration count, rather than width multiplied by visual depth. fileciteturn16file0 fileciteturn18file0

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

The current shader exposes a single `_Spread` coefficient and uses equal coefficients for X and Z. It also makes propagation speed depend on how many times `Update()` is repeated. This means that changing simulation frequency, texture resolution or iteration count changes the apparent physics. fileciteturn8file0

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

The current FIFO queue processes one impact per fixed tick. At 50 Hz, ten simultaneous contacts can take up to 0.2 seconds to enter the texture, which will feel visibly delayed. fileciteturn4file0

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

Unity visibility callbacks can be useful, but they should not be the sole production criterion because editor Scene cameras and other cameras can influence renderer visibility. Use an explicit gameplay-camera frustum test or a central water-visibility manager. Unity documents renderer visibility callbacks as camera-dependent, which is why an explicit camera policy is safer. citeturn3search0

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

Dispatch size is calculated from the kernel’s `numthreads` declaration and the state-texture dimensions. Unity requires runtime compute support checks through `SystemInfo.supportsComputeShaders`, and supported random-write formats should be validated per platform. citeturn3search1turn3search18turn5search4

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

A mirrored sorting-layer texture will not reproduce objects outside the main view and is not a true reflection, but this limitation is often acceptable on a low tier. The current project already enables Camera Sorting Layer Texture, so the fallback can use the existing renderer facility. fileciteturn24file0

## Proposed hybrid runtime, physics and authoring system

### Runtime module responsibilities

| Module | Responsibility |
|---|---|
| `Water2_5DController` | Holds dimensions and profiles; coordinates modules |
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
| `Water2_5DEditor` | Foldouts, validation, actions and scene handles |

### Resource ownership

The existing custom component assigns shared materials and references shared Custom Render Texture assets. This makes multiple water bodies vulnerable to shared visual settings and shared ripple state. fileciteturn4file0

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

The Cainos water automatically creates or configures a trigger collider and `BuoyancyEffector2D`, updates the effector’s surface level when the water’s fill changes, and filters participating layers. fileciteturn17file0

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

The Cainos system applies force opposite to linear velocity and torque opposite to angular velocity while a Rigidbody2D remains in the water. fileciteturn18file0

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

The current custom system uses random strength between `0.2` and `0.6`, while only direction is influenced by the Rigidbody2D’s vertical velocity. fileciteturn4file0

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

The Cainos system has mature splash selection, bubble trails, surface particles and underwater particles, but it creates splash and bubble objects through `Instantiate`. fileciteturn18file0

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

Splash selection can use object width and speed thresholds, much like the Cainos configuration. Bubble emission should scale with collider area and remain attached only while the body is submerged. Continuous surface and underwater particle systems should scale their emission regions and rates from water width, depth or volume. Cainos already uses per-unit emission scaling and resizes particle shapes when the water changes. fileciteturn17file0turn18file0

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

Each should carry the same `WaterInteraction` payload rather than only a level integer and position. Cainos currently exposes a splash UnityEvent but not a unified event model. fileciteturn16file0turn18file0

### Inspector layout

Cainos achieves its panelled layout through `FoldoutGroup` attributes, a custom Lucid Editor inspector, extensive tooltips, Undo integration and scene handles. fileciteturn16file0 fileciteturn19file0

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

All handle changes must use Unity Undo and update meshes, triggers and bounds consistently. Cainos’ current editor already demonstrates the desired Undo and scene-handle workflow for two dimensions. fileciteturn19file0

A standalone editor is preferable to making the new system depend on Lucid Editor. The included Lucid copy is MIT-licensed but archived upstream and no longer an ideal foundation for new long-term tooling; retaining the same visual organisation through standard Unity editor APIs avoids adding another package dependency. fileciteturn33file0 citeturn4search0

## Implementation roadmap and file-by-file plan

### Refactor milestone

The first code milestone should preserve the current rendered result while splitting responsibilities.

Replace the current `Init()` with:

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

Changing a colour should call only `ApplyStyleSettings()`. Changing wave speed should call only `ApplyQualitySettings()` or simulation parameter application. Changing water dimensions should rebuild geometry and volumes, then recreate the simulation texture only when its calculated resolution changes.

Avoid calling a complete initialisation path from `OnValidate`, because that can:

- Rebuild a 26,000-vertex mesh while dragging a colour field.
- Clear the simulation.
- Reallocate render textures.
- Alter shared assets.
- Disrupt reflection cameras.

### Simulation optimisation milestone

Implement an optimised CRT backend with:

```text
Rectangular world-relative resolution
Two-channel half-float state where supported
No mipmaps
No automatic mip generation
30 Hz default simulation
Two substeps default
Preallocated impact storage
All pending impacts processed per step
World-relative impact radii
Idle and visibility suspension
Per-second damping
Aspect-correct propagation coefficients
```

Use a custom accumulator rather than `FixedUpdate` as the only simulation clock:

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

The visual simulation should not be implicitly tied to the 2D physics timestep. Impacts generated during physics can simply be queued.

### Reflection milestone

Replace `SimplePlanarReflection` with a scene-level manager:

```text
WaterReflectionManager
    Dictionary<ReflectionGroupKey, ReflectionGroup>
```

A group key should include:

```text
Plane normal
Quantised plane height
Culling mask
Quality profile
Camera identity
```

Each group owns:

```text
One disabled reflection camera
One render texture
Last rendered camera pose
Last update frame
Registered visible water surfaces
Dirty state
```

The manager renders only groups that contain at least one visible surface. It must exclude reflection cameras and water layers from reflection culling to avoid recursion.

### Physics and interaction milestone

Create:

- `WaterSurfaceInteraction2D`.
- `WaterPhysicsVolume2D`.
- `WaterDepthAnchor`.
- A logical contact tracker.
- `WaterInteraction` event payloads.
- Layer filtering for trigger and non-trigger colliders.

Port the Cainos behaviour conceptually:

- Layer-filtered enter/stay/exit handling.
- Velocity-sensitive disturbance.
- Linear and angular drag.
- Buoyancy-effector configuration.
- Splash size and speed filtering.
- Bubble duration and amount scaling. fileciteturn17file0turn18file0

Do not copy the Cainos CPU spring update into the new controller.

### FX milestone

Implement a pool API:

```csharp
public interface IWaterFXPool
{
    WaterFXHandle Spawn(
        WaterFXDefinition definition,
        in WaterInteraction interaction);
}
```

Pool operations must not allocate in steady state. Prewarm sizes should be profile-driven, and pool exhaustion should either expand in a controlled manner or reject low-priority requests.

### Compute milestone

Only after the optimised CRT benchmark is complete, implement `ComputeRippleSimulator`.

The compute backend should preserve exactly the same public settings and `HeightTexture` contract so scenes do not depend on the backend.

A useful comparison is:

```text
Same water dimensions
Same texels per unit
Same simulation frequency
Same wave speed
Same damping
Same impacts
Same top/front materials
```

This isolates backend cost rather than comparing visually different configurations.

### Existing-file changes

| Existing file or asset | Proposed action |
|---|---|
| `InteractiveWater.cs` | Reduce to migration façade or replace with `Water2_5DController` |
| `SimplePlanarReflection.cs` | Deprecate and replace with `WaterReflectionManager` |
| `CRT_RippleSimulation.shader` | Add separate X/Z coefficients and remove artist-dependent timestep coupling |
| `RippleSimulation.asset` | Stop using as shared runtime state; use it only as a template or remove |
| `AmbientWave.asset` | Remove after analytical waves are validated |
| `CRT_AmbientWave.shadergraph` | Replace with shared HLSL function |
| `TopMesh.shadergraph` | Sample runtime ripple state and analytical waves; consume reflection matrix |
| `FrontMesh.shadergraph` | Use analytical seam waves and runtime ripple state; retain tint, caustics and distortion |
| `TopMesh.mat` | Treat as immutable template |
| `FrontMesh.mat` | Treat as immutable template |
| `PixelWater.cs` | Behavioural reference only; do not merge the spring solver |
| `PixelWaterEditor.cs` | Workflow reference for tooltips, groups, Undo and handles |
| `Renderer2D.asset` | Add quality-specific Camera Sorting Layer downsampling where required |
| `UniversalRP.asset` | Keep a project profile; introduce platform quality variants only when measured |

The current top material already references ambient, ripple and reflection textures, while the front material exposes the desired caustic, distortion, colour and depth properties. This means the visual migration can preserve the existing look while changing how textures and values are supplied. fileciteturn26file0 fileciteturn27file0

### New file structure

```text
Assets/Water2_5D/
├── Runtime/
│   ├── Core/
│   │   ├── Water2_5DController.cs
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
│   ├── Water2_5DEditor.cs
│   ├── WaterSceneHandles.cs
│   ├── WaterEditorValidation.cs
│   └── InteractiveWaterMigrationWizard.cs
├── Shaders/
├── Materials/
├── Profiles/
├── Prefabs/
└── Tests/
```

### Recommended delivery sequence

| Delivery stage | Result |
|---|---|
| Baseline | Original prefab and benchmark scene captured unchanged |
| Structural refactor | Same visuals, modular code and correct resource disposal |
| Authoring | Grouped inspector, tooltips, actions, warnings and scene handles |
| Physics | Separate buoyancy and crossing triggers, drag and events |
| Interaction | Velocity-sensitive, depth-aware, multi-impact ripple insertion |
| CRT optimisation | Rectangular no-mip state, adaptive rate and inactivity suspension |
| Ambient optimisation | Analytical ambient waves replace the ambient CRT |
| Reflection | Shared adaptive manager and quality fallbacks |
| FX | Pooled splashes, bubbles and continuous particles |
| Compute experiment | Optional backend measured against optimised CRT |
| Migration | Wizard and prefab conversion workflow |
| Production validation | Target-device performance and automated acceptance suite |

## Benchmarking, quality tiers and acceptance criteria

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

Unity’s Rendering Profiler exposes batches, SetPass calls, triangles and vertices; the Frame Debugger exposes individual rendering events; the Memory Profiler and Memory module expose texture, mesh and managed-memory usage; and Profile Analyzer assists comparison across captured frame ranges. citeturn3search14turn7search1turn7search3turn7search5turn7search9

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

The repository’s own notices state that Cainos content is vendor material whose redistribution rights must be verified, that the included Lucid Editor copy carries an MIT attribution requirement, and that the custom water’s original provenance also needs confirmation. The repository intentionally has no single root licence because its content has mixed origins. fileciteturn33file0

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