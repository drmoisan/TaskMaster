using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.Interfaces;

namespace TaskVisualization
{
    public class FlagChangeGroup : IFlagChangeGroup
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public FlagChangeGroup(IApplicationGlobals globals, MailItem item)
        {
            Globals = globals.ThrowIfNull();
            Item = item.ThrowIfNull();
            Subject = item.Subject;
        }

        #endregion ctor

        #region Properties

        internal virtual IApplicationGlobals Globals { get; set; }
        internal virtual MailItem Item { get; set; }
        internal string Subject { get; private set; }

        public virtual BlockingCollection<IFlagChangeItem> FlagChangeItems { get; set; } = [];

        #endregion Properties

        #region Methods

        public virtual bool TryEnqueue(string classifierName, IEnumerable<string> original, IEnumerable<string> revised)
        {
            var (changeCount, untrain, train) = original.CompareTo(revised);
            if (changeCount > 0)
            {
                FlagChangeItems.Add(new FlagChangeItem { ClassifierName = classifierName, UntrainFlags = [.. untrain], TrainFlags = [.. train] });
                return true;
            }
            else { return false; }
        }

        public virtual async Task ProcessGroupAsync(CancellationToken cancel = default)
        {
            MailItemHelper helper = null;
            try
            {
                helper = await MailItemHelper.FromMailItemAsync(Item, Globals, cancel, false).ConfigureAwait(false);
                cancel.ThrowIfCancellationRequested();
                await helper.TokenizeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                logger.Warn($"Processing of {nameof(FlagChangeGroup)} was cancelled.", e);
                return;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error processing mail item with subject {Subject}. {e.Message}", e);
                return;
            }
            while (FlagChangeItems.TryTake(out var item) && !cancel.IsCancellationRequested)
            {
                await TryProcessFlagItemAsync(item, helper, cancel).ConfigureAwait(false);
            }
        }

        internal virtual async Task<bool> TryProcessFlagItemAsync(IFlagChangeItem item, MailItemHelper helper, CancellationToken cancel)
        {
            try
            {
                await ProcessFlagItemAsync(item, helper, cancel).ConfigureAwait(false);
                return true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error processing flag change item with classifier {item.ClassifierName}. {e.Message}", e);
                return false;
            }
        }

        internal virtual async Task ProcessFlagItemAsync(IFlagChangeItem item, MailItemHelper helper, CancellationToken cancel)
        {
            if (Globals.AF.Manager.TryGetValue(item.ClassifierName, out var classifierTask))
            {
                var classifier = await classifierTask;
                if (item.UntrainFlags.Count > 0) 
                { 
                    await classifier.UnTrainMultiTagAsync(item.UntrainFlags, helper.Tokens, 1, cancel).ConfigureAwait(false);
                }
                if (item.TrainFlags.Count > 0)
                {
                    await classifier.TrainMultiTagAsync(item.TrainFlags, helper.Tokens, 1, cancel).ConfigureAwait(false);
                }
                classifier.Serialize();
            }
        }

        #endregion Methods
    }
}
