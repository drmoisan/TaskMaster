using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class EfcHomeController
    {
        internal Func<
            string,
            bool,
            bool,
            bool,
            bool,
            Task<bool>
        > MoveToFolderAsyncAction { get; set; }

        internal Action<string> MoveFailureMessageAction { get; set; } =
            text => MessageBox.Show(text);

        internal Action<
            IApplicationGlobals,
            string,
            List<MailItemHelper>
        > MoveMetricsAction { get; set; }

        public async Task ExecuteMovesAsync()
        {
            if (!TryBeginExecuteMoves())
            {
                return;
            }

            try
            {
                await ExecuteMovesCoreAsync();
            }
            finally
            {
                ResetExecuteMovesState();
            }
        }

        /// <summary>
        /// Takes the single-move guard, returning true to exactly one caller. The compare and the
        /// assignment are one indivisible operation, so competing callers cannot both observe the
        /// unset state and both proceed.
        /// </summary>
        internal bool TryBeginExecuteMoves()
        {
            return Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0;
        }

        /// <summary>
        /// Releases the single-move guard so a later move can begin.
        /// </summary>
        internal void ResetExecuteMovesState()
        {
            Interlocked.Exchange(ref _isExecuting, 0);
        }

        internal async Task ExecuteMovesCoreAsync()
        {
            var selectedFolder = _formController.SelectedFolder;
            var moveConversation = _formController.MoveConversation;
            var convInfo = SelectMoveMetricsItems(
                DataModel.ConversationResolver.ConversationInfo.SameFolder,
                moveConversation,
                DataModel.Mail.EntryID
            );

            var globals = _globals;
            var result = await MoveToFolderAsync(
                selectedFolder,
                _formController.SaveAttachments,
                _formController.SaveEmail,
                _formController.SavePictures,
                moveConversation
            );

            HandleMoveResult(result, globals, selectedFolder, convInfo);
        }

        internal Task<bool> MoveToFolderAsync(
            string selectedFolder,
            bool saveAttachments,
            bool saveEmail,
            bool savePictures,
            bool moveConversation
        )
        {
            return MoveToFolderAsyncAction is null
                ? _dataModel.MoveToFolderAsync(
                    selectedFolder,
                    saveAttachments,
                    saveEmail,
                    savePictures,
                    moveConversation
                )
                : MoveToFolderAsyncAction(
                    selectedFolder,
                    saveAttachments,
                    saveEmail,
                    savePictures,
                    moveConversation
                );
        }

        internal static List<MailItemHelper> SelectMoveMetricsItems(
            IEnumerable<MailItemHelper> sameFolder,
            bool moveConversation,
            string mailEntryId
        )
        {
            return moveConversation
                ? sameFolder.ToList()
                : sameFolder.Where(itemInfo => itemInfo.EntryId == mailEntryId).ToList();
        }

        internal void HandleMoveResult(
            bool result,
            IApplicationGlobals globals,
            string selectedFolder,
            List<MailItemHelper> movedItems
        )
        {
            if (!result)
            {
                MoveFailureMessageAction($"Cannot move to folderpath {selectedFolder}");
                return;
            }

            if (MoveMetricsAction is not null)
            {
                MoveMetricsAction(globals, selectedFolder, movedItems);
                return;
            }

            QuickFileMetrics_WRITE(globals.FS.Filenames.EmailSession, selectedFolder, movedItems);
        }
    }
}
