using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS
{
    public static class FolderConverter
    {
        public static string ToFsFolder(this string OlFolderBranch, string OlFolderRoot, string FsFolderRoot) 
        {
            if (String.IsNullOrEmpty(OlFolderBranch)) throw new ArgumentNullException(nameof(OlFolderBranch));
            if (String.IsNullOrEmpty(OlFolderRoot)) throw new ArgumentNullException(nameof(OlFolderRoot));
            if (String.IsNullOrEmpty(FsFolderRoot)) throw new ArgumentNullException(nameof(FsFolderRoot));

            Uri olBranchURI = new Uri(OlFolderBranch);
            Uri olRootURI = new Uri(OlFolderRoot);

            if (olBranchURI.Scheme != olBranchURI.Scheme) 
            {
                throw new ArgumentException(
                "OlFolderBranch and OlFolderRoot are not the same type of folderpath"); 
            } 

            Uri relativeUri = olBranchURI.MakeRelativeUri(olRootURI);
            
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (relativePath[0].Equals("."))
                throw new ArgumentOutOfRangeException(nameof(relativeUri), 
                    $"{OlFolderBranch} is not a branch of {OlFolderRoot}");

            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return Path.GetFullPath(FsFolderRoot + relativePath);
        }

        public static string ToFsFolder(this Folder OlFolderBranch, string OlFolderRoot, string FsFolderRoot)
        {
            return OlFolderBranch.FolderPath.ToFsFolder(OlFolderRoot, FsFolderRoot);
        }
    }
}
