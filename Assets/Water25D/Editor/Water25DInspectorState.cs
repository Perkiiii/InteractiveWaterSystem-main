using UnityEditor;

namespace Water25D.Editor
{
    /// <summary>
    /// Editor-only state for the authoring workflow. Nothing here is serialized onto runtime
    /// components, so changing foldouts or handle visibility cannot dirty a scene or prefab.
    /// </summary>
    internal static class Water25DInspectorState
    {
        private const string Prefix = "Water25D.Inspector.";

        public static bool GetFoldout(string section, bool defaultValue)
        {
            return EditorPrefs.GetBool(Prefix + "Foldout." + section, defaultValue);
        }

        public static void SetFoldout(string section, bool value)
        {
            EditorPrefs.SetBool(Prefix + "Foldout." + section, value);
        }

        public static bool ShowSceneHandles
        {
            get { return EditorPrefs.GetBool(Prefix + "ShowSceneHandles", true); }
            set { EditorPrefs.SetBool(Prefix + "ShowSceneHandles", value); }
        }
    }
}
