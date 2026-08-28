using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class EfcHomeController
    {
        public void QuickFileMetrics_WRITE(
            string filename,
            string selectedFolder,
            List<MailItemHelper> moved
        )
        {
            if (moved is null || moved.Count == 0)
            {
                return;
            }

            QuickFileMetrics_WRITE(
                filename,
                selectedFolder,
                moved,
                _stopWatch.Elapsed.TotalSeconds
            );
        }

        /// <summary>
        /// Writes the session metrics for the currently selected mail item. This overload is
        /// mandated by <see cref="QuickFiler.Interfaces.IFilerHomeController"/> and derives the
        /// folder and moved-item arguments the same way <c>ExecuteMovesCoreAsync</c> does.
        /// </summary>
        /// <param name="filename">Name of the session-metrics file, relative to MyDocuments.</param>
        /// <remarks>
        /// Returns without writing and without throwing when the form controller, the data model,
        /// or the data model's mail item is absent, following the silent no-op precedent of the
        /// three-argument overload rather than surfacing an exception at an interface boundary.
        /// </remarks>
        public void QuickFileMetrics_WRITE(string filename)
        {
            if (_formController is null || DataModel is null || DataModel.Mail is null)
            {
                return;
            }

            var moved = SelectMoveMetricsItems(
                DataModel.ConversationResolver.ConversationInfo.SameFolder,
                _formController.MoveConversation,
                DataModel.Mail.EntryID
            );

            QuickFileMetrics_WRITE(filename, _formController.SelectedFolder, moved);
        }

        internal void QuickFileMetrics_WRITE(
            string filename,
            string selectedFolder,
            List<MailItemHelper> moved,
            double elapsedSeconds
        )
        {
            var dataLines = BuildQuickFileMetricLines(
                _dependencies.MetricsNowFactory(),
                elapsedSeconds,
                selectedFolder,
                moved
            );
            if (dataLines.Length == 0)
            {
                return;
            }

            if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot))
            {
                _dependencies.MetricsLineWriter(filename, dataLines, folderRoot);
            }
        }

        internal static string[] BuildQuickFileMetricLines(
            DateTime currentDateTime,
            double elapsedSeconds,
            string selectedFolder,
            List<MailItemHelper> moved
        )
        {
            if (moved is null || moved.Count == 0)
            {
                return Array.Empty<string>();
            }

            var curDateText = currentDateTime.ToString("MM/dd/yyyy");
            var curTimeText = currentDateTime.ToString("hh:mm");
            var dataLineBeg = curDateText + "," + curTimeText + ",";

            var duration = elapsedSeconds;
            duration /= moved.Count;
            // The metrics file is machine-read, so numeric fields are rendered with the invariant
            // culture rather than the operator's locale, which would emit a decimal comma and
            // corrupt the CSV field count.
            var durationText = duration.ToString("##0", CultureInfo.InvariantCulture);
            var durationMinutesText = (duration / 60d).ToString(
                "##0.00",
                CultureInfo.InvariantCulture
            );
            var folderText = QfcCollectionController.xComma(selectedFolder);

            return moved
                .Select(itemInfo =>
                    dataLineBeg
                    + QfcCollectionController.xComma(itemInfo.Subject)
                    + $",SingleSorted,{durationText},{durationMinutesText},"
                    + $"{QfcCollectionController.xComma(itemInfo.ToRecipientsName)},"
                    + $"{QfcCollectionController.xComma(itemInfo.SenderName)},Email,{folderText},"
                    + $"{itemInfo.SentDate.ToString("MM/dd/yyyy")},"
                    + $"{itemInfo.SentDate.ToString("HH:mm:ss")}"
                )
                .ToArray();
        }
    }
}
