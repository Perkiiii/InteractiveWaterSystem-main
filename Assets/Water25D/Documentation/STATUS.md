# Water25D implementation status

## Environment

- Unity source of truth: `6000.5.4f1` from `ProjectSettings/ProjectVersion.txt`.
- URP source of truth: `17.5.0` from the installed project packages.
- Package root: `Assets/Water25D/`.
- Runtime namespace: `Water25D`.
- Architectural source of truth: `Assets/Docs/IMPLEMENTATION_PLAN.md`.

## Implemented in this slice

- Finalized the package technical identity as `Water25D` across folders, assembly definitions, namespaces, menus, shaders, tests, and package documentation. Existing reference systems were not renamed or modified.
- Added the focused runtime/editor assembly boundaries, non-destructive generated hierarchy repair, persistent package-owned surface/ripple materials and style/quality profiles, and explicit edit/play-mode disposal.
- Made controller authoring deterministic with `[ExecuteAlways]`, `Reset()`, editor-only default-asset repair on scene open, and a creation menu that repairs the hierarchy and assigns defaults immediately.
- Made generated meshes transient editor/runtime resources. Editor preview meshes are rebuilt on reload and are not serialized into scenes or prefabs.
- Split hierarchy, geometry, rendering, physics, ripple, and reflection responsibilities into focused package-owned modules coordinated by `Water25DController`.
- Added the XZ top surface and XY front surface presentation model with separate thin surface-crossing and full buoyancy volumes.
- Added rectangular, world-density-derived CRT ripple state with RGHalf/RGFloat fallback, no mipmaps, bounded catch-up, per-second damping, aspect-correct propagation, queued impacts, visibility suspension, idle suspension, and an impact safety cap. Pending impacts now inject through bounded update zones before one shared full-surface propagation update per simulation step.
- Added shared analytical ambient waves used by both top and front surface shaders so the waterline remains coherent without a second ambient CRT.
- Added `WaterReflectionManager` and quantized compatible-plane grouping. Stylized reflection uses no reflection camera; planar mode owns one adaptive camera and render texture per compatible group.
- Added fixed-capacity pooled splash and bubble FX. Gameplay FX requests reject exhaustion and do not instantiate or destroy during normal use.
- Added the deterministic benchmark driver and editor generator under `Samples/Benchmark`.
- Added production tests for reflection grouping, disabled reflection, and FX pool exhaustion/return behaviour.

### Inspector authoring UX slice

- Replaced the dashboard-style controller editor with a minimal standard-Unity inspector: a disabled Script row followed by six persistent top-level foldouts named Basic, Rendering, FX, Physics, Event, and Action.
- Moved Ambient Waves, Contact Ripples, and Reflection into compact nested Rendering foldouts; combined Physics and Interaction under Physics; moved performance estimates, validation, and generated hierarchy details into nested Action foldouts.
- Removed the custom Water25D header, authoring subtitle, selected-object/status strip, warning/material badges, toolbar, and section cards without changing controller runtime code.
- Added theme-aware editor styles, editor-only foldout/handle state, inline style and quality profile editing, shared-asset warnings, duplicate/make-unique/default actions, and standalone profile inspectors.
- Added conditional controls for reflection, FX, physics, and ripple quality, plus calculated geometry/state-memory/scheduling estimates that are explicitly labeled as estimates.
- Added grouped validation with safe Undo-backed repairs, generated-hierarchy checks, material/profile/shader checks, and project setup warnings.
- Added Undo-aware Scene view handles for width, visual depth, physical depth, and waterline, with 0.1-unit snapping and editor preview refresh.
- Added minimal runtime preview/reset APIs so editor changes refresh dependent loaded controllers without exposing editor types to runtime code.

### Flat-stylized migration foundation slice

