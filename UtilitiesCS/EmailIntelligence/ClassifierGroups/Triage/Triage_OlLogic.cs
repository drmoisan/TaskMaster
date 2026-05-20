using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fizzler;
using Microsoft.Graph.Models;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups
{
    public class Triage_OlLogic
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public Triage_OlLogic(EmailIntelligence.Triage parent)
        {
            Parent = parent;
        }

        internal EmailIntelligence.Triage Parent { get; set; }

        private const string SchemaSite =
            "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}";
        private const string RegExSchemaSite =
            "http://schemas\\.microsoft\\.com/mapi/string/{00020329-0000-0000-C000-000000000046}";

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
                if (explorer is null)
                {
                    logger.Debug("Could not grab handle on Explorer");
                    return;
                }

                View view = explorer.CurrentView as View;
                if (view is null)
                {
                    logger.Debug("Could not grab handle on View");
                    return;
                }

                string existingFilter = view.Filter;
                logger.Debug($"Existing filter: {existingFilter}");

                var parser = new DASLFilterParser();
                var logicTree = parser.Parse(existingFilter);
                parser.PrintTree(logicTree, 0);

                var pattern =
                    @"""http://schemas\.microsoft\.com/mapi/string/\{00020329-0000-0000-C000-000000000046}/Triage"" (= '[ABC]'|LIKE '%[ABC]%')";
                Regex objRegex = new(pattern);

                string strippedFilter = ParseAndStripFilter(existingFilter);
                logger.Debug($"Stripped filter: {strippedFilter}");

                string newFilter = triageValues.IsNullOrEmpty()
                    ? ""
                    : string.Join(" OR ", triageValues.Select(value => $"[Triage] = '{value}'"));

                if (!existingFilter.IsNullOrEmpty())
                {
                    string pattern2 =
                        @"\[Triage\]\s*=\s*'[^']*'(\s*OR\s*\[Triage\]\s*=\s*'[^']*')*";
                    if (Regex.IsMatch(existingFilter, pattern2))
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

        public TreeNode<string> StripFilter(Regex regex, TreeNode<string> tree)
        {
            foreach (var child in tree.Children)
            {
                StripFilter(regex, child);
            }

            var match = regex.Match(tree.Value);
            if (match.Success)
            {
                if (tree.Parent is not null)
                {
                    var parent = tree.Parent;
                    parent.Children.Remove(tree);
                    tree.Parent = null;
                    if (parent.Parent is not null)
                    {
                        var grandParent = parent.Parent;
                        if (parent.ChildCount == 1)
                        {
                            var brother = parent.Children.First();
                            grandParent.Parent.RemoveChild(parent);
                            parent.Parent = null;
                            grandParent.AddChild(brother);
                            brother.Parent = grandParent;
                        }
                        else
                        {
                            grandParent.Children.Remove(parent);
                            parent.Parent = null;
                        }
                        return grandParent;
                    }
                    else
                    {
                        if (parent.ChildCount == 1)
                        {
                            var brother = parent.Children.First();
                            brother.Parent = null;
                            return brother;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return tree;
            }
        }

        public string ParseAndStripFilter(string strFilter)
        {
            var triageField = Regex.Escape($"\"{MAPIFields.Schemas.Triage}\"");
            var pattern =
                $@"(?:\s*(?:\(\s*)?{triageField}\s*(?:LIKE\s*'%[ABC]%'|=\s*'[ABC]')\s*(?:\)\s*)?(?:OR\s*)?)+";
            var strippedFilter = Regex.Replace(strFilter, pattern, "");
            strippedFilter = Regex.Replace(strippedFilter, @"\(\s*\)", "");
            strippedFilter = Regex.Replace(strippedFilter, @"\s{2,}", " ").Trim();
            return strippedFilter;
        }

        public async Task TrainSelectionAsync(string triageId, CancellationToken token = default)
        {
            var selection = Parent?.Globals?.Ol?.App?.ActiveExplorer()?.Selection;
            if (selection is null)
            {
                logger.Debug("Could not grab handle on Selection");
                return;
            }
            var mailItems = selection
                .Cast<object>()
                .Where(x => x is MailItem)
                .Cast<MailItem>()
                .ToAsyncEnumerable();

            await foreach (var mailItem in mailItems.WithCancellation(token))
            {
                var helper = await MailItemHelper.FromMailItemAsync(
                    mailItem,
                    Parent.Globals,
                    token,
                    false
                );
                await Parent.TestActionAsync(helper, triageId, token);
                await Parent.TrainAsync(helper.Tokens, triageId, token);
            }

            Parent.ClassifierGroup.Serialize();
        }

        public async Task UnTrainSelectionAsync(CancellationToken token = default)
        {
            var selection = Parent?.Globals?.Ol?.App?.ActiveExplorer()?.Selection;
            if (selection is null)
            {
                logger.Debug("Could not grab handle on Selection");
                return;
            }
            var mailItems = selection
                .Cast<object>()
                .Where(x => x is MailItem)
                .Cast<MailItem>()
                .ToAsyncEnumerable();

            await foreach (var mailItem in mailItems.WithCancellation(token))
            {
                var helper = await MailItemHelper.FromMailItemAsync(
                    mailItem,
                    Parent.Globals,
                    token,
                    false
                );
                var triageId = helper.Triage;
                await Parent.UnTrainAsync(helper.Tokens, triageId, token);
                helper.Item.DeleteUdf("Triage");
            }

            Parent.ClassifierGroup.Serialize();
        }
    }
}
