using System;
using System.Linq;
using System.Threading.Tasks;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public partial class QfcDatamodel
    {
        public Frame<int, string> InitDf(Explorer activeExplorer)
        {
            var df = DfDeedle.GetEmailDataInView(activeExplorer);

            // Filter out non-email items
            df = df.FilterRowsBy("MessageClass", "IPM.Note");
            //df.Display(new List<string> { "RowKey" });
            // Filter to the latest email in each conversation
            var dfFiltered = MostRecentByConversation(df);

            // Sort by triage classification and then date
            var dfSorted = SortTriageDate(dfFiltered);

            return dfSorted;
        }

        /// <summary>
        /// If Outlook is not in offline mode, save the state and toggle it to offline mode
        /// </summary>
        /// <param name="offline"></param>
        /// <returns></returns>
        private async Task<bool> ToggleOfflineMode(bool offline)
        {
            if (!offline)
            {
                var commandBars = _activeExplorer.CommandBars;
                if (!offline)
                {
                    commandBars.ExecuteMso("ToggleOnline");
                }
                await TimeProvider.Delay(TimeSpan.FromMilliseconds(5));
            }
            return offline;
        }

        public async Task InitDfAsync(Explorer activeExplorer, ProgressTracker progress)
        {
            var df = await GetEmailsInViewDfAsync(activeExplorer, progress).ConfigureAwait(false);

            if (df is not null)
            {
                //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Filtering df ... ");
                // Filter out non-email items
                df = df.FilterRowsBy("MessageClass", "IPM.Note");

                // Filter to the latest email in each conversation
                var dfFiltered = MostRecentByConversation(df);

                //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Sorting df ... ");
                // Sort by triage classification and then date
                _frame = SortTriageDate(dfFiltered);

                progress.Report(100);
            }
        }

        private async Task<Frame<int, string>> GetEmailsInViewDfAsync(
            Explorer activeExplorer,
            ProgressTracker progress
        )
        {
            Frame<int, string> df = null;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Toggle offline mode");
            var offline = await ToggleOfflineMode(_globals.Ol.NamespaceMAPI.Offline);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(DfDeedle.GetEmailDataInViewAsync)} ... ");
            try
            {
                df = await DfDeedle
                    .GetEmailDataInViewAsync(
                        activeExplorer,
                        Token,
                        TokenSource,
                        progress.Increment(3).SpawnChild(78)
                    )
                    .ConfigureAwait(false);
                await ToggleOfflineMode(offline);

                //df.DisplayDialog();

                return df;
            }
            catch (TaskCanceledException)
            {
                //logger.Debug($"{nameof(DfDeedle.GetEmailDataInViewAsync)} Task cancelled");
                await ToggleOfflineMode(offline);
                return null;
            }
            catch (System.Exception e)
            {
                await ToggleOfflineMode(offline);
                logger.Error(
                    $"{nameof(DfDeedle.GetEmailDataInViewAsync)} Error. \n {e.Message}\n{e.StackTrace}"
                );
                throw e;
            }
        }

        public Frame<int, string> SortTriageDate(Frame<int, string> df)
        {
            var sorter = new EmailSorter(SortOptionsEnum.Default);

            var dfClone = df.Clone();

            var s1 = dfClone.GetColumn<DateTime>("SentOn");
            var s2 = dfClone.GetColumn<string>("Triage");
            var added = s1.ZipInner(s2)
                .Select(t => sorter.GetSortKey(triage: t.Value.Item2, dateTime: t.Value.Item1));
            dfClone.AddColumn("NewKey", added);

            dfClone = dfClone.SortRows("NewKey");

            var dfSorted = dfClone.IndexRowsWith(Enumerable.Range(0, dfClone.RowCount).Reverse());

            dfSorted = dfSorted.SortRowsByKey();

            dfSorted.DropColumn("NewKey");
            return dfSorted;
        }

        public Frame<int, string> MostRecentByConversation(Frame<int, string> df)
        {
            var topics = df.GetColumn<string>("ConversationId").Values.Distinct().ToArray();

            var rows = topics.Select(topic =>
            {
                var dfConversation = df.FilterRowsBy("ConversationId", topic);
                var maxSentOn = dfConversation.GetColumn<DateTime>("SentOn").Values.Max();
                var row = dfConversation.FilterRowsBy("SentOn", maxSentOn).Rows.FirstValue();
                //var dfDateIdx = dfConversation.IndexRows<DateTime>("SentOn", keepColumn: true);
                //var addr = dfDateIdx.RowIndex.Locate(maxSentOn);
                //var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                //var row = dfConversation.Rows.GetAt(idx);
                return row;
            });

            var dfFiltered = Frame.FromRows(rows);
            return dfFiltered;
        }
    }
}
