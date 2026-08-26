#nullable enable
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
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS
{
    public static class FolderConverter
    {
        internal static Func<
            string,
            (bool legal, string revisedFolder)
        > AlternativeFolderPrompt { get; set; } = AskUserForAlternatives;

        internal static Func<
            string,
            string,
            BoxIcon,
            Dictionary<string, Func<Task<string>>>,
            string
        > AlternativeFolderSelectionDialog { get; set; } =
            (message, title, icon, options) => MyBox.ShowDialog(message, title, icon, options)!;

        internal static Func<
            string,
            string,
            string,
            string
        > AlternativeFolderInputDialog { get; set; } =
            (prompt, title, defaultValue) => InputBox.ShowDialog(prompt, title, defaultValue)!;

        private static readonly char[] IllegalFolderCharacters = Path.GetInvalidFileNameChars();

        private static readonly char[] SegmentSeparators = { '\\', '/' };

        private static readonly string[] ReservedDeviceNames =
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9",
        };

        /// <summary>
        /// Validates each DERIVED path segment as a Windows folder name and returns the violated
        /// rule, or null when every segment is valid (#614 D5a/D5b). Only segments this converter
        /// derives from the Outlook branch are passed here. The caller-supplied filesystem
        /// ancestor is never validated, because a legitimate root of the OneDrive-for-business
        /// shape contains a dot, a space, and a hyphen and is not this converter's to reject.
        /// </summary>
        private static string? FindInvalidSegmentRule(string derivedRelativePath)
        {
            foreach (
                string segment in derivedRelativePath.Split(
                    SegmentSeparators,
                    StringSplitOptions.RemoveEmptyEntries
                )
            )
            {
                if (segment.IndexOfAny(IllegalFolderCharacters) >= 0)
                {
                    return "a folder name contains a character Windows forbids";
                }

                if (segment.EndsWith(".", StringComparison.Ordinal))
                {
                    return "a folder name ends with a dot";
                }

                if (segment.EndsWith(" ", StringComparison.Ordinal))
                {
                    return "a folder name ends with a space";
                }

                if (IsReservedDeviceName(segment))
                {
                    return "a folder name is a reserved Windows device name";
                }
            }

            return null;
        }

        /// <summary>Reports whether a single segment is a reserved Windows device name.</summary>
        private static bool IsReservedDeviceName(string segment)
        {
            int dot = segment.IndexOf('.');
            string stem = dot < 0 ? segment : segment.Substring(0, dot);
            return Array.IndexOf(ReservedDeviceNames, stem.ToUpperInvariant()) >= 0;
        }

        private static bool IsLegalFolderName(string folderName)
        {
            if (folderName.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                return !folderName.Any(c => IllegalFolderCharacters.Contains(c));
            }
        }

        private static (bool legal, string revisedFolder) IsLegalFolderName(
            string folderName,
            bool askUser
        )
        {
            string revisedFolder = folderName;
            var legal = IsLegalFolderName(revisedFolder);
            if (!legal && askUser)
            {
                (legal, revisedFolder) = AlternativeFolderPrompt(revisedFolder);
            }
            return (legal, revisedFolder);
        }

        private static (bool legal, string revisedFolder) AskUserForAlternatives(
            string illegalFolderName
        )
        {
            var illegal = GetIllegalFolderChars(illegalFolderName).SentenceJoin();
            var dict = BuildAlternativesDictionary(illegalFolderName);
            var result = AlternativeFolderSelectionDialog(
                $"Folder cannot contain characters {illegal}. How should we proceed?",
                "Folder Error",
                BoxIcon.Question,
                dict
            );
            if (result.IsNullOrEmpty())
            {
                return (false, illegalFolderName);
            }
            else
            {
                var (legal, revisedFolder) = IsLegalFolderName(result, true);
                if (legal)
                {
                    return (true, revisedFolder);
                }
                else
                {
                    return AskUserForAlternatives(revisedFolder);
                }
            }
        }

        private static Dictionary<string, Func<Task<string>>> BuildAlternativesDictionary(
            string illegalFolderName
        )
        {
            var dict = new Dictionary<string, Func<Task<string>>>();
            dict.Add("Skip", async () => await Task.FromResult(""));
            dict.Add(
                "Replace with underscore",
                async () => await Task.Run(() => SanitizeFilename(illegalFolderName))
            );
            dict.Add(
                "Remove illegal characters",
                async () => await Task.Run(() => RemoveIllegalCharacters(illegalFolderName))
            );
            dict.Add(
                "Enter new folder name",
                async () =>
                    await Task.Run(() =>
                        AlternativeFolderInputDialog(
                            "Enter new folder name",
                            "Folder Error",
                            SanitizeFilename(illegalFolderName)
                        )
                    )
            );
            return dict;
        }

        /// <summary>
        /// Removes only the characters Windows forbids in a folder name (#614 D5f). The previous
        /// implementation replaced the whole name with the empty string, so the "Remove illegal
        /// characters" option silently produced an empty folder name.
        /// </summary>
        private static string RemoveIllegalCharacters(string folderName)
        {
            return new string(
                folderName.Where(c => !IllegalFolderCharacters.Contains(c)).ToArray()
            );
        }

        private static char[] GetIllegalFolderChars(string folderName)
        {
            return folderName.Where(c => IllegalFolderCharacters.Contains(c)).ToArray();
        }

        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));
            var regex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+");
            return regex.Replace(filename, "_");
        }

        public static string ToFsFolderpath(
            this string olBranchPath,
            string olAncestorPath,
            string fsAncestorEquivalent
        )
        {
            if (string.IsNullOrEmpty(olBranchPath))
                throw new ArgumentNullException(nameof(olBranchPath));
            if (string.IsNullOrEmpty(olAncestorPath))
                throw new ArgumentNullException(nameof(olAncestorPath));
            if (string.IsNullOrEmpty(fsAncestorEquivalent))
                throw new ArgumentNullException(nameof(fsAncestorEquivalent));

            if (
                !ArchiveStemContract.TryMakeArchiveRelative(
                    olBranchPath,
                    olAncestorPath,
                    out string fsPathExDividers
                )
            )
            {
                throw new ArgumentException(
                    $"{nameof(olBranchPath)} is not a branch of {nameof(olAncestorPath)}. The values are withheld from this message because they can contain a mailbox address or user-profile path.",
                    nameof(olBranchPath)
                );
            }

            var fsPath =
                fsPathExDividers.Length == 0
                    ? fsAncestorEquivalent
                    : fsAncestorEquivalent.TrimEnd(SegmentSeparators)
                        + Path.DirectorySeparatorChar
                        + fsPathExDividers;

            string? invalidSegmentRule = FindInvalidSegmentRule(fsPathExDividers);
            if (invalidSegmentRule != null)
            {
                throw new ArgumentException(
                    "The Outlook branch maps to an invalid Windows folder path because "
                        + invalidSegmentRule
                        + ". The value is withheld from this message because it can contain a mailbox address or user-profile path.",
                    nameof(fsPath)
                );
            }

            return fsPath;
        }

        public static string ToFsFolderpath(
            this Folder olFolderBranch,
            string olAncestor,
            string fsAncestorEquivalent
        )
        {
            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
        }

        public static string ToFsFolderpath(
            this MAPIFolder olFolderBranch,
            string olAncestor,
            string fsAncestorEquivalent
        )
        {
            return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, fsAncestorEquivalent);
        }

        public static string? ToFsFolderpath(
            this Folder olFolderBranch,
            IApplicationGlobals appGlobals
        )
        {
            var olBranchPath = olFolderBranch.FolderPath;
            string olAncestor = ResolveOlRoot(olBranchPath, appGlobals);

            if (appGlobals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, folderRoot);
            }
            else
            {
                return null;
            }
        }

        public static string? ToFsFolderpath(
            this MAPIFolder olFolderBranch,
            IApplicationGlobals appGlobals
        )
        {
            var olBranchPath = olFolderBranch.FolderPath;
            string olAncestor = ResolveOlRoot(olBranchPath, appGlobals);

            if (appGlobals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
            {
                return olFolderBranch.FolderPath.ToFsFolderpath(olAncestor, folderRoot);
            }
            else
            {
                return null;
            }
        }

        public static string ResolveOlRoot(string olBranchPath, IApplicationGlobals appGlobals)
        {
            if (
                ArchiveStemContract.TryMakeArchiveRelative(
                    olBranchPath,
                    appGlobals.Ol.ArchiveRootPath,
                    out _
                )
            )
            {
                return appGlobals.Ol.ArchiveRootPath;
            }

            if (
                ArchiveStemContract.TryMakeArchiveRelative(
                    olBranchPath,
                    appGlobals.Ol.InboxPath,
                    out _
                )
            )
            {
                return appGlobals.Ol.InboxPath;
            }

            throw new ArgumentException(
                "The folder is not a branch of any known root folder. The path is withheld from this message because it can contain a mailbox address."
            );
        }
    }
}
