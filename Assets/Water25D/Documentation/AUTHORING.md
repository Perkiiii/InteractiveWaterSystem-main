# Water25D authoring workflow

The Water25D controller uses a standard Unity Inspector workflow with persistent editor-only foldout state. Foldout choices and scene-handle visibility are stored in `EditorPrefs`; they are not serialized into scenes, prefabs, or runtime components.

## Controller sections

Select a `Water25DController` to work through the package in this order:

- **Basic** — width, visual depth, physical depth, waterline, interaction lane, crossing-band thickness, and scene-handle visibility. The help text calls out the local XZ top surface, local XY front surface, and flat 2D gameplay surface.
- **Rendering** — shared style and quality profile fields, inline appearance controls, package-owned material templates, sorting layers/orders, and nested Ambient Waves, Contact Ripples, and Reflection controls.
- **FX** — pooled splash and bubble definitions and pool capacity.
- **Physics** — buoyancy, drag, surface interaction, collider masks, trigger participation, and the separate surface-crossing/buoyancy-volume arrangement.
- **Event** — UnityEvent surface-enter, surface-exit, submerged, and resurfaced hooks.
- **Action** — regular authoring buttons for defaults, hierarchy repair, preview refresh, geometry rebuild, ripple reset, generated-surface selection, and setup documentation. Diagnostics and Advanced are compact nested foldouts here.

## Profile workflow

Style and quality profiles are configuration assets. The controller shows their values inline so common tuning does not require leaving the water object. The package default warning explains that editing a shared asset affects every loaded user.

Use **Make Unique Copy** or **Duplicate** before creating a one-off variation. **Create New** creates a project asset initialized from the package default. **Package Default** restores the package-owned reference without mutating the asset. Profile changes refresh loaded controllers through the editor-only preview API; runtime mutable materials, meshes, and ripple textures remain instance-owned.

The standalone `WaterStyleProfile` and `WaterQualityProfile` inspectors use the same grouped presentation and expose reset, duplication, validation, usage, and setup-documentation actions.

## Scene handles

With **Show Scene Handles** enabled, select the water root in the Scene view. Color-coded handles expose width, visual depth, physical depth, and waterline. Values snap to 0.1 units and record Undo operations; generated preview meshes and dependent editor views are refreshed after a change. Handles are editor-only and do not add a runtime dependency.

## Validation and performance

Open **Action > Diagnostics** and run **Validate** after changing hierarchy, profiles, materials, masks, reflection, or ripple quality. **Fix All Safe Issues** only applies package-owned, reversible fixes. Performance values are calculated estimates, not profiler measurements.

Use the estimates in **Action > Diagnostics** to review vertex counts, ripple resolution, state memory, cell-update scheduling, and planar reflection size. Use the Unity Profiler and Frame Debugger before recording measured claims about allocations, render-texture recreation, draw counts, or reflection cost.

The current authoring surface intentionally exposes only implemented package behavior. Distortion, blur, caustics, light shafts, and a compute ripple backend are not presented as available features in this slice.
