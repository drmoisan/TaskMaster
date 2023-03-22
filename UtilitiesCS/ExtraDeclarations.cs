#pragma warning disable CS1591,CS1573,CS0465,CS0649,CS8019,CS1570,CS1584,CS1658,CS0436,CS8981
using global::System;
using global::System.Diagnostics;
using global::System.Diagnostics.CodeAnalysis;
using global::System.Runtime.CompilerServices;
using global::System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using winmdroot = global::Windows.Win32;
namespace Windows.Win32
{

    /// <content>
    /// Contains extern methods from "USER32.dll".
    /// </content>
    public static partial class PInvoke
    {
        [DllImport("USER32.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern winmdroot.Foundation.HWND FindWindowA(string lpClassName = null, string lpWindowName = null);

        [DllImport("USER32.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetWindowLongPtr(winmdroot.Foundation.HWND hWnd, WindowLongParam nIndex);

        public enum WindowLongParam
        {
            /// <summary>Sets a new address for the window procedure.</summary>
            /// <remarks>You cannot change this attribute if the window does not belong to the same process as the calling thread.</remarks>
            GWL_WNDPROC = -4,

            /// <summary>Sets a new application instance handle.</summary>
            GWLP_HINSTANCE = -6,

            GWLP_HWNDPARENT = -8,

            /// <summary>Sets a new identifier of the child window.</summary>
            /// <remarks>The window cannot be a top-level window.</remarks>
            GWL_ID = -12,

            /// <summary>Sets a new window style.</summary>
            GWL_STYLE = -16,

            /// <summary>Sets a new extended window style.</summary>
            /// <remarks>See <see cref="ExWindowStyles"/>.</remarks>
            GWL_EXSTYLE = -20,

            /// <summary>Sets the user data associated with the window.</summary>
            /// <remarks>This data is intended for use by the application that created the window. Its value is initially zero.</remarks>
            GWL_USERDATA = -21,

            /// <summary>Sets the return value of a message processed in the dialog box procedure.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_MSGRESULT = 0,

            /// <summary>Sets new extra information that is private to the application, such as handles or pointers.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_USER = 8,

            /// <summary>Sets the new address of the dialog box procedure.</summary>
            /// <remarks>Only applies to dialog boxes.</remarks>
            DWLP_DLGPROC = 4
        }
    }
}