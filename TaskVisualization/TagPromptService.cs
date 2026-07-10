using System.Diagnostics.CodeAnalysis;
using Tags;

namespace TaskVisualization
{
    /// <summary>
    /// Production adapter for <see cref="ITagPromptService"/>. This is the only place that
    /// constructs a live <see cref="TagViewer"/> + <see cref="TagController"/> and calls
    /// <c>ShowDialog()</c>, so the controller logic that consumes it stays testable behind
    /// the seam.
    /// </summary>
    /// <remarks>
    /// File-level <see cref="ExcludeFromCodeCoverageAttribute"/>: this class is a WinForms
    /// dialog host (constructs a live form and shows a modal dialog); it has no logic-isolating
    /// seam of its own. The testable seam is the <see cref="ITagPromptService"/> interface,
    /// against which the controller assign paths are covered with a mock.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public sealed class TagPromptService : ITagPromptService
    {
        /// <inheritdoc />
        public TagPromptResult Prompt(TagPromptRequest request)
        {
            using (var viewer = new TagViewer())
            {
                var controller = new TagController(
                    viewerInstance: viewer,
                    dictOptions: request.Options,
                    autoAssigner: request.AutoAssigner,
                    prefixes: request.Prefixes,
                    userEmailAddress: request.UserEmailAddress,
                    selections: request.Selections,
                    prefixKey: request.PrefixKey,
                    objItemObject: request.ObjItemObject
                );

                if (!string.IsNullOrEmpty(request.Caption))
                {
                    controller.SetCaption(request.Caption);
                }

                viewer.ShowDialog();

                bool cancelled = controller.ExitType == "Cancel";
                return new TagPromptResult(cancelled, controller.SelectionAsString());
            }
        }
    }
}
