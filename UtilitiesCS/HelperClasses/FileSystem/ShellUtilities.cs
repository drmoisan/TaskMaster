using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObjectListViewDemo
{
    /// <summary>
    /// ShellUtilities contains routines to interact with the Windows Shell.
    /// </summary>
    public class ShellUtilities()
    {
        /// <summary>
        /// Execute the default verb on the file or directory identified by the given path.
        /// For documents, this will open them with their normal application. For executables,
        /// this will cause them to run.
        /// </summary>
        /// <param name="path">The file or directory to be executed</param>
        /// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
        /// <remarks>The same effect can be achieved by <code>System.Diagnostics.Process.Start(path)</code>.</remarks>
        public int Execute(string path)
        {
            return Execute(path, "");
        }

        /// <summary>
        /// Execute the given operation on the file or directory identified by the given path.
        /// Example operations are "edit", "print", "explore".
        /// </summary>
        /// <param name="path">The file or directory to be operated on</param>
        /// <param name="operation">What operation should be performed</param>
        /// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
        public int Execute(string path, string operation)
        {
            IntPtr result = ShellExecute(0, operation, path, "", "", SW_SHOWNORMAL);
            return result.ToInt32();
        }

        /// <summary>
        /// Get the string that describes the file's type.
        /// </summary>
        /// <param name="path">The file or directory whose type is to be fetched</param>
        /// <returns>A string describing the type of the file, or an empty string if something goes wrong.</returns>
        public string GetFileType(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            int flags = SHGFI_TYPENAME;
            IntPtr result = SHGetFileInfo(path, 0, out shfi, Marshal.SizeOf(shfi), flags);
            if (result == IntPtr.Zero)
                return string.Empty;
            else
                return shfi.szTypeName;
        }

        /// <summary>
        /// Return the icon for the given file/directory.
        /// </summary>
        /// <param name="path">The full path to the file whose icon is to be returned</param>
        /// <param name="isSmallImage">True if the small (16x16) icon is required, otherwise the 32x32 icon will be returned</param>
        /// <param name="useFileType">If this is true, only the file extension will be considered</param>
        /// <returns>The icon of the given file, or null if something goes wrong</returns>
        public Icon GetFileIcon(string path, bool isSmallImage, bool useFileType)
        {
            int flags = SHGFI_ICON;
            if (isSmallImage)
                flags |= SHGFI_SMALLICON;

            int fileAttributes = 0;
            if (useFileType)
            {
                flags |= SHGFI_USEFILEATTRIBUTES;
                if (System.IO.Directory.Exists(path))
                    fileAttributes = FILE_ATTRIBUTE_DIRECTORY;
                else
                    fileAttributes = FILE_ATTRIBUTE_NORMAL;
            }

            SHFILEINFO shfi = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(path, fileAttributes, out shfi, Marshal.SizeOf(shfi), flags);
            if (result == IntPtr.Zero)
                return null;
            else
                return Icon.FromHandle(shfi.hIcon);
        }

        /// <summary>
        /// Return the index into the system image list of the image that represents the given file.
        /// </summary>
        /// <param name="path">The full path to the file or directory whose icon is required</param>
        /// <returns>The index of the icon, or -1 if something goes wrong</returns>
        /// <remarks>This is only useful if you are using the system image lists directly. Since there is
        /// no way to do that in .NET, it isn't a very useful.</remarks>
        public int GetSysImageIndex(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            int flags = SHGFI_ICON | SHGFI_SYSICONINDEX;
            IntPtr result = SHGetFileInfo(path, 0, out shfi, Marshal.SizeOf(shfi), flags);
            //if (result.ToInt32() == 0)
            if (result == IntPtr.Zero)
                return -1;
            else
                return shfi.iIcon;
        }

        #region Native methods

        private const int SHGFI_ICON = 0x00100;     // get icon
        private const int SHGFI_DISPLAYNAME = 0x00200;     // get display name
        //private const int SHGFI_TYPENAME = 0x00400;     // get type name
        private const int SHGFI_TYPENAME = 0x000000400;     // get type name
        private const int SHGFI_ATTRIBUTES = 0x00800;     // get attributes
        private const int SHGFI_ICONLOCATION = 0x01000;     // get icon location
        private const int SHGFI_EXETYPE = 0x02000;     // return exe type
        private const int SHGFI_SYSICONINDEX = 0x04000;     // get system icon index
        private const int SHGFI_LINKOVERLAY = 0x08000;     // put a link overlay on icon
        private const int SHGFI_SELECTED = 0x10000;     // show icon in selected state
        private const int SHGFI_ATTR_SPECIFIED = 0x20000;     // get only specified attributes
        private const int SHGFI_LARGEICON = 0x00000;     // get large icon
        private const int SHGFI_SMALLICON = 0x00001;     // get small icon
        private const int SHGFI_OPENICON = 0x00002;     // get open icon
        private const int SHGFI_SHELLICONSIZE = 0x00004;     // get shell size icon
        private const int SHGFI_PIDL = 0x00008;     // pszPath is a pidl
        private const int SHGFI_USEFILEATTRIBUTES = 0x00010;     // use passed dwFileAttribute
        //if (_WIN32_IE >= 0x0500)
        private const int SHGFI_ADDOVERLAYS = 0x00020;     // apply the appropriate overlays
        private const int SHGFI_OVERLAYINDEX = 0x00040;     // Get the index of the overlay

        private const int FILE_ATTRIBUTE_NORMAL = 0x00080;     // Normal file
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x00010;     // Directory

        private const int MAX_PATH = 260;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const int SW_SHOWNORMAL = 1;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ShellExecute(int hwnd, string lpOperation, string lpFile,
            string lpParameters, string lpDirectory, int nShowCmd);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes,
            out SHFILEINFO psfi, int cbFileInfo, int uFlags);

        #endregion
    }

}
