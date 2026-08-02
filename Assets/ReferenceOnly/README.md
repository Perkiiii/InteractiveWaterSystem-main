# Optional local development references

This directory is reserved for optional third-party visual references installed locally by individual developers. Its local contents are not part of Water25D and must not become runtime, editor, or serialized package dependencies.

Each developer must obtain any reference asset directly from its original creator and comply with the terms supplied by that creator. Approved reference pages currently include:

- [Ameye's Stylized Water Shader](https://ameye.dev/notes/stylized-water-shader/)
- [Minions Art — Shader Graph Interactive Water](https://www.patreon.com/minionsart/posts/shader-graph-30490169)

Only project-owned reimplementations may enter `Assets/Water25D/`. Do not copy third-party source code, Shader Graphs, scripts, textures, materials, particles, prefabs, pipeline assets, or other downloaded assets into the Water25D package without explicit licensing approval.

Exclude locally installed reference assets from every Water25D export and distribution. The repository ignores all contents of this directory except this README and its Unity `.meta` file. Keep original downloaded archives under the ignored root-level `ReferencePackages/` directory, outside Unity's `Assets` database.
