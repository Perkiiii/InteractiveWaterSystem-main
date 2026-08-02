# Water25D Rendering, Migration, and Production Validation

## Production decision and repository baseline

This is the authoritative implementation plan for the flat-stylized redesign. Repository completion evidence belongs in `Assets/Water25D/Documentation/STATUS.md`; this section records only the implementation foundation that determines the remaining architecture and phase order.

The package now has `FlatStylized`/`SimulatedRipples` routing; a geometrically flat XZ top with four vertices and two triangles; fixed-capacity procedural rings; qualified downward and upward waterline crossings; and fixed-capacity logical-body tracking used by both surface and buoyancy volumes. Side entry, entry from below, and stationary initial contact do not synthesize crossings. Body-keyed contact foam is implemented for `FlatStylized` only, including top/front rendering and reflection occlusion, while `SimulatedRipples` continues to route qualified impacts to the CRT. Omitted, invalid, zero, and negative impact radii now resolve through the same sanitized quality-profile radius in both modes.

The latest completed Unity validation recorded in `Assets/Water25D/Documentation/STATUS.md` is 55 passing EditMode tests and nine passing PlayMode tests. A later PlayMode attempt disconnected in the Unity MCP initialization/transport layer and produced no test result; it does not replace or invalidate the successful nine-test run. Profiling, Frame Debugger validation, target-device testing, and clean-project import remain incomplete.

Distance-spaced wakes are the next bounded implementation task. Later gaps include painterly interaction masks, consolidated rendering-property ownership, the complete stylized top/front presentation, final reflection integration, expanded splash presentation, migration tooling, and production validation. Existing controllers remain compatible with `SimulatedRipples`; genuinely new controllers use `FlatStylized`.

The selected production architecture is:

```text
Water25DController
├── WaterGeometryModule
│   ├── flat four-vertex top mesh in FlatStylized
│   └── tessellated top mesh in SimulatedRipples
├── WaterPhysicsModule
│   ├── WaterSurfaceInteraction2D
│   └── WaterPhysicsVolume2D
├── WaterSurfacePresentationModule
│   ├── fixed-capacity procedural rings
│   ├── body-keyed contact foam
│   └── distance-spaced wake segments
├── WaterRippleModule
│   └── optional CRT backend in SimulatedRipples only
├── WaterReflectionModule
│   └── registration with shared WaterReflectionManager
├── WaterRenderingModule
│   └── sole final Material Property Block writer
└── WaterFXController
    ├── entry splash pools
    ├── exit splash pools
    └── underwater/bubble pools
```

The key production decisions are firm:

| Area | Production decision |
|---|---|
| New water objects | `FlatStylized` |
| Existing water objects | Remain `SimulatedRipples` until explicitly converted |
| Flat top geometry | Four vertices, two triangles, no vertex displacement |
| Flat responsiveness | Fragment-stage normals, colour, highlights, foam and reflection distortion |
| Rings | Fixed-capacity procedural shader data |
| Contact foam | Body-keyed slots driven by qualified logical contacts |
| Wakes | Fixed-capacity distance-based capsule segments |
| CRT backend | Retained temporarily as optional legacy-compatible `SimulatedRipples` |
| Planar reflections | Shared by compatible reflection groups |
| Low-cost reflections | Stylised shader fallback without a second camera |
| Toon Water URP | Development reference only |
| Package portability | No mandatory external Asset Store dependency |

Clean-project import, allocation profiling, target-device benchmarking, and full planar-reflection validation remain release gates rather than optional follow-up work.

## Remaining four-phase production track

### Phase 1 — Wake completion and interaction validation

Implement fixed-capacity, distance-spaced wake segments for qualified moving surface contacts. Include frame-rate-independent distance accumulation, speed thresholds, direction and width handling, a controlled reversal policy, deterministic replacement at capacity, fading, top/front analytical rendering, ring/foam/wake coexistence, flat/simulated isolation, and EditMode and PlayMode tests.

Preserve the established behaviour model:

```text
Vertical crossing          -> ring or CRT impact plus splash event
Persistent surface contact -> body-keyed foam
Lateral surface movement   -> wake segments
Full submersion or exit    -> foam and wakes fade
```

This phase must not add painted textures, advanced normals, Fresnel, caustics, new reflection rendering, or splash artwork. All interaction types should function correctly at its completion even if their appearance remains mathematically clean.

### Phase 2 — Rendering ownership and painterly interactions

Make `WaterRenderingModule` the sole final `MaterialPropertyBlock` writer. It combines mode, style and quality profiles, rings, contact foam, wakes, the optional simulated ripple texture, reflection output, and top/front presentation properties. The reflection manager owns cameras and textures but exposes a snapshot for this consolidated upload path.

Add optional grayscale atlases for rings, contact foam, and wakes. Support stable effect variants and rotation, optional age-based frames, analytical bounds rejection before sampling, analytical fallbacks, placeholder or generated test masks, and profile-controlled influence. Gameplay data continues to determine centre, size, direction, age, intensity, and fade; artwork determines brush edge, gaps, asymmetry, and painterly breakup. Final hand-painted artwork is not required.

### Phase 3 — Stylized water and reflection presentation

Use the authorized local Ameye Stylized Water Shader as the direct visual base.
Copy the minimum desktop graph dependency closure through Unity's
`AssetDatabase.CopyAsset`, remap copied graph/subgraph/material references to
new package GUIDs, and keep the source graph recognizable in the package-owned
authoring assets. The copied graph's displacement node must be converted to a
flat no-op before it can be used by the flat path.

The production materials use the package-owned HLSL compatibility fork of the
copied graph's surface responsibilities because Water25D must expose fixed
interaction arrays and the shared reflection snapshot through one final
`MaterialPropertyBlock` writer. This fork is not a second visual design: it keeps
the source colour, panning, layered-normal, foam, stylized-lighting, refraction,
opacity, and Fresnel behavior, then adds the Water25D interaction/reflection
contract at the fragment stage. The exact source-to-destination mapping belongs
in `PHASE3_REFERENCE_ADAPTATION.md`.

The fork must provide:

- Top surface: source shallow/deep colour, optional bands, two scrolling detail/normal layers, Fresnel, stylized highlights, copied foam breakup, refraction controls, shared stylized/planar reflection blending and distortion, plus fixed-capacity painterly rings, contact foam, and wakes.
- Front surface: the same copied colour/normal/foam/timing language adapted to the XY front quad, underwater tint and opacity, optional Camera Sorting Layer Texture distortion, optional project-owned caustics, top-edge foam, and coherent ring/foam/wake intersections.
- Reflections: camera-free stylized fallback, shared planar output, Fresnel weighting, normal/ring/wake disturbance, foam occlusion, disabled behavior, and camera/group/lifecycle validation. The fork must not run the Ameye reflection component or create a second camera/RenderTexture.

The copied Gerstner, buoyancy, demo, renderer/pipeline, and interaction-camera
systems are excluded. `FlatStylized` remains a four-vertex, two-triangle top
with fragment-only presentation and no vertex displacement.

Do not introduce physical-wave buoyancy, mandatory scene-depth foam, mandatory
opaque/depth textures, external water packages, or an orthographic
interaction-camera stamping pipeline.

### Phase 4 — FX, tooling, migration, and production validation

Complete pooled entry/exit splash categories, small/medium/large flipbook variants, strength/width selection, stable scale/flip/timing variation, placeholder sprite sheets, and existing-effect fallbacks. Add coherent Low, Medium, and High controls for painterly masks, normals, Fresnel, highlights, flow, caustics, shafts, front distortion, reflection quality, interaction capacities, and splash quality.

Finish Inspector organization, missing-asset and renderer-configuration warnings, Camera Sorting Layer Texture and reflection-layer validation, explicit Undo-backed simulated-to-flat migration, shared-profile duplication safety, stress/comparison scenarios, lifecycle and allocation diagnostics, package-dependency scans, save/reopen/prefab tests, clean-project import instructions, and benchmark tooling.

Final painted ring/foam/wake atlases, hand-drawn splash frames, surface-detail and caustic artwork, human visual tuning, room-specific reflection setup, and target-device performance decisions remain post-implementation artistic or empirical work.

## Approved development references

The following resources are approved design and implementation references only. They are not runtime, editor, serialized, or package dependencies, and their approval does not grant permission to copy their source or assets:

