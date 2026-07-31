## **Project**

This is a Unity 6 URP side-view game containing a custom 2.5D water system.

The canonical water-system architecture and migration plan is:

`Docs/Water2_5D/IMPLEMENTATION_PLAN.md`

Read the relevant sections of that document before changing water-system code.

## **Working rules**

* Work on one implementation phase at a time.  
* Preserve the original water prefab and current rendered appearance until a replacement has passed comparison tests.  
* Do not rewrite the complete water system in one change.  
* Do not implement the compute-shader backend before the optimized Custom Render Texture backend has been benchmarked.  
* Treat Cainos code and assets as behavioral references only. Reimplement required behavior in project-owned code unless redistribution rights have been confirmed.  
* Do not unintentionally modify shared material assets, Custom Render Texture assets, or other project templates at runtime.  
* Every active water body must own independent mutable ripple state unless sharing is explicitly designed.  
* Avoid editing Unity scenes, prefabs, Shader Graph files, or serialized assets manually unless the task explicitly requires it.  
* Do not fabricate Unity compilation, profiler, rendering, or test results when Unity Editor is unavailable.  
* Keep changes small enough to review and revert independently.

## **Required handoff**

For every implementation task:

1. Summarize the approach before editing.  
2. List every changed or created file.  
3. Explain behavior changes and compatibility risks.  
4. Report validations that were actually run.  
5. List Unity Editor checks that still require manual verification.  
6. Update `Docs/Water2_5D/STATUS.md` when a milestone is completed.

## **Definition of done**

A change is not complete merely because the C\# code appears correct. It must preserve serialized references, compile in the project’s Unity version, avoid unexpected allocations or resource sharing, and include clear manual validation steps where automated Unity validation was unavailable.

