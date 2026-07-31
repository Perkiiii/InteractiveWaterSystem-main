# AGENTS.md — Improved Unity 2.5D Water System

## Mission

This repository contains two existing water implementations used as references:

* A custom GPU-driven 2.5D water system under `Assets/InteractiveWaterSystem/`.
* Cainos Interactive Pixel Water under `Assets/Cainos/Interactive Pixel Water/`.

The goal is to build a new, modular, production-oriented, and self-contained water package under:

```text
Assets/Water25D/
```

The completed package must be movable into another compatible Unity project without requiring the original custom water implementation, Cainos assets, demo scenes, Lucid Editor, or Unity MCP.

Do not turn the existing systems into one combined implementation. Preserve them as baselines while building the new package separately.

---

## Sources of Truth

Use the following precedence when information conflicts:

1. The current user task.
2. This `AGENTS.md`.
3. `Assets/Docs/IMPLEMENTATION_PLAN.md` for architectural intent and milestone order.
4. `Assets/Water25D/Documentation/STATUS.md`, once it exists, for completed and active milestones.
5. Actual repository code and serialized project settings for current implementation state.
6. README files and older explanatory documentation.

Important distinctions:

* The implementation plan describes the intended destination.
* Existing code describes what is currently implemented.
* `ProjectSettings/ProjectVersion.txt` is the source of truth for the Unity Editor version.
* `Packages/manifest.json` and `Packages/packages-lock.json` are the source of truth for package versions.
* Do not rely on Unity or URP version numbers copied into older documentation.

If a referenced document or status file does not exist, report that fact. Do not invent its contents or assume a milestone has been completed.

---

## Repository Map

```text
AGENTS.md
Assets/
├── Docs/
│   └── IMPLEMENTATION_PLAN.md
├── InteractiveWaterSystem/
│   ├── Scripts/
│   │   ├── InteractiveWater.cs
│   │   └── SimplePlanarReflection.cs
│   └── ArtAssets/
├── Cainos/
│   └── Interactive Pixel Water/
├── DemoScenes/
├── Settings/
└── Water25D/                      Target package root
    ├── Runtime/
    │   ├── Core/
    │   ├── Simulation/
    │   ├── Physics/
    │   ├── Rendering/
    │   ├── FX/
    │   └── Settings/
    ├── Editor/
    ├── Shaders/
    ├── Materials/
    ├── Profiles/
    ├── Prefabs/
    ├── Samples/
    ├── Tests/
    │   ├── EditMode/
    │   └── PlayMode/
    └── Documentation/
Packages/
ProjectSettings/
THIRD_PARTY_NOTICES.md
```

The `Assets/Water25D/` structure is the intended destination. Do not create every directory pre-emptively; create directories only when the current implementation phase needs them.

---

## Default Ownership Boundaries

### Reference-only areas

Treat these paths as read-only unless the task explicitly requests a baseline fix, migration adapter, or comparison-scene change:

```text
Assets/InteractiveWaterSystem/
Assets/Cainos/
Assets/DemoScenes/
```

Do not casually refactor, rename, move, reserialize, or retune assets in these folders.

### Package-owned area

New production implementation belongs under:

```text
Assets/Water25D/
```

Package-owned code must use the namespace:

```csharp
Water25D
```

unless an existing package-owned file establishes a more specific nested namespace.

### Project-level exceptions

Some features may require changes outside the package root, such as:

* Layers or sorting layers.
* URP renderer configuration.
* Camera Sorting Layer Texture configuration.
* Package manifest changes.
* Build settings.
* Test project setup.

Make these changes only when required by the current task. Keep them minimal and list them explicitly in the handoff.

---

## Self-Contained Package Requirement

The new package must not acquire permanent runtime, editor, or serialized dependencies on:

```text
Assets/InteractiveWaterSystem/
Assets/Cainos/
Assets/DemoScenes/
Assets/Cainos/Third Party/Lucid Editor/
```

This includes:

