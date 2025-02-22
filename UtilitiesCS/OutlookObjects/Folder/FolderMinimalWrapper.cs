using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UtilitiesCS.Extensions.Lazy;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public class FolderMinimalWrapper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal FolderMinimalWrapper() { }

        public FolderMinimalWrapper(Outlook.MAPIFolder olFolder, Outlook.MAPIFolder olRoot)
        {
            OlFolder = (Outlook.Folder)olFolder;
            OlRoot = (Outlook.Folder)olRoot;
            ResetLazy();
        }
        
        public FolderMinimalWrapper(string name, string relativePath)
        {
            Name = name; 
            RelativePath = relativePath;
        }

        [JsonIgnore]
        internal Outlook.Folder OlRoot { get; set; }

        [JsonIgnore]
        public Outlook.Folder OlFolder { get; set; }

        [JsonProperty]
        public string Name { get => _lazyName?.Value; private set => _lazyName = value?.ToLazy(); }
        private Lazy<string> _lazyName;
        
        [JsonProperty]
        public string RelativePath { get => _lazyRelativePath?.Value; set => _lazyRelativePath = value?.ToLazy(); }
        private Lazy<string> _lazyRelativePath;
        internal virtual string ToRelativePath()
        {
            if (OlRoot is null || OlFolder is null)
            {
                logger.Warn($"{nameof(OlRoot)} or {nameof(OlFolder)} is null. Unable to load {nameof(RelativePath)}.\n" +
                    $"Call hierarchy {new StackTrace().GetMyTraceString()}");
                return null;
            }
            else if (OlFolder.FolderPath == OlRoot.FolderPath)
            {
                logger.Warn($"{nameof(OlFolder.FolderPath)} is the same as {nameof(OlRoot.FolderPath)}. " +
                    $"Returning full path.\nCall hierarchy {new StackTrace().GetMyTraceString()}");
                return OlFolder.FolderPath;
            }
            else if (!OlFolder.FolderPath.Contains(OlRoot.FolderPath))
            {
                logger.Warn($"{nameof(OlFolder.FolderPath)} does not contain {nameof(OlRoot.FolderPath)}. " +
                    $"Returning full path.\nCall hierarchy {new StackTrace().GetMyTraceString()}");
                return OlFolder.FolderPath;
            }
            else
            {
                return OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "");
            }
        }

        internal virtual void RestoreFromRelativePath(Outlook.Folder olRoot)
        {
            if (olRoot is null || string.IsNullOrEmpty(RelativePath))
            {
                logger.Warn($"{nameof(olRoot)} is null or {nameof(RelativePath)} is empty. Unable to load folder from relative path.");
                return;
            }

            try
            {
                OlRoot = olRoot;
                var pathParts = RelativePath.Split(['\\'], StringSplitOptions.RemoveEmptyEntries);
                Outlook.Folder currentFolder = olRoot;

                foreach (var part in pathParts)
                {
                    currentFolder = currentFolder.Folders.Cast<Outlook.Folder>().FirstOrDefault(f => f.Name.Equals(part, StringComparison.OrdinalIgnoreCase));
                    if (currentFolder == null)
                    {
                        logger.Warn($"Folder part '{part}' not found in the path '{RelativePath}'.");
                        return;
                    }
                }

                OlFolder = currentFolder;
            }
            catch (Exception e)
            {
                logger.Error($"Error loading folder from relative path '{RelativePath}'. {e.Message}", e);
            }
        }

        public void ResetLazy()
        {            
            _lazyRelativePath = new Lazy<string>(ToRelativePath);
            _lazyName = new Lazy<string>(() => OlFolder?.Name);
        }

        
    }
}