- Added the serialized `WaterSurfaceMode` contract with `SimulatedRipples = 0` and `FlatStylized = 1` under `Runtime/Rendering`.
- Existing controllers remain simulated when the new field is absent or zero-valued. `Reset()`, the Water25D creation menu, and the explicit new-object editor-default path assign `FlatStylized` to genuinely new controllers without changing loaded controllers.
- Gated CRT creation, ticking, reset, impact routing, and resource ownership by surface mode. Flat mode exposes no ripple texture, releases an existing simulator on mode change, and leaves simulated-mode dimensions and impact routing unchanged.
- Added the mode to the Inspector with disabled CRT controls and a bounded-slice informational message. Inspector changes refresh authoring state so mode transitions release or recreate runtime resources through the normal lifecycle.
- Created `Water25D_VisualFlat` as a duplicated validation scene, retained its `Water25D` simulated baseline, and added an independently positioned `Water25D_FlatTest` object with package-owned profiles, materials, generated hierarchy references, and explicit `FlatStylized` mode.
- This slice intentionally does not implement flat geometry, no-displacement shader branches, procedural rings, contact foam, wakes, or new splash presentation.

### Flat geometry and no-vertex-displacement shader contract

- Added a FlatStylized top mesh with exactly four XZ corner vertices, two triangles, six indices, complete bounds, upward-facing normals, and the existing top-surface origin and corner UV convention. No tessellation or tangent data is generated for this mode.
- Added a FlatStylized front mesh path using a four-vertex, two-triangle XY quad. Its top edge remains at the waterline through the existing `FrontSurface` child transform; width and physical depth resize the affected geometry, while visual depth rebuilds only the top mesh.
- Retained the SimulatedRipples tessellated top/front builders and their existing geometry density contract. Simulated mode continues to use ambient-wave and CRT vertex displacement.
- Added `_SurfaceMode` to the existing top/front shaders and bind it per renderer through the existing `MaterialPropertyBlock`. The mode branch occurs before top ambient/CRT displacement and before front ambient displacement; shader names and the simulated branch remain unchanged.
- Mode transitions replace and dispose generated meshes through `WaterRuntimeResources`, preserve the existing generated hierarchy, and continue to use the foundation slice's FlatStylized CRT ownership gate.
- Updated editor metrics/diagnostics and EditMode/PlayMode coverage for mesh topology, UV/orientation/bounds, waterline seam placement, visual-depth-only rebuilds, mode transitions, MPB mode values, and simulated-mode tessellation.
- The preceding geometry-contract slice intentionally did not implement procedural surface rings, foam, wakes, or new splash presentation; the ring work is recorded below.

Exact files changed for this bounded slice:

- `Assets/Water25D/Runtime/Core/WaterMeshBuilder.cs`.
- `Assets/Water25D/Runtime/Core/WaterGeometryModule.cs`.
- `Assets/Water25D/Runtime/Core/Water25DController.cs`.
- `Assets/Water25D/Runtime/Core/WaterRenderingModule.cs`.
- `Assets/Water25D/Runtime/Rendering/WaterShaderIds.cs`.
- `Assets/Water25D/Shaders/Water25D_TopSurface.shader`.
- `Assets/Water25D/Shaders/Water25D_FrontSurface.shader`.
- `Assets/Water25D/Editor/Water25DInspectorUtility.cs`.
- `Assets/Water25D/Editor/Water25DEditor.cs`.
- `Assets/Water25D/Tests/EditMode/WaterMeshBuilderTests.cs`.
- `Assets/Water25D/Tests/EditMode/WaterSurfaceModeEditModeTests.cs`.
- `Assets/Water25D/Tests/EditMode/Water25DEditorTests.cs`.
- `Assets/Water25D/Tests/PlayMode/WaterControllerPlayModeTests.cs`.
- `Assets/Water25D/Documentation/STATUS.md`.

Exact files changed for the flat-stylized migration foundation slice:

