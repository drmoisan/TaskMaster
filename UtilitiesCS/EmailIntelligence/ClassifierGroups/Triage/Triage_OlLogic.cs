using Fizzler;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage
{
    public class Triage_OlLogic
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Triage_OlLogic(EmailIntelligence.Triage parent)
        {
            Parent = parent;
        }

        internal EmailIntelligence.Triage Parent { get; set; }

        
        public async Task FilterViewAsync()
        {
            await Task.Run(FilterView);
        }

        public void FilterView() 
        { 
            var choices = new List<string> { "A", "B", "C" };
            var selections = Parent.Globals.TD.SelectFromList(choices);
            var triageValues = selections?.Select(x => x.Last()).ToArray() ?? [];
            FilterView(triageValues);
        }

        public void FilterView(char[] triageValues)
        {
            try
            {
                Explorer explorer = Parent?.Globals?.Ol?.App?.ActiveExplorer();
                if (explorer is null) { logger.Debug("Could not grab handle on Explorer"); return; }
                
                View view = explorer.CurrentView as View;
                if (view is null) { logger.Debug("Could not grab handle on View"); return; }

                string existingFilter = view.Filter;
                string newFilter = triageValues.IsNullOrEmpty()? "" : 
                    string.Join(" OR ", triageValues.Select(value => $"[Triage] = '{value}'"));

                if (!existingFilter.IsNullOrEmpty())
                {
                    string pattern = @"\[Triage\]\s*=\s*'[^']*'(\s*OR\s*\[Triage\]\s*=\s*'[^']*')*";
                    if (Regex.IsMatch(existingFilter, pattern))
                    {
                        // Replace existing Triage filter
                        view.Filter = Regex.Replace(existingFilter, pattern, newFilter);
                    }
                    else
                    {
                        if (existingFilter.StartsWith("@SQL="))
                        {
                            // Handle DASL filter
                            view.Filter = $"{existingFilter} AND ({newFilter})";
                        }
                        else
                        {
                            // Handle JET filter
                            view.Filter = $"({existingFilter}) AND ({newFilter})";
                        }
                    }
                }
                else
                {
                    view.Filter = newFilter;
                }

                view.Apply();
                
                
            }
            catch (System.Exception ex)
            {
                logger.Error("Error applying filter to view: ", ex);
            }
        }
        
        public async Task TrainSelectionAsync(string triageId, CancellationToken token = default) 
        { 
            var selection = Parent?.Globals?.Ol?.App?.ActiveExplorer()?.Selection;
            if (selection is null) { logger.Debug("Could not grab handle on Selection"); return; }
            await selection.Cast<object>()
                .Where(x => x is MailItem)
                .Cast<MailItem>()
                .ToAsyncEnumerable()
                .SelectAwaitWithCancellation(async (mailItem, token) => await MailItemHelper.FromMailItemAsync(mailItem, Parent.Globals, token, false))
                .SelectAwaitWithCancellation(async (helper, token) => await Task.Run(() => helper.Tokens, token))
                .ForEachAwaitWithCancellationAsync((tokens, token) => Parent.TrainAsync(tokens, triageId, token), token);
                
            Parent.ClassifierGroup.Serialize();
        }

    }
}

