#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public partial class EmailDataMiner
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors and private fields

        public EmailDataMiner(IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
        }

        private IApplicationGlobals _globals;

        // Never reassigned anywhere across the 4 EmailDataMiner partial files after this
        // `= default` (null) inline initializer; annotated nullable to reflect that, with
        // justified `!` at each of the 9 consumption sites in EmailDataMiner.FolderExtraction.cs.
        private SegmentStopWatch? _sw = default;
        internal const int MaxObjectSize = 1000000000;

        #endregion Constructors and private fields

        #region ETL - Extract, Transform, Load For Data Mining

        [ExcludeFromCodeCoverage]
        public async Task<ScBag<MinedMailInfo>?> MineEmails()
        {
            if (SynchronizationContext.Current is null)
            {
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            }

            var offline = await ToggleOfflineMode(_globals.Ol.NamespaceMAPI.Offline);

            var folderGroups = await Task.Run(async () => await ExtractOlFolderChunks());

            await Transform(folderGroups, ToIItemInfoArray, withValidation: false);
            await Transform<IItemInfo[], MinedMailInfo[]>(ToMinedMail);
            await Transform<MinedMailInfo[], MinedMailInfo[]>(Consolidate);

            await ToggleOfflineMode(offline);
            if (_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                var folderPath = Path.Combine(folderRoot, "Bayesian");
                return new ScBag<MinedMailInfo>(await Load<MinedMailInfo[]>(folderPath));
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        public async Task DeleteStagingFilesAsync()
        {
            await Task.Run(DeleteStagingFilesFromAppData);
        }

        [ExcludeFromCodeCoverage]
        private void DeleteStagingFilesFromAppData()
        {
            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                return;
            }

            DeleteStagingFiles(folderRoot, Directory.Exists, Directory.GetFiles, File.Delete);
        }

        internal static void DeleteStagingFiles(
            string folderRoot,
            Func<string, bool> directoryExists,
            Func<string, string[]> getFiles,
            Action<string> deleteFile
        )
        {
            var folderPath = Path.Combine(folderRoot, "Bayesian");
            if (!directoryExists(folderPath))
            {
                return;
            }

            var files = getFiles(folderPath);
            foreach (var file in files)
            {
                try
                {
                    deleteFile(file);
                }
                catch (System.Exception e)
                {
                    logger.Error($"Error deleting file {file}. \n{e.Message}\n{e.StackTrace}");
                }
            }
        }

        #endregion ETL - Extract, Transform, Load For Data Mining

        #region Helper Methods

        private string GetProgressMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg =
                $"Completed {complete} of {count} ({seconds:N2} spm) "
                + $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        /// <summary>
        /// If Outlook is not in offline mode, save the state and toggle it to offline mode
        /// </summary>
        /// <param name="offline"></param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private async Task<bool> ToggleOfflineMode(bool offline)
        {
            if (!offline)
            {
                var commandBars = _globals.Ol.App.ActiveExplorer().CommandBars;
                if (!offline)
                {
                    commandBars.ExecuteMso("ToggleOnline");
                }
                await Task.Delay(5);
            }
            return offline;
        }

        #endregion Helper Methods
    }
}