* C# type references.
* Prefab references.
* Material or texture references.
* Shader references.
* Particle or audio references.
* Serialized GUID references.
* Resources loaded by hard-coded paths.
* Editor code that assumes the reference packages are installed.

Temporary migration or comparison adapters are allowed only when explicitly requested. Mark every temporary dependency clearly and record its planned removal in the package status document.

Unity MCP is development tooling only. Package runtime or editor code must not depend on it.

When a project-level renderer, layer, or sorting-layer setting is required, provide validation and setup documentation rather than silently assuming it exists in every destination project.

---

## Required Workflow

Before editing:

1. Read the relevant implementation-plan sections.
2. Inspect the actual files involved.
3. Check `git status` and the current diff.
4. Identify the current delivery phase.
5. Search for an existing package-owned equivalent before creating a new type.
6. State a short implementation plan, then proceed unless the task is genuinely blocked.

During implementation:

* Work on one milestone or independently reviewable slice at a time.
* Prefer small changes that preserve a compiling intermediate state.
* Avoid unrelated formatting, package upgrades, material retuning, or asset reserialization.
* Do not implement planned features merely because they appear later in the implementation plan.
* Do not delete or replace the legacy systems before migration and comparison validation are complete.
* Preserve existing behaviour unless the task explicitly changes it.

After implementation:

1. Inspect the complete diff.
2. Remove accidental serialized changes.
3. Run all available relevant validation.
4. Update package documentation when behaviour or architecture changed.
5. Update `Assets/Water25D/Documentation/STATUS.md` only when the task actually advances or completes a milestone.
6. Provide the required handoff described below.

If the package status document does not yet exist, create it only when the task establishes the package root or begins implementation tracking. Do not claim milestones were completed before they were verified.

---

## Delivery Order

Follow the implementation plan in this order:

1. Baseline capture and deterministic benchmark setup.
2. Structural refactor with visual parity.
3. Runtime resource ownership and disposal.
4. Inspector and authoring workflow.
5. Separate buoyancy and surface-crossing volumes.
6. Depth-aware, velocity-sensitive interaction.
7. Optimized Custom Render Texture simulation.
8. Analytical ambient waves.
9. Shared adaptive reflection management.
10. Pooled splash, bubble, foam, and underwater FX.
11. Optional compute-backend experiment.
12. Migration tooling and package validation.
13. Target-device production validation.

Do not skip directly to later phases because they seem more interesting.

In particular:

* Do not implement the compute backend before the optimized CRT backend is complete and benchmarked.
* Do not remove the ambient-wave CRT before an analytical replacement has been visually compared.
* Do not replace the current reflection component before the reflection manager has equivalent output available.
* Do not remove legacy components during early migration stages.

---

## Architectural Invariants

The new system must retain the following presentation model:

```text
XZ top surface
    visible contact ripples
    analytical ambient waves
    foam and edge treatment
    optional reflection

XY front surface
    underwater tint
    distortion
    caustics
    depth fade
    underwater FX
```

Gameplay and visual simulation remain separate:

```text
GPU height field
    visual displacement only

Flat 2D buoyancy surface
    Rigidbody2D gameplay

Thin surface-crossing trigger
    splashes, ripples, enter and exit events

Full underwater volume
    buoyancy, drag, bubbles and submerged state
```

Do not introduce GPU-to-CPU height readback as the default gameplay solution.

### Runtime responsibilities

Keep responsibilities separated:

* `Water25DController` coordinates modules.
* `WaterMeshBuilder` owns geometry generation.
* `WaterRuntimeResources` owns generated resources and disposal.
* `IWaterRippleSimulator` defines the simulation contract.
* Simulation implementations do not own gameplay physics.
* Interaction components produce logical impact events.
* Physics components do not sample GPU height state.
* Reflection rendering is managed centrally.
* FX creation goes through pools.
* Style and quality profiles contain shared immutable configuration.
* Per-instance mutable state remains owned by the water instance.

