using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS
{
    public static class FolderConverter
    {
        private static char[] IllegalFolderCharacters { get => @"[\/:*?""<>|].".ToCharArray(); }

        private static bool IsLegalFolderName(string folderName)
        {
            return !folderName.Any(c => IllegalFolderCharacters.Contains(c));
        }

        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            var regex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]*");
            return regex.Replace(filename, "_");
        }

        public static string ToFsFolderpath(this string olBranchPath, string olAncestorPath, string fsAncestorEquivalent) 
        {
            if (string.IsNullOrEmpty(olBranchPath)) throw new ArgumentNullException(nameof(olBranchPath));
            if (string.IsNullOrEmpty(olAncestorPath)) throw new ArgumentNullException(nameof(olAncestorPath));
            if (string.IsNullOrEmpty(fsAncestorEquivalent)) throw new ArgumentNullException(nameof(fsAncestorEquivalent));
            if (!IsLegalFolderName(olBranchPath)) throw new ArgumentException($"{nameof(olBranchPath)} has a value of {olBranchPath} which contains illegal characters", nameof(olBranchPath));
            if (!IsLegalFolderName(olAncestorPath)) throw new ArgumentException($"{nameof(olAncestorPath)} has a value of {olAncestorPath} which contains illegal characters", nameof(olBranchPath));

            Uri olBranchURI = new Uri(olBranchPath);
            Uri olRootURI = new Uri(olAncestorPath);

            if (olRootURI.Scheme != olBranchURI.Scheme) 
            {
                throw new ArgumentException("OlFolderBranch and OlFolderRoot are not the same type of folderpath"); 
            } 

            Uri relativeUri = olBranchURI.MakeRelativeUri(olRootURI);
            
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (relativePath[0].Equals("."))
                throw new ArgumentOutOfRangeException(nameof(relativeUri), 
                    $"{olBranchPath} is not a branch of {olAncestorPath}");

            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return Path.GetFullPath(fsAncestorEquivalent + relativePath);
        }

        public static string ToFsFolderpath(this Folder olFolderBranch, string olAncestor, string fsAncestorEquivalent)
        {
            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
        }

        public static string ToFsFolderpath(this MAPIFolder olFolderBranch, string olAncestor, string fsAncestorEquivalent)
        {
            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
        }

        public static string ToFsFolderpath(this Folder olFolderBranch, IApplicationGlobals appGlobals)
        {
            var olBranchPath = olFolderBranch.FolderPath;
            string olAncestor = ResolveOlRoot(olBranchPath, appGlobals);

            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, appGlobals.FS.FldrRoot);
        }

        public static string ToFsFolderpath(this MAPIFolder olFolderBranch, IApplicationGlobals appGlobals)
        {
            var olBranchPath = olFolderBranch.FolderPath;
            string olAncestor = ResolveOlRoot(olBranchPath, appGlobals);

            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, appGlobals.FS.FldrRoot);
        }

        public static string ResolveOlRoot(string olBranchPath, IApplicationGlobals appGlobals)
        {
            if (olBranchPath.Contains(appGlobals.Ol.ArchiveRootPath))
            {
                return appGlobals.Ol.ArchiveRootPath;
            }
            else if (olBranchPath.Contains(appGlobals.Ol.EmailRootPath))
            {
                return appGlobals.Ol.EmailRootPath;
            }
            else
            {
                throw new ArgumentException($"Folder {olBranchPath} is not a branch of any known root folder");
            }
        }
    }
}
