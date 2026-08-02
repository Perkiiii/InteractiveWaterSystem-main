# Water25D setup

1. In the Unity Editor, use `GameObject > Water 2.5D > Water25D Controller`, or add `Water25DController` to an empty GameObject.
2. Select the root and inspect the generated `TopSurface`, `FrontSurface`, `SurfaceCrossingTrigger`, `BuoyancyVolume`, `ReflectionAnchor`, and `FXRoot` children. The controller repairs missing expected children without deleting unrelated children.
3. The creation menu assigns `Water25D_DefaultStyle`, `Water25D_MediumQuality`, `Water25D_Top.mat`, `Water25D_Front.mat`, and `Water25D_RippleSimulation.mat` as persistent package-owned defaults. Override them only when the water needs project-owned templates; runtime property blocks and ripple textures remain instance-owned.
4. Place the root at the desired waterline. Set `Waterline Local Y` when the root transform should remain at another local anchor.
5. Configure `Surface Interaction Layers`, `Surface Trigger Interaction Layers`, and `Buoyancy Layers` independently. The thin surface trigger produces enter/exit events and impacts; the full volume provides buoyancy, drag, submerged state, and bubbles.
6. Keep `Reflection Mode` at `Stylized` for a camera-free presentation. Select `Planar` only when a planar reflection is required; compatible water planes share a manager-owned camera and render texture.
7. Enable effects and optionally assign project-owned `WaterFXDefinition` assets. Splash and bubble pools are prewarmed during runtime configuration and reject requests when exhausted.

## Flat-stylized Phase 3 presentation

New controllers use `Water25D_FlatStylizedStyle` and `Water25D_FlatMediumQuality` when those package assets are available. Existing serialized controllers keep their current profiles and remain compatible with `SimulatedRipples`.

The flat-stylized top is a four-vertex XZ quad. Its shallow/deep colour gradient, restrained banding, calm two-layer normal detail, Fresnel tint, broad highlights, boundary foam, and interaction-ring/wake response are evaluated in the package-owned shader. The front remains a coherent XY surface with the matching waterline band, depth tint, optional distortion, and optional caustic contribution. Neither surface uses vertex displacement in `FlatStylized` mode.

Optional refraction is source-gated: enable it only when the project provides a valid URP Camera Opaque Texture, and enable front distortion only when the 2D Renderer provides a valid Camera Sorting Layer Texture. Optional caustics require a package- or project-owned texture in the style profile; a missing texture safely disables the quality feature.

Reflection has three explicit modes. `Disabled` omits reflection blending, `Stylized` uses a camera-free horizon-to-sky fallback, and `Planar` uses the shared adaptive reflection manager. Planar surfaces share one camera and texture only when camera, plane, culling, exclusion, resolution, and update settings match. The water surface layer is excluded automatically; use `Reflection Exclusion Mask` for additional helper or reflection-only layers.

The default quality settings produce a 320 x 104 ripple state for a 20 x 6.5 top surface at 16 texels per world unit. The state is runtime-owned, rectangular, RGHalf when supported, has no mipmaps, and is released when the water is disabled or destroyed.

The top surface is local XZ, from `(0, waterline, 0)` to `(width, waterline, visual depth)`. The front surface is local XY and extends downward by physical depth. Gameplay remains a flat 2D surface with an explicit depth lane; it does not read GPU height data. Generated preview meshes are transient and are rebuilt after a domain reload or scene reopen; persistent surface materials are saved with the scene or prefab.

## Inspector and authoring workflow

The controller Inspector starts with a disabled Script row followed by six persistent foldouts: Basic, Rendering, FX, Physics, Event, and Action. Ambient Waves, Contact Ripples, and Reflection are nested under Rendering. Diagnostics and Advanced are nested under Action. Profile values are editable inline, while shared profile assets display a warning only while their expanded settings are being edited and offer **Make Unique Copy**, **Duplicate**, **Create New**, and **Package Default** actions. See [AUTHORING.md](AUTHORING.md) for the complete workflow.

**Action > Diagnostics** reports calculated geometry, ripple-resolution, memory, scheduling, and planar-reflection estimates. These are authoring estimates, not measured profiler data; it also groups validation findings and provides Undo-backed safe repairs. Scene handles expose width, visual depth, physical depth, and waterline with 0.1-unit snapping and Undo.

## Manual verification

- Open `Assets/Water25D/Samples/Water25D_VisualValidation.unity` and select the `Water25D` root. Confirm the disabled Script row, six collapsed top-level foldouts, generated hierarchy, Basic fields, and inline Rendering profile fields in the Inspector.
- Enable **Show Scene Handles** in Basic. In the Scene view, confirm the labeled top/front surfaces and width, visual-depth, physical-depth, and waterline handles; move one by 0.1 units and use Undo to restore it.
- Open Rendering > Contact Ripples and Action > Diagnostics. Confirm the quality profile controls, calculated ripple resolution/state estimate, and validation actions. Enter Play Mode and verify the reset and center-test controls are available only while the simulator can run.
- Enter Play Mode and use an oblique `Main Camera` view. Expect a coherent analytical wave along the XZ top surface and XY front surface seam.
- Call `Water25DController.CreateContactRippleAt` from a test script or drop a dynamic `Rigidbody2D` through the thin trigger. Expect one logical surface-enter event for a multi-collider body and a CRT impact state update.
- Switch Reflection Mode to `Planar`, assign the scene camera, and inspect the Frame Debugger for one reflection camera render per compatible group. Switch back to `Stylized` and confirm no reflection camera is created.
- Trigger surface enter/exit and submerged events, then confirm FX entries are reused after their lifetime and that no gameplay-time `Instantiate`/`Destroy` occurs.
- Use the Frame Debugger to confirm the ripple state has no mipmaps. Use the Profiler to inspect GC allocations, RenderTexture recreation, and reflection cost before recording performance claims.
- Use `GameObject > Water 2.5D > Create Deterministic Benchmark Scene` to generate the benchmark scene, then record measurements separately; the package does not contain unmeasured performance numbers.

Current authoring captures from the sample scene are stored under [`Documentation/Validation`](Validation/): [basic and rendering](Validation/water25d-inspector-basic-rendering.png), [ripples and performance](Validation/water25d-inspector-ripples-performance.png), [validation warning](Validation/water25d-inspector-validation-warning.png), and [scene handles](Validation/water25d-scene-handles.png).
