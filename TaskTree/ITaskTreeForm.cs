using System;
using System.Collections.Generic;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;

namespace TaskTree
{
    /// <summary>
    /// Intent-named facade over the concrete <see cref="TaskTreeForm"/> WinForms surface that
    /// <see cref="TaskTreeController"/> consumes. Deriving from
    /// <see cref="UtilitiesCS.Interfaces.IWinForm.IForm"/> keeps the controller decoupled from the
    /// concrete form while preserving the form contract. The concrete
    /// <c>BrightIdeasSoftware.TreeListView</c> control and any other UI-framework types
    /// (<c>OLVColumn</c>/<c>SplitContainer</c>/<c>ModelFilter</c>) are deliberately NOT exposed; every
    /// control operation is expressed as an intent-named member and mapped to the underlying controls
    /// entirely inside the concrete form.
    /// </summary>
    public interface ITaskTreeForm : IForm
    {
        /// <summary>Assigns the controller that the form's event handlers forward to.</summary>
        void SetController(TaskTreeController controller);

        /// <summary>
        /// Wires the tree view getters, applies the initial incomplete-item filter, sets the roots,
        /// and performs the initial sort.
        /// </summary>
        /// <param name="roots">The root nodes to display.</param>
        /// <param name="incompleteFilter">
        /// Predicate selecting the models to keep visible (the concrete form wraps it in the
        /// third-party model filter).
        /// </param>
        void InitializeTreeView(
            IEnumerable<TreeNode<ToDoItem>> roots,
            Predicate<object> incompleteFilter
        );

        /// <summary>
        /// Sets the visible-model filter. A <c>null</c> predicate clears the filter; a non-null
        /// predicate is wrapped by the concrete form.
        /// </summary>
        void SetModelFilter(Predicate<object> filter);

        /// <summary>Re-applies the current sort to the tree.</summary>
        void SortTree();

        /// <summary>Expands every node in the tree.</summary>
        void ExpandAllNodes();

        /// <summary>Collapses every node in the tree.</summary>
        void CollapseAllNodes();

        /// <summary>Replaces the tree roots and rebuilds the visual tree without preserving state.</summary>
        void RebuildTree(IEnumerable<TreeNode<ToDoItem>> roots);

        /// <summary>Auto-sizes the tree columns to the container.</summary>
        void AutoSizeTreeColumns();

        /// <summary>Returns the currently selected node, or <c>null</c> when nothing is selected.</summary>
        TreeNode<ToDoItem> GetSelectedNode();

        /// <summary>Re-runs the control-resize layout for the form.</summary>
        void ResizeControls();
    }

    /// <summary>
    /// Narrow adapter over the drop-event tree controls the move logic mutates. Exposes only the two
    /// operations the move methods require so the host-neutral move logic never references the
    /// concrete <c>BrightIdeasSoftware.TreeListView</c> control.
    /// </summary>
    public interface ITreeVisual
    {
        /// <summary>Adds a model to the visual tree.</summary>
        void AddObject(object model);

        /// <summary>Removes a model from the visual tree.</summary>
        void RemoveObject(object model);
    }
}
