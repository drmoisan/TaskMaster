#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Pure selection mapping for the QuickFiler breadcrumb (#351 P3-T9): maps the visible
    /// row/subfolder selection of a <see cref="BreadcrumbStateModel"/> to the exact
    /// <c>GetSelectedFolder()</c> output string — the full folder path for Path A suggestion rows
    /// and expanded-subfolder selections, the verbatim string (including the literal
    /// <c>"Trash to Delete"</c>) for Path B rows (G10/FR-7) — plus the index/item lookup helpers
    /// backing <c>SetFolderSelectedIndex</c>/<c>SetFolderSelectedItem</c>/<c>FolderContains</c>/
    /// <c>GetFolderItems</c>. No I/O, WinForms, or WebView2 references.
    /// </summary>
    public static class BreadcrumbSelectionMap
    {
        /// <summary>
        /// The selection output string: the selected subfolder's full path when a subfolder is
        /// selected, the leaf's full path for a suggestion row, the verbatim string for a plain
        /// row, or null when nothing is selected (legacy no-selection contract).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is null.</exception>
        public static string? GetSelectedFolder(BreadcrumbStateModel model)
        {
            RequireModel(model);
            var row = model.SelectedRow;
            if (row == null)
            {
                return null;
            }
            if (model.SelectedSubfolderIndex >= 0)
            {
                return row.Subfolders[model.SelectedSubfolderIndex].FolderPath;
            }
            return RowValue(row);
        }

        /// <summary>
        /// The per-row output strings in display order (the <c>GetFolderItems()</c> contract).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is null.</exception>
        public static string[] GetFolderItems(BreadcrumbStateModel model)
        {
            RequireModel(model);
            var items = new string[model.Rows.Count];
            for (int i = 0; i < model.Rows.Count; i++)
            {
                items[i] = RowValue(model.Rows[i]);
            }
            return items;
        }

        /// <summary>
        /// True when a row's output string equals <paramref name="item"/> exactly (ordinal; the
        /// <c>FolderContains(string)</c> contract).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> or <paramref name="item"/> is null.</exception>
        public static bool FolderContains(BreadcrumbStateModel model, string item)
        {
            return IndexOfItem(model, item) >= 0;
        }

        /// <summary>
        /// The first row index whose output string equals <paramref name="item"/> exactly
        /// (ordinal), or -1 when no row matches (the explicit unknown-item signal).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> or <paramref name="item"/> is null.</exception>
        public static int IndexOfItem(BreadcrumbStateModel model, string item)
        {
            RequireModel(model);
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            for (int i = 0; i < model.Rows.Count; i++)
            {
                if (string.Equals(RowValue(model.Rows[i]), item, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Selects the first row whose output string equals <paramref name="item"/> (the
        /// <c>SetFolderSelectedItem</c> contract). Returns false without changing the selection
        /// when no row matches, mirroring the legacy ComboBox unknown-item no-op.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> or <paramref name="item"/> is null.</exception>
        public static bool TrySelectItem(BreadcrumbStateModel model, string item)
        {
            int index = IndexOfItem(model, item);
            if (index < 0)
            {
                return false;
            }
            model.SelectRow(index);
            return true;
        }

        /// <summary>
        /// The output string of one row: leaf full path for suggestion rows, verbatim text for
        /// plain rows (byte-identical, including <c>"Trash to Delete"</c>).
        /// </summary>
        private static string RowValue(BreadcrumbRow row)
        {
            return row.IsSuggestion ? row.Chain[row.Chain.Count - 1].FolderPath : row.VerbatimText!;
        }

        private static void RequireModel(BreadcrumbStateModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
        }
    }
}