- `Assets/Water25D/Runtime/Rendering/WaterSurfaceMode.cs` and its Unity-generated `.meta` file.
- `Assets/Water25D/Runtime/Core/Water25DController.cs`.
- `Assets/Water25D/Runtime/Simulation/WaterRippleModule.cs`.
- `Assets/Water25D/Editor/Water25DEditorDefaults.cs`.
- `Assets/Water25D/Editor/Water25DMenu.cs`.
- `Assets/Water25D/Editor/Water25DEditor.cs`.
- `Assets/Water25D/Tests/EditMode/WaterSurfaceModeEditModeTests.cs` and its Unity-generated `.meta` file.
- `Assets/Water25D/Tests/PlayMode/WaterControllerPlayModeTests.cs`.
- `Assets/Water25D/Samples/Water25D_VisualFlat.unity` and its Unity-generated `.meta` file.
- `Assets/Water25D/Documentation/STATUS.md`.

### Flat-stylized procedural surface ring slice

- Added `WaterSurfacePresentationModule` and `WaterSurfaceRenderData` as one nonserialized controller-owned presentation module. The module owns only fixed-capacity procedural ring slots and prepared shader data; it creates no GameObjects, renderers, particle systems, materials, meshes, render textures, or separate draw calls.
- Fixed shader storage at 16 entries. The quality setting `MaximumSurfaceRings` defaults to 8 and is clamped to 1–16. When full, expired slots are reclaimed first; otherwise the oldest active slot is replaced deterministically and `ReplacedSurfaceRingCount` increments. All slots and both upload arrays are allocated once at module construction.
- Added style settings for ring lifetime, expansion multiplier, thickness, softness, and intensity with defaults of 1.25 seconds, 6.0, 0.05, 0.04, and 0.75. Values are sanitized and participate in style equality/hash behavior. Ring capacity participates in broad quality equality/hash behavior but is intentionally excluded from `SimulationEquals`, so changing it does not recreate CRT state.
- Added `Water25DController.CreateSurfaceImpactAt(...)` and retained both `CreateContactRippleAt(...)` overloads as source-compatible forwarders. SimulatedRipples continues to validate and queue the existing CRT impact; FlatStylized maps in-bounds points to local XZ world units, creates a ring, owns no CRT, and accepts deterministic replacement when capacity is full.
- Added a focused `TryGetSurfaceLocalXZ(...)` mapping path shared by the ring and UV calculations. Shader reconstruction uses local XZ units from `_WaterSize`, so rings remain circular on rectangular surfaces.
- Added lightweight MPB uploads that read each renderer's existing block and update only `_WaterRingCount`, `_WaterRingsA`, and `_WaterRingsB`. Full authoring applies initialize zero ring data, while transient updates preserve style, CRT, mode, reflection, and unrelated instance properties.
- Extended the existing top and front passes with bounded 16-entry annulus evaluation. The top blends a fading expanding ring toward `_FoamColor` without vertex displacement. The front evaluates the same ring at local Z = 0 in a narrow top seam band and only shows it once the radius reaches the front plane.
- Reused the existing `TestPlayer` Rigidbody2D/collider already present in `Water25D_VisualFlat` for Play Mode impact routing. No sample driver or scene serialization change was needed, and `Water25D_VisualValidation.unity` was not modified.
- The existing interaction component still has known incomplete side-entry and below-surface crossing qualification; this slice routes its accepted events but does not correct those semantics. No architectural deviation from the controller/module ownership model was introduced.
- Crossing qualification, logical-body contact tracking, contact foam, wakes, splash redesign, ring-derived normals, reflection distortion, Fresnel changes, and final reflection tuning remain intentionally unimplemented.

Exact files changed for the procedural surface ring slice:

- `Assets/Water25D/Runtime/Rendering/WaterSurfacePresentationModule.cs` and its Unity-generated `.meta` file.
- `Assets/Water25D/Runtime/Rendering/WaterSurfaceRenderData.cs` and its Unity-generated `.meta` file.
- `Assets/Water25D/Runtime/Core/Water25DController.cs`.
- `Assets/Water25D/Runtime/Core/WaterRenderingModule.cs`.
- `Assets/Water25D/Runtime/Physics/WaterSurfaceInteraction2D.cs`.
- `Assets/Water25D/Runtime/Rendering/WaterShaderIds.cs`.
- `Assets/Water25D/Runtime/Settings/WaterQualityProfile.cs`.
- `Assets/Water25D/Runtime/Settings/WaterStyleProfile.cs`.
- `Assets/Water25D/Editor/Water25DEditor.cs`.
- `Assets/Water25D/Editor/WaterQualityProfileEditor.cs`.
- `Assets/Water25D/Editor/WaterStyleProfileEditor.cs`.
- `Assets/Water25D/Shaders/Water25D_TopSurface.shader`.
- `Assets/Water25D/Shaders/Water25D_FrontSurface.shader`.
- `Assets/Water25D/Tests/EditMode/WaterSurfacePresentationTests.cs` and its Unity-generated `.meta` file.
- `Assets/Water25D/Tests/PlayMode/WaterControllerPlayModeTests.cs`.
- `Assets/Water25D/Documentation/STATUS.md`.

No sample scene, profile asset, material asset, prefab, package manifest, project setting, or legacy/reference-system file was changed for this slice.

## Package boundaries

The Water25D runtime and editor assemblies have no code dependency on `Assets/InteractiveWaterSystem/`, `Assets/Cainos/`, `Assets/DemoScenes/`, or Lucid Editor. Unity MCP is development tooling and is not referenced by the package code.

The current `Assets/Water25D/Samples/Water25D_VisualValidation.unity` scene does still contain serialized references to the legacy `InteractiveWaterSystem` SpriteLit shader/material and the `DemoScenes` player-controller script. The core package code is isolated, but the package root is not yet fully portable as a copied asset tree until those sample dependencies are replaced or explicitly isolated.

## Validation record