Do not rebuild another monolithic `InteractiveWater.cs`.

---

## Resource Ownership Rules

Project assets are templates and configuration. Runtime state must be owned explicitly.

### Per-water mutable resources

Each active water body must own independent mutable state unless sharing is an intentional documented feature:

* Ripple simulation state.
* Generated meshes.
* Per-instance material data.
* Impact queues.
* Contact tracking.
* Activity and visibility state.

Two water bodies must never share mutable ripple state accidentally.

### Shared resources

These may be shared intentionally:

* Immutable material templates.
* Immutable style profiles.
* Immutable quality profiles.
* Shader assets.
* FX definitions.
* Reflection resources owned by a compatible reflection-plane group.
* Global or scene-level FX pools.

### Runtime asset safety

Never change committed template assets at runtime through calls such as:

```csharp
sharedMaterial.SetFloat(...)
sharedMaterial.SetTexture(...)
```

when the value differs per water instance.

Use an appropriate combination of:

* `MaterialPropertyBlock`.
* Per-instance material ownership.
* Runtime-created render textures.
* Per-reflection-group resources.

Generated meshes, materials, render textures, buffers, cameras, and temporary GameObjects must have an explicit owner and disposal path.

Dispose or destroy replaced resources before assigning new ones. Handle Editor and Play Mode destruction correctly.

---

## Simulation Rules

The first production backend is an optimized Custom Render Texture implementation.

Its intended characteristics are:

* Runtime-owned state.
* Rectangular resolution based on world-space texel density.
* Approximately equal world-space cell sizes on X and Z.
* Two-channel half-float state where supported.
* No simulation mipmaps.
* No automatic mip generation.
* Configurable simulation frequency.
* Configurable bounded catch-up substeps.
* Per-second damping.
* Aspect-correct X and Z propagation coefficients.
* All pending impacts processed per simulation update up to a documented safety cap.
* Visibility and inactivity suspension.
* No steady-state managed allocation.

Do not treat iteration count as the primary artist-facing wave-speed control.

Impact radius is expressed in world units and converted independently to U and V.

The compute backend must remain behind `IWaterRippleSimulator` and must preserve the same public behaviour and settings contract as the CRT backend.

---

## Physics and Interaction Rules

Use two distinct 2D trigger volumes:

### Surface-crossing trigger

* Thin band around the waterline.
* Detects legitimate entries and exits through the surface.
* Produces ripple, splash, enter, exit, submerge, and resurface events.
* Does not provide buoyancy.

### Buoyancy volume

* Covers the full playable underwater area.
* Configures or works with `BuoyancyEffector2D`.
* Tracks submerged Rigidbody2D objects.
* Applies optional custom drag.
* Produces underwater and bubble state.
* Does not create a surface splash when entered through its side or bottom.

Track logical Rigidbody2D contacts rather than treating every Collider2D event as a separate object.

A multi-collider character should produce one logical entry and one logical exit.

Do not apply strong `BuoyancyEffector2D` drag and strong custom drag simultaneously unless deliberate and documented.

---

## Reflection Rules

Planar reflection rendering must scale with compatible water-plane groups, not raw water-object count.

A compatible group is determined by relevant values such as:

* Plane normal.
* Plane height within a documented tolerance.
* Main camera.
* Reflection culling mask.
* Reflection quality settings.

Reflection cameras must not render:

* Themselves.
* Other reflection cameras.
* Reflection-only helper objects.
* Water layers when doing so would recurse.

Low-quality modes should avoid a reflection camera entirely.

Do not create one always-rendering reflection camera per water body in the new package.

---

## FX Rules

Runtime splash, bubble, foam, droplet, and underwater effects must use pooling.

Normal gameplay must not repeatedly call `Instantiate` and `Destroy` for water FX.

Pools must have:

* An explicit owner.
* Configurable prewarm counts.
* Predictable exhaustion behaviour.
* Cleanup on scene or owner disposal.
* No steady-state allocation after warm-up.

