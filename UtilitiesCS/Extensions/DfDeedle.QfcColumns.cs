using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS
{
#nullable enable

    public static partial class DfDeedle
    {
        /// <summary>
        /// Number of 3000 ms deadlines the column-add work is given before it fails loudly. The
        /// total budget is unchanged from the previous retry loop, so no folder that succeeds
        /// today begins to fail on timing alone.
        /// </summary>
        private const int AttemptLimit = 3;

        private static void AddQfcColumns(Table table, MAPIFolder folder)
        {
            if (!EnsureTriageColumnExists(folder))
            {
                MessageBoxInvoker(
                    "Cannot proceed without the required 'Triage' column. Execution will stop.",
                    "Missing Required Column",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                throw new InvalidOperationException("Required column 'Triage' does not exist.");
            }

            // Each column operation is timed individually so a single slow column is attributable
            // in the log rather than hidden inside one aggregate interval.
            var columnStopwatch = Stopwatch.StartNew();
            table.Columns.Add("SentOn");
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Add",
                $"column=SentOn; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );

            columnStopwatch.Restart();
            table.Columns.Add(MAPIFields.Schemas.ConversationId);
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Add",
                $"column={MAPIFields.Schemas.ConversationId}; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );

            columnStopwatch.Restart();
            table.Columns.Add(MAPIFields.Schemas.Triage);
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Add",
                $"column={MAPIFields.Schemas.Triage}; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );

            columnStopwatch.Restart();
            table.Columns.Remove("Subject");
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Remove",
                $"column=Subject; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );

            columnStopwatch.Restart();
            table.Columns.Remove("CreationTime");
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Remove",
                $"column=CreationTime; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );

            columnStopwatch.Restart();
            table.Columns.Remove("LastModificationTime");
            columnStopwatch.Stop();
            LogDfTiming(
                "AddQfcColumns Columns.Remove",
                $"column=LastModificationTime; elapsedMs={columnStopwatch.ElapsedMilliseconds}"
            );
        }

        /// <summary>
        /// Adds the QuickFiler columns to <paramref name="table"/>, retrying on timeout.
        /// </summary>
        /// <param name="columnAdder">
        /// Test seam for the column-adding work performed inside the timed <see cref="Task.Run"/>.
        /// When null the production <see cref="AddQfcColumns"/> path is used. Declared as
        /// <c>Action&lt;object, object&gt;</c> rather than over the interop types because embedded
        /// interop types cannot be used as generic type arguments across an assembly boundary
        /// (CS1769); the same constraint already applies to the <c>DefaultTableEtl</c> seam.
        /// </param>
        /// <param name="timeProvider">
        /// Clock used to arm the timeout. When null the system clock is used; tests supply a
        /// fake clock so every deadline in the retry sequence is deterministic.
        /// </param>
        internal static async Task AddQfcColumnsAsync(
            Table table,
            MAPIFolder folder,
            CancellationToken token,
            int counter,
            Action<object, object>? columnAdder = null,
            TimeProvider? timeProvider = null
        )
        {
            Action<object, object> adder =
                columnAdder ?? ((t, f) => AddQfcColumns((Table)t, (MAPIFolder)f));

            // The work is started exactly once and re-deadlined, never restarted. A blocking
            // synchronous COM call cannot be cancelled on .NET Framework: Task.Run with a token
            // suppresses scheduling only and cannot interrupt a call already inside the interop
            // marshaller, so starting a second task would overlap the first against the same
            // table rather than replace it.
            var work = Task.Run(() => adder(table, folder), token);

            // counter is the starting attempt index, so the existing call site, which passes 0,
            // keeps the same total budget of three deadlines of 3000 ms.
            for (var attempt = counter; attempt < AttemptLimit; attempt++)
            {
                try
                {
                    await work.TimeoutAfter(3000, timeProvider);
                    return;
                }
                catch (TaskCanceledException)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                catch (TimeoutException)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            var folderName = folder?.Name ?? "(unknown folder)";
            throw new TimeoutException(
                $"The column add step timed out after 9000 ms for folder '{folderName}'. "
                    + "The folder did not return from the column add within its budget."
            );
        }

        private static bool EnsureTriageColumnExists(MAPIFolder folder)
        {
            if (folder is null)
            {
                return false;
            }

            if (HasUserDefinedProperty(folder, "Triage"))
            {
                return true;
            }

            var createResult = MessageBoxInvoker(
                "The required 'Triage' column does not exist in this folder.\nWould you like to create it now?",
                "Create Required Column",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning
            );

            if (createResult != System.Windows.Forms.DialogResult.Yes)
            {
                return false;
            }

            try
            {
                folder.UserDefinedProperties.Add(
                    "Triage",
                    OlUserPropertyType.olText,
                    true,
                    Type.Missing
                );
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBoxInvoker(
                    $"Failed to create 'Triage' column.\n{ex.Message}",
                    "Column Creation Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                return false;
            }
        }

        private static bool HasUserDefinedProperty(MAPIFolder folder, string propertyName)
        {
            if (folder?.UserDefinedProperties is null || string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            // Times the COM enumeration rather than the cheap null guard above: on a slow store
            // this loop is the step that stalls the column add, and the elapsed value is what
            // makes a stalled launch attributable.
            var enumerationStopwatch = Stopwatch.StartNew();
            try
            {
                foreach (UserDefinedProperty property in folder.UserDefinedProperties)
                {
                    if (
                        string.Equals(
                            property.Name,
                            propertyName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                enumerationStopwatch.Stop();
                LogDfTiming(
                    "HasUserDefinedProperty user-defined property enumeration",
                    $"propertyName={propertyName}; elapsedMs={enumerationStopwatch.ElapsedMilliseconds}"
                );
            }
        }

        /// <summary>
        /// The column names the email dataframe projection indexes unconditionally. The casing
        /// asymmetry is intentional and is part of the contract, sourced from the field schema
        /// map: capital D in <c>EntryID</c>, lowercase d in <c>ConversationId</c>.
        /// </summary>
        private static readonly string[] RequiredEmailColumns =
        {
            "EntryID",
            "MessageClass",
            "SentOn",
            "ConversationId",
            "Triage",
        };

        /// <summary>
        /// Validates that the column-index map returned by the table ETL carries every column the
        /// email dataframe projection reads, so a short column set is reported at the folder that
        /// produced it rather than surfacing later as an unattributable
        /// <see cref="KeyNotFoundException"/> from the row builder.
        /// </summary>
        /// <param name="columnInfo">Maps field name to column index in the ETL data array.</param>
        /// <param name="folderName">Folder name reported in the diagnostic.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when any required column is absent. The message names every missing column and
        /// the folder.
        /// </exception>
        internal static void ValidateRequiredEmailColumns(
            Dictionary<string, int> columnInfo,
            string folderName
        )
        {
            // Compared ordinally against the key set rather than through ContainsKey, so the
            // ordinal contract holds whatever comparer the caller's dictionary carries. The
            // producing table utility uses the default ordinal comparer, and the required names
            // differ only by casing from plausible variants, so a case-insensitive match here
            // would accept a key the projection cannot actually index.
            var missing = RequiredEmailColumns
                .Where(name => !columnInfo.Keys.Contains(name, StringComparer.Ordinal))
                .ToList();

            if (missing.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $"The table for folder '{folderName}' is missing "
                    + $"{missing.Count} required column(s): {string.Join(", ", missing)}. "
                    + "The QuickFiler column add did not produce the expected column set, so the "
                    + "email data frame cannot be built for this folder."
            );
        }
    }
}