- Connected Unity Editor refreshed and compiled the package successfully after the authoring pass.
- Historical foundation validation before the ring slice: `Water25D.Tests.EditMode`, 36 total, 36 passed, 0 failed, 0 skipped; `Water25D.Tests.PlayMode`, 2 total, 2 passed, 0 failed, 0 skipped.
- Current complete EditMode run through Unity MCP job `7bb323d27e124a0cb323e2dc98174973`: `Water25D.Tests.EditMode`, 47 total, 47 passed, 0 failed, 0 skipped, result state `Passed`.
- Current complete PlayMode run through Unity MCP job `ce419f1808ae44c39482dfbd54253637`: `Water25D.Tests.PlayMode`, 3 total, 3 passed, 0 failed, 0 skipped, result state `Passed`.
- The completed EditMode run ended with the Unity editor idle, ready for tools, not compiling, and not in Play Mode. Console inspection returned no errors or warnings; only the Test Runner result-save and performance-cleanup log entries were present.
- A later PlayMode relaunch after the profile-default test timed out in the Unity MCP initialization/transport layer and returned no test result; it is not counted as a pass or failure. The completed PlayMode job above remains valid because the post-run source change only added an EditMode test.
- Unity shader compiler log entries for the changed top and front shaders completed with `ok=1`; no shader compilation error was observed.
- Visual validation scene: `Assets/Water25D/Samples/Water25D_VisualValidation.unity`. Unity inspection confirmed persistent materials on both renderers, all five default controller asset references, and `{fileID: 0}` generated mesh slots after save. The [persistent-material capture](Validation/water25d-persistent-materials.png) shows the saved scene rendering without magenta error output.
- In Play Mode, `CreateContactRippleAt` returned `true`; after a subsequent Unity frame, GPU readback of the 320 x 104 RGHalf state contained 132,848 nonzero bytes out of 133,120. This confirms impact state reached the CRT. A final target-device visual ripple comparison has not been claimed.
- Package-owned runtime and editor code returned no legacy/vendor references. The only package matches are intentional portability and audit instructions in documentation.
- Existing authoring captures: [basic/rendering](Validation/water25d-inspector-basic-rendering.png), [ripples/performance](Validation/water25d-inspector-ripples-performance.png), [validation warning](Validation/water25d-inspector-validation-warning.png), and [scene handles](Validation/water25d-scene-handles.png). These captures predate the six-section layout correction and must be regenerated before visual parity is claimed.
- For the flat-mode foundation, the live Unity `6000.5.4f1` Editor had previously run the complete source revision successfully. The current ring-slice rerun above is the authoritative validation for this status entry.
- The scene scaffold was saved through the connected Unity Editor after locating `Water25D_VisualFlat` by asset name. The original `Water25D_VisualValidation.unity` was not modified.
- `Water25D_VisualFlat.unity.meta` has GUID `2db428c6b97345c4f9d7706316fedd19`, distinct from the source scene GUID `f649cad0f7c3d6844b77164df4d889c5`; the duplicated scenes retain identical dependency GUID sets.
- Static YAML validation confirmed the retained `SimulatedRipples` baseline, `Water25D_FlatTest` as `FlatStylized`, no zero script, GameObject, profile, or material references, and only the expected runtime-generated mesh slots with `{fileID: 0}`.
- Final repository validation passed `git diff --check`; legacy reference trees, the original validation scene, and unrelated project settings are clean. Unity-generated temporary scene/project-setting serialization was restored before this final audit.
- For the flat geometry and shader-contract slice, the live Unity `6000.5.4f1` Editor had previously refreshed and recompiled the package, then ran the complete source suites with no failures or skips. The current ring-slice run above also retained those tests.
- Structural assertions covered the four-vertex flat top/front meshes, bounds, corner UVs, normals/winding, fixed waterline edge, visual-depth-only top rebuild, mode-switch mesh disposal, MPB `_SurfaceMode`, flat CRT absence, and simulated tessellation/CRT retention.
- `git diff --check` passed after the final source and documentation changes. `git status --short` was inspected; no final `ProjectSettings`, `Packages`, legacy reference-tree, source validation-scene, material, profile, prefab, or tracked `.meta` changes belong to this slice.
- `rg -n "Cainos|InteractiveWaterSystem|DemoScenes|Lucid" Assets/Water25D` returned documentation-only audit/portability matches; no package runtime or editor code dependency was introduced.
- `git diff --check` passed after the ring implementation and status update. `git status --short` showed only the listed Water25D source, shader, editor, test, generated-new-asset-meta, and documentation files.
- The repository branch contains the inspected baseline commit `ece5fe9c6df3ccc6a220095816622d3e5670c4d5` as an ancestor; no reset or rollback was performed.
- The requested Water25D-specific `Assets/Water25D/Documentation/IMPLEMENTATION_PLAN.md` path does not exist in this repository. The existing repository-level plan at `Assets/Docs/IMPLEMENTATION_PLAN.md` and the flat-stylized plan were used instead.

## Not completed

- Original-system baseline capture, measured profiler benchmark, and target-device production validation.
- Clean-project copy/import validation and migration tooling.
- Removal or isolation of the legacy serialized references from the visual-validation sample; the package-root portability claim therefore remains incomplete.
- Compute simulation backend; the CRT backend remains the first production backend.
- Full planar-reflection visual comparison, Frame Debugger capture, and allocation profiling.
- Benchmark measurements. The generator is present, but no performance numbers are recorded.
- Before/after screenshots of the legacy generic inspector were not available, so visual parity is documented by the current authoring captures and workflow description rather than a pixel comparison.
- A post-correction capture of the six collapsed top-level bars has not yet been produced.
- Full multi-selection/prefab-stage authoring review, measured profiler capture, and target-device validation remain outstanding.
- Gameplay-camera-aware visibility scheduling and vertical-crossing-weighted impact strength remain follow-up work.
- Automatic project layer, sorting-layer, URP renderer-feature, or Camera Sorting Layer Texture setup.

### Flat-mode manual validation still required