Do not copy Cainos prefabs, textures, particles, shaders, or other vendor assets into the package.

Reimplement desired behaviour using project-owned assets and code.

---

## Coding Standards

* Use C# compatible with the Unity version in `ProjectSettings/ProjectVersion.txt`.
* Use package APIs compatible with versions in `Packages/manifest.json`.
* Use PascalCase for types, methods, properties, and public members.
* Use `_camelCase` for private fields.
* Use `[SerializeField] private` rather than public fields for Inspector configuration.
* Keep runtime classes focused on one responsibility.
* Prefer explicit dependency wiring over global searches.
* Do not use `FindObjectOfType`, `FindObjectsOfType`, or `GameObject.Find` for runtime coordination.
* Do not call `GetComponent` repeatedly in per-frame or per-physics-step paths.
* Avoid LINQ, temporary arrays, closure allocation, string construction, and collection creation in hot paths.
* Comments should explain non-obvious reasoning, ownership, numerical constraints, or Unity-specific lifecycle behaviour.
* Editor-only types belong under an `Editor/` folder or behind `#if UNITY_EDITOR`.
* Runtime assemblies must not reference editor assemblies.
* Avoid new package dependencies unless the task explicitly requires one and the dependency is documented.

Do not suppress numerical-instability warnings or lifecycle problems merely to make the Console quiet.

---

## Unity Serialization and Asset Safety

Do not manually alter existing `.meta` GUIDs.

Do not regenerate or replace `.meta` files for existing assets.

When Unity Editor is unavailable:

* Do not fabricate a `.meta` file for an existing asset.
* Do not claim that a new asset has been imported successfully.
* Report any required import or GUID verification as manual Unity work.

Avoid raw edits to these files unless the task explicitly requires them:

```text
*.unity
*.prefab
*.mat
*.asset
*.shadergraph
*.renderTexture
*.meta
ProjectSettings/*.asset
```

When such an edit is required:

* Make the smallest possible change.
* Preserve unrelated serialized values.
* Preserve GUID and fileID references.
* Inspect the final diff for unintended reserialization.
* Describe the exact Editor verification required.

Do not hand-edit Shader Graph serialization to perform an ordinary shader-code change. Prefer package-owned `.hlsl`, `.shader`, or Shader Graph Custom Function code where appropriate.

---

## Licensing and Reference-Code Rules

Treat `Assets/Cainos/` as vendor content.

Do not copy, redistribute, rename, or incorporate Cainos code or assets into `Assets/Water25D/`.

Cainos may be inspected for behavioural ideas such as:

* Inspector grouping.
* Buoyancy configuration.
* Contact filtering.
* Splash thresholds.
* Bubble duration.
* Particle sizing.
* Scene handles.

Reimplement those ideas in package-owned code.

The provenance and redistribution terms of the original custom water implementation are not fully resolved. Preserve it as a baseline and avoid presenting copied code or assets as newly owned package content.

Do not add or change a root license as part of an implementation task unless licensing is the explicit task.

Follow `THIRD_PARTY_NOTICES.md`.

---

## Validation

### Always run

```bash
git diff --check
git status --short
```

Inspect the complete diff for:

* Unrelated package upgrades.
* Unexpected material-value changes.
* Scene or prefab reserialization.
* Changed GUIDs.
* Modified vendor content.
* References from `Assets/Water25D/` into reference-only directories.
* Generated Unity files that should not be committed.

Search package-owned runtime and editor code for unintended reference-system dependencies:

```bash
rg -n "Cainos|InteractiveWaterSystem|DemoScenes|Lucid" Assets/Water25D
```

Explain every intentional result. Documentation and explicitly temporary migration adapters may be valid exceptions.

### Unity tests

When a usable Unity executable is available through `UNITY_PATH`, run relevant tests sequentially:

```bash
"$UNITY_PATH" \
  -batchmode \
  -nographics \
  -projectPath "$PWD" \
  -runTests \
  -testPlatform EditMode \
  -testResults EditMode-results.xml \
  -logFile EditMode.log \
  -quit
```

