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
            components = new System.ComponentModel.Container();
            ImageList1 = new System.Windows.Forms.ImageList(components);
            ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            But_ReloadTree = new System.Windows.Forms.Button();
            But_ReloadTree.Click += new EventHandler(But_ReloadTree_Click);
            But_ExpandCollapse = new System.Windows.Forms.Button();
            But_ExpandCollapse.Click += new EventHandler(But_ExpandCollapse_Click);
            But_ShowHideComplete = new System.Windows.Forms.Button();
            But_ShowHideComplete.Click += new EventHandler(But_ShowHideComplete_Click);
            SplitContainer1 = new System.Windows.Forms.SplitContainer();
            TLV = new BrightIdeasSoftware.TreeListView();
            TLV.ModelCanDrop += new EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(HandleModelCanDrop);
            TLV.ModelDropped += new EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(HandleModelDropped);
            TLV.ItemActivate += new EventHandler(TLV_ItemActivate);
            TLV.FormatRow += new EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(FormatRow);
            OlvTaskSubject = new BrightIdeasSoftware.OLVColumn();
            OlvToDoID = new BrightIdeasSoftware.OLVColumn();
            OlvColumn1 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn2 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn3 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn6 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn4 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn5 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn7 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn8 = new BrightIdeasSoftware.OLVColumn();
            OlvColumn9 = new BrightIdeasSoftware.OLVColumn();
            ContextMenuStrip1.SuspendLayout();
            FlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).BeginInit();
            SplitContainer1.Panel1.SuspendLayout();
            SplitContainer1.Panel2.SuspendLayout();
            SplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TLV).BeginInit();
            SuspendLayout();
            // 
            // ImageList1
            // 
            ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            ImageList1.ImageSize = new System.Drawing.Size(16, 16);
            ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ContextMenuStrip1
            // 
            ContextMenuStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolStripMenuItem1 });
            ContextMenuStrip1.Name = "ContextMenuStrip1";
            ContextMenuStrip1.Size = new System.Drawing.Size(104, 26);
            // 
            // ToolStripMenuItem1
            // 
            ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            ToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            ToolStripMenuItem1.Text = "Open";
            // 
            // FlowLayoutPanel1
            // 
            FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            FlowLayoutPanel1.Controls.Add(But_ReloadTree);
            FlowLayoutPanel1.Controls.Add(But_ExpandCollapse);
            FlowLayoutPanel1.Controls.Add(But_ShowHideComplete);
            FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            FlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            FlowLayoutPanel1.Size = new System.Drawing.Size(1369, 50);
            FlowLayoutPanel1.TabIndex = 1;
            // 
            // But_ReloadTree
            // 
            But_ReloadTree.Location = new System.Drawing.Point(3, 3);
            But_ReloadTree.Name = "But_ReloadTree";
            But_ReloadTree.Size = new System.Drawing.Size(117, 23);
            But_ReloadTree.TabIndex = 0;
            But_ReloadTree.Text = "Reload Tree";
            But_ReloadTree.UseVisualStyleBackColor = true;
            // 
            // But_ExpandCollapse
            // 
            But_ExpandCollapse.Location = new System.Drawing.Point(126, 3);
            But_ExpandCollapse.Name = "But_ExpandCollapse";
            But_ExpandCollapse.Size = new System.Drawing.Size(117, 23);
            But_ExpandCollapse.TabIndex = 1;
            But_ExpandCollapse.Text = "Expand / Collapse All";
            But_ExpandCollapse.UseVisualStyleBackColor = true;
            // 
            // But_ShowHideComplete
            // 
            But_ShowHideComplete.Location = new System.Drawing.Point(249, 3);
            But_ShowHideComplete.Name = "But_ShowHideComplete";
            But_ShowHideComplete.Size = new System.Drawing.Size(117, 23);
            But_ShowHideComplete.TabIndex = 3;
            But_ShowHideComplete.Text = "Show/Hide Complete";
            But_ShowHideComplete.UseVisualStyleBackColor = true;
            // 
            // SplitContainer1
            // 
            SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            SplitContainer1.IsSplitterFixed = true;
            SplitContainer1.Location = new System.Drawing.Point(0, 0);
            SplitContainer1.Name = "SplitContainer1";
            SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer1.Panel1
            // 
            SplitContainer1.Panel1.AccessibleName = "SplitContainer1Panel1";
            SplitContainer1.Panel1.Controls.Add(FlowLayoutPanel1);
            SplitContainer1.Panel1MinSize = 35;
            // 
            // SplitContainer1.Panel2
            // 
            SplitContainer1.Panel2.AccessibleName = "SplitContainer1Panel2";
            SplitContainer1.Panel2.Controls.Add(TLV);
            SplitContainer1.Size = new System.Drawing.Size(1369, 476);
            SplitContainer1.TabIndex = 2;
            // 
            // TLV
            // 
            TLV.AllColumns.Add(OlvTaskSubject);
            TLV.AllColumns.Add(OlvToDoID);
            TLV.AllColumns.Add(OlvColumn1);
            TLV.AllColumns.Add(OlvColumn3);
            TLV.AllColumns.Add(OlvColumn2);
            TLV.AllColumns.Add(OlvColumn9);
            TLV.AllColumns.Add(OlvColumn6);
            TLV.AllColumns.Add(OlvColumn4);
            TLV.AllColumns.Add(OlvColumn5);
            TLV.AllColumns.Add(OlvColumn7);
            TLV.AllColumns.Add(OlvColumn8);
            TLV.AllowColumnReorder = true;
            TLV.AllowDrop = true;
            TLV.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            TLV.CellEditUseWholeCell = false;
            TLV.CheckBoxes = true;
            TLV.CheckedAspectName = "Value.Complete";
            TLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { OlvTaskSubject, OlvToDoID, OlvColumn3, OlvColumn2, OlvColumn9, OlvColumn6, OlvColumn4, OlvColumn7, OlvColumn8 });
            TLV.Cursor = System.Windows.Forms.Cursors.Default;
            TLV.Dock = System.Windows.Forms.DockStyle.Fill;
            TLV.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TLV.HideSelection = false;
            TLV.IsSimpleDragSource = true;
            TLV.IsSimpleDropSink = true;
            TLV.Location = new System.Drawing.Point(0, 0);
            TLV.Name = "TLV";
            TLV.ShowGroups = false;
            TLV.ShowImagesOnSubItems = true;
            TLV.Size = new System.Drawing.Size(1369, 422);
            TLV.SmallImageList = ImageList1;
            TLV.TabIndex = 0;
            TLV.UseCompatibleStateImageBehavior = false;
            TLV.UseFiltering = true;
            TLV.View = System.Windows.Forms.View.Details;
            TLV.VirtualMode = true;
            // 
            // OlvTaskSubject
            // 
            OlvTaskSubject.AspectName = "Value.TaskSubject";
            OlvTaskSubject.Text = "Task Subject";
            OlvTaskSubject.Width = 246;
            // 
            // OlvToDoID
            // 
            OlvToDoID.AspectName = "Value.ToDoID";
            OlvToDoID.Text = "To Do ID";
            OlvToDoID.Width = 117;
            // 
            // OlvColumn1
            // 
            OlvColumn1.AspectName = "Value.Project";
            OlvColumn1.DisplayIndex = 1;
            OlvColumn1.IsVisible = false;
            OlvColumn1.Text = "Project";
            OlvColumn1.Width = 114;
            // 
            // OlvColumn2
            // 
            OlvColumn2.AspectName = "Value.Topic";
            OlvColumn2.Text = "Topic";
            OlvColumn2.Width = 94;
            // 
            // OlvColumn3
            // 
            OlvColumn3.AspectName = "Value.People";
            OlvColumn3.Text = "People";
            OlvColumn3.Width = 122;
            // 
            // OlvColumn6
            // 
            OlvColumn6.AspectName = "Value.MetaTaskLvl";
            OlvColumn6.Text = "Lvl";
            // 
            // OlvColumn4
            // 
            OlvColumn4.AspectName = "Value.MetaTaskSubject";
            OlvColumn4.Text = "Meta Task";
            OlvColumn4.Width = 187;
            // 
            // OlvColumn5
            // 
            OlvColumn5.AspectName = "Value.TaskCreateDate";
            OlvColumn5.DisplayIndex = 6;
            OlvColumn5.IsVisible = false;
            OlvColumn5.Text = "Created";
            OlvColumn5.Width = 146;
            // 
            // OlvColumn7
            // 
            OlvColumn7.AspectName = "Value.StartDate";
            OlvColumn7.Text = "Started";
            OlvColumn7.Width = 100;
            // 
            // OlvColumn8
            // 
            OlvColumn8.AspectName = "Value.InFolder";
            OlvColumn8.Text = "In Folder";
            OlvColumn8.Width = 103;
            // 
            // OlvColumn9
            // 
            OlvColumn9.AspectName = "Value.Context";
            OlvColumn9.Text = "Context";
            OlvColumn9.Width = 112;
            // 
            // TaskTreeForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6.0f, 13.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1369, 476);
            Controls.Add(SplitContainer1);
            Name = "TaskTreeForm";
            Text = "TaskTreeForm";
            ContextMenuStrip1.ResumeLayout(false);
            FlowLayoutPanel1.ResumeLayout(false);
            SplitContainer1.Panel1.ResumeLayout(false);
            SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainer1).EndInit();
            SplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TLV).EndInit();
            Load += new EventHandler(TaskTreeForm_Load);
            Resize += new EventHandler(TaskTreeForm_Resize);
            ResumeLayout(false);
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
        internal BrightIdeasSoftware.TreeListView TLV;
        internal BrightIdeasSoftware.OLVColumn OlvColumn9;
    }
}