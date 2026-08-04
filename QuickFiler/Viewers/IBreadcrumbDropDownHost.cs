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
        Task<bool> OpenAsync(Rectangle anchorScreenBounds, Rectangle workingArea, Size desiredSize);

        /// <summary>Closes with explicit-commit or rollback semantics.</summary>
        bool Close(BreadcrumbDropDownCloseReason reason);

        /// <summary>Retains the latest theme for the popup surface.</summary>
        void SetTheme(string theme);

        /// <summary>Closes and releases the current lazy surface while keeping the host reusable.</summary>
        void Reset();
    }
}
