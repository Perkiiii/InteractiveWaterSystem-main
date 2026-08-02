# Water25D Phase 3 reference fork and adaptation

This record supersedes the earlier concept-only Ameye audit. The project owner
authorized direct use of the local Stylized Water Shader under
`Assets/ReferenceOnly/Stylized Water Shader/` and the local
`Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph`.

The authorized source files were copied through Unity `AssetDatabase.CopyAsset`.
The copied graph, subgraphs, and material were then remapped to the new
package-owned GUIDs. No `.meta` file was copied or hand-authored, and no
production Water25D asset retains a serialized reference into `Assets/ReferenceOnly/`.

## Chosen integration strategy

Water25D uses Option B: existing `SimulatedRipples` materials and controllers
retain their serialized shader/material contract. The new `FlatStylized` profile
uses package-owned Ameye-derived top/front materials. The copied Ameye graph and
its source-default material remain package-owned authoring/provenance assets; the
runtime materials use the package-owned HLSL compatibility fork of that graph's
surface responsibilities so Water25D can keep fixed-capacity arrays, one final
`MaterialPropertyBlock` writer, and the existing reflection snapshot contract.

The compatibility fork retains the Ameye colour, panning, layered-normal,
foam, stylized-lighting, and refraction responsibilities. Water25D-specific
interaction and reflection code remains in the fork's final fragment path. The
fork does not use the source package's displacement, buoyancy, camera, renderer,
or reflection ownership.

## Minimum copied dependency closure