- Open `Assets/Water25D/Samples/Water25D_VisualFlat.unity` and confirm `Water25D` remains the simulated baseline while `Water25D_FlatTest` is independently positioned and set to `FlatStylized`.
- Enter Play Mode and confirm the flat object has no `CustomRenderTexture`, while an explicitly simulated object still creates and updates its ripple texture.
- Switch `Water25D_FlatTest` to `SimulatedRipples`, confirm its ripple resource initializes, then switch back to `FlatStylized` and confirm the resource is released.
- Resize the flat object and confirm no CRT is created. Save, close, reopen, and confirm both mode values persist.
- Inspect the flat object for missing scripts, materials, profiles, generated hierarchy references, Console errors, and Console warnings introduced by this slice.
- Confirm the Inspector shows the mode, disables the CRT controls in flat mode, displays the bounded-slice informational message, and supports Undo/Redo for mode changes.
- Compare the retained simulated baseline against the original visual-validation scene. Final flat visual appearance is not an acceptance requirement for this task.

### Flat-geometry manual validation still required

- Open `Assets/Water25D/Samples/Water25D_VisualFlat.unity`; select `Water25D_FlatTest` and verify the top surface is geometrically flat, the front panel's top edge meets the waterline, and the retained `Water25D` object remains the simulated baseline.
- In Edit Mode, resize width, visual depth, physical depth, and waterline independently. Confirm width/physical-depth changes affect the intended meshes, visual-depth changes rebuild only the top mesh, and waterline changes move the front surface without changing its mesh.
- Enter Play Mode and confirm FlatStylized shows no ambient or CRT vertex motion and owns no `CustomRenderTexture`; confirm the simulated baseline retains ambient waves, CRT ripples, and its tessellated geometry.
- Switch the flat object to `SimulatedRipples` and back, checking mesh topology, `_SurfaceMode`, CRT creation/release, and that no duplicate generated hierarchy children appear.
- Use Inspector changes and Undo/Redo for mode, dimensions, and waterline; save, close, reopen, and confirm serialized mode/profile/material/hierarchy references remain intact.
- Check both modes for pink materials, Console errors/warnings introduced by this slice, incorrect normals/culling, a visible front/top seam, and unexpected reflection or sorting changes. Use the Frame Debugger if shader branch behavior needs confirmation.
- No manual visual, Frame Debugger, profiler, clean-project import, or target-device verification was performed in this task; the checklist above remains required.

### Procedural-ring manual validation still required

- Open `Assets/Water25D/Samples/Water25D_VisualFlat.unity`, enter Play Mode, and trigger impacts near the centre, left, right, front, and back of `Water25D_FlatTest`.
- Confirm a thin expanding ring appears on the flat top, remains circular on the rectangular surface, clips at bounds, overlaps other rings, fades cleanly, and has no vertex displacement.
- Exceed the configured capacity and confirm the oldest active ring is replaced predictably; confirm ring animation creates no hierarchy children, renderers, meshes, materials, particles, or additional draw calls.
- Confirm the front seam reacts only after an expanding ring reaches the front plane, with no permanent top/front seam gap.
- Confirm the simulated baseline continues to use CRT displacement without procedural rings, and that reflection remains visible while ring MPBs animate.
- Switch flat to simulated and back, resize the flat object, disable/re-enable it, and confirm rings clear, start empty, and do not create a CRT in FlatStylized.
- Inspect the ring fields in shared-profile and unique-copy workflows, exercise Undo/Redo, save/close/reopen the scene, and confirm no missing references, pink materials, shader errors, or new Console errors.
- No manual visual, Frame Debugger, profiler, clean-project import, or target-device verification was performed for procedural rings; automated tests covered lifecycle, routing, fixed storage, MPB preservation, and shader data shape only.

## Known setup requirements

Project-level rendering and sorting configuration is intentionally not modified by the package. Follow `SETUP.md` and `PORTABILITY.md` when moving the package to another project.

## Milestone status

The active flat-stylized redesign milestone was advanced by this bounded slice but is not complete. The next bounded task is:

```text
Implement qualified top-surface crossing detection, reusable logical-body contact tracking, and body-keyed contact foam while preserving procedural rings and simulated CRT behavior.
```

Full prefab-stage, multi-selection, clean-project portability, measured profiling, target-device validation, and the remaining flat presentation features remain follow-up work.
