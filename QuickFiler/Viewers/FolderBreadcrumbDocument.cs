namespace QuickFiler.Viewers
{
    /// <summary>Composes both compiled assets into the self-contained QFC document.</summary>
    internal static class FolderBreadcrumbDocument
    {
        internal const string ScriptMarker = "/*FOLDER_BREADCRUMB_SCRIPT*/";

        internal static string Html =>
            Properties.Resources.FolderBreadcrumb.Replace(
                ScriptMarker,
                Properties.Resources.FolderBreadcrumbScript
            );
    }
}
