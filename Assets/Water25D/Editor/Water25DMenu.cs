using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Water25D.Editor
{
    internal static class Water25DMenu
    {
        [MenuItem("GameObject/Water 2.5D/Water 2.5D Controller", false, 10)]
        private static void CreateWater()
        {
            CreateWaterInScene(SceneManager.GetActiveScene(), true);
        }

        // The menu action targets the active scene, while EditMode tests use this private
        // overload to exercise the same creation path inside an unsaved preview scene.
        private static GameObject CreateWaterInScene(Scene destinationScene, bool selectCreatedObject)
        {
            var gameObject = new GameObject("Water25D");
            try
            {
                if (destinationScene.IsValid() && destinationScene.isLoaded)
                {
                    SceneManager.MoveGameObjectToScene(gameObject, destinationScene);
                }

                Undo.RegisterCreatedObjectUndo(gameObject, "Create Water 2.5D");
                var controller = gameObject.AddComponent<Water25DController>();
                Water25DEditorDefaults.AssignDefaults(controller, true);
                controller.RepairHierarchyAndRebuild();
                if (selectCreatedObject)
                {
                    Selection.activeGameObject = gameObject;
                    EditorGUIUtility.PingObject(gameObject);
                }

                return gameObject;
            }
            catch
            {
                Object.DestroyImmediate(gameObject);
                throw;
            }
        }
    }
}
