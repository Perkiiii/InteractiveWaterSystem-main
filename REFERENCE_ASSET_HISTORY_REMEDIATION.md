# Reference Asset History-Remediation Report

> **Status:** Proposal only. No history rewrite, force-push, remote deletion, or release modification has been performed.

## Scope and evidence

The third-party development-reference assets listed below first entered repository history in commit `b2e5d4b50f885798835f7d3bde2e94cd9ad112b3`. At the start of this audit that commit was the tip of both local `main` and `origin/main`, so the affected reachable range began at `b2e5d4b50f885798835f7d3bde2e94cd9ad112b3` and continued through `main`—at that time, the single tip commit. No tag containing the commit was found. Recheck every remote ref immediately before any remediation because branches and tags may have changed since this report.

No licence or attribution file was present with these assets. No explicit public source-redistribution permission was located on [Ameye's official tutorial page](https://ameye.dev/notes/stylized-water-shader/) or [Minions Art's official Patreon tutorial page](https://www.patreon.com/minionsart/posts/shader-graph-30490169). The pages provide tutorial or download access, but the inspected material did not state a permission to redistribute the downloaded source assets publicly. This is an evidence report, not a legal conclusion.

## Exact affected tracked paths

```text
Assets/ReferenceOnly/Stylized Water Shader.meta
Assets/ReferenceOnly/Stylized Water Shader/Materials.meta
Assets/ReferenceOnly/Stylized Water Shader/Materials/Shader Graphs_Stylized Water.mat
Assets/ReferenceOnly/Stylized Water Shader/Materials/Shader Graphs_Stylized Water.mat.meta
Assets/ReferenceOnly/Stylized Water Shader/Scripts.meta
Assets/ReferenceOnly/Stylized Water Shader/Scripts/BuoyantObject.cs
Assets/ReferenceOnly/Stylized Water Shader/Scripts/BuoyantObject.cs.meta
Assets/ReferenceOnly/Stylized Water Shader/Scripts/GerstnerWaveDisplacement.cs
Assets/ReferenceOnly/Stylized Water Shader/Scripts/GerstnerWaveDisplacement.cs.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Blended Normals.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Blended Normals.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Depth Fade.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Depth Fade.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/DistortUV.hlsl
Assets/ReferenceOnly/Stylized Water Shader/Shaders/DistortUV.hlsl.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/GerstnerWaves.hlsl
Assets/ReferenceOnly/Stylized Water Shader/Shaders/GerstnerWaves.hlsl.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/HSVLerp.hlsl
Assets/ReferenceOnly/Stylized Water Shader/Shaders/HSVLerp.hlsl.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Lighting.hlsl
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Lighting.hlsl.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Overlay.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Overlay.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Panning UV.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Panning UV.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Refracted UV.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Refracted UV.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Scene Position.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Scene Position.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Shore Color.shadersubgraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Shore Color.shadersubgraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Stylized Water.shadergraph
Assets/ReferenceOnly/Stylized Water Shader/Shaders/Stylized Water.shadergraph.meta
Assets/ReferenceOnly/Stylized Water Shader/Textures.meta
Assets/ReferenceOnly/Stylized Water Shader/Textures/Foam 4.png
Assets/ReferenceOnly/Stylized Water Shader/Textures/Foam 4.png.meta
Assets/ReferenceOnly/Stylized Water Shader/Textures/Foam 5.png
Assets/ReferenceOnly/Stylized Water Shader/Textures/Foam 5.png.meta
Assets/ReferenceOnly/Stylized Water Shader/Textures/Normals 1.png
Assets/ReferenceOnly/Stylized Water Shader/Textures/Normals 1.png.meta
Assets/ReferenceOnly/Stylized Water Shader/URP-HighFidelity-Renderer.asset
Assets/ReferenceOnly/Stylized Water Shader/URP-HighFidelity-Renderer.asset.meta
Assets/ReferenceOnly/Stylized Water Shader/URP-HighFidelity.asset
Assets/ReferenceOnly/Stylized Water Shader/URP-HighFidelity.asset.meta
Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph
Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph.meta
```

## Backup before an approved rewrite

1. Freeze pushes and notify all collaborators.
2. Create an offline mirror: `git clone --mirror https://github.com/Perkiiii/InteractiveWaterSystem-main.git ../InteractiveWaterSystem-main-before-reference-purge.git`.
3. In the mirror, run `git fsck --full` and save `git show-ref` output outside the repository.
4. Preserve the mirror read-only until the rewritten repository and all required builds have been verified.

## Proposed rewrite

After explicit owner approval, run from a fresh disposable mirror or clone with `git-filter-repo` installed:

```text
git filter-repo --force --path "Assets/ReferenceOnly/Stylized Water Shader" --path "Assets/ReferenceOnly/Stylized Water Shader.meta" --path "Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph" --path "Assets/ReferenceOnly/StylizedWaterInteractiveUpdate.shadergraph.meta" --invert-paths
```

At the time of this report, `main` was the only local or remote-tracking branch containing the introduction commit and no affected tags were found. Re-enumerate all local and remote branches and tags before rewriting. Every affected branch and tag must be rewritten consistently.

After verification and a coordinated maintenance window, the exact push required for the currently identified branch would be:

```text
git push origin --force-with-lease main
```

If the preflight finds affected tags, each rewritten tag must also be force-updated explicitly, for example:

```text
git push origin --force refs/tags/<tag>:refs/tags/<tag>
```

Do not use a blanket tag force-push without first identifying the affected tags.

## Collaborator and retention impact

A rewrite changes commit identifiers. Collaborators must stop work during the rewrite, preserve any unpublished work separately, and make a fresh clone afterward rather than merge old history into the rewritten repository. Open pull requests and external commit links may need recreation or repair. Forks, prior clones, CI artifacts, package caches, GitHub caches, and downloaded archives may retain copies even after the canonical repository is rewritten.

Obtain explicit repository-owner approval before executing this proposal and coordinate any required notices or platform support requests.