```bash
"$UNITY_PATH" \
  -batchmode \
  -nographics \
  -projectPath "$PWD" \
  -runTests \
  -testPlatform PlayMode \
  -testResults PlayMode-results.xml \
  -logFile PlayMode.log \
  -quit
```

Run the smallest relevant test set first, then broader tests when practical.

Do not substitute a successful C# text inspection or `dotnet` command for Unity compilation.

If Unity is unavailable, say exactly that. Never claim:

* Unity compilation passed.
* Shader compilation passed.
* A scene opened correctly.
* A prefab retained its references.
* Tests passed.
* Rendering matched the baseline.
* Profiling targets were achieved.

unless those actions were actually performed.

### Manual Unity verification

Rendering, shaders, serialized assets, physics behaviour, Inspector workflows, profiling, and prefab migration usually require Editor verification.

List precise steps, including:

* Scene to open.
* Prefab or GameObject to select.
* Component and field to inspect.
* Expected visual or physics result.
* Profiler module or Frame Debugger event to inspect.
* Comparison baseline.
* Whether Play Mode and Edit Mode must both be checked.

---

## Performance Expectations

Do not describe analytical workload estimates as measured performance.

Use terms such as:

* Estimated cell updates.
* Approximate texture memory.
* Expected allocation reduction.
* Profiling target.

Use measured milliseconds, memory, draw counts, or frame times only when captured with Unity profiling tools.

Steady-state goals for normal gameplay are:

* Zero managed allocation from water runtime code.
* No render-texture recreation.
* No mesh regeneration.
* No material instantiation.
* No particle `Instantiate` or `Destroy`.
* Near-zero simulation work for inactive off-screen water.
* Reflection cost proportional to active reflection groups.

These remain goals until verified on target hardware.

---

## Change Discipline

Keep each task independently reviewable and reversible.

Do not:

* Rewrite the complete system in one task.
* Implement multiple future milestones opportunistically.
* Reformat unrelated files.
* Rename unrelated symbols.
* Upgrade Unity packages as part of a water feature.
* Change tuning values without documenting why.
* Modify source/reference systems to make the new package easier to write.
* Hide missing validation.
* Delete the baseline before migration acceptance.
* Commit benchmark results that were not produced by the benchmark configuration described in the plan.

Report out-of-scope issues rather than silently fixing them.

---

## Required Handoff

End every implementation task with these sections:

### Summary

What was implemented and why.

### Files changed

List every created, modified, moved, or deleted file.

### Behaviour changes

Describe visible, runtime, Editor, serialization, and compatibility effects.

### Validation performed

List exact commands, Unity tests, scenes, profiler captures, or inspections that actually ran.

### Validation not performed

State what could not be run and why.

### Manual Unity work

Provide exact remaining Editor steps.

If none are required, write:

```text
No Unity Editor work required.
```

### Risks and follow-up

List compatibility risks, temporary dependencies, incomplete migration work, and later milestones that were intentionally not implemented.

### Milestone status

State whether the task:

* Did not change milestone status.
* Advanced an active milestone.
* Completed a milestone.

Update the package status document only when the milestone statement is supported by the completed work and validation.

---

## Definition of Done

A task is complete only when:

* Its requested scope is implemented.
* The change follows the active implementation phase.
* Package-owned files are under `Assets/Water25D/`, except documented project-level changes.
* No unintended dependency on a baseline or vendor system was introduced.
* Mutable runtime resources have explicit ownership and cleanup.
* Shared project assets are not mutated unintentionally.
* Serialized references and GUIDs were preserved.
* Relevant available validation was run.
* Unavailable validation is clearly disclosed.
* Manual Unity verification is specific and actionable.
* Documentation reflects any changed public behaviour or architecture.
* Milestone status is accurate.

Code that merely appears plausible is not sufficient evidence that a Unity water feature is complete.
