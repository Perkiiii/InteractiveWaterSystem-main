# Water25D setup

1. In the Unity Editor, use `GameObject > Water 2.5D > Water25D Controller`, or add `Water25DController` to an empty GameObject.
2. Select the root and inspect the generated `TopSurface`, `FrontSurface`, `SurfaceCrossingTrigger`, `BuoyancyVolume`, `ReflectionAnchor`, and `FXRoot` children. The controller repairs missing expected children without deleting unrelated children.
3. Assign `WaterStyleProfile` and `WaterQualityProfile` assets when shared authored settings are needed. Without material templates, the package shaders are used through per-instance runtime materials.
4. Place the root at the desired waterline. Set `Waterline Local Y` when the root transform should remain at another local anchor.
5. Configure `Surface Interaction Layers`, `Surface Trigger Interaction Layers`, and `Buoyancy Layers` independently. The thin surface trigger produces enter/exit events and impacts; the full volume provides buoyancy, drag, submerged state, and bubbles.
6. Keep `Reflection Mode` at `Stylized` for a camera-free presentation. Select `Planar` only when a planar reflection is required; compatible water planes share a manager-owned camera and render texture.
7. Enable effects and optionally assign project-owned `WaterFXDefinition` assets. Splash and bubble pools are prewarmed during runtime configuration and reject requests when exhausted.

The default quality settings produce a 320 x 104 ripple state for a 20 x 6.5 top surface at 16 texels per world unit. The state is runtime-owned, rectangular, RGHalf when supported, has no mipmaps, and is released when the water is disabled or destroyed.

The top surface is local XZ, from `(0, waterline, 0)` to `(width, waterline, visual depth)`. The front surface is local XY and extends downward by physical depth. Gameplay remains a flat 2D surface with an explicit depth lane; it does not read GPU height data.

## Manual verification

- Open `Assets/Water25D/Samples/Water25D_VisualValidation.unity` and select the `Water25D` root. Confirm the generated hierarchy and the reflection/FX sections in the Inspector.
- Enter Play Mode and use an oblique `Main Camera` view. Expect a coherent analytical wave along the XZ top surface and XY front surface seam.
- Call `Water25DController.CreateContactRippleAt` from a test script or drop a dynamic `Rigidbody2D` through the thin trigger. Expect one logical surface-enter event for a multi-collider body and a CRT impact state update.
- Switch Reflection Mode to `Planar`, assign the scene camera, and inspect the Frame Debugger for one reflection camera render per compatible group. Switch back to `Stylized` and confirm no reflection camera is created.
- Trigger surface enter/exit and submerged events, then confirm FX entries are reused after their lifetime and that no gameplay-time `Instantiate`/`Destroy` occurs.
- Use the Frame Debugger to confirm the ripple state has no mipmaps. Use the Profiler to inspect GC allocations, RenderTexture recreation, and reflection cost before recording performance claims.
- Use `GameObject > Water 2.5D > Create Deterministic Benchmark Scene` to generate the benchmark scene, then record measurements separately; the package does not contain unmeasured performance numbers.
