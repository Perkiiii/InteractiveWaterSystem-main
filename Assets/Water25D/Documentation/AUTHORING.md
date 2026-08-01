# Water25D authoring workflow

The Water25D controller uses a standard Unity Inspector workflow with persistent editor-only foldout state. Foldout choices and scene-handle visibility are stored in `EditorPrefs`; they are not serialized into scenes, prefabs, or runtime components.

## Controller sections

Select a `Water25DController` to work through the package in this order:

- **Basic** — width, visual depth, physical depth, waterline, interaction lane, crossing-band thickness, and scene-handle visibility. The help text calls out the local XZ top surface, local XY front surface, and flat 2D gameplay surface.
- **Rendering** — shared style profile, inline surface colors, package-owned material templates, sorting layers/orders, and safe profile/material actions.
- **Ambient Waves** — shared analytical-wave settings and the quality profile's ambient band controls. The direction helper normalizes the direction before storing it.
- **Contact Ripples** — ripple enablement, impact settings, shared quality profile, visual amplitude, and runtime-only simulator status. Reset and center-test controls are guarded outside Play Mode.
- **Reflection** — disabled, stylized, or planar mode. Camera and quality inputs are shown only when relevant; compatible-plane group diagnostics are read-only.
- **FX** — project-owned splash/bubble definitions, pool capacity, and fallback warnings. Missing optional definitions are recoverable warnings, not automatic asset imports.
- **Physics** — buoyancy, drag, and the separate surface-crossing/buoyancy-volume arrangement.
- **Interaction** — layer masks, trigger participation, and read-only links to the generated volumes.
- **Events** — UnityEvent entry/exit/submerge/resurface hooks with short behavior notes.
- **Performance** — calculated mesh/ripple memory and scheduling estimates. These values are derived from the current dimensions and quality profile; they are not profiler measurements.
- **Validation** — grouped errors, warnings, and information results. Safe fixes use Undo and can repair the generated hierarchy, assign package defaults, or rebuild preview geometry.
- **Advanced** — generated hierarchy references, ownership summaries, refresh/rebuild actions, and explicit runtime resource controls.

## Profile workflow

Style and quality profiles are configuration assets. The controller shows their values inline so common tuning does not require leaving the water object. The package default warning explains that editing a shared asset affects every loaded user.

Use **Make Unique Copy** or **Duplicate** before creating a one-off variation. **Create New** creates a project asset initialized from the package default. **Package Default** restores the package-owned reference without mutating the asset. Profile changes refresh loaded controllers through the editor-only preview API; runtime mutable materials, meshes, and ripple textures remain instance-owned.

The standalone `WaterStyleProfile` and `WaterQualityProfile` inspectors use the same grouped presentation and expose reset, duplication, validation, usage, and setup-documentation actions.

## Scene handles

With **Show Scene Handles** enabled, select the water root in the Scene view. Color-coded handles expose width, visual depth, physical depth, and waterline. Values snap to 0.1 units and record Undo operations; generated preview meshes and dependent editor views are refreshed after a change. Handles are editor-only and do not add a runtime dependency.

## Validation and performance

Run **Validate** after changing hierarchy, profiles, materials, masks, reflection, or ripple quality. **Fix All Safe Issues** only applies package-owned, reversible fixes. Missing optional FX definitions remain a warning until project-owned definitions are assigned.

Use the Performance section to review estimated vertex counts, ripple resolution, state memory, and cell-update scheduling. Use the Unity Profiler and Frame Debugger before recording measured claims about allocations, render-texture recreation, draw counts, or reflection cost.

The current authoring surface intentionally exposes only implemented package behavior. Distortion, blur, caustics, light shafts, and a compute ripple backend are not presented as available features in this slice.
