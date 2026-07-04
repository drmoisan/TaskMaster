using System;
using System.Collections.Generic;
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

            QuickFileMetrics_WRITE(filename, selectedFolder, moved, _stopWatch.Elapsed.Seconds);
        }

        public void QuickFileMetrics_WRITE(string filename)
        {
            throw new NotImplementedException();
        }

        internal void QuickFileMetrics_WRITE(
            string filename,
            string selectedFolder,
            List<MailItemHelper> moved,
            int elapsedSeconds
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
            int elapsedSeconds,
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
            var durationText = duration.ToString("##0");
            var durationMinutesText = (duration / 60d).ToString("##0.00");

            return moved
                .Select(itemInfo =>
                    dataLineBeg
                    + QfcCollectionController.xComma(itemInfo.Subject)
                    + $",SingleSorted,{durationText},{durationMinutesText},{itemInfo.ToRecipientsName}"
                    + $"{itemInfo.SenderName},Email,{selectedFolder},{itemInfo.SentDate.ToString("MM/dd/yyyy")},"
                    + $"{itemInfo.SentDate.ToString("HH:mm:ss")}"
                )
                .ToArray();
        }
    }
}
