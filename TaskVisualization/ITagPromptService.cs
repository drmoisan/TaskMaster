using System.Collections.Generic;
using Tags;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization
{
    /// <summary>
    /// Seam over the four assign-dialog prompts (People / Context / Project / Topic).
    /// It replaces the in-line <c>new TagViewer(); new TagController(...); ShowDialog()</c>
    /// blocks in the controller so unit tests never construct a live form or show a popup.
    /// The production adapter <see cref="TagPromptService"/> is the only place that
    /// constructs <c>Tags.TagViewer</c>/<c>Tags.TagController</c>.
    /// </summary>
    /// <remarks>
    /// Defined in <c>TaskVisualization</c> so sibling feature #298
    /// (EditFilterController / ManageFilters, which open the same class of Tags dialogs)
    /// can reuse this seam rather than duplicating it.
    /// </remarks>
    public interface ITagPromptService
    {
        /// <summary>
        /// Shows the tag-assignment dialog for the supplied request and returns the result.
        /// </summary>
        TagPromptResult Prompt(TagPromptRequest request);
    }

    /// <summary>
    /// Immutable request describing a single tag-assignment prompt. Mirrors the arguments
    /// the controller formerly passed inline to <c>TagController</c>.
    /// </summary>
    public sealed class TagPromptRequest
    {
        /// <summary>Creates a request.</summary>
        public TagPromptRequest(
            SortedDictionary<string, bool> options,
            IAutoAssign autoAssigner,
            IList<IPrefix> prefixes,
            IList<string> selections,
            string prefixKey,
            object objItemObject,
            string userEmailAddress,
            string caption
        )
        {
            Options = options;
            AutoAssigner = autoAssigner;
            Prefixes = prefixes;
            Selections = selections;
            PrefixKey = prefixKey;
            ObjItemObject = objItemObject;
            UserEmailAddress = userEmailAddress;
            Caption = caption;
        }

        /// <summary>Filtered category options presented in the dialog.</summary>
        public SortedDictionary<string, bool> Options { get; }

        /// <summary>Auto-assigner used by the dialog's auto-assign button.</summary>
        public IAutoAssign AutoAssigner { get; }

        /// <summary>Prefix definitions.</summary>
        public IList<IPrefix> Prefixes { get; }

        /// <summary>Initially-selected values.</summary>
        public IList<string> Selections { get; }

        /// <summary>Prefix key that identifies the field being assigned.</summary>
        public string PrefixKey { get; }

        /// <summary>The Outlook item object supplied to the dialog controller.</summary>
        public object ObjItemObject { get; }

        /// <summary>User email address (avoids self-tagging).</summary>
        public string UserEmailAddress { get; }

        /// <summary>
        /// Optional dialog caption. When null or empty, the dialog's default caption is used
        /// (only the Project assignment sets a custom caption in current behavior).
        /// </summary>
        public string Caption { get; }
    }

    /// <summary>
    /// Result of a tag-assignment prompt.
    /// </summary>
    public sealed class TagPromptResult
    {
        /// <summary>Creates a result.</summary>
        public TagPromptResult(bool cancelled, string selection)
        {
            Cancelled = cancelled;
            Selection = selection;
        }

        /// <summary>True when the user cancelled the dialog (exit type "Cancel").</summary>
        public bool Cancelled { get; }

        /// <summary>The selection as a comma-separated string (from SelectionAsString()).</summary>
        public string Selection { get; }
    }
}
