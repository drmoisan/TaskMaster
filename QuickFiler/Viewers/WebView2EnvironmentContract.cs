#nullable enable
using System;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Single owner of the WebView2 environment values shared by every WebView2 host in QuickFiler
    /// (#792). The breadcrumb host, the QFC item viewer and the EFC item viewer must all resolve
    /// the same user-data folder and the same additional browser arguments, because WebView2
    /// shares one browser process per user-data folder: a second environment created for the
    /// same folder with different options fails <c>CoreWebView2Environment.CreateAsync</c> with
    /// HRESULT 0x8007139F (ERROR_INVALID_STATE, "The group or resource is not in the correct
    /// state to perform the requested operation"). Reading every value from here keeps the
    /// creation sites from diverging.
    /// </summary>
    internal static class WebView2EnvironmentContract
    {
        /// <summary>
        /// The additional browser argument every host passes, so no viewer keeps browsing data.
        /// The trailing space is part of the shared value.
        /// </summary>
        internal const string AdditionalBrowserArguments = "--incognito ";

        /// <summary>
        /// Leaf folder name under LocalApplicationData that every host uses as its user-data
        /// folder.
        /// </summary>
        internal const string UserDataFolderName = "WindowsFormsWebView2";

        /// <summary>
        /// Resolves the shared user-data folder path. Pure: combines paths and creates nothing on
        /// disk.
        /// </summary>
        internal static string ResolveUserDataFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                UserDataFolderName
            );
        }

        /// <summary>
        /// Creates a fresh options instance carrying <see cref="AdditionalBrowserArguments"/>. A
        /// new instance is returned on every call because the SDK options type is mutable.
        /// </summary>
        internal static CoreWebView2EnvironmentOptions CreateOptions()
        {
            return new CoreWebView2EnvironmentOptions(AdditionalBrowserArguments);
        }
    }
}
