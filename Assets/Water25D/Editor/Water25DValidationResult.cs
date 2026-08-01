using UnityEngine;

namespace Water25D.Editor
{
    public enum Water25DValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum Water25DFixAction
    {
        None,
        AssignPackageDefaults,
        RepairHierarchy,
        AssignTopMaterial,
        AssignFrontMaterial,
        AssignRippleMaterial,
        AssignStyleProfile,
        AssignQualityProfile,
        SelectObject
    }

    /// <summary>
    /// An editor validation finding with an intentionally small, safe fix vocabulary.
    /// </summary>
    public sealed class Water25DValidationResult
    {
        public Water25DValidationSeverity Severity { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public Water25DFixAction FixAction { get; private set; }
        public Object TargetObject { get; private set; }

        public bool HasFix
        {
            get { return FixAction != Water25DFixAction.None; }
        }

        public Water25DValidationResult(
            Water25DValidationSeverity severity,
            string title,
            string message,
            Water25DFixAction fixAction = Water25DFixAction.None,
            Object targetObject = null)
        {
            Severity = severity;
            Title = title;
            Message = message;
            FixAction = fixAction;
            TargetObject = targetObject;
        }
    }
}
