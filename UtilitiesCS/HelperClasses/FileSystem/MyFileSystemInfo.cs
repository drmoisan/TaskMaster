using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.HelperClasses.FileSystem;

namespace ObjectListViewDemo
{
    /// <summary>
    /// From ObjectListView Demo by BrightIdeaSoftware.com
    /// Standard .NET FileSystemInfos are always not equal to each other.
    /// When we try to refresh a directory, our controls can't match up new
    /// files with existing files. They are also sealed so we can't just subclass them.
    /// This class is a wrapper around a FileSystemInfo that simply provides
    /// equality.
    /// </summary>
    public class MyFileSystemInfo : IEquatable<MyFileSystemInfo>
    {
        public MyFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null) throw new ArgumentNullException("fileSystemInfo");
            this.info = new FileSystemInfoWrapper(fileSystemInfo);
        }

        public MyFileSystemInfo(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            this.info = new FileInfoWrapper(fileInfo);
        }

        public MyFileSystemInfo(DirectoryInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            this.info = new DirectoryInfoWrapper(fileInfo);
        }

        public MyFileSystemInfo(IFileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null) throw new ArgumentNullException(nameof(fileSystemInfo));
            this.info = fileSystemInfo;
        }

        public bool IsDirectory { get { return this.AsDirectory != null; } }

        public IDirectoryInfo AsDirectory { get { return this.info as IDirectoryInfo; } }
        //public IDirectoryInfo AsDirectory 
        //{ 
        //    get 
        //    {
        //        var di = this.info as IDirectoryInfo;
        //        if (di is null)
        //        {
        //            var fi = this.info as IFileInfo;
        //            if (fi is not null) { di = fi.Directory; }
        //        }
        //        return di; 
        //    } 
        //}
        public IFileInfo AsFile { get { return this.info as IFileInfo; } }

        public IFileSystemInfo Info
        {
            get { return this.info; }
        }
        private readonly IFileSystemInfo info;

        public string Name
        {
            get { return this.info.Name; }
        }

        public string Extension
        {
            get { return this.info.Extension; }
        }

        public DateTime CreationTime
        {
            get { return this.info.CreationTime; }
        }

        public DateTime LastWriteTime
        {
            get { return this.info.LastWriteTime; }
        }

        public string FullName
        {
            get { return this.info.FullName; }
        }

        public FileAttributes Attributes
        {
            get { return this.info.Attributes; }
        }

        public long Length
        {
            get { return this.AsFile.Length; }
        }

        public IEnumerable GetFileSystemInfos()
        {
            ArrayList children = new ArrayList();
            if (this.IsDirectory)
            {
                foreach (IFileSystemInfo x in this.AsDirectory.GetFileSystemInfos())
                    children.Add(new MyFileSystemInfo(x));
            }
            return children;
        }

        // Two file system objects are equal if they point to the same file system path

        public bool Equals(MyFileSystemInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.info.FullName, this.info.FullName);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MyFileSystemInfo)) return false;
            return Equals((MyFileSystemInfo)obj);
        }
        public override int GetHashCode()
        {
            return (this.info != null ? this.info.FullName.GetHashCode() : 0);
        }
        public static bool operator ==(MyFileSystemInfo left, MyFileSystemInfo right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(MyFileSystemInfo left, MyFileSystemInfo right)
        {
            return !Equals(left, right);
        }
    }
}