- [Ameye's Stylized Water Shader](https://ameye.dev/notes/stylized-water-shader/) informs colour gradients, Fresnel response, animated normals, stylized highlights, foam layering, refraction, caustics, and reflection presentation.
- [Minions Art — Shader Graph Interactive Water](https://www.patreon.com/minionsart/posts/shader-graph-30490169) informs hand-painted ripple presentation, stable randomized rotation, expansion/fade behaviour, painterly interaction masks, distortion, edge foam, and depth-based colouring.
- [Unity — How to Make Nature Shaders with Shader Graph in 2022 LTS](https://unity.com/blog/engine-platform/nature-shaders-with-shader-graph-in-2022-lts) informs restrained stylized motion, graphic flow lines, thin highlights, foam balance, ripple presentation, planar-reflection composition, and animation-inspired art direction.

Water25D will reimplement selected concepts with project-owned C#, HLSL, shaders, profiles, tests, and artwork. Third-party code, shaders, textures, prefabs, particles, or other assets will not be copied into the distributable package without explicit licensing approval.

## Ripple simulation disposition

The current backend is a meaningful implementation, but it is neither cheap enough nor sufficiently validated to remain on the default flat-stylised path.

The interface is deliberately narrow: availability, suspension state, output height texture, dropped-impact diagnostics, impact enqueueing, ticking, reset and disposal. Gameplay does not sample the GPU state, which is an important architectural property because it prevents visual resolution or GPU readback from becoming gameplay-authoritative. See `Assets/Water25D/Runtime/Simulation/IWaterRippleSimulator.cs`.

`WaterRippleModule` owns exactly one simulator for a water object. It recreates that simulator when the water dimensions, simulation-relevant quality values or material template change, and disposes it through the instance-owned runtime resource container. See `Assets/Water25D/Runtime/Simulation/WaterRippleModule.cs` and `Assets/Water25D/Runtime/Core/WaterRuntimeResources.cs`.

The concrete implementation has the following characteristics:

| Property | Current implementation |
|---|---|
| Resolution | World-density derived |
| Default density | 16 texels per world unit |
| Minimum | 64 × 32 |
| Maximum | 512 × 192 |
| Default 20 × 6.5 water | 320 × 104 |
| Preferred format | `RGHalf` |
| Fallback | `RGFloat` |
| Colour space | Linear |
| Filtering | Bilinear |
| Wrapping | Clamp |
| Mipmaps | Disabled |
| Buffering | Double-buffered |
| Update mode | On demand |
| Default frequency | 30 Hz |
| Propagation substeps | 2 |
| Catch-up cap | 2 simulation steps per frame |
| Impact cap per step | 32 |
| Pending-impact capacity | 128 |
| Idle timeout | 2 seconds |
| Delta-time clamp | 0.25 seconds |
| Stability limit | Combined X/Z spread capped at 0.45 |
| Damping | Exponential per-second damping |

These values come directly from `WaterQualitySettings` and the resource creation path in `Assets/Water25D/Runtime/Settings/WaterQualityProfile.cs` and `Assets/Water25D/Runtime/Core/WaterRuntimeResources.cs`.

At the default 320 × 104 size, an `RGHalf` image contains 33,280 pixels at four bytes per pixel, or 133,120 bytes. The two visible state buffers therefore require at least 266,240 bytes before Unity object, alignment, driver and swap-copy overhead. The `RGFloat` fallback doubles that minimum to 532,480 bytes. This is an analytical storage estimate, not a measured GPU-memory capture.

At 30 Hz with two full propagation substeps, the default water processes an estimated 1,996,800 propagated pixel executions per second while active. The propagation shader samples the current and previous height plus four neighbours, so its shader work is materially greater than that pixel count alone suggests. Each queued impact also schedules a bounded update zone. This is an analytical workload estimate derived from `Assets/Water25D/Runtime/Settings/WaterQualityProfile.cs`, `Assets/Water25D/Runtime/Simulation/CustomRenderTextureRippleSimulator.cs`, and `Assets/Water25D/Shaders/Water25D_RippleSimulation.shader`, not a profiler measurement.

### CRT batching correction and remaining validation

The original audit identified deferred Custom Render Texture update state as the most serious backend issue. The simulator still changes `_ImpactHeight`, `_ImpactCenter`, `_ImpactRadius` and the update-zone array, calls `CustomRenderTexture.Update(1)`, then repeats that process for the next impact. See `Assets/Water25D/Runtime/Simulation/CustomRenderTextureRippleSimulator.cs`.

Unity documents that `Update()` does not execute immediately. Requested updates occur at the beginning of a subsequent frame using the then-current Custom Render Texture state. Unity specifically warns that changing an update-zone array between two `Update()` calls can cause both updates to use the second array. Double buffering also performs a texture copy on each swap, with cost dependent on resolution and update frequency. See Unity's [Custom Render Texture update and double-buffering documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/class-CustomRenderTexture-configure.html).

The current simulator schedules pending impacts through separate bounded update zones before one shared full-surface propagation update per simulation step. A completed PlayMode GPU readback confirms that one accepted impact reached the CRT state, as recorded in `Assets/Water25D/Documentation/STATUS.md`; it does not prove that multiple queued impacts produce spatially distinct results. Spatially distinct multi-impact rendering, Frame Debugger inspection, and measured cost still require validation.

The retained design target is **one immutable impact batch per simulation step**, rather than one mutable-material request per impact. The following sketch documents that intended contract rather than current progress evidence:

```csharp
const int MaximumShaderImpacts = 32;

private readonly Vector4[] _impactDataA =
    new Vector4[MaximumShaderImpacts];
// centreU, centreV, radiusU, radiusV

private readonly Vector4[] _impactDataB =
    new Vector4[MaximumShaderImpacts];
// signed strength, unused, unused, unused

private void UploadImpactBatch(int count)
{
    _material.SetInt(ImpactCountId, count);
    _material.SetVectorArray(ImpactDataAId, _impactDataA);
    _material.SetVectorArray(ImpactDataBId, _impactDataB);

    _texture.ClearUpdateZones();
    _texture.SetUpdateZones(_fullSurfaceImpactZone);
    _texture.Update(1);
}
```

The impact shader then loops over the immutable uploaded batch during a single requested pass. With a maximum of 32 impacts per simulation step, this is bounded and straightforward to test. A separate impact-stamp RenderTexture is a valid later optimisation if profiling shows that a full impact pass is too expensive, but it should not be the first correctness repair.

The second defect is idle suspension. When no impacts are pending, `_idleTime` advances only while the renderer is invisible. If the renderer is visible, idle time is reset to zero every tick, so a visible calm water never suspends. See `Assets/Water25D/Runtime/Simulation/CustomRenderTextureRippleSimulator.cs`.

The required policy is:

```text
No impacts and residual energy below threshold:
    advance idle timer regardless of visibility

Invisible:
    permit earlier suspension
    suppress presentation uploads
    preserve bounded pending impacts

Visible but calm:
    stop CRT updates after IdleTimeout
    resume immediately on a new impact
```

A reliable energy threshold cannot be obtained from the GPU without readback. The practical first implementation should therefore suspend after a conservative period derived from damping and the last accepted impact, such as:

```text
suspension delay =
    max(profile IdleTimeout, estimated decay duration)
```

Alternatively, the simulator can continue a fixed number of post-impact steps and then suspend. That produces deterministic work without GPU readback.

### World-space mapping and shader influence

`Water25DController.TryGetSurfaceUV` transforms the requested world position into the water root’s local space and maps local X/Z to `[0,1]²`, rejecting points outside the rectangular top surface. `GetInteractionWorldPosition` converts 2D gameplay X/Y to the configured top-surface depth lane. This mapping is suitable for both CRT impacts and procedural rings, although it should be centralised into the planned `WaterSurfaceMapping` value type. See `Assets/Water25D/Runtime/Core/Water25DController.cs`.

The CRT currently affects only the top shader. Its red channel is sampled in the vertex shader and multiplied by `_RippleAmplitude`; the resulting value is added to vertex Y together with analytical ambient waves. The front shader does not sample the CRT. It uses only analytical ambient displacement, multiplied by front-mesh UV Y so that the upper front edge moves with the top. See `Assets/Water25D/Shaders/Water25D_TopSurface.shader` and `Assets/Water25D/Shaders/Water25D_FrontSurface.shader`.

This produces three consequences:

1. The current simulated silhouette depends on top-mesh tessellation.
2. The front and top are connected only by the shared analytical wave function, not by CRT ripple displacement.
3. The current CRT contributes no normals, colour modulation or reflection distortion.

### Final CRT role

The final recommendation is:

> **Disable CRT allocation and execution entirely in `FlatStylized`. Retain it temporarily as an explicitly optional `SimulatedRipples` compatibility mode. Do not combine it with procedural rings by default. Add optional CRT-derived normal and reflection distortion only inside `SimulatedRipples`, after bounded multi-impact behaviour is spatially validated and profiled.**

The options compare as follows:

| Outcome | Assessment |
|---|---|
| Disable entirely in `FlatStylized` | Required. It avoids per-water RenderTextures, propagation passes and tessellated geometry. |
| Retain in `SimulatedRipples` | Required initially for non-destructive migration. |
| CRT normals/reflection without displacement | Technically useful, but only as a simulated-mode option because it still incurs the entire simulation cost. |
| CRT plus procedural rings | Disabled by default. It risks duplicated visual responses and pays both simulation and fragment-loop costs. |
| Immediate removal | Rejected because existing objects and public methods currently depend on the simulated appearance. |
| Eventual removal | Permitted after usage telemetry, migration adoption and at least one deprecation cycle. |

The simulated mode should expose separate channel weights:

```text
_CrtHeightInfluence
_CrtNormalInfluence
_CrtReflectionInfluence
```

Legacy objects begin with:

```text
height      = current RippleAmplitude
normal      = 0
reflection  = 0
```

Newly authored simulated profiles may instead use restrained values such as:

```text
height      = 0.05–0.15 world units
normal      = 0.10–0.35
reflection  = 0.004–0.015 UV
```

This preserves existing visuals while allowing a less deforming simulated style without changing the semantics of `FlatStylized`.

The original implementation-change map is retained below. Mode routing, flat resource disposal, and bounded impact processing are implemented; optional simulated-mode CRT normal/reflection influence and the remaining profiling work are not complete.

| Path | Required change |
|---|---|
| `Runtime/Rendering/WaterSurfaceMode.cs` | Add `SimulatedRipples = 0`, `FlatStylized = 1`. |
| `Runtime/Core/Water25DController.cs` | Gate `_ripple.Ensure`, ticking, reset and resource access by surface mode. |
| `Runtime/Simulation/CustomRenderTextureRippleSimulator.cs` | Replace mutable per-impact requests with one immutable batch; fix suspension; expose queue and step diagnostics. |
| `Runtime/Simulation/WaterRippleModule.cs` | Include mode in the creation/rebuild contract; ensure flat mode disposes resources. |
| `Runtime/Core/WaterRuntimeResources.cs` | Preserve current ownership; add diagnostic byte estimates if useful. |
| `Runtime/Settings/WaterQualityProfile.cs` | Retain simulation fields for compatibility; group them under simulated-mode settings. |
| `Runtime/Settings/WaterStyleProfile.cs` | Add separate CRT height, normal and reflection weights. |
| `Shaders/Water25D_RippleSimulation.shader` | Add bounded impact-array pass. |
| `Shaders/Water25D_TopSurface.shader` | Sample CRT only in simulated mode; derive optional gradient for normals/reflection. |
| `Editor/Water25DEditor.cs` | Hide simulation controls in flat mode; show compatibility warning and diagnostics in simulated mode. |
| `Tests/EditMode/WaterProductionTests.cs` | Replace the request-counter test with spatial GPU readback and distinct-impact assertions. |
| `Tests/PlayMode/WaterControllerPlayModeTests.cs` | Split flat and simulated tests; assert flat creates no CRT. |
| `Samples/Benchmark/Water25DBenchmarkDriver.cs` | Add mode selection and explicit CRT test scenarios. |

## Flat-stylised rendering and reflection integration

`FlatStylized` must remain geometrically flat at every point in its lifecycle. Responsiveness comes from fragment-stage shading, not vertex movement.

The current top shader uses transparent alpha blending, `ZWrite Off` and `Cull Off`, and supports both `Universal2D` and `UniversalForward` passes. Its simulated branch retains ambient and CRT vertex displacement. Its flat branch bypasses vertex displacement and already renders fixed-capacity procedural rings and body-keyed contact foam, while retaining the existing stylized-gradient or planar-reflection paths. See `Assets/Water25D/Shaders/Water25D_TopSurface.shader`.

The front shader also uses transparent blending, `ZWrite Off` and `Cull Off`. Its simulated branch retains ambient displacement; its flat branch remains geometrically fixed and evaluates ring intersections and contact foam at the top/front seam in addition to the shallow-to-deep colour treatment. See `Assets/Water25D/Shaders/Water25D_FrontSurface.shader`.

### Geometry and shader contract

The top vertex shader must branch on `_SurfaceMode`:

```hlsl
float3 positionOS = input.positionOS.xyz;

if (_SurfaceMode < 0.5)
{
    // SimulatedRipples
    positionOS.y += EvaluateWaterAmbientWaves(...);

    if (_RippleEnabled > 0.5)
    {
        positionOS.y +=
            SampleRippleHeight(input.uv) *
            _CrtHeightInfluence;
    }
}

// FlatStylized performs no position modification.
```

The front shader follows the same rule. In flat mode, its top edge and every lower vertex remain fixed. This gives a deterministic flat silhouette, stable sprite sorting and no dependence on mesh density.

The flat top should be exactly four vertices and two triangles. The front may remain a simple quad unless another non-displacement fragment effect requires additional vertex data. Existing tessellated mesh construction remains available only for `SimulatedRipples`.

Required render states are:

```shaderlab
Tags
{
    "RenderPipeline" = "UniversalPipeline"
    "Queue" = "Transparent"
    "RenderType" = "Transparent"
    "IgnoreProjector" = "True"
}

Blend SrcAlpha OneMinusSrcAlpha
ZWrite Off
ZTest LEqual
Cull Off
```

`ZTest LEqual` should be explicit rather than relying on the default.

### Ring, foam and wake inputs

The medium quality tier should use fixed arrays with deliberately small capacities:

```text
Rings:         8
Contact foams: 4
Wake segments: 8
```

Recommended property layout:

```hlsl
float _SurfaceMode;
float4 _WaterSize;                 // width, visual depth, inverse width, inverse depth

int _WaterRingCount;
float4 _WaterRingsA[MAX_RINGS];    // centreXZ, age01, intensity
float4 _WaterRingsB[MAX_RINGS];    // start radius, end radius, thickness, softness
float4 _WaterRingsC[MAX_RINGS];    // noise phase, secondary count, spacing, direction

int _WaterFoamCount;
float4 _WaterFoamsA[MAX_FOAMS];    // centreXZ, half width, intensity
float4 _WaterFoamsB[MAX_FOAMS];    // age01, depth01, noise phase, flags

int _WaterWakeCount;
float4 _WaterWakesA[MAX_WAKES];    // startXZ, endXZ
float4 _WaterWakesB[MAX_WAKES];    // half width, age01, intensity, noise phase

float _AmbientNormalStrength;
float _AmbientReflectionDistortion;
float _RingNormalStrength;
float _RingReflectionDistortion;
float _WakeNormalStrength;
float _WakeReflectionDistortion;
float _FoamReflectionOcclusion;
float _FresnelStrength;
float _FresnelPower;
float _HighlightStrength;
float _InteractionColourStrength;
```

The arrays should be allocated once in `WaterSurfacePresentationModule`. The rendering module uploads them only when interaction data changes or slot ages require a visual update. No ring, foam or wake creates a GameObject or draw call.

A ring contributes:

```text
annulus mask
radial pseudo-normal
small radial reflection offset
thin highlight
minor local colour shift
optional secondary annuli
```

A contact-foam slot contributes:

```text
soft noisy ellipse or capsule
high reflection occlusion
foam colour and alpha
minimal or zero reflection distortion
minor normal breakup
```

A wake contributes:

```text
oriented capsule mask
directional pseudo-normal
directional reflection offset
foam or bright trail
age-dependent taper
```

These effects must use local XZ world units, not UV distance, so rings stay circular on non-square water surfaces.

### Reflection implementation

The current `WaterReflectionModule` disposes its old registration every time `Configure` is called, then creates a new registration if Play Mode is active and reflection is not disabled. See `Assets/Water25D/Runtime/Rendering/WaterReflectionModule.cs`.

`WaterReflectionManager` groups registrations by:

```text
source camera
quantised plane height
quantised plane normal
culling mask
reflection mode
resolution scale
update interval
```

Plane height is quantised at one-centimetre increments and the normal at one-thousandth increments. Registrations with equal group keys therefore share one reflection group. See `Assets/Water25D/Runtime/Rendering/WaterReflectionGroup.cs` and `Assets/Water25D/Runtime/Rendering/WaterReflectionManager.cs`.

Stylised registrations create no reflection camera or RenderTexture. Planar groups create one camera and one `ARGB32` RenderTexture, copy the source camera, reflect its position and orientation, invert culling for the render, and apply the resulting texture and view-projection matrix to every group member. See `Assets/Water25D/Runtime/Rendering/WaterReflectionManager.cs`.

This grouping model should be retained. It gives the required scaling property: planar reflection cost grows with active compatible groups, not directly with the number of water objects.

The following production corrections are required:

| Current issue | Production correction |
|---|---|
| `Renderer.isVisible` includes Scene view and unrelated cameras | Test each registered renderer’s bounds against the configured source camera frustum. |
| Only camera position and rotation drive immediate invalidation | Also track projection matrix, orthographic size, field of view, aspect, pixel size, culling mask and camera enable state. |
| No oblique water-plane clip | Add an oblique clip plane or document and reject visible leakage during capture validation. |
| Water exclusion is inferred from top-renderer layers | Require an explicit reflection-exclusion mask covering top, front and other reflective water renderers. |
| Excluding a top-renderer layer may hide unrelated scene objects | Validate that Water25D uses a dedicated rendering layer for planar mode. |
| Last unregister destroys the manager immediately or schedules destruction | Keep one manager alive for the scene or clear `_instance` before deferred destruction and make re-registration safe. |
| Reflection texture is destroyed without an explicit release call | Call `Release()` before destruction. |
| `Camera.Render()` is used directly under URP | Test against URP’s supported render-request API and retain whichever path passes Unity 6 visual and performance validation. |
| Reflection registration writes its own MPB | Move to a single final MPB writer in `WaterRenderingModule`. |

Unity’s SRP render-request API supports URP single-camera requests and provides a pipeline-native way to render a camera outside the ordinary loop. The current `Camera.Render()` path should not be replaced speculatively, but both paths must be compared under the project’s Unity 6 and URP version. See Unity's [`RenderPipeline.SubmitRenderRequest` documentation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rendering.RenderPipeline.SubmitRenderRequest.html).

`Camera.CopyFrom` should preserve an orthographic source camera’s projection properties, but repository tests do not currently prove correct orthographic clipping, projection or texture mapping. Orthographic support therefore remains a required rendering test, not an assumed production capability.

### Single-writer Material Property Block model

At present, `WaterRenderingModule` clears and rewrites one property block, resetting reflection values to identity and disabled, while `WaterReflectionManager` later reads and modifies the block. See `Assets/Water25D/Runtime/Core/WaterRenderingModule.cs` and `Assets/Water25D/Runtime/Rendering/WaterReflectionManager.cs`.

The production model should have one final writer:

```text
WaterReflectionManager
    owns reflection cameras and RTs
    exposes immutable group output:
        texture
        view-projection matrix
        valid flag
        stylized-fallback flag
        render age

WaterSurfacePresentationModule
    owns ring/foam/wake slot data

WaterRippleModule
    exposes optional simulated texture

WaterRenderingModule
    combines all data
    writes the complete top/front MPB once
```

This eliminates one-frame property races and prevents one module from erasing another module’s values.

### Reflection response

The final top-fragment order should be:

```text
base top colour
+ ambient normal/distortion
+ ring normal/distortion/highlight
+ wake normal/distortion/trail
+ Fresnel weighting
+ stylised or planar reflection
- foam reflection occlusion
+ contact foam and boundary foam
+ final alpha
```

A suitable conceptual implementation is:

```hlsl
float3 normalWS = float3(0, 1, 0);
normalWS = ApplyAmbientNormal(normalWS, localXZ);
normalWS = ApplyRingNormals(normalWS, ringData);
normalWS = ApplyWakeNormals(normalWS, wakeData);
normalWS = normalize(normalWS);

float fresnel =
    pow(1.0 - saturate(dot(normalWS, viewDirectionWS)),
        _FresnelPower) *
    _FresnelStrength;

float2 reflectionOffset =
    ambientOffset +
    ringOffset +
    wakeOffset;

float3 reflectedColour =
    EvaluateReflection(
        input.worldPos,
        reflectionOffset,
        fresnel);

reflectedColour *=
    1.0 - foamMask * _FoamReflectionOcclusion;
```

Calm production defaults should remain subtle:

| Property | Low | Medium | High |
|---|---:|---:|---:|
| Ambient reflection distortion | 0.0010 | 0.0025 | 0.0040 |
| Peak ring distortion | 0.0040 | 0.0080 | 0.0120 |
| Peak wake distortion | 0.0030 | 0.0060 | 0.0100 |
| Ambient normal strength | 0.05 | 0.10 | 0.15 |
| Ring normal strength | 0.10 | 0.18 | 0.25 |
| Foam reflection occlusion | 0.70 | 0.85 | 0.95 |
| Fresnel strength | 0.20 | 0.30 | 0.40 |
| Fresnel power | 4.0 | 4.0 | 5.0 |
| Planar reflection strength | Disabled | 0.35 | 0.45 |
| Stylised reflection strength | 0.25 | 0.30 | 0.35 |

These are starting values for visual validation, not measured optimums.

### Stylised and disabled modes

`WaterReflectionMode.Disabled` should bind no reflection texture, no fallback and zero reflection strength.

`WaterReflectionMode.Stylized` should remain camera-free. It should use:

```text
view-angle Fresnel
top-to-back gradient
optional sky/environment tint
ambient normal variation
ring/wake distortion applied to the analytic gradient
foam occlusion
```

Because no texture is sampled, “distortion” in stylised mode means perturbing the analytic gradient and highlights rather than moving an image.

`WaterReflectionMode.Planar` uses the shared group texture and projection matrix. Reflection UVs must be clamped or faded outside valid projected bounds to prevent edge smearing.

### Top/front coherence

The top and front must share:

```text
surface colour at the waterline
foam colour
surface-line width
interaction time base
water size
waterline
surface mode
```

In flat mode, geometric coherence is guaranteed by leaving both meshes undisplaced and placing the front’s top edge exactly at the top plane.

Visual interaction coherence should use the same local-space arrays. The front shader evaluates only a narrow band near its upper edge:

```text
rings:
    evaluate the ring’s intersection with the front plane

contact foam:
    project body-centred foam by local X and fade vertically

wakes:
    display only when a wake capsule reaches the front plane
```

This avoids an arbitrary independent front animation. Most rings at the middle depth lane will remain visible only on the top until their radius reaches the front edge, which is physically coherent.

The front shader may optionally sample the 2D Renderer’s Camera Sorting Layer Texture for mild behind-water distortion. Unity documents that this texture contains sorting layers captured up to a configured foremost layer and should be sampled only after the configured capture point. It is separate from scene depth. See Unity's [2D Renderer Data documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/2DRendererData-overview.html).

Scene-colour distortion should therefore be optional and limited to:

```text
FrontDistortionStrength:
    0.000–0.003 low
    0.002–0.006 medium
    0.004–0.010 high
```

The core flat shader must not require `_CameraDepthTexture` or `_CameraOpaqueTexture`. URP creates those textures according to the corresponding pipeline or camera setting, so the flat path must degrade gracefully when they are unavailable. See Unity's [URP camera component reference](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/camera-component-reference.html).

Environmental scene-depth foam remains postponed and optional. Character contact foam must use Water25D’s authoritative contact data rather than sprite depth.

## Toon Water URP assessment

The official [Unity Asset Store listing for Toon Water URP](https://marketplace.unity.com/packages/vfx/shaders/toon-water-urp-170520) identifies Piotr T. as the publisher and lists version **1.19**, released on **17 July 2022**, with an original Unity version of **2019.3.0**. The listing does not declare Unity 6, URP 17 or Render Graph compatibility.

Public feature descriptions associated with asset ID `170520` state that the package includes:

```text
URP 7.2.1 or later support
desktop and mobile Shader Graphs
shader function source
toon specular highlights
toon foam
depth-based colour
custom transparency
Fresnel
world-position UVs
orthographic-camera support
desktop planar reflections
a custom desktop-shader Inspector
```

The [Asset Store listing](https://marketplace.unity.com/packages/vfx/shaders/toon-water-urp-170520) also describes the package's planar reflections and custom Inspector. Those claims must be verified in a licensed prototype before any compatibility conclusion is made for this project.

There is no credible public evidence that version 1.19 has been updated for:

```text
Unity 6000.5
URP 17.5
Unity 6 Render Graph
the URP 2D Renderer
Camera Sorting Layer Texture
Water25D’s shared reflection groups
Water25D’s XZ-top and XY-front dual-plane model
Water25D’s fixed procedural interaction arrays
```

Unity 6’s recommended route for new custom URP rendering work is the Render Graph API; Unity states that non-Render-Graph compatibility paths are no longer being developed or improved. An asset last updated in July 2022 should therefore not be assumed Render Graph compatible without a licensed source inspection and executable prototype. See Unity's [URP compatibility-mode documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/compatibility-mode.html).

### Fit with Water25D

| Water25D requirement | Toon Water URP fit |
|---|---|
| Flat XZ top surface | Likely achievable |
| Separate XY front surface | Not demonstrated |
| Custom ring arrays | Requires graph modification |
| Body-keyed contact foam | Requires graph modification |
| Wake capsule arrays | Requires graph modification |
| Water25D reflection texture and matrix | Requires removal or bypass of its own reflection implementation |
| Shared reflection grouping | Conflicts with a per-material or per-water reflection script unless disabled |
| CRT optional input | Requires graph modification |
| 2D Renderer passes | Not established |
| Camera Sorting Layer Texture | Not established |
| Unity 6 Render Graph | Not established |
| Core compilation when absent | Requires strict optional isolation |
| Redistributable package source | Not permitted as a Water25D dependency bundle |

Shader Graph source makes it technically possible for a licensed developer to add custom properties, arrays through custom functions, or alternate reflection input. It does not make the integration inexpensive. Supporting Water25D would still require:

```text
custom ring/foam/wake functions
Water25D MPB property names
external reflection-texture support
disabling the asset’s reflection script
a separate front shader
URP 2D Renderer pass validation
Unity 6 graph upgrades
mobile variant testing
```

The package’s own planar-reflection script should never run alongside `WaterReflectionManager`. Doing so would duplicate reflection cameras, produce incompatible ownership and undermine compatible-plane sharing.

### Licensing

The [Asset Store listing](https://marketplace.unity.com/packages/vfx/shaders/toon-water-urp-170520) identifies the applicable standard licence, and Unity's [Asset Store Terms and EULA](https://unity.com/legal/as-terms) define per-seat Extension Asset rules and restrictions on distributing or commercialising raw assets outside the permitted licensed-product terms. Those terms do not provide a basis for redistributing the package's source Shader Graphs, scripts or textures as Water25D package content.

A core Water25D package that requires Toon Water URP would therefore create:

```text
seat-management obligations
missing-dependency compilation risk
licensing friction for every package consumer
uncertain Unity 6 compatibility
an external reflection architecture conflict
a second shader-authoring workflow
```

### Firm recommendation

> **Use Toon Water URP only as a privately licensed development and Shader Graph reference. Do not make it a dependency, do not ship its content, and do not implement an adapter in the first production release.**

The outcome comparison is:

| Option | Decision |
|---|---|
| Do not use it at all | Too restrictive; visual comparison may still be useful. |
| Development reference | **Selected.** |
| Optional adapter | Postponed until the core renderer ships and a licensed Unity 6 prototype proves value. |
| Required dependency | Rejected. |

A future optional adapter is acceptable only if all of the following are true:

```text
the core package already works independently
the adapter lives outside Assets/Water25D core
the adapter has its own asmdef
compilation is gated by an explicit scripting define
no core asset serializes a reference to Toon Water URP
its reflection code is disabled
the licensed user installs Toon Water URP separately
Unity 6, URP 17 and 2D Renderer tests pass
```

## Non-destructive migration and file changes

The migration must distinguish between **existing serialized controllers** and **newly created controllers**.

The surface-mode enum should be:

```csharp
public enum WaterSurfaceMode
{
    SimulatedRipples = 0,
    FlatStylized = 1
}
```

Zero is deliberately assigned to `SimulatedRipples`. Existing serialized controllers do not contain the new field, so their safe fallback is the legacy-compatible path. The field must not have a `FlatStylized` initializer.

New controllers become flat through `Reset()`, the creation menu and editor default assignment:

```csharp
private void Reset()
{
    _surfaceMode = WaterSurfaceMode.FlatStylized;
    _serializedDataVersion = CurrentSerializedDataVersion;
    EnsureModules();
    ApplyAuthoringChanges();
}
```

`Reset()` is appropriate for newly added or explicitly reset components; it is not relied upon for loading old scenes.

### Behaviour after update

| Asset or consumer | Behaviour after package update |
|---|---|
| Existing controller in a scene | Remains visually simulated |
| Existing controller in a prefab | Remains visually simulated |
| Existing prefab instance | Keeps its current profile, material, reflection and FX overrides |
| New controller from menu | Uses `FlatStylized` and new flat profiles |
| New controller via `AddComponent` | `Reset()` assigns `FlatStylized` |
| Existing style profile | Retains all current values and GUID |
| Existing quality profile | Retains simulation values and GUID |
| Existing top/front materials | Keep the same shader names and references |
| Existing ripple material | Retained for simulated mode |
| Existing reflection configuration | Retained |
| Existing FX definitions | Retained and used as fallback variants |
| Existing public ripple calls | Continue to compile |
| Missing Toon Water URP | No compile, import or runtime effect |

The current profile and material assets must not be repurposed into the new visual defaults. Existing consumers may share them across many scenes, so changing their values would silently alter every object that references them.

Create new defaults:

```text
Assets/Water25D/Profiles/Water25D_FlatStylizedStyle.asset
Assets/Water25D/Profiles/Water25D_FlatMediumQuality.asset
```

Existing assets remain legacy-compatible:

```text
Assets/Water25D/Profiles/Water25D_DefaultStyle.asset
Assets/Water25D/Profiles/Water25D_MediumQuality.asset
```

The new style asset may use the existing package top/front material templates because the shader names are retained and the mode is selected through MPB data.

### Profile and controller versions

Add hidden serialized versions:

```csharp
[SerializeField, HideInInspector]
private int _serializedDataVersion;
```

The same concept should be applied to `WaterStyleProfile` and `WaterQualityProfile`.

Recommended versions:

```text
Controller version 0:
    pre-flat architecture

Controller version 1:
    explicit WaterSurfaceMode
    flat presentation settings available

Style profile version 0:
    current profile

Style profile version 1:
    ring, foam, wake, Fresnel and distortion fields

Quality profile version 0:
    current profile

Quality profile version 1:
    ring, foam, wake capacities and presentation suspension
```

Do not perform destructive automatic conversion in `OnValidate`. `OnValidate` may sanitise values, but should not switch an existing object to flat mode or replace shared profile references.

An explicit editor migration action should:

1. Discover selected scene objects, prefab roots or project prefabs.
2. Display the number of affected controllers and shared profiles.
3. Record Undo for scene and prefab objects.
4. Optionally duplicate shared profiles before changing them.
5. Set `FlatStylized`.
6. Assign flat-profile defaults only when requested.
7. Preserve reflection, physics, sorting, dimensions, layers, events and FX.
8. Clear or disable CRT resources at runtime.
9. Record the new data version.
10. Save only when the user confirms the migration action.

### Public API compatibility

The current public method:

```csharp
CreateContactRippleAt(
    Vector3 worldPosition,
    float initialStrength,
    bool initialUp,
    float radius)
```

must remain source-compatible. See `Assets/Water25D/Runtime/Core/Water25DController.cs`.

Its routing becomes:

```text
SimulatedRipples:
    enqueue CRT impact

FlatStylized:
    create procedural ring through WaterSurfacePresentationModule
```

Add a mode-neutral API:

```csharp
public bool CreateSurfaceImpactAt(
    Vector3 worldPosition,
    float strength,
    bool initialUp = true,
    float radius = -1f);
```

`CreateContactRippleAt` forwards to the new method. It should not become a compile-time error in the first migration release.

Recommended deprecation stages:

| Release stage | Treatment |
|---|---|
| Introduction | Method remains supported and undocumented as deprecated. |
| Next minor release | `[Obsolete(message, false)]`; forwards normally. |
| Next major release | Review usage and migration adoption. |
| Later major release | Remove only if package consumers have a replacement and simulated mode has been formally retired. |

`WaterInteractionEventType` values must remain in their current order. New values such as `SurfaceContact` are appended. Existing UnityEvents remain unchanged. New event payload fields are additive.

### `FormerlySerializedAs`

Use `[FormerlySerializedAs]` only for fields that are genuinely renamed. It is not needed for:

```text
new _surfaceMode
new data-version fields
new presentation capacities
new ring/foam/wake values
```

Retain the current field names where practical:

```text
_enableRippleSimulation
_rippleSimulationMaterialTemplate
_styleProfile
_qualityProfile
_reflectionMode
_reflectionCameraSource
_reflectionCullingMask
_reflectionResolutionScale
_reflectionUpdateIntervalFrames
_reflectionStrength
_splashDefinition
_bubbleDefinition
_maximumFxPoolSize
```

In flat mode, `_enableRippleSimulation` is interpreted as a simulated-mode compatibility setting and hidden from the normal flat Inspector. It is not immediately renamed or deleted.

### Prefab override handling and rollback

The migration tool must modify the smallest appropriate ownership level:

```text
Prefab asset selected:
    edit prefab contents and preserve child-reference GUIDs

Prefab instance selected:
    default to instance override
    offer explicit apply-to-prefab action separately

Scene object selected:
    create scene-level serialized changes only

Shared profile selected:
    warn that all consumers will change
    offer duplicate-and-reassign
```

Rollback consists of:

```text
set mode back to SimulatedRipples
restore the legacy style/quality profile references
retain all original ripple settings
retain original material references
recreate CRT state on next Play Mode authoring application
```

Active ripple state itself is transient and cannot be restored across a mode switch, which is acceptable.

### File-by-file migration table

This table intentionally preserves the full redesign change map. The surface-mode enum, flat geometry, mode-gated CRT lifecycle, procedural-ring storage/rendering, qualified crossings, shared logical-body tracking, and flat-only body-keyed contact foam entries describe completed foundation work. Wake storage/rendering, painterly masks, final rendering ownership, migration tooling, and the later production gates remain planned work.

| Exact path | Change | Serialized-data risk |
|---|---|---|
| `Assets/Water25D/Runtime/Rendering/WaterSurfaceMode.cs` | Add enum with simulated mode at zero. | High if numeric order changes later. |
| `Assets/Water25D/Runtime/Core/Water25DController.cs` | Add mode and version fields; add presentation module; mode-gate geometry/ripple; preserve public API. | High; central migration surface. |
| `Assets/Water25D/Runtime/Core/WaterGeometryModule.cs` | Select flat or tessellated top mesh; remove waterline-only mesh invalidation. | Low; generated meshes are transient. |
| `Assets/Water25D/Runtime/Core/WaterMeshBuilder.cs` | Add four-vertex flat top and optional flat front builders. | None for existing serialized assets. |
| `Assets/Water25D/Runtime/Core/WaterRenderingModule.cs` | Become sole final MPB writer; bind all presentation/reflection data. | Medium; material references must remain untouched. |
| `Assets/Water25D/Runtime/Core/WaterRuntimeResources.cs` | Preserve resource ownership; ensure reflection/ripple resources release explicitly. | Low. |
| `Assets/Water25D/Runtime/Rendering/WaterSurfacePresentationModule.cs` | New fixed-capacity ring, foam and wake owner. | None; runtime-only. |
| `Assets/Water25D/Runtime/Rendering/WaterSurfaceRenderData.cs` | New preallocated shader data structure. | None. |
| `Assets/Water25D/Runtime/Rendering/WaterShaderIds.cs` | Add mode, array, Fresnel and distortion IDs. | None. |
| `Assets/Water25D/Runtime/Rendering/WaterReflectionModule.cs` | Avoid unnecessary unregister/register; consume reflection snapshots. | Low. |
| `Assets/Water25D/Runtime/Rendering/WaterReflectionManager.cs` | Camera-aware visibility, invalidation, clipping, cleanup and output state. | Medium; runtime behaviour changes. |
| `Assets/Water25D/Runtime/Rendering/WaterReflectionGroup.cs` | Preserve grouping keys; add projection-relevant key data only if needed. | Low. |
| `Assets/Water25D/Runtime/Simulation/WaterRippleModule.cs` | Mode-gate creation and disposal. | Low. |
| `Assets/Water25D/Runtime/Simulation/CustomRenderTextureRippleSimulator.cs` | Batch impacts and repair suspension. | Medium; simulated visual differences possible. |
| `Assets/Water25D/Runtime/Simulation/IWaterRippleSimulator.cs` | Add diagnostics only if required; preserve existing members. | Public API risk if members are removed. |
| `Assets/Water25D/Runtime/Physics/WaterInteractionEvent.cs` | Append explicit surface mapping, width, speeds, body ID and flags. | Medium; append-only required. |
| `Assets/Water25D/Runtime/Physics/WaterSurfaceInteraction2D.cs` | Qualify actual top crossings; fixed logical-body tracking; contact samples. | High behavioural importance. |
| `Assets/Water25D/Runtime/Physics/WaterPhysicsVolume2D.cs` | Share contact tracking and clean invalid bodies. | Medium. |
| `Assets/Water25D/Runtime/Physics/WaterLogicalBodyContactTracker.cs` | Reusable fixed-capacity logical-body tracker used by surface and buoyancy volumes; implemented foundation for wake ownership. | None. |
| `Assets/Water25D/Runtime/Settings/WaterStyleProfile.cs` | Add versioned ring, foam, wake, Fresnel and distortion values. | Medium; absent fields require safe migration defaults. |
| `Assets/Water25D/Runtime/Settings/WaterQualityProfile.cs` | Add versioned presentation capacities and suspension policy; retain CRT settings. | Medium. |
| `Assets/Water25D/Runtime/FX/WaterFXController.cs` | Add distinct entry/exit and size variants. | Medium; preserve existing splash fallback. |
| `Assets/Water25D/Runtime/FX/WaterFXDefinition.cs` | Add optional classification metadata without renaming old fields. | Low. |
| `Assets/Water25D/Runtime/FX/WaterFXPool.cs` | Preserve prewarming; add diagnostics and deterministic exhaustion policy. | Low. |
| `Assets/Water25D/Shaders/Water25D_TopSurface.shader` | Flat branch, procedural interactions, Fresnel, reflection distortion and CRT channels. | High visual risk; shader name must not change. |
| `Assets/Water25D/Shaders/Water25D_FrontSurface.shader` | Flat branch, seam evaluation and optional sorting-layer colour distortion. | High visual risk; shader name must not change. |
| `Assets/Water25D/Shaders/Water25D_RippleSimulation.shader` | Immutable bounded impact batch. | Medium simulated-mode risk. |
| `Assets/Water25D/Materials/Water25D_Top.mat` | Add conservative defaults; preserve GUID and shader reference. | Medium if old material defaults are overwritten. |
| `Assets/Water25D/Materials/Water25D_Front.mat` | Add seam and flat defaults; preserve GUID. | Medium. |
| `Assets/Water25D/Profiles/Water25D_DefaultStyle.asset` | Preserve legacy values and GUID. | High if altered. |
| `Assets/Water25D/Profiles/Water25D_MediumQuality.asset` | Preserve simulation values; add safe version fields. | High if altered destructively. |
| `Assets/Water25D/Profiles/Water25D_FlatStylizedStyle.asset` | New default style. | None. |
| `Assets/Water25D/Profiles/Water25D_FlatMediumQuality.asset` | New default quality profile. | None. |
| `Assets/Water25D/Editor/Water25DEditor.cs` | Mode-specific controls, diagnostics and previews. | Low runtime risk; high authoring importance. |
| `Assets/Water25D/Editor/Water25DEditorDefaults.cs` | Assign flat defaults only to genuinely new/missing configurations. | High if it mutates old loaded scenes. |
| `Assets/Water25D/Editor/Water25DMenu.cs` | Create new controllers explicitly in flat mode. | Low. |
| `Assets/Water25D/Editor/Water25DMigration.cs` | New Undo-backed migration and rollback actions. | High; requires extensive tests. |
| `Assets/Water25D/Editor/Water25DValidation.cs` | Add mode/resource/dependency/reflection checks. | Low. |
| `Assets/Water25D/Samples/Benchmark/Water25DBenchmarkDriver.cs` | Add scenario selection and measured counters. | None. |
| `Assets/Water25D/Samples/Water25D_VisualValidation.unity` | Add side-by-side legacy and flat objects. | Sample-only. |
| `Assets/Water25D/Tests/EditMode/WaterProductionTests.cs` | Replace scheduling-only CRT test; expand reflection and pool tests. | None. |
| `Assets/Water25D/Tests/PlayMode/WaterControllerPlayModeTests.cs` | Separate flat and simulated expectations. | None. |
| `Assets/Water25D/Documentation/STATUS.md` | Record only completed measurements and validation. | Documentation accuracy risk. |
| `Assets/Water25D/Documentation/PORTABILITY.md` | Add clean-project and forbidden-reference procedure. | None. |
| `Assets/Water25D/Documentation/FLAT_STYLIZED.md` | New architecture, authoring and migration guide. | None. |

## Validation, profiling and release acceptance

The existing benchmark driver can create up to 32 waters and 64 dynamic bodies and issue deterministic impacts, but it currently records no profiler measurements and primarily targets CRT impacts. See `Assets/Water25D/Samples/Benchmark/Water25DBenchmarkDriver.cs` and `Assets/Water25D/Documentation/STATUS.md`.

The latest completed validation comprises 55 passing EditMode tests and nine passing PlayMode tests. Coverage now includes flat/simulated routing, flat geometry, procedural rings, qualified crossings, logical-body contact tracking, body-keyed flat-mode foam, lifecycle paths, and preserved simulated CRT behaviour. A later Unity MCP-disconnected PlayMode attempt returned no result and is not counted. Rendering appearance, Frame Debugger behaviour, allocation profiling, target-device performance, and clean-project portability remain unverified.

The validation plan must therefore be executable and divided into behavioural, rendering, performance and portability gates.

### EditMode tests

| Test file | Required assertions |
|---|---|
| `WaterSurfaceModeSerializationTests.cs` | Enum numeric values remain stable; old serialized controller resolves to simulated; new controller through `Reset` and menu resolves to flat. |
| `WaterSurfaceMigrationTests.cs` | Existing profile/material/reflection/FX references survive migration; rollback restores simulated mode; shared profiles are duplicated only when requested. |
| `WaterMeshBuilderTests.cs` | Flat top has four vertices and two triangles; every top Y is identical; simulated mesh remains deterministic. |
| `WaterRenderingPropertyTests.cs` | Flat mode binds zero vertex-displacement influence; required arrays and counts are clamped; reflection and presentation properties coexist in one MPB. |
| `WaterSurfaceMappingTests.cs` | Correct world/local/UV mapping for translated, rotated and scaled roots; outside points are rejected. |
| `WaterSurfacePresentationTests.cs` | Ring radius, fade and boundary behaviour; foam slot keying; wake spacing and reversal; deterministic exhaustion. |
| `WaterRippleSimulationTests.cs` | Flat mode allocates no CRT; simulated mode chooses correct dimensions/format; two or more impacts create spatially distinct results; catch-up cap and suspension operate correctly. |
| `WaterReflectionGroupingTests.cs` | Compatible planes share a group; incompatible camera/plane/mask/quality values do not; disabled/stylised modes create no planar resources. |
| `WaterReflectionLifecycleTests.cs` | Reconfigure-last-registration race is absent; RTs release; manager survives or recreates safely; orthographic key/projection changes invalidate correctly. |
| `WaterFXPoolTests.cs` | Prewarm capacity is fixed; exhaustion is deterministic; returned entries are reused; steady-state spawns do not instantiate. |
| `WaterPackageDependencyTests.cs` | Every `AssetDatabase.GetDependencies` result is under `Assets/Water25D/`, an allowed Unity package, or an explicitly approved project setting. |
| `WaterOptionalIntegrationTests.cs` | Core assemblies compile without Toon Water URP, defines or serialized references. |
| `WaterEditorMigrationTests.cs` | Undo, redo, prefab-stage handling, multi-selection and profile duplication are safe. |

The package-dependency test should scan every package asset rather than relying on source-text scans alone:

```csharp
foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
{
    if (!assetPath.StartsWith("Assets/Water25D/"))
        continue;

    foreach (var dependency in
             AssetDatabase.GetDependencies(assetPath, true))
    {
        Assert.That(
            IsAllowedDependency(dependency),
            $"Forbidden dependency: {assetPath} -> {dependency}");
    }
}
```

### PlayMode tests

The current surface interaction component tracks logical `Rigidbody2D` bodies through fixed-capacity aggregate contact state and qualifies actual downward and upward crossings in `FixedUpdate`. Side entry, entry from below, and stationary initial contact do not synthesize crossings. Multiple colliders contribute to one logical body, and contact foam is keyed to that body in `FlatStylized`. The scenario matrix below remains the broader production acceptance set; wake rows and the unperformed rendering/lifecycle checks are still outstanding.

Required PlayMode scenarios are:

| Scenario | Acceptance result |
|---|---|
| Two colliders on one Rigidbody enter from above | One logical entry, one ring, one entry splash. |
| Colliders leave on separate frames | One logical exit only after the last eligible collider leaves. |
| Side entry into the thin trigger | No surface ring or splash. |
| Entry from below | No entry splash; optional submerged event remains separate. |
| Upward top crossing | One exit ring and one exit splash. |
| Rest at waterline | One persistent foam slot; no repeated crossings. |
| Slow lateral movement | No wake emissions below threshold. |
| Fast lateral movement | Wake segments appear at distance intervals independent of frame rate. |
| Direction reversal | Wake accumulator resets or produces one controlled transition. |
| Full submersion | Contact foam fades; underwater FX may remain. |
| Thirty-two impacts | Capacity and exhaustion policy are deterministic. |
| Flat mode | No `CustomRenderTexture`, no tessellated top requirement and no vertex displacement. |
| Simulated mode | CRT remains functional and resizes safely. |
| Runtime resize | Old meshes and CRT are replaced and released; presentation slots clear or remap according to policy. |
| Disable and re-enable | All registrations and resources recreate without duplicates. |
| Destroy controller | Pools, meshes, materials, CRT and reflection registrations are released. |
| Scene unload | No hidden reflection manager, camera or runtime RT leaks. |
| Play Mode exit/domain reload | No `HideAndDontSave` package objects survive. |
| Two water instances | No mutable material, ring array, CRT or reflection registration is shared accidentally. |

Resource-lifecycle tests should record object counts before and after the scenario using Unity’s object discovery APIs and force a frame boundary where `Destroy` is deferred.

### Rendering captures

Create deterministic comparison captures with a fixed camera, fixed exposure, fixed render scale and fixed random seed:

| Capture | Required comparison |
|---|---|
| Calm silhouette | Flat top and front remain perfectly straight. |
| Stylised reflection | Responsive gradient/Fresnel without a reflection camera. |
| Planar reflection | Correct image orientation, clipping and strength. |
| Disabled reflection | No residual reflection texture or fallback. |
| Centre ring | Circular, readable and subtle. |
| Boundary rings | Correct clipping at all four top edges. |
| Contact foam | Centred on aggregate logical-body contact and scaled by width. |
| Wake direction | Trail aligns with movement in both directions. |
| Entry splash | Spawned at qualified downward crossing. |
| Exit splash | Spawned at qualified upward crossing. |
| Top/front seam | Matching colour and foam with no geometric gap. |
| Orthographic camera | Correct planar projection and transparent sorting. |
| Perspective camera | Correct Fresnel and reflection projection. |
| Sprite sorting | Sprites before, inside and behind water render in documented order. |
| Quality tiers | Low, medium and high retain the same style with expected detail reductions. |
| Flat versus simulated | Flat has no silhouette displacement; simulated preserves legacy behaviour. |

Unity’s 2D Renderer sorting order depends on sorting layer/order, render queue, camera distance and sorting groups. Orthographic and perspective modes use different distance interpretations, so both must be captured. See Unity's [2D Renderer sorting documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/2d-renderer-sorting.html).

Image tests should use perceptual or thresholded comparisons rather than exact pixel equality. Shader and GPU differences across platforms make exact images too fragile for a portable package.

### Performance scenarios

Profile at least the following matrix:

| Scenario | Modes |
|---|---|
| One visible idle water | Flat/stylised, flat/disabled, simulated/stylised |
| One interacting water | Medium maximum active interactions |
| Four visible waters | Shared reflection plane and separate reflection planes |
| Eight waters, four off-screen | Suspension and camera-specific visibility |
| Thirty-two simultaneous impacts | Flat rings and simulated impact queue |
| Maximum rings | Low, medium, high caps |
| Maximum contact foams | Low, medium, high caps |
| Maximum wakes | Low, medium, high caps |
| Maximum splash effects | All pools active |
| Reflection disabled | Baseline |
| Stylised reflection | Camera-free baseline |
| Planar reflection | One group, four compatible groups |
| FlatStylized | All quality tiers |
| SimulatedRipples | All supported simulation tiers |
| Toon Water prototype | Private comparison only if licensed and Unity 6 compatible |

Record:

```text
main-thread frame time
render-thread frame time
GPU frame time
water-only GPU markers
CRT simulation GPU time
reflection camera GPU time
draw calls
batches and SetPass calls
Render Graph pass count
RenderTexture memory
buffer/uniform memory
managed allocations
native object creation
pool expansion count
dropped/reclaimed effect count
impact-to-visible-effect latency
99th-percentile frame time
```

Capture at least:

```text
300 warm-up frames
1,800 measured frames at 60 Hz target
median
95th percentile
99th percentile
maximum
```

Do not include editor repaint or profiler-connection overhead in final player measurements. Use Development Players for detailed markers and non-Development Players for final representative performance.

### Starting quality tiers

| Setting | Low | Medium | High |
|---|---:|---:|---:|
| Active rings | 4 | 8 | 16 |
| Contact foam slots | 2 | 4 | 8 |
| Wake segments | 4 | 8 | 16 |
| Visual bodies tracked | 4 | 8 | 16 |
| Wake emissions per physics step | 1 | 2 | 4 |
| Reflection mode default | Stylised | Stylised | Planar optional |
| Planar resolution scale | Disabled | 0.25 | 0.50 |
| Planar update interval | N/A | 3 frames | 1–2 frames |
| Front scene-colour distortion | Off | Optional | Optional |
| Environmental depth foam | Off | Off | Experimental |
| Simulated texels/unit | 8 | 16 | 24 |
| Simulated maximum resolution | 256 × 96 | 512 × 192 | 768 × 288 |
| Simulated frequency | 20 Hz | 30 Hz | 45 Hz |
| Simulated propagation substeps | 1 | 2 | 2 |
| Simulated impacts/step | 8 | 32 | 32 |
| Simulated pending impacts | 32 | 128 | 256 |

The simulated values apply only when a user explicitly selects `SimulatedRipples`; they are not part of the flat path.

### Measurable release budgets

Because the repository does not identify target hardware, absolute budgets must be confirmed on the project’s designated low, median and high target devices. The following are starting gates for a 60 Hz game, not claims about current measured performance.

For a median desktop reference at 1920 × 1080:

| Scenario | Main-thread budget | GPU budget |
|---|---:|---:|
| One idle flat/stylised water | ≤ 0.05 ms | ≤ 0.20 ms |
| One interacting flat water at medium caps | ≤ 0.15 ms | ≤ 0.45 ms |
| Four flat/stylised waters | ≤ 0.25 ms | ≤ 1.00 ms |
| One quarter-resolution planar group | ≤ 0.15 ms CPU submission | ≤ 1.50 ms peak |
| Eight waters, half off-screen | Off-screen half adds ≤ 0.05 ms total CPU | No measurable water GPU work for hidden surfaces |
| Thirty-two procedural impacts | ≤ 0.25 ms impact-processing frame | ≤ 0.75 ms incremental water GPU |
| One medium simulated water | Must be at least 20% more expensive than or equal to flat; otherwise investigate measurement validity | Explicitly measured and reported |

For a median mobile reference at 1280 × 720:

```text
Stylised reflection is the default.
Planar reflection is disabled by default.
Medium caps are reduced to Low where required.
One idle flat water should remain below 0.15 ms GPU.
One interacting flat water should remain below 0.35 ms GPU.
Four visible flat waters should remain below 1.0 ms total water GPU.
```

Universal release gates are:

| Requirement | Gate |
|---|---|
| Managed allocation | 0 bytes per steady-state frame after warm-up |
| Mesh generation | No normal-play regeneration; only explicit resize/mode change |
| RenderTexture creation | No normal-play recreation; only mode, size, camera-resolution or quality change |
| Material creation | No normal-play creation |
| Effect objects | No routine `Instantiate` or `Destroy` after pool warm-up |
| Draw calls | Ring, foam and wake counts do not add surface draw calls |
| Off-screen work | No reflection render, no presentation upload and no CRT propagation after suspension |
| Reflection scaling | Render count equals active compatible groups, not water count |
| Effect latency | Impact visible within one rendered frame; no more than 33 ms at 60 Hz |
| Pool expansion | Zero after warm-up |
| Flat versus simulated | Flat is at least 20% cheaper in water GPU time or saves a documented minimum of 0.20 ms for one representative interacting water |
| Frame-time regression | Expired effects return within 10% of the matching idle 99th-percentile frame time |
| Resource cleanup | Zero leaked package cameras, RTs, meshes, materials or hidden GameObjects |
| Clean import | Zero compile errors and all tests pass in a clean compatible project |
| External references | Zero forbidden dependencies outside the package and approved Unity packages |

The “20% cheaper” gate should be evaluated against the same dimensions, camera, reflection mode and interaction schedule. If simulated mode happens to be dormant while flat mode is displaying maximum interactions, that is not a valid comparison.

### Clean-project portability procedure

Create a fresh Unity 6 project using the minimum supported Unity and URP versions, then:

```text
copy Assets/Water25D and all .meta files
install only declared Unity package dependencies
do not install Cainos, legacy water, Lucid, Unity MCP or Toon Water URP
allow compilation
run EditMode tests
run PlayMode tests
create a new Water25D object
save and reopen the scene
create a prefab
instantiate the prefab
enter and exit Play Mode
build one desktop player
build one mobile or WebGL target where supported
run the visual-validation sample
scan all package dependencies
```

`Assets/Water25D/Documentation/STATUS.md` records that package runtime and editor code have no dependency on the identified reference systems, while also recording the remaining sample-scene references and the fact that clean-project execution has not yet been completed.

## Final production decisions

| Required decision | Direct answer |
|---|---|
| Final role of the CRT backend | Retain temporarily as optional `SimulatedRipples`. Allocate and tick it only in that mode. Bounded impact processing is implemented; spatially validate multi-impact behaviour and complete suspension/profiling evidence before production support. Permit optional normal/reflection influence in simulated mode, but no CRT use in `FlatStylized`. Begin deprecation only after adoption and profiling evidence. |
| Shader and reflection architecture | Package-owned HLSL top and front shaders, geometrically flat in `FlatStylized`, with one final MPB writer. Stylised reflection is analytic and camera-free; planar reflection is supplied by compatible shared groups. |
| Ring, foam and wake material influence | Rings alter radial pseudo-normal, reflection UV, highlight and minor colour. Foam adds colour/alpha and strongly occludes reflection. Wakes add directional pseudo-normal, reflection offset and a fading capsule trail. None displaces flat geometry. |
| Toon Water URP | Use only as a privately licensed development reference. Do not require it, ship it or build a first-release adapter. |
| Existing migration | Existing objects remain `SimulatedRipples`; existing profiles, materials, ripple values, reflection settings, FX definitions and events are preserved. New objects use `FlatStylized`. Conversion is explicit, Undo-backed and reversible. |
| Exact files requiring modification | The completed foundation already added the mode enum, presentation/render-data types, flat geometry, rings, qualified crossings, logical-body tracker and contact foam. Remaining work modifies the controller, rendering/reflection modules, presentation data, profiles, FX, shaders, editor tooling, benchmark, tests and documentation as detailed above, and adds migration/painterly resources only in their bounded phases. |
| Automated and manual validation | Preserve the completed 55-test EditMode and nine-test PlayMode baseline. Extend it for wakes, serialization/migration, painterly properties, reflection lifecycle, cleanup, optional dependencies and package dependencies; capture flat silhouette, interactions, seam, reflection modes, orthographic projection and sprite sorting. |
| Performance budgets and starting tiers | Start at 4/2/4, 8/4/8 and 16/8/16 ring/foam/wake slots. Flat medium should remain below approximately 0.45 ms GPU for one interacting 1080p water on the selected median desktop target and below 0.35 ms at 720p on the selected median mobile target. Planar reflection starts at quarter resolution and three-frame intervals. |
| Criteria for making FlatStylized the default | Genuinely new controllers already use `FlatStylized`; existing objects are not automatically switched. Production acceptance still requires completed interactions, cleanup, reflection, visual comparison, clean-project validation and measured evidence that flat mode is materially cheaper. |
| Production-ready release gate | All automated tests pass; target-player captures pass; zero steady-state GC and creation churn are demonstrated; no leaks occur across disable, unload or Play Mode exit; reflection cost scales by group; flat is measurably cheaper; clean-project import and sample execution succeed; package licensing is clarified; measured results are documented separately from analytical estimates. |
