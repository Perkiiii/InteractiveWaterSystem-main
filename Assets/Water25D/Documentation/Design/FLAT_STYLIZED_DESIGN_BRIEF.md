# Water25D Flat-Stylized Redesign

> **Document status:** Design investigation resolved. This file records the selected direction without duplicating the implementation plan or progress log.

## Original design question

Based on the latest implementation of `Assets/Water25D/`, how should the existing system be changed so that its default appearance becomes calm, geometrically flat, reflective water with localized rings, contact foam, wakes, and entry/exit splashes, while preserving its existing physics, interaction, reflection, editor, and package architecture?

## Resolved direction

Water25D's production direction is calm, geometrically flat, reflective water with localized rings, body-keyed contact foam, distance-based wakes, and pooled flipbook entry/exit splashes. It preserves the existing modular physics, interaction, reflection, authoring, and package architecture instead of creating a second water framework.

Newly created water follows `FlatStylized`. Existing serialized controllers remain compatible with `SimulatedRipples` until an explicit, reversible migration is requested. The standard flat mode uses a four-vertex XZ top with fragment-stage presentation and retains the XY front surface for underwater colour, distortion, caustics, flow, foam, and interaction intersections. Gameplay buoyancy remains a flat 2D system independent from visual ripple state.

## Architectural decisions

- `Water25DController` coordinates the existing focused modules.
- `FlatStylized` uses a geometrically flat XZ top; the XY front surface continues to provide underwater presentation.
- `WaterSurfacePresentationModule` owns fixed-capacity procedural rings, logical-body contact foam, and distance-based wake data without spawning per-effect renderers or GameObjects.
- `WaterRenderingModule` becomes the sole final `MaterialPropertyBlock` writer for style, quality, interaction, ripple, and reflection data.
- Rings, contact foam, and wakes use analytical placement with optional painterly mask atlases. Missing artwork falls back to analytical rendering.
- The CRT backend remains available only for `SimulatedRipples`; `FlatStylized` does not allocate or tick it.
- Shared reflection management remains central. Low-cost stylized reflection is camera-free, while compatible planar reflections share cameras and textures.
- Entry/exit flipbook splashes and underwater effects use owned, prewarmed pools.
- Flat 2D buoyancy remains independent from all presentation state.
- Existing profiles, materials, prefabs, and controllers are not destructively retuned or converted.

## Visual direction

The desired result is animation-inspired rather than physically literal:

- restrained shallow/deep colour transitions and optional graphic bands;
- calm scrolling surface detail with stylized highlights and Fresnel response;
- balanced reflection and refraction with controlled distortion;
- irregular painted rings, foam breakup, and wake streaks with stable variation;
- thin flow and edge highlights rather than dense noise;
- restrained front-water distortion, caustics, light shafts, and top-edge foam;
- coherent interaction marks across the top/front seam;
- no vertex displacement in `FlatStylized`.

## Approved development references

The following resources are approved design and implementation references only. They are not runtime or package dependencies, and their approval does not grant permission to copy their source or assets:

- [Ameye's Stylized Water Shader](https://ameye.dev/notes/stylized-water-shader/) — reference for colour gradients, Fresnel response, animated normals, stylized highlights, foam layering, refraction, caustics, and reflection presentation.
- [Minions Art — Shader Graph Interactive Water](https://www.patreon.com/minionsart/posts/shader-graph-30490169) — reference for hand-painted ripple presentation, stable randomized rotation, expansion and fade behaviour, painterly interaction masks, distortion, edge foam, and depth-based colouring.
- [Unity — How to Make Nature Shaders with Shader Graph in 2022 LTS](https://unity.com/blog/engine-platform/nature-shaders-with-shader-graph-in-2022-lts) — reference for restrained stylized motion, graphic flow lines, thin edge highlights, foam balance, ripple presentation, planar-reflection composition, and animation-inspired art direction.

Water25D will reimplement selected concepts using project-owned C#, HLSL, shaders, profiles, tests, and artwork. Third-party code, shaders, textures, prefabs, particles, or other assets will not be copied without explicit licensing approval.

## Explicit non-goals

- No Gerstner displacement or physical-wave gameplay buoyancy in standard flat mode.
- No mandatory scene-depth foam, opaque texture, depth texture, or Camera Sorting Layer Texture dependency.
- No orthographic interaction-camera or RenderTexture stamping pipeline; Water25D already owns authoritative interaction data.
- No external shader-package dependency or first-release adapter.
- No copied third-party code, textures, particles, prefabs, or shader graphs.
- No automatic conversion of existing `SimulatedRipples` controllers.

## Implementation routing

The four remaining bounded phases are:

1. Wake completion and interaction validation.
2. Rendering ownership and painterly interactions.
3. Stylized water and reflection presentation.
4. FX, tooling, migration, and production validation.

Detailed implementation requirements, acceptance criteria, file changes, tests, budgets, and migration rules belong in `FLAT_STYLIZED_IMPLEMENTATION_PLAN.md`. Exact implementation progress, Unity validation evidence, manual checks, and known tooling issues belong in `Assets/Water25D/Documentation/STATUS.md`.
