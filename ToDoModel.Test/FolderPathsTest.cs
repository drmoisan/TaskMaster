using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using System.IO;

namespace ToDoModel.Test
{
    internal class FolderPathsTest : IFileSystemFolderPaths
    {
        public IAppStagingFilenames Filenames => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrAppData => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrFlow => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrMyD => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrPreReads => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrRoot => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrStaging => throw new NotImplementedException();

        string IFileSystemFolderPaths.FldrPythonStaging
        {
            get 
            {
                return Path.Combine(
                    Environment.GetEnvironmentVariable("OneDriveCommercial"),
                    "Email attachments from Flow", 
                    "Combined", 
                    "data");
            }
        }

        void IFileSystemFolderPaths.Reload()
        {
            throw new NotImplementedException();
        }
    }
}
