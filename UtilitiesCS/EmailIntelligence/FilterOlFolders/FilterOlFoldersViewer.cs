using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ObjectListViewDemo;

namespace UtilitiesCS
{
    public partial class FilterOlFoldersViewer : Form
    {
        public FilterOlFoldersViewer()
        {
            InitializeComponent();

            SetupColumns();
            SetupDragAndDrop();
            SetupTree();

            // You can change the way the connection lines are drawn by changing the pen
            TreeListView.TreeRenderer renderer = this.treeListView.TreeColumnRenderer;
            renderer.LinePen = new Pen(Color.Firebrick, 0.5f);
            renderer.LinePen.DashStyle = DashStyle.Dot;
        }

        private void SetupDragAndDrop()
        {

            // Setup the tree so that it can drop and drop.

            // Dropping doesn't do anything, but it does show how it works

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

            treeListView.ModelCanDrop += delegate (object sender, ModelDropEventArgs e) {
                e.Effect = DragDropEffects.None;
                if (e.TargetModel == null)
                    return;

                if (e.TargetModel is DirectoryInfo)
                    e.Effect = e.StandardDropActionFromKeys;
                else
                    e.InfoMessage = "Can only drop on directories";
            };

            treeListView.ModelDropped += delegate (object sender, ModelDropEventArgs e) {
                String msg = String.Format("{2} items were dropped on '{1}' as a {0} operation.",
                    e.Effect, ((DirectoryInfo)e.TargetModel).Name, e.SourceModels.Count);
                MessageBox.Show(msg, "Object List View Demo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void SetupTree()
        {

            // TreeListView require two delegates:
            // 1. CanExpandGetter - Can a particular model be expanded?
            // 2. ChildrenGetter - Once the CanExpandGetter returns true, ChildrenGetter should return the list of children

            // CanExpandGetter is called very often! It must be very fast.

            this.treeListView.CanExpandGetter = delegate (object x) {
                return ((MyFileSystemInfo)x).IsDirectory;
            };

            // We just want to get the children of the given directory.
            // This becomes a little complicated when we can't (for whatever reason). We need to report the error 
            // to the user, but we can't just call MessageBox.Show() directly, since that would stall the UI thread
            // leaving the tree in a potentially undefined state (not good). We also don't want to keep trying to
            // get the contents of the given directory if the tree is refreshed. To get around the first problem,
            // we immediately return an empty list of children and use BeginInvoke to show the MessageBox at the 
            // earliest opportunity. We get around the second problem by collapsing the branch again, so it's children
            // will not be fetched when the tree is refreshed. The user could still explicitly unroll it again --
            // that's their problem :)
            this.treeListView.ChildrenGetter = delegate (object x) {
                try
                {
                    return ((MyFileSystemInfo)x).GetFileSystemInfos();
                }
                catch (UnauthorizedAccessException ex)
                {
                    this.BeginInvoke((MethodInvoker)delegate () {
                        this.treeListView.Collapse(x);
                        MessageBox.Show(this, ex.Message, "ObjectListViewDemo", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    });
                    return new ArrayList();
                }
            };

            // Once those two delegates are in place, the TreeListView starts working
            // after setting the Roots property.

            // List all drives as the roots of the tree
            ArrayList roots = new ArrayList();
            foreach (DriveInfo di in DriveInfo.GetDrives())
            {
                if (di.IsReady)
                    roots.Add(new MyFileSystemInfo(new DirectoryInfo(di.Name)));
            }
            this.treeListView.Roots = roots;
        }

        private void SetupColumns()
        {
            // The column setup here is identical to the File Explorer example tab --
            // nothing specific to the TreeListView. 

            // The only difference is that we don't setup anything to do with grouping,
            // since TreeListViews can't show groups.

            SysImageListHelper helper = new SysImageListHelper(this.treeListView);
            this.olvColumnName.ImageGetter = delegate (object x) {
                return helper.GetImageIndex(((MyFileSystemInfo)x).FullName);
            };

            // Get the size of the file system entity. 
            // Folders and errors are represented as negative numbers
            this.olvColumnSize.AspectGetter = delegate (object x) {
                MyFileSystemInfo myFileSystemInfo = (MyFileSystemInfo)x;

                if (myFileSystemInfo.IsDirectory)
                    return (long)-1;

                try
                {
                    return myFileSystemInfo.Length;
                }
                catch (System.IO.FileNotFoundException)
                {
                    // Mono 1.2.6 throws this for hidden files
                    return (long)-2;
                }
            };

            // Show the size of files as GB, MB and KBs. By returning the actual
            // size in the AspectGetter, and doing the conversion in the 
            // AspectToStringConverter, sorting on this column will work off the
            // actual sizes, rather than the formatted string.
            this.olvColumnSize.AspectToStringConverter = delegate (object x) {
                long sizeInBytes = (long)x;
                if (sizeInBytes < 0) // folder or error
                    return "";
                return FormatFileSize(sizeInBytes);
            };

            // Show the system description for this object
            this.olvColumnFileType.AspectGetter = delegate (object x) {
                return ShellUtilities.GetFileType(((MyFileSystemInfo)x).FullName);
            };

            // Show the file attributes for this object
            // A FlagRenderer masks off various values and draws zero or images based 
            // on the presence of individual bits.
            this.olvColumnAttributes.AspectGetter = delegate (object x) {
                return ((MyFileSystemInfo)x).Attributes;
            };
            FlagRenderer attributesRenderer = new FlagRenderer();
            attributesRenderer.ImageList = imageListSmall;
            attributesRenderer.Add(FileAttributes.Archive, "archive");
            attributesRenderer.Add(FileAttributes.ReadOnly, "readonly");
            attributesRenderer.Add(FileAttributes.System, "system");
            attributesRenderer.Add(FileAttributes.Hidden, "hidden");
            attributesRenderer.Add(FileAttributes.Temporary, "temporary");
            this.olvColumnAttributes.Renderer = attributesRenderer;

            // Tell the filtering subsystem that the attributes column is a collection of flags
            this.olvColumnAttributes.ClusteringStrategy = new FlagClusteringStrategy(typeof(FileAttributes));
        }

        /// <summary>
        /// Format a file size into a more intelligible value
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string FormatFileSize(long size)
        {
            int[] limits = new int[] { 1024 * 1024 * 1024, 1024 * 1024, 1024 };
            string[] units = new string[] { "GB", "MB", "KB" };

            for (int i = 0; i < limits.Length; i++)
            {
                if (size >= limits[i])
                    return String.Format("{0:#,##0.##} " + units[i], ((double)size / limits[i]));
            }

            return String.Format("{0} bytes", size);
        }
    }
}
