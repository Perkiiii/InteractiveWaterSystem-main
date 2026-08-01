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

## Package boundaries

`Assets/Water25D/` has no runtime, editor, or serialized dependency on `Assets/InteractiveWaterSystem/`, `Assets/Cainos/`, `Assets/DemoScenes/`, or Lucid Editor. Those systems remain reference-only baselines. Unity MCP is development tooling and is not referenced by the package.

## Validation record

- Connected Unity Editor refreshed and compiled the package successfully.
- EditMode: `Water25D.Tests.EditMode`, 10 passed, 0 failed. This includes persistent default asset/shader checks, prefab save/reload lifecycle checks, and CRT batching counters.
- PlayMode: `Water25D.Tests.PlayMode`, 1 passed, 0 failed.
- Visual validation scene: `Assets/Water25D/Samples/Water25D_VisualValidation.unity`. Unity inspection confirmed persistent materials on both renderers, all five default controller asset references, and `{fileID: 0}` generated mesh slots after save. The [persistent-material capture](Validation/water25d-persistent-materials.png) shows the saved scene rendering without magenta error output.
- In Play Mode, `CreateContactRippleAt` returned `true`; after a subsequent Unity frame, GPU readback of the 320 x 104 RGHalf state contained 132,848 nonzero bytes out of 133,120. This confirms impact state reached the CRT. A final target-device visual ripple comparison has not been claimed.
- Package-scoped legacy-name search returned no matches after the rename.

## Not completed

- Original-system baseline capture, measured profiler benchmark, and target-device production validation.
- Clean-project copy/import validation and migration tooling.
- Compute simulation backend; the CRT backend remains the first production backend.
- Full planar-reflection visual comparison, Frame Debugger capture, and allocation profiling.
- Benchmark measurements. The generator is present, but no performance numbers are recorded.
- Gameplay-camera-aware visibility scheduling and vertical-crossing-weighted impact strength remain follow-up work.
- Automatic project layer, sorting-layer, URP renderer-feature, or Camera Sorting Layer Texture setup.

## Known setup requirements

Project-level rendering and sorting configuration is intentionally not modified by the package. Follow `SETUP.md` and `PORTABILITY.md` when moving the package to another project.
