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
        private static GUIStyle _foldout;
        private static GUIStyle _subsection;
        private static GUIStyle _statusRow;
        private static GUIStyle _toolbar;
        private static GUIStyle _inlineButton;
        private static GUIStyle _metricLabel;
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

        public static GUIStyle Foldout
        {
            get { Ensure(); return _foldout; }
        }

        public static GUIStyle Subsection
        {
            get { Ensure(); return _subsection; }
        }

        public static GUIStyle StatusRow
        {
            get { Ensure(); return _statusRow; }
        }

        public static GUIStyle Toolbar
        {
            get { Ensure(); return _toolbar; }
        }

        public static GUIStyle InlineButton
        {
            get { Ensure(); return _inlineButton; }
        }

        public static GUIStyle MetricLabel
        {
            get { Ensure(); return _metricLabel; }
        }

        public static GUIStyle SmallButton
        {
            get { Ensure(); return _smallButton; }
        }

        public static Color ValidColor
        {
            get { return EditorGUIUtility.isProSkin ? new Color(0.45f, 0.86f, 0.55f) : new Color(0.08f, 0.48f, 0.18f); }
        }

        public static Color WarningColor
        {
            get { return EditorGUIUtility.isProSkin ? new Color(1f, 0.72f, 0.25f) : new Color(0.70f, 0.36f, 0.02f); }
        }

        public static Color ErrorColor
        {
            get { return EditorGUIUtility.isProSkin ? new Color(1f, 0.43f, 0.43f) : new Color(0.70f, 0.08f, 0.08f); }
        }

        internal static void Ensure()
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
            _foldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(3, 0, 0, 0)
            };
            _subsection = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(2, 2, 5, 2)
            };
            _statusRow = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(7, 7, 4, 4),
                margin = new RectOffset(3, 3, 3, 3)
            };
            _toolbar = new GUIStyle(EditorStyles.toolbar)
            {
                padding = new RectOffset(3, 3, 3, 3),
                margin = new RectOffset(4, 4, 0, 4)
            };
            _inlineButton = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 18f
            };
            _metricLabel = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                wordWrap = false
            };
            _smallButton = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 18f,
                padding = new RectOffset(4, 4, 1, 1)
            };
        }
    }
}
