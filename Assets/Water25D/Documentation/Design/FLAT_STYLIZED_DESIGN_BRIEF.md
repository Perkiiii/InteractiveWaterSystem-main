\# Water25D Flat-Stylized Redesign

\#\# Primary question

Based on the latest GitHub implementation of \`Assets/Water25D/\`, how should the existing system be changed so that its default appearance becomes calm, geometrically flat, reflective water with localized rings, contact foam, wakes, and entry/exit splashes, while preserving its existing physics, interaction, reflection, editor, and package architecture?

\#\# Goals

\#\#\# 1\. Audit the current Water25D implementation

Inspect the latest repository commit and document:

\* The current \`Assets/Water25D/\` structure.  
\* The current phase and validation state from \`STATUS.md\`.  
\* Existing runtime, editor, shader, profile, prefab, sample, and test files.  
\* Which systems are complete, partial, placeholder, unused, or missing.  
\* The verified Unity, URP, renderer, assembly, and project-setting dependencies.

Use \`Water25D\` consistently as the technical identifier.

Treat current code and serialized assets as implementation evidence. Treat planning documents as architectural intent rather than proof of completed work.

\#\#\# 2\. Trace the current architecture and runtime flow

Explain how the actual repository currently handles:

\* Hierarchy generation and repair.  
\* Top and front mesh generation.  
\* Runtime materials and resources.  
\* Surface-crossing detection.  
\* Buoyancy and underwater physics.  
\* Logical multi-collider contact tracking.  
\* Ripple simulation.  
\* Reflection registration and rendering.  
\* FX requests and pooling.  
\* Profiles, Inspector editing, resizing, disabling, and cleanup.

Produce architecture and sequence diagrams using the real classes and methods.

\#\#\# 3\. Design the flat-stylized surface architecture

Determine the smallest coherent architectural change required to support:

\* A geometrically flat XZ top surface.  
\* Reflection-led visual presentation.  
\* Expanding surface rings.  
\* Contact foam around partially submerged objects.  
\* Distance-based wakes.  
\* Pooled entry and exit splashes.  
\* Continued XY front-surface underwater rendering.  
\* Flat gameplay buoyancy independent from visual effects.

Compare extending existing modules with introducing a dedicated surface-presentation module. Avoid creating a second competing water framework.

Choose one architecture and justify it.

\#\#\# 4\. Select implementations for rings, foam, wakes, and splashes

Compare practical implementation options for each feature.

For rings, compare:

\* Pooled procedural ring renderers.  
\* A per-water interaction-mask texture.  
\* GPU arrays or buffers.  
\* Reusing the current ripple texture.

For foam, compare:

\* Emitter-driven contact foam.  
\* A persistent foam mask.  
\* Optional scene-depth foam for opaque environmental geometry.

For wakes and splashes, determine:

\* Ownership.  
\* Pooling.  
\* Interaction-data requirements.  
\* Effect selection from speed, direction, object width, and entry or exit state.  
\* Multi-collider deduplication.  
\* Off-screen behavior.

Recommend a first production implementation for each feature.

\#\#\# 5\. Produce a file-by-file implementation plan

List every relevant file that should be:

\* Modified.  
\* Added.  
\* Deprecated.  
\* Left unchanged.  
\* Removed only after migration.

For each file include:

\* Exact path.  
\* Current responsibility.  
\* Planned responsibility.  
\* Fields and methods affected.  
\* Serialized or public API changes.  
\* Resource ownership.  
\* Dependencies.  
\* Migration risks.  
\* Required tests.

Include implementation-level C\# and shader sketches for the main architectural changes.

End with a phased sequence of small, reviewable implementation packages.  
