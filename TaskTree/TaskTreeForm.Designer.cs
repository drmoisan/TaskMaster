using System;
using System.Diagnostics;

namespace TaskTree
{
    public partial class TaskTreeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.But_ReloadTree = new System.Windows.Forms.Button();
            this.But_ExpandCollapse = new System.Windows.Forms.Button();
            this.But_ShowHideComplete = new System.Windows.Forms.Button();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TreeLv = new BrightIdeasSoftware.TreeListView();
            this.OlvTaskSubject = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvToDoID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn9 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn6 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn7 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.OlvColumn8 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ContextMenuStrip1.SuspendLayout();
            this.FlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TreeLv)).BeginInit();
            this.SuspendLayout();
            // 
            // ImageList1
            // 
            this.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ImageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem1});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(148, 42);
            // 
            // ToolStripMenuItem1
            // 
            this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            this.ToolStripMenuItem1.Size = new System.Drawing.Size(147, 38);
            this.ToolStripMenuItem1.Text = "Open";
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowLayoutPanel1.Controls.Add(this.But_ReloadTree);
            this.FlowLayoutPanel1.Controls.Add(this.But_ExpandCollapse);
            this.FlowLayoutPanel1.Controls.Add(this.But_ShowHideComplete);
            this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(2738, 50);
            this.FlowLayoutPanel1.TabIndex = 1;
            // 
            // But_ReloadTree
            // 
            this.But_ReloadTree.Location = new System.Drawing.Point(6, 6);
            this.But_ReloadTree.Margin = new System.Windows.Forms.Padding(6);
            this.But_ReloadTree.Name = "But_ReloadTree";
            this.But_ReloadTree.Size = new System.Drawing.Size(234, 44);
            this.But_ReloadTree.TabIndex = 0;
            this.But_ReloadTree.Text = "Reload Tree";
            this.But_ReloadTree.UseVisualStyleBackColor = true;
            this.But_ReloadTree.Click += new System.EventHandler(this.But_ReloadTree_Click);
            // 
            // But_ExpandCollapse
            // 
            this.But_ExpandCollapse.Location = new System.Drawing.Point(252, 6);
            this.But_ExpandCollapse.Margin = new System.Windows.Forms.Padding(6);
            this.But_ExpandCollapse.Name = "But_ExpandCollapse";
            this.But_ExpandCollapse.Size = new System.Drawing.Size(234, 44);
            this.But_ExpandCollapse.TabIndex = 1;
            this.But_ExpandCollapse.Text = "Expand / Collapse All";
            this.But_ExpandCollapse.UseVisualStyleBackColor = true;
            this.But_ExpandCollapse.Click += new System.EventHandler(this.But_ExpandCollapse_Click);
            // 
            // But_ShowHideComplete
            // 
            this.But_ShowHideComplete.Location = new System.Drawing.Point(498, 6);
            this.But_ShowHideComplete.Margin = new System.Windows.Forms.Padding(6);
            this.But_ShowHideComplete.Name = "But_ShowHideComplete";
            this.But_ShowHideComplete.Size = new System.Drawing.Size(234, 44);
            this.But_ShowHideComplete.TabIndex = 3;
            this.But_ShowHideComplete.Text = "Show/Hide Complete";
            this.But_ShowHideComplete.UseVisualStyleBackColor = true;
            this.But_ShowHideComplete.Click += new System.EventHandler(this.But_ShowHideComplete_Click);
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.SplitContainer1.IsSplitterFixed = true;
            this.SplitContainer1.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer1.Margin = new System.Windows.Forms.Padding(6);
            this.SplitContainer1.Name = "SplitContainer1";
            this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer1.Panel1
            // 
            this.SplitContainer1.Panel1.AccessibleName = "SplitContainer1Panel1";
            this.SplitContainer1.Panel1.Controls.Add(this.FlowLayoutPanel1);
            this.SplitContainer1.Panel1MinSize = 35;
            // 
            // SplitContainer1.Panel2
            // 
            this.SplitContainer1.Panel2.AccessibleName = "SplitContainer1Panel2";
            this.SplitContainer1.Panel2.Controls.Add(this.TreeLv);
            this.SplitContainer1.Size = new System.Drawing.Size(2738, 915);
            this.SplitContainer1.SplitterWidth = 8;
            this.SplitContainer1.TabIndex = 2;
            // 
            // TreeLv
            // 
            this.TreeLv.AllColumns.Add(this.OlvTaskSubject);
            this.TreeLv.AllColumns.Add(this.OlvToDoID);
            this.TreeLv.AllColumns.Add(this.OlvColumn1);
            this.TreeLv.AllColumns.Add(this.OlvColumn3);
            this.TreeLv.AllColumns.Add(this.OlvColumn2);
            this.TreeLv.AllColumns.Add(this.OlvColumn9);
            this.TreeLv.AllColumns.Add(this.OlvColumn6);
            this.TreeLv.AllColumns.Add(this.OlvColumn4);
            this.TreeLv.AllColumns.Add(this.OlvColumn5);
            this.TreeLv.AllColumns.Add(this.OlvColumn7);
            this.TreeLv.AllColumns.Add(this.OlvColumn8);
            this.TreeLv.AllowColumnReorder = true;
            this.TreeLv.AllowDrop = true;
            this.TreeLv.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.TreeLv.CellEditUseWholeCell = false;
            this.TreeLv.CheckBoxes = true;
            this.TreeLv.CheckedAspectName = "Value.Complete";
            this.TreeLv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OlvTaskSubject,
            this.OlvToDoID,
            this.OlvColumn3,
            this.OlvColumn2,
            this.OlvColumn9,
            this.OlvColumn6,
            this.OlvColumn4,
            this.OlvColumn7,
            this.OlvColumn8});
            this.TreeLv.Cursor = System.Windows.Forms.Cursors.Default;
            this.TreeLv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeLv.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeLv.HideSelection = false;
            this.TreeLv.IsSimpleDragSource = true;
            this.TreeLv.IsSimpleDropSink = true;
            this.TreeLv.Location = new System.Drawing.Point(0, 0);
            this.TreeLv.Margin = new System.Windows.Forms.Padding(6);
            this.TreeLv.Name = "TreeLv";
            this.TreeLv.ShowGroups = false;
            this.TreeLv.ShowImagesOnSubItems = true;
            this.TreeLv.Size = new System.Drawing.Size(2738, 857);
            this.TreeLv.SmallImageList = this.ImageList1;
            this.TreeLv.TabIndex = 0;
            this.TreeLv.UseCompatibleStateImageBehavior = false;
            this.TreeLv.UseFiltering = true;
            this.TreeLv.View = System.Windows.Forms.View.Details;
            this.TreeLv.VirtualMode = true;
            this.TreeLv.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.FormatRow);
            this.TreeLv.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.HandleModelCanDrop);
            this.TreeLv.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.HandleModelDropped);
            this.TreeLv.ItemActivate += new System.EventHandler(this.TLV_ItemActivate);
            // 
            // OlvTaskSubject
            // 
            this.OlvTaskSubject.AspectName = "Value.TaskSubject";
            this.OlvTaskSubject.Text = "Task Subject";
            this.OlvTaskSubject.Width = 246;
            // 
            // OlvToDoID
            // 
            this.OlvToDoID.AspectName = "Value.ToDoID";
            this.OlvToDoID.Text = "To Do ID";
            this.OlvToDoID.Width = 241;
            // 
            // OlvColumn1
            // 
            this.OlvColumn1.AspectName = "Value.Project.AsStringNoPrefix";
            this.OlvColumn1.DisplayIndex = 1;
            this.OlvColumn1.IsVisible = false;
            this.OlvColumn1.Text = "Project";
            this.OlvColumn1.Width = 114;
            // 
            // OlvColumn3
            // 
            this.OlvColumn3.AspectName = "Value.People.AsStringNoPrefix";
            this.OlvColumn3.Text = "People";
            this.OlvColumn3.Width = 187;
            // 
            // OlvColumn2
            // 
            this.OlvColumn2.AspectName = "Value.Topic.AsStringNoPrefix";
            this.OlvColumn2.Text = "Topic";
            this.OlvColumn2.Width = 160;
            // 
            // OlvColumn9
            // 
            this.OlvColumn9.AspectName = "Value.Context.AsStringNoPrefix";
            this.OlvColumn9.Text = "Context";
            this.OlvColumn9.Width = 269;
            // 
            // OlvColumn6
            // 
            this.OlvColumn6.AspectName = "Value.MetaTaskLvl";
            this.OlvColumn6.Text = "Lvl";
            // 
            // OlvColumn4
            // 
            this.OlvColumn4.AspectName = "Value.MetaTaskSubject";
            this.OlvColumn4.Text = "Meta Task";
            this.OlvColumn4.Width = 187;
            // 
            // OlvColumn5
            // 
            this.OlvColumn5.AspectName = "Value.TaskCreateDate";
            this.OlvColumn5.DisplayIndex = 6;
            this.OlvColumn5.IsVisible = false;
            this.OlvColumn5.Text = "Created";
            this.OlvColumn5.Width = 146;
            // 
            // OlvColumn7
            // 
            this.OlvColumn7.AspectName = "Value.StartDate";
            this.OlvColumn7.Text = "Started";
            this.OlvColumn7.Width = 216;
            // 
            // OlvColumn8
            // 
            this.OlvColumn8.AspectName = "Value.InFolder";
            this.OlvColumn8.Text = "In Folder";
            this.OlvColumn8.Width = 198;
            // 
            // TaskTreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2738, 915);
            this.Controls.Add(this.SplitContainer1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "TaskTreeForm";
            this.Text = "TaskTreeForm";
            this.Load += new System.EventHandler(this.TaskTreeForm_Load);
            this.Resize += new System.EventHandler(this.TaskTreeForm_Resize);
            this.ContextMenuStrip1.ResumeLayout(false);
            this.FlowLayoutPanel1.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TreeLv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal BrightIdeasSoftware.OLVColumn OlvToDoID;
        internal BrightIdeasSoftware.OLVColumn OlvTaskSubject;
        internal System.Windows.Forms.ImageList ImageList1;
        internal BrightIdeasSoftware.OLVColumn OlvColumn1;
        internal BrightIdeasSoftware.OLVColumn OlvColumn2;
        internal BrightIdeasSoftware.OLVColumn OlvColumn3;
        internal BrightIdeasSoftware.OLVColumn OlvColumn4;
        internal BrightIdeasSoftware.OLVColumn OlvColumn5;
        internal System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
        internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem1;
        internal BrightIdeasSoftware.OLVColumn OlvColumn6;
        internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
        internal System.Windows.Forms.Button But_ReloadTree;
        internal System.Windows.Forms.Button But_ExpandCollapse;
        internal System.Windows.Forms.Button But_ShowHideComplete;
        internal System.Windows.Forms.SplitContainer SplitContainer1;
        internal BrightIdeasSoftware.OLVColumn OlvColumn7;
        internal BrightIdeasSoftware.OLVColumn OlvColumn8;
        internal BrightIdeasSoftware.TreeListView TreeLv;
        internal BrightIdeasSoftware.OLVColumn OlvColumn9;
    }
}