using UnityEditor;
using UnityEngine;

namespace Water25D.Editor
{
    /// <summary>
    /// Shared, theme-aware GUI styles for the package authoring surfaces. Styles are created
    /// once per editor domain so the inspector does not rebuild them on every repaint.
    /// </summary>
    internal static class Water25DInspectorStyles
    {
        private static bool _initialized;
        private static GUIStyle _header;
        private static GUIStyle _subtitle;
        private static GUIStyle _sectionCard;
        private static GUIStyle _subsection;
        private static GUIStyle _smallButton;

        public static GUIStyle Header
        {
            get { Ensure(); return _header; }
        }

        public static GUIStyle Subtitle
        {
            get { Ensure(); return _subtitle; }
        }

        public static GUIStyle SectionCard
        {
            get { Ensure(); return _sectionCard; }
        }

        public static GUIStyle Subsection
        {
            get { Ensure(); return _subsection; }
        }

        public static GUIStyle SmallButton
        {
            get { Ensure(); return _smallButton; }
        }

        public static void Ensure()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _header = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 9, 9),
                margin = new RectOffset(4, 4, 4, 4)
            };
            _subtitle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Italic
            };
            _sectionCard = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 5, 8),
                margin = new RectOffset(3, 3, 4, 4)
            };
            _subsection = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(2, 2, 5, 2)
            };
            _smallButton = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 18f,
                padding = new RectOffset(4, 4, 1, 1)
            };
        }
    }
}
