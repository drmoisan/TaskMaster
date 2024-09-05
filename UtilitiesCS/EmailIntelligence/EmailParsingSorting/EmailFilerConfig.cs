using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.EmailIntelligence.EmailParsingSorting
{
    public class EmailFilerConfig
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers

        public EmailFilerConfig() { }

        public EmailFilerConfig(bool savePictures,
                                   string destinationOlStem,
                                   bool saveMsg,
                                   bool saveAttachments,
                                   bool removePreviousFsFiles,
                                   IApplicationGlobals appGlobals,
                                   string olAncestor,
                                   string fsAncestorEquivalent)
        {
            SavePictures = savePictures;
            DestinationOlStem = destinationOlStem;
            SaveMsg = saveMsg;
            SaveAttachments = saveAttachments;
            RemovePreviousFsFiles = removePreviousFsFiles;
            Globals = appGlobals;
            OlAncestor = olAncestor;
            FsAncestorEquivalent = fsAncestorEquivalent;
        }

        #endregion Constructors and Initializers

        #region Public Properties

        private bool _savePictures = false;
        public bool SavePictures { get => _savePictures; set => _savePictures = value; }

        private string _destinationOlStem = "";
        public string DestinationOlStem { get => _destinationOlStem; set => _destinationOlStem = value; }

        private string _destinationOlPath;
        public string DestinationOlPath { get => _destinationOlPath; set => _destinationOlPath = value; }

        private bool _saveMsg = false;
        public bool SaveMsg { get => _saveMsg; set => _saveMsg = value; }

        private bool _saveAttachments = false;
        public bool SaveAttachments { get => _saveAttachments; set => _saveAttachments = value; }

        private bool _removePreviousFsFiles = false;
        public bool RemovePreviousFsFiles { get => _removePreviousFsFiles; set => _removePreviousFsFiles = value; }

        private IApplicationGlobals _globals;
        public IApplicationGlobals Globals { get => _globals; set => _globals = value; }

        private string _olAncestor = "";
        public string OlAncestor { get => _olAncestor; set => _olAncestor = value; }

        private string _fsAncestorEquivalent;
        public string FsAncestorEquivalent { get => _fsAncestorEquivalent; set => _fsAncestorEquivalent = value; }

        private string _saveFsPath;
        public string SaveFsPath { get => _saveFsPath; set => _saveFsPath = value; }

        private string _deleteFsPath;
        public string DeleteFsPath { get => _deleteFsPath; set => _deleteFsPath = value; }

        private Folder _originFolder;
        public Folder OriginFolder { get => _originFolder; set => _originFolder = value; }

        private string _originOlStem;
        public string OriginOlStem { get => _originOlStem; set => _originOlStem = value; }

        private Folder _destinationOlFolder;
        public Folder DestinationOlFolder { get => _destinationOlFolder; set => _destinationOlFolder = value; }

        private bool _deleteAndUnTrain;
        public bool DeleteAndUnTrain { get => _deleteAndUnTrain; set => _deleteAndUnTrain = value; }

        private bool _canSort;
        public bool CanSort { get => _canSort; set => _canSort = value; }

        #endregion Public Properties

        #region Public Methods

        public bool IsDeleteRelevant(Folder currentFolder) 
        {
            currentFolder.ThrowIfNull();

            if ((currentFolder.FolderPath != Globals.Ol.EmailRootPath) &&
                (currentFolder.FolderPath.Contains(OlAncestor)) &&
                (currentFolder.FolderPath != OlAncestor))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void ResolvePaths(Folder currentFolder)
        {
            //TraceUtility.LogMethodCall(currentFolder, DestinationOlStem, Globals, OlAncestor, FsAncestorEquivalent);
            
            DestinationOlPath = $"{OlAncestor}\\{DestinationOlStem}";
            SaveFsPath = DestinationOlPath.ToFsFolderpath(OlAncestor, FsAncestorEquivalent);
            DeleteAndUnTrain = IsDeleteRelevant(currentFolder);
            DeleteFsPath = DeleteAndUnTrain ? currentFolder.ToFsFolderpath(OlAncestor, FsAncestorEquivalent) : null;
            DestinationOlFolder = TryResolveDestinationFolder();
            OriginFolder = currentFolder;
            OriginOlStem = GetStem(OlAncestor, currentFolder.FolderPath);
            CanSort = DestinationOlFolder is not null;

        }

        public void ResolvePaths()
        {
            //TraceUtility.LogMethodCall(DestinationOlStem, Globals, OlAncestor, FsAncestorEquivalent);

            DestinationOlPath = $"{OlAncestor}\\{DestinationOlStem}";
            SaveFsPath = DestinationOlPath.ToFsFolderpath(OlAncestor, FsAncestorEquivalent);            
            DestinationOlFolder = TryResolveDestinationFolder();
        }

        public Folder TryResolveDestinationFolder()
        {
            try
            {
                var destinationOlFolder = new OlFolderHelper(Globals).GetFolder(DestinationOlPath, Globals.Ol.App);
                return destinationOlFolder;
            }
            catch (System.Exception e)
            {
                //logger.Debug($"Cannot grab handle on Folder {DestinationOlPath}. Emails will not be moved");
                logger.Error(e);
                return null;
            }
        }

        public string GetStem(string olAncestor, string folderPath)
        {
            return folderPath.Replace(olAncestor, "").TrimStart('\\');
        }

        #endregion Public Methods
    }
}
