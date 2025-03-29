using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using SDILReader;


namespace UtilitiesCS
{
    public static class FolderConverter
    {
        private static char[] IllegalFolderCharacters { get => @"[\/:*?""<>|].".ToCharArray(); }

        private static bool IsLegalFolderName(string folderName)
        {
            if (folderName.IsNullOrEmpty()) { return false; }
            else { return !folderName.Any(c => IllegalFolderCharacters.Contains(c)); }
        }

        private static (bool legal, string revisedFolder) IsLegalFolderName(string folderName, bool askUser)
        {
            string revisedFolder = folderName;
            var legal = IsLegalFolderName(revisedFolder);
            if (!legal && askUser)
            {
                (legal, revisedFolder) = AskUserForAlternatives(revisedFolder);
            }
            return (legal, revisedFolder);
        }

        private static (bool legal, string revisedFolder) AskUserForAlternatives(string illegalFolderName)
        {
            var illegal = GetIllegalFolderChars(illegalFolderName).SentenceJoin();
            var dict = BuildAlternativesDictionary(illegalFolderName);
            var result = MyBox.ShowDialog($"Folder cannot contain characters {illegal}. How should we proceed?", "Folder Error", BoxIcon.Question, dict);
            if (result.IsNullOrEmpty()) { return (false, illegalFolderName); }
            else 
            {
                var (legal, revisedFolder) = IsLegalFolderName(result, true);
                if (legal) { return (true, revisedFolder); }
                else { return AskUserForAlternatives(revisedFolder); }
            }            
        }

        private static Dictionary<string, Func<Task<string>>> BuildAlternativesDictionary(string illegalFolderName)
        {
            var dict = new Dictionary<string, Func<Task<string>>>();
            dict.Add("Skip", async () => await Task.FromResult(""));            
            dict.Add("Replace with underscore", async () => await Task.Run(() => SanitizeFilename(illegalFolderName)));
            dict.Add("Remove illegal characters", async () => await Task.Run(() => illegalFolderName.Replace(illegalFolderName, "")));
            dict.Add("Enter new folder name", async () => await Task.Run(() => InputBox.ShowDialog("Enter new folder name", "Folder Error", SanitizeFilename(illegalFolderName))));
            return dict;
        }

        private static char[] GetIllegalFolderChars(string folderName)
        {
            return folderName.Where(c => IllegalFolderCharacters.Contains(c)).ToArray();
        }

        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            var regex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+");
            return regex.Replace(filename, "_");
        }

        public static string ToFsFolderpath(this string olBranchPath, string olAncestorPath, string fsAncestorEquivalent, bool ask = true) 
        {
            if (string.IsNullOrEmpty(olBranchPath)) 
                throw new ArgumentNullException(nameof(olBranchPath));
            if (string.IsNullOrEmpty(olAncestorPath)) 
                throw new ArgumentNullException(nameof(olAncestorPath));
            if (string.IsNullOrEmpty(fsAncestorEquivalent)) 
                throw new ArgumentNullException(nameof(fsAncestorEquivalent));

            var fsPath = olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent);

            var fsPathExDividers = fsPath.Substring(3).Replace($"{Path.DirectorySeparatorChar}", "");
                        
            if (!IsLegalFolderName(fsPathExDividers))
            {
                throw new ArgumentException(
                    $"{nameof(fsPathExDividers)} has a value of {fsPathExDividers} which contains illegal characters {GetIllegalFolderChars(fsPathExDividers).SentenceJoin()}", 
                    nameof(fsPath));
            }

            return fsPath;

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

            if (appGlobals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, folderRoot);
            }
            else { return null; }
        }

        public static string ToFsFolderpath(this MAPIFolder olFolderBranch, IApplicationGlobals appGlobals)
        {
            var olBranchPath = olFolderBranch.FolderPath;
            string olAncestor = ResolveOlRoot(olBranchPath, appGlobals);

            if (appGlobals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, folderRoot);
            }
            else { return null; }
            
        }

        public static string ResolveOlRoot(string olBranchPath, IApplicationGlobals appGlobals)
        {
            if (olBranchPath.Contains(appGlobals.Ol.ArchiveRootPath))
            {
                return appGlobals.Ol.ArchiveRootPath;
            }
            else if (olBranchPath.Contains(appGlobals.Ol.InboxPath))
            {
                return appGlobals.Ol.InboxPath;
            }
            else
            {
                throw new ArgumentException($"Folder {olBranchPath} is not a branch of any known root folder");
            }
        }
    }
}
