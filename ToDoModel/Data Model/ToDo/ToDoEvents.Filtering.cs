using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ToDoModel
{
    // why: issue #328. This partial holds the store-filtering/enumeration surface of ToDoEvents
    // relocated out of ToDoEvents.cs to keep both files within the 500-line file-size limit after
    // the dead-method deletion and the IApplicationGlobals threading. The [ExcludeFromCodeCoverage]
    // attribute is declared once on the primary ToDoEvents.cs partial and applies to the whole
    // type (a duplicate here would be CS0579), so this relocation stays coverage-exempt.
    public static partial class ToDoEvents
    {
        public static void WriteToCSV(string filename, string[] strOutput, bool overwrite = false)
        {
            // CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
            if (overwrite | File.Exists(filename) == false)
            {
                using (var sw = new StreamWriter(filename))
                {
                    for (int i = 0; i < strOutput.Length; i++)
                        sw.WriteLine(strOutput[i]);
                }
            }
            else
            {
                using (var sw = new StreamWriter(filename, append: true))
                {
                    for (int i = 0; i < strOutput.Length; i++)
                        sw.WriteLine(strOutput[i]);
                }
            }
        }

        public static void WriteToCSV(string filename, string strOutput, bool overwrite = false)
        {
            // CLEANUP: Determine if ThisAddIn.WriteToCSV function is needed. If so, move it to a library
            if (overwrite | File.Exists(filename) == false)
            {
                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine(strOutput);
                }
            }
            else
            {
                using (var sw = new StreamWriter(filename, append: true))
                {
                    sw.WriteLine(strOutput);
                }
            }
        }

        public static IAsyncEnumerable<object> GetAsyncEnumerableOfToDoItemsInView(
            IApplicationGlobals globals
        )
        {
            var olApp = globals.Ol.App;
            var storesWrapper = globals.Ol.StoresWrapper;
            var olView = (Outlook.View)olApp.ActiveExplorer().CurrentView;
            var strFilter = "@SQL=" + olView.Filter;
            var items = olApp
                .Session.Stores?.Cast<Store>()
                // why: issue #328. Route store inclusion through the single shared predicate.
                // Fail-open when the model is not yet loaded (storesWrapper is null) per AC7.
                ?.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))
                ?.ToAsyncEnumerable()
                ?.Select(store => store.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                ?.SelectMany(folder =>
                    strFilter == "@SQL="
                        ? folder?.Items?.Cast<object>()?.ToAsyncEnumerable()
                        : folder?.Items?.Restrict(strFilter)?.Cast<object>()?.ToAsyncEnumerable()
                );
            return items;
        }

        public static async Task RefreshToDoIdSplitsAsync(IApplicationGlobals globals)
        {
            var itemsAsyncEnum = GetAsyncEnumerableOfToDoItemsInView(globals);
            // ForEachAwaitAsync is obsolete (CS0618) per the framework's migration guidance
            // ("Use the language support for async foreach instead"), but replacing it with
            // `await foreach` here is a control-flow change to a production async method, not
            // an annotation-only edit. Suppressing narrowly preserves the exact pre-existing
            // behavior (no behavior change per AC7). This diagnostic surfaced only via the
            // capstone's solution-wide P2-T17 gate (ToDoModel.csproj is outside this plan's two
            // declared Phase 2 scope trees, UtilitiesCS/EmailIntelligence/** and
            // UtilitiesCS/OutlookObjects/Folder/**) — flagged for the orchestrator in this
            // plan's final report.
#pragma warning disable CS0618
            await itemsAsyncEnum.ForEachAwaitAsync(async item =>
                await Task.Run(() => TrySplitToDoID(item))
            );
#pragma warning restore CS0618
        }

        private static void TrySplitToDoID(object item)
        {
            try
            {
                new ToDoItem(new OutlookItem(item)).SplitID();
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
    }
}
