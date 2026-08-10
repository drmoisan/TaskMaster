#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>Why an open selector is being closed.</summary>
    public enum BreadcrumbDropDownCloseReason
    {
        /// <summary>Enter or row activation explicitly committed the pending identity.</summary>
        ExplicitCommit,

        /// <summary>Escape, outside click, lost activation, or another automatic close cancels.</summary>
        Uncommitted,
    }

    /// <summary>Owned native popup lifecycle used by the ItemViewer integration.</summary>
    public interface IBreadcrumbDropDownHost : IDisposable
    {
        /// <summary>True while the native popup is open.</summary>
        bool IsOpen { get; }

        /// <summary>The popup messenger after successful lazy initialization.</summary>
        IWebViewMessenger? PopupMessenger { get; }

        /// <summary>Raised once when a new popup messenger is ready for hub attachment.</summary>
        event EventHandler? PopupMessengerReady;

        /// <summary>Creates as needed, places, and opens the owned popup.</summary>
        /// <remarks>Delegates with <c>takeFocus: true</c>, so every existing caller is unchanged.</remarks>
        Task<bool> OpenAsync(Rectangle anchorScreenBounds, Rectangle workingArea, Size desiredSize);

        /// <summary>
        /// Creates as needed, places, and opens the owned popup, carrying an explicit focus intent.
        /// </summary>
        /// <param name="anchorScreenBounds">The collapsed anchor's screen rectangle.</param>
        /// <param name="workingArea">The working area the popup must be placed within.</param>
        /// <param name="desiredSize">The requested popup size.</param>
        /// <param name="takeFocus">
        /// <see langword="true"/> to move focus onto the popup surface, matching the 3-parameter
        /// overload. <see langword="false"/> to open without taking focus: the fresh-open
        /// focus-pending step and the already-open re-focus step are both suppressed.
        /// </param>
        /// <returns>The same open-result contract as the 3-parameter overload.</returns>
        /// <remarks>
        /// Additive for issue #438. A search-driven open must not move the caret away from the
        /// textbox the user is typing in, whereas explicit gestures (mouse toggle, Down arrow,
        /// <c>JumpToFolderDropDown</c>) keep focus-on-open through the defaulting 3-parameter
        /// overload. This is the sanctioned, gesture-scoped qualification of issue #400 AC-13.
        /// </remarks>
        Task<bool> OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize,
            bool takeFocus
        );

        /// <summary>Closes with explicit-commit or rollback semantics.</summary>
        bool Close(BreadcrumbDropDownCloseReason reason);

        /// <summary>Retains the latest theme for the popup surface.</summary>
        void SetTheme(string theme);

        /// <summary>Closes and releases the current lazy surface while keeping the host reusable.</summary>
        void Reset();
    }
}
