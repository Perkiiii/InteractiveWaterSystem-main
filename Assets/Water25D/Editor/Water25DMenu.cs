using UnityEditor;
using UnityEngine;

namespace Water25D.Editor
{
    internal static class Water25DMenu
    {
        [MenuItem("GameObject/Water 2.5D/Water 2.5D Controller", false, 10)]
        private static void CreateWater()
        {
            var gameObject = new GameObject("Water25D");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Water 2.5D");
            var controller = gameObject.AddComponent<Water25DController>();
            Water25DEditorDefaults.AssignDefaults(controller);
            controller.RepairHierarchyAndRebuild();
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}