| Source file | Source subgraph/function/texture/material | Water25D copied destination | Treatment | Changes made | Reason for changes | Runtime dependency |
|---|---|---|---|---|---|---|
| `Assets/ReferenceOnly/Stylized Water Shader/Shaders/Stylized Water.shadergraph` | Desktop `GraphData`; colour, normals, foam, refraction, lighting, and vertex sections | `Assets/Water25D/Shaders/Stylized/Water25D_AmeyeStylizedWater.shadergraph` | Copied and modified | Internal asset GUIDs remapped; the Gerstner custom-function node is renamed `FlatModeNoDisplacement`, uses an inline zero-offset/flat-normal body, and has no source HLSL GUID | Keep the graph as the direct Ameye visual base while removing vertex displacement and the reference HLSL dependency | Authoring/provenance asset; production uses the package-owned HLSL fork below |
| Same source graph | Copied material defaults | `Assets/Water25D/Materials/Stylized/Water25D_AmeyeSourceDefaults.mat` | Copied and remapped | Shader and the three texture references point only to package-owned copies | Preserve the source's useful defaults for inspection without retaining reference GUIDs | Authoring/comparison asset; not assigned by legacy controllers |
| `Shaders/Blended Normals.shadersubgraph` | `Blended Normals` | `Shaders/Stylized/SubGraphs/Ameye_BlendedNormals.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Preserve the source normal-layer graph | Referenced by copied graph |
| `Shaders/Depth Fade.shadersubgraph` | `Depth Fade` | `Shaders/Stylized/SubGraphs/Ameye_DepthFade.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Preserve the source depth shaping | Referenced by copied graph |
| `Shaders/Overlay.shadersubgraph` | `Overlay` | `Shaders/Stylized/SubGraphs/Ameye_Overlay.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Preserve source overlay blending | Referenced by copied graph |
| `Shaders/Panning UV.shadersubgraph` | `Panning UV` | `Shaders/Stylized/SubGraphs/Ameye_PanningUV.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Preserve source world/local panning behavior | Referenced by copied graph |
| `Shaders/Refracted UV.shadersubgraph` | `Refracted UV` | `Shaders/Stylized/SubGraphs/Ameye_RefractedUV.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Preserve source refraction UV structure | Referenced by copied graph |
| `Shaders/Scene Position.shadersubgraph` | `Scene Position` nested dependency | `Shaders/Stylized/SubGraphs/Ameye_ScenePosition.shadersubgraph` | Copied unchanged | GUID-safe destination remap only | Close the copied depth/refraction graph dependency | Referenced by copied subgraphs |
| `Shaders/DistortUV.hlsl` | `DistortUV_float` | `Shaders/Stylized/Includes/Ameye_DistortUV.hlsl` | Copied unchanged | New Unity GUID only | Reuse the source refraction/distortion function | Included by `Water25D_AmeyeAdaptation.hlsl` |
| `Shaders/Lighting.hlsl` | `LightingSpecular`, `MainLighting`, `AdditionalLighting` | `Shaders/Stylized/Includes/Ameye_Lighting.hlsl` | Copied unchanged | New Unity GUID only | Preserve source toon/specular lighting functions | Included by the package adaptation seam |
| `Textures/Normals 1.png` | Source layered normal map | `Textures/Stylized/Ameye_Normals1.png` | Copied unchanged | New Unity GUID/imported asset | Provide the source normal detail by default | Bound by the new style profile/materials |
| `Textures/Foam 4.png` | Intersection foam breakup | `Textures/Stylized/Ameye_Foam4.png` | Copied unchanged | New Unity GUID/imported asset | Provide source foam breakup for contact/intersection treatment | Bound by Ameye top/front materials |
| `Textures/Foam 5.png` | Surface foam breakup | `Textures/Stylized/Ameye_Foam5.png` | Copied unchanged | New Unity GUID/imported asset | Provide source foam breakup for rings/surface treatment | Bound by Ameye top/front materials and flat profile detail input |
| `Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph` | Interactive graph foam, distortion, refraction, and mask ideas | None | Inspected; not copied | No interaction camera, global RT, `_GlobalEffectRT`, or `_OrthographicCamSize` adopted | Water25D's fixed arrays are authoritative | None |

The source tree contains no separate mobile graph, front shader, caustic texture,
gradient asset, reflection script, or independent reflection camera required by
the desktop graph. Those were recorded during inventory and are intentionally not
invented or copied. The source Gerstner include, buoyancy script, demo scripts,
the unused `HSVLerp.hlsl` helper, and URP renderer/pipeline assets are also
outside the closure.

## Package-owned production fork

| Source responsibility | Package destination | Treatment |
|---|---|---|
| Graph depth colour and band shaping | `Shaders/Stylized/Water25D_AmeyeTopSurface.shader`, `Water25D_AmeyeFrontSurface.shader`, `Includes/Water25D_AmeyeAdaptation.hlsl` | Adapted to Water25D local surface coordinates and profile values |
| Graph panning UV and normal animation | Same shaders plus `Water25D_AmeyeAdaptation.hlsl` | Uses the copied `Ameye_DistortUV`/panning contract with safe local UV fallback |
| Graph layered normals | Same shaders | Source normal copy is sampled through the existing Water25D profile/MPB inputs; no vertex displacement is added in `FlatStylized` |
| Graph stylized lighting | `Water25D_AmeyeAdaptation.hlsl` and both shaders | Uses the copied source specular function with Water25D's profile direction/strength and no required source light loop |
| Graph foam breakup | Both shaders | Source Foam 4/Foam 5 samples multiply analytical Water25D boundary/ring/contact/wake masks; missing painterly atlases still use analytical fallback |
| Graph refraction/distortion | Both shaders | Copied `DistortUV_float` is used only when Water25D's optional input contract is enabled |
| Water25D fixed interactions | Existing `Water25D_InteractionMasks.hlsl` and the new top/front shaders | Preserved fixed arrays for rings, body-keyed contact foam, wakes, metadata, and analytical fallback |
| Water25D reflection | Existing `WaterReflectionManager`, `WaterReflectionModule`, `WaterRenderingModule` and new top shader | Shader consumes the published texture, matrix, enabled/fallback state, strength, tint, Fresnel weighting, and interaction distortion; no Ameye camera or reflection pass runs |

`Water25D_AmeyeTopSurface.shader` remains a four-vertex-compatible fragment
presentation shader: the `FlatStylized` branch never changes vertex positions.
The front shader reuses the same copied colours, normals, foam, timing, and
optional caustic contract without forcing horizontal-surface assumptions onto the
XY front quad.

## Retained, modified, and removed source behavior

Retained from the source implementation:

- shallow/deep colour treatment and graphic banding;
- local/world-stable panning and animated detail;
- layered normal animation;
- Fresnel response and stylized highlights;
- foam breakup and threshold shaping;
- optional refraction/distortion and transparency-compatible opacity treatment;
- copied normal and foam texture defaults.

Modified for Water25D:

- source graph asset references use new package-owned GUIDs;
- the source displacement node is a zero-offset flat-mode node;
- top/front shaders use Water25D profile and MPB property IDs;
- rings, contact foam, wakes, and painterly metadata are evaluated from existing
  fixed arrays;
- reflection is sampled from the shared Water25D manager snapshot;
- optional scene inputs remain source-gated and disabled by default;
- front presentation uses the same copied visual language without an interaction
  camera or horizontal-depth assumptions.

Removed or rejected:

- Gerstner vertex displacement;
- wave-driven 3D buoyancy and any GPU-to-CPU height readback;
- source reflection scripts/cameras and per-water reflection ownership;
- demo scripts/scenes and source renderer/pipeline assets;
- interaction cameras, global interaction RenderTextures,
  `_GlobalEffectRT`, and `_OrthographicCamSize`;
- mandatory depth/opaque/sorting-layer texture requirements.

## Provenance and licence note

The project owner explicitly authorized copying, modifying, and using the local
Ameye assets for this Water25D integration. No licence text or attribution file
was supplied inside the authorized source folder, so this record does not invent
licence terms. `THIRD_PARTY_NOTICES.md` records the authorization and the absence
of supplied terms; the owner should retain any original purchase/licence evidence
outside this package documentation.

Deleting `Assets/ReferenceOnly/` after this integration must not break Water25D.
The only remaining `ReferenceOnly` matches under the package are intentional
provenance and audit documentation.
