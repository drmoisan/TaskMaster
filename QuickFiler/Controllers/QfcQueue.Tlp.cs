using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Table-layout-panel manipulation part of <see cref="QfcQueue"/>. This part exists because the
    /// base file <c>QfcQueue.cs</c> stood at 507 physical lines, already past the repository's
    /// 500-line ceiling, before issue #871 added any injectable seam to it. The whole
    /// <c>Tlp Manipulation</c> region moved here verbatim; no member was renamed, retyped or
    /// re-signed by the split, and the primary constructor and its field initializers stay on the
    /// base part.
    /// </summary>
    public partial class QfcQueue
    {
        #region Tlp Manipulation

        private TableLayoutPanel _tlpTemplate;
        public TableLayoutPanel TlpTemplate
        {
            get => _tlpTemplate;
            set
            {
                _tlpTemplate = value.Clone();
                _tlpTemplate.Name = "TemplateTableLayout";
                //_templateViewer.L1v0L2_PanelMain.Controls.Remove(_templateViewer.L1v0L2L3v_TableLayout);
                //_templateViewer.L1v0L2L3v_TableLayout = _tlpTemplate;
                //_templateViewer.L1v0L2L3v_TableLayout.Parent = _templateViewer.L1v0L2_PanelMain;
            }
        }

        internal void ActivateTlpTemplate(TableLayoutPanel tlp)
        {
            //_templateViewer.Text = "TemplateViewer";
            //_templateViewer.L1v0L2_PanelMain.Controls.Remove(_templateViewer.L1v0L2L3v_TableLayout);
            //_templateViewer.L1v0L2L3v_TableLayout = tlp;
            //_templateViewer.L1v0L2L3v_TableLayout.Parent = _templateViewer.L1v0L2_PanelMain;
            //_templateViewer.L1v0L2L3v_TableLayout.Visible = true;
        }

        //private QfcFormViewer _templateViewer = new();

        private TlpCellStates _tlpStates;
        public TlpCellStates TlpStates
        {
            get => _tlpStates;
            set => _tlpStates = value;
        }

        private Func<CancellationToken, ItemViewer> _itemViewerFactory = ItemViewerQueue.Dequeue;

        /// <summary>
        /// Issue #871 injectable seam S3 for the per-row item viewer. The default is the static
        /// <c>ItemViewerQueue.Dequeue</c> method group, so production behaviour is unchanged; a test
        /// assigns a factory returning a headless stand-in so that <c>AddAsync</c> is reachable
        /// without the process-wide viewer queue. The default is a declaration initializer rather
        /// than a lazy getter because the method group captures no instance state. The member is a
        /// property over a backing field rather than an auto-property because C# permits no
        /// accessor body on an auto-property and the null guard is part of the seam contract.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal Func<CancellationToken, ItemViewer> ItemViewerFactory
        {
            get => _itemViewerFactory;
            set => _itemViewerFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Action<TableLayoutPanel, ItemViewer, int> _viewerRowPlacer;

        /// <summary>
        /// Issue #871 injectable seam S4 for placing a viewer into a row of the table layout panel.
        /// The default is the <c>AddViewerToTlp</c> method group, which remains declared on this
        /// part with its body unchanged, so production behaviour is unchanged; a test assigns a
        /// recording substitute so the panel, viewer and index can be asserted without a live
        /// WinForms layout pass. The getter is lazy rather than a declaration initializer because
        /// the default is an instance method and a field or auto-property initializer cannot
        /// reference the instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer
        {
            get => _viewerRowPlacer ??= AddViewerToTlp;
            set => _viewerRowPlacer = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> _itemGroupFactory;

        /// <summary>
        /// Issue #871 injectable seam S5 for the per-row item group the loader builds. The default
        /// is the <c>AddAsync</c> method group, which remains declared on this part with its
        /// signature unchanged, so production behaviour is unchanged; a test assigns a recording
        /// substitute so the loader's index mapping and argument flow can be asserted without a
        /// live viewer. S4 and S5 are both present deliberately: a single coarse seam in place of
        /// S4 would make the loader coverable while leaving the production default it displaces
        /// permanently uncovered, which relocates the untestable region rather than closing it. The
        /// getter is lazy rather than a declaration initializer because the default is an instance
        /// method and a field or auto-property initializer cannot reference the instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory
        {
            get => _itemGroupFactory ??= AddAsync;
            set => _itemGroupFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Func<TableLayoutPanel, TableLayoutPanel> _backgroundTlpFactory = tlp =>
            tlp.Clone(name: "BackgroundTableLayout");

        /// <summary>
        /// Issue #871 injectable seam S6 for the background page's table layout panel. The default
        /// lambda performs the same reflection-driven clone call, with the same named argument, as
        /// the expression it replaces on the enqueue path, so production behaviour is unchanged; a
        /// test assigns a factory returning a sentinel panel so the enqueue path is reachable
        /// without a live WinForms control graph. The default is a declaration initializer rather
        /// than a lazy getter because the lambda captures no instance state.
        /// </summary>
        /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
        internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory
        {
            get => _backgroundTlpFactory;
            set => _backgroundTlpFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal async Task<QfcItemGroup> AddAsync(
            TableLayoutPanel tlp,
            MailItem mailItem,
            int indexNumber
        )
        {
            //TraceUtility.LogMethodCall(tlp, mailItem, indexNumber);

            var grp = new QfcItemGroup(mailItem);
            var viewer = ItemViewerFactory(_token);
            grp.ItemViewer = viewer;
            await UiIdleCallAsync(() => ViewerRowPlacer(tlp, viewer, indexNumber));
            return grp;
        }

        internal void AddViewerToTlp(TableLayoutPanel tlp, ItemViewer viewer, int indexNumber)
        {
            //TraceUtility.LogMethodCall(tlp, viewer, indexNumber);

            viewer.Parent = tlp;
            tlp.SetCellPosition(viewer, new TableLayoutPanelCellPosition(0, indexNumber));
            tlp.SetColumnSpan(viewer, 2);
            viewer.AutoSize = true;
            viewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            viewer.Dock = DockStyle.Fill;
        }

        internal void AdjustTlp(TableLayoutPanel tlp, int newRowCount, RowStyle rowStyleTemplate)
        {
            var oldRowCount = tlp.RowCount - 1;
            if (oldRowCount != newRowCount)
            {
                var diff = newRowCount - Math.Max(0, oldRowCount);
                if (diff > 0)
                {
                    tlp.InsertSpecificRow(oldRowCount, rowStyleTemplate, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height + (int)Math.Round(rowStyleTemplate.Height * diff, 0)
                    );
                }
                else
                {
                    tlp.RemoveSpecificRow(newRowCount, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height - (int)Math.Round(rowStyleTemplate.Height * diff, 0)
                    );
                }
            }
        }

        // LoadControllersViewersAsync lives in the partial part QfcQueue.Enqueue.cs; see that file
        // for the reason.

        public async Task ChangeIterationSize(
            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) entry,
            int newRowCount,
            RowStyle rowStyleTemplate
        )
        {
            // Wait for all jobs to finish to prevent conflicts
            await JobsToFinish(100, _token);

            // Adjust template for future jobs
            AdjustTlp(TlpTemplate, newRowCount, rowStyleTemplate);

            // Cache old queue in private collection,
            var oldQueue = _queue;

            // Externally visible queue is now empty, but job is marked as running
            _queue =
                new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();

            //logger.Debug($"{nameof(ChangeIterationSize)} called and jobsRunning increased to {_jobsRunning}");
            Interlocked.Increment(ref _jobsRunning);

            var queue =
                new BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>();

            while (oldQueue.Count > 0)
            {
                var nextEntry = oldQueue.Take();
                GrowEntry(ref entry, ref nextEntry, newRowCount, rowStyleTemplate);
                if (entry.ItemGroups.Count == newRowCount)
                {
                    RenumberGroups(entry.ItemGroups);
                    queue.Add(entry);
                    if (nextEntry.ItemGroups.Count > 0)
                    {
                        entry = nextEntry;
                    }
                    else
                    {
                        if (oldQueue.Count > 0)
                        {
                            entry = oldQueue.Take();
                        }
                        else
                        {
                            entry = default;
                        }
                    }
                }
            }

            if (entry != default)
            {
                var items = await _homeController.DataModel.DequeueNextItemGroupAsync(
                    newRowCount - entry.ItemGroups.Count,
                    1000
                );
                if (items.Count > 0)
                {
                    AdjustTlp(entry.Tlp, newRowCount, rowStyleTemplate);
                    var extraGroups = await LoadControllersViewersAsync(
                        items,
                        _globals,
                        _homeController,
                        _qfcCollectionController,
                        entry.Tlp,
                        entry.ItemGroups.Count
                    );
                    extraGroups.ForEach(group => entry.ItemGroups.Add(group));
                }
                RenumberGroups(entry.ItemGroups);
                queue.Add(entry);
            }

            // Discard top element in queue which will always be a duplicate
            _ = queue.Take();

            // Set the externally visible queue to the new queue and mark the job as complete
            _queue = queue;
            Interlocked.Decrement(ref _jobsRunning);
            //logger.Debug($"{nameof(ChangeIterationSize)} completed and jobsRunning decreased to {_jobsRunning}");
        }

        public void RenumberGroups(List<QfcItemGroup> itemGroups)
        {
            var digits = itemGroups.Count >= 10 ? 2 : 1;
            for (int i = 0; i < itemGroups.Count; i++)
            {
                itemGroups[i].ItemController.ItemNumberDigits = digits;
                itemGroups[i].ItemController.ItemNumber = i + 1;
            }
        }

        public void GrowEntry(
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) target,
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) source,
            int newRowCount,
            RowStyle rowStyleTemplate
        )
        {
            var currentCount = target.ItemGroups.Count;
            var grow = Math.Min(newRowCount - currentCount, source.ItemGroups.Count);

            AdjustTlp(target.Tlp, newRowCount, rowStyleTemplate);

            if (grow == 0)
            {
                return;
            }

            for (int i = 0; i < grow; i++)
            {
                var itemViewer = source.Tlp.Controls[i];
                var position = source.Tlp.GetCellPosition(itemViewer);
                itemViewer.Parent = target.Tlp;
                target.Tlp.SetCellPosition(
                    itemViewer,
                    new TableLayoutPanelCellPosition(position.Column, currentCount + i)
                );
                var group = source.ItemGroups[0];
                target.ItemGroups.Add(group);
                source.ItemGroups.RemoveAt(0);
                group.ItemController.ItemNumber = currentCount + i + 1;
            }

            source.Tlp.RemoveSpecificRow(0, grow);

            source.Tlp.MinimumSize = new System.Drawing.Size(
                source.Tlp.MinimumSize.Width,
                source.Tlp.MinimumSize.Height - (int)Math.Round(rowStyleTemplate.Height * grow, 0)
            );
        }

        #endregion Tlp Manipulation
    }
}
