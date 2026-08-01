# Water25D portability checklist

The package is designed to be copied as a self-contained `Assets/Water25D/` folder. Copy the folder and its adjacent `.meta` files into a compatible Unity project, then allow Unity to import the assets before creating a controller.

The package-owned runtime and editor code only depends on Unity runtime/editor APIs and the project’s URP shader library. It does not require the original custom water implementation, Cainos content, demo scenes, Lucid Editor, or Unity MCP.

## Destination-project checklist

- Use a Unity version and URP package compatible with the package source project. Confirm the destination project’s versions from `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, and `Packages/packages-lock.json`.
- Confirm the destination project has the 2D physics components used by the package, including `BuoyancyEffector2D`.
- Create or map project layers and sorting layers as needed for the destination game. The package does not silently add or rename project-level layers.
- Configure any required URP renderer features or Camera Sorting Layer Texture settings in the destination project. These are project policy, not serialized package dependencies.
- Import the package in a clean destination project and create a controller through the package menu. Confirm that the five default assets are assigned, all generated children and physics volumes are created without references outside `Assets/Water25D/`, and the top/front renderer material slots contain persistent package materials.
- Save and reopen a scene or prefab containing the controller. Confirm that the surface materials and profiles remain assigned and that preview meshes regenerate without becoming serialized scene mesh data.
- Run the package EditMode and PlayMode tests in the destination project before accepting a migration.

## Dependency audit

From the package root, the expected audit is:

```text
rg -n "Cainos|InteractiveWaterSystem|DemoScenes|Lucid" Assets/Water25D
```

Any result must be documentation describing the boundary or an explicitly temporary migration adapter. The current package has no runtime dependency on those systems.
