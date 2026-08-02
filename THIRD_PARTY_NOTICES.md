# Third-party notices

This repository contains a mixture of project-specific work and third-party Unity assets. This file records what was found in the project; it is not a license grant and is not a substitute for checking the original purchase or redistribution terms.

## Cainos assets

The following vendor content is present under `Assets/Cainos`:

- `Assets/Cainos/Interactive Pixel Water`
- `Assets/Cainos/Pixel Art Platformer - Village Props`
- `Assets/Cainos/Common`
- `Assets/Cainos/Third Party/Lucid Editor`

The project includes vendor documentation and changelogs, including the [Cainos Interactive Pixel Water documentation](https://docs.cainos.net/interactive-pixel-water). Confirm that the repository owner has permission to redistribute these assets in a public GitHub repository before publishing. If the assets are licensed only to the purchaser, remove them from the public distribution and document how users can obtain them instead.

The Cainos folder also contains optional `.unitypackage` patch archives. They are ignored by the repository's Unity `.gitignore` and are not part of the tracked source set.

## Lucid Editor / Annulus Games

The included files under `Assets/Cainos/Third Party/Lucid Editor` contain a modified/local copy of Lucid Editor. The included `License.txt` identifies the upstream project as [Annulus Games LucidEditor](https://github.com/AnnulusGames/LucidEditor) and includes the MIT License with copyright attribution to Annulus Games (2023).

Keep `Assets/Cainos/Third Party/Lucid Editor/License.txt` with any redistribution of those files.

## Custom water-system provenance

The original project documentation referenced `https://github.com/daothienphu/InteractiveWaterSystem` for the custom `InteractiveWater` and `SimplePlanarReflection` implementation. Confirm whether this repository is a fork or derivative, preserve any required attribution, and add an appropriate license before publishing the custom code.

## Ameye Stylized Water Shader — authorized Phase 3 fork

The project owner explicitly authorized the Water25D Phase 3 integration to copy,
modify, and use the local source under:

- `Assets/ReferenceOnly/Stylized Water Shader/`
- `Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph`

The minimum Ameye visual dependency closure is now copied into package-owned
folders under `Assets/Water25D/Shaders/Stylized/`,
`Assets/Water25D/Textures/Stylized/`, and
`Assets/Water25D/Materials/Stylized/`. The copy was performed through Unity's
`AssetDatabase.CopyAsset`; destination GUIDs are distinct and production assets
do not serialize references into `Assets/ReferenceOnly/`. The exact mapping and
adaptations are recorded in
[`Assets/Water25D/Documentation/Design/PHASE3_REFERENCE_ADAPTATION.md`](Assets/Water25D/Documentation/Design/PHASE3_REFERENCE_ADAPTATION.md).

No licence text or attribution file was supplied inside the authorized source
folder. This notice records the owner's authorization but does not invent licence
terms or grant public redistribution rights. Preserve the original purchase or
licence evidence supplied to the project owner before distributing the package.

The Minions Art graph remains an inspected development reference only. Its
interaction-camera and global RenderTexture architecture was not copied or
adopted; Water25D's fixed interaction arrays remain authoritative.

## Before adding a root `LICENSE`

The repository currently has no root license because the project contains mixed-origin code, shaders, textures, fonts, prefabs, and scenes. Add a root license only after deciding which files you own and which third-party terms apply. A root license should not accidentally relicense the Cainos or Annulus Games content.
