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

## Before adding a root `LICENSE`

The repository currently has no root license because the project contains mixed-origin code, shaders, textures, fonts, prefabs, and scenes. Add a root license only after deciding which files you own and which third-party terms apply. A root license should not accidentally relicense the Cainos or Annulus Games content.
